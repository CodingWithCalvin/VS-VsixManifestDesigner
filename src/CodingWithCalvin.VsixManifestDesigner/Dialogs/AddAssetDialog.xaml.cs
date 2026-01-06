using System;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for adding or editing assets.
/// </summary>
public partial class AddAssetDialog : DialogWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _manifestFilePath;
    private ProjectInfo? _selectedProject;

    /// <summary>
    /// Gets the asset being edited.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAssetDialog"/> class for adding.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="manifestFilePath">The path to the manifest file being edited.</param>
    public AddAssetDialog(IServiceProvider serviceProvider, string? manifestFilePath = null)
        : this(serviceProvider, new Asset(), manifestFilePath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAssetDialog"/> class for editing.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="asset">The asset to edit.</param>
    /// <param name="manifestFilePath">The path to the manifest file being edited.</param>
    public AddAssetDialog(IServiceProvider serviceProvider, Asset asset, string? manifestFilePath = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _manifestFilePath = manifestFilePath;

        Asset = new Asset
        {
            Type = asset.Type,
            Source = asset.Source,
            Path = asset.Path,
            ProjectName = asset.ProjectName,
            ProjectFullPath = asset.ProjectFullPath,
            TargetPath = asset.TargetPath,
            VsixSubPath = asset.VsixSubPath,
            Addressable = asset.Addressable
        };

        InitializeComponent();

        TypeComboBox.ItemsSource = AssetTypes.All;
        TypeComboBox.SelectedItem = string.IsNullOrEmpty(Asset.Type) ? AssetTypes.VsPackage : Asset.Type;

        foreach (ComboBoxItem item in SourceComboBox.Items)
        {
            if (item.Tag?.ToString() == Asset.Source)
            {
                SourceComboBox.SelectedItem = item;
                break;
            }
        }

        if (SourceComboBox.SelectedItem == null)
        {
            SourceComboBox.SelectedIndex = 0;
        }

        ProjectTextBox.Text = Asset.ProjectName ?? string.Empty;
        PathTextBox.Text = Asset.Path;
        VsixSubPathTextBox.Text = Asset.VsixSubPath ?? string.Empty;
        AddressableCheckBox.IsChecked = Asset.Addressable;

        UpdateSourceVisibility();
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateWarningVisibility();
    }

    private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Clear source-specific fields when source changes
        _selectedProject = null;
        if (ProjectTextBox != null)
        {
            ProjectTextBox.Text = string.Empty;
        }
        if (PathTextBox != null)
        {
            PathTextBox.Text = string.Empty;
        }

        UpdateSourceVisibility();
        UpdateWarningVisibility();
    }

    private void UpdateSourceVisibility()
    {
        var source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        var isProjectSource = source == "Project";
        var isFileSource = source == "File";

        // Show project picker only for Project source
        ProjectLabel.Visibility = isProjectSource ? Visibility.Visible : Visibility.Collapsed;
        ProjectGrid.Visibility = isProjectSource ? Visibility.Visible : Visibility.Collapsed;

        // Show path picker only for File source
        PathLabel.Visibility = isFileSource ? Visibility.Visible : Visibility.Collapsed;
        PathGrid.Visibility = isFileSource ? Visibility.Visible : Visibility.Collapsed;

        // CurrentProject source shows nothing additional
    }

    private void UpdateWarningVisibility()
    {
        var selectedType = TypeComboBox.SelectedItem as string;
        var isTemplateType = AssetTypes.IsTemplate(selectedType ?? string.Empty);
        var source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

        // Show warning if template asset type + project source + SDK-style project without VsixSdk
        if (isTemplateType && source == "Project" && _selectedProject != null)
        {
            if (_selectedProject.IsSdkStyle && !_selectedProject.UsesVsixSdk)
            {
                WarningBorder.Visibility = Visibility.Visible;
                return;
            }
        }

        WarningBorder.Visibility = Visibility.Collapsed;
    }

    private void BrowseProject_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

        var dialog = new ProjectPickerDialog(_serviceProvider, _manifestFilePath);
        if (DialogHelper.ShowDialogWithOwner(dialog) == true && dialog.SelectedProject != null)
        {
            _selectedProject = dialog.SelectedProject;
            ProjectTextBox.Text = _selectedProject.Name;
            UpdateWarningVisibility();
        }
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Asset File",
            Filter = "All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            PathTextBox.Text = dialog.FileName;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "CurrentProject";
        var isProjectSource = source == "Project";
        var isCurrentProjectSource = source == "CurrentProject";
        var isFileSource = source == "File";

        if (isProjectSource && string.IsNullOrWhiteSpace(ProjectTextBox.Text))
        {
            MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (isFileSource && string.IsNullOrWhiteSpace(PathTextBox.Text))
        {
            MessageBox.Show("Please specify a file path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Asset.Type = TypeComboBox.SelectedItem as string ?? AssetTypes.VsPackage;
        Asset.Source = source;

        if (isCurrentProjectSource)
        {
            // CurrentProject uses %CurrentProject% token - no ProjectReference needed
            Asset.Path = "%CurrentProject%";
            Asset.ProjectName = null;
            Asset.ProjectFullPath = null;
            Asset.TargetPath = null;
        }
        else if (isProjectSource)
        {
            Asset.ProjectName = ProjectTextBox.Text;
            Asset.ProjectFullPath = _selectedProject?.FullPath;

            // Template assets use Path for the required folder and TargetPath for the project token
            var requiredFolder = AssetTypes.GetRequiredFolder(Asset.Type);
            if (!string.IsNullOrEmpty(requiredFolder))
            {
                Asset.Path = requiredFolder;
                Asset.TargetPath = AssetTypes.GenerateProjectToken(ProjectTextBox.Text, Asset.Type);
            }
            else
            {
                Asset.Path = AssetTypes.GenerateProjectToken(ProjectTextBox.Text, Asset.Type);
                Asset.TargetPath = null;
            }
        }
        else
        {
            Asset.Path = PathTextBox.Text;
            Asset.ProjectName = null;
            Asset.ProjectFullPath = null;
            Asset.TargetPath = null;
        }

        Asset.VsixSubPath = string.IsNullOrWhiteSpace(VsixSubPathTextBox.Text) ? null : VsixSubPathTextBox.Text;
        Asset.Addressable = AddressableCheckBox.IsChecked ?? false;

        DialogResult = true;
        Close();
    }
}
