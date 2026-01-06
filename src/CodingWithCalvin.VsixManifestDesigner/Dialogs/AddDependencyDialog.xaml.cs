using System;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for adding or editing dependencies.
/// </summary>
public partial class AddDependencyDialog : DialogWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _manifestFilePath;
    private ProjectInfo? _selectedProject;

    /// <summary>
    /// Gets the dependency being edited.
    /// </summary>
    public Dependency Dependency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDependencyDialog"/> class for adding.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="manifestFilePath">The path to the manifest file being edited.</param>
    public AddDependencyDialog(IServiceProvider serviceProvider, string? manifestFilePath = null)
        : this(serviceProvider, new Dependency(), manifestFilePath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDependencyDialog"/> class for editing.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="dependency">The dependency to edit.</param>
    /// <param name="manifestFilePath">The path to the manifest file being edited.</param>
    public AddDependencyDialog(IServiceProvider serviceProvider, Dependency dependency, string? manifestFilePath = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _manifestFilePath = manifestFilePath;

        Dependency = new Dependency
        {
            Id = dependency.Id,
            DisplayName = dependency.DisplayName,
            Version = dependency.Version,
            Source = dependency.Source,
            Location = dependency.Location,
            ProjectFullPath = dependency.ProjectFullPath
        };

        InitializeComponent();

        foreach (ComboBoxItem item in SourceComboBox.Items)
        {
            if (item.Tag?.ToString() == Dependency.Source)
            {
                SourceComboBox.SelectedItem = item;
                break;
            }
        }

        if (SourceComboBox.SelectedItem == null)
        {
            SourceComboBox.SelectedIndex = 0;
        }

        IdTextBox.Text = Dependency.Id;
        DisplayNameTextBox.Text = Dependency.DisplayName;
        VersionTextBox.Text = Dependency.Version;
        LocationTextBox.Text = Dependency.Location ?? string.Empty;

        UpdateSourceVisibility();
    }

    private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSourceVisibility();
    }

    private void UpdateSourceVisibility()
    {
        var source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

        // Show project picker for Project source
        var isProjectSource = source == "Project";
        ProjectLabel.Visibility = isProjectSource ? Visibility.Visible : Visibility.Collapsed;
        ProjectGrid.Visibility = isProjectSource ? Visibility.Visible : Visibility.Collapsed;

        // Show location for File source
        var isFileSource = source == "File";
        LocationLabel.Visibility = isFileSource ? Visibility.Visible : Visibility.Collapsed;
        LocationGrid.Visibility = isFileSource ? Visibility.Visible : Visibility.Collapsed;

        // Enable/disable ID field based on source
        IdTextBox.IsEnabled = !isProjectSource;
    }

    private void BrowseProject_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

        var dialog = new ProjectPickerDialog(_serviceProvider, _manifestFilePath)
        {
            ShowOnlyVsixProjects = true
        };

        if (DialogHelper.ShowDialogWithOwner(dialog) == true && dialog.SelectedProject != null)
        {
            _selectedProject = dialog.SelectedProject;
            ProjectTextBox.Text = _selectedProject.Name;

            // Auto-populate ID from project name
            IdTextBox.Text = _selectedProject.Name;
        }
    }

    private void BrowseLocation_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select VSIX File",
            Filter = "VSIX Files (*.vsix)|*.vsix|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            LocationTextBox.Text = dialog.FileName;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Manual";

        if (string.IsNullOrWhiteSpace(IdTextBox.Text))
        {
            MessageBox.Show("Please specify a dependency ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (source == "Project" && _selectedProject == null)
        {
            MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (source == "File" && string.IsNullOrWhiteSpace(LocationTextBox.Text))
        {
            MessageBox.Show("Please specify a file location.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Dependency.Id = IdTextBox.Text;
        Dependency.DisplayName = DisplayNameTextBox.Text;
        Dependency.Version = VersionTextBox.Text;
        Dependency.Source = source;
        Dependency.Location = source == "File" ? LocationTextBox.Text : null;
        Dependency.ProjectFullPath = source == "Project" ? _selectedProject?.FullPath : null;

        DialogResult = true;
        Close();
    }
}
