using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.Services;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for selecting a project from the solution.
/// </summary>
public partial class ProjectPickerDialog : DialogWindow
{
    private readonly SolutionService _solutionService;

    /// <summary>
    /// Gets the selected project.
    /// </summary>
    public ProjectInfo? SelectedProject { get; private set; }

    /// <summary>
    /// Gets or sets whether to filter to only show VSIX projects.
    /// </summary>
    public bool ShowOnlyVsixProjects { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPickerDialog"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ProjectPickerDialog(IServiceProvider serviceProvider)
    {
        _solutionService = new SolutionService(serviceProvider);

        InitializeComponent();

        Resources.Add("BoolToVisibilityConverter", new BooleanToVisibilityConverter());

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
#pragma warning disable CS4014, VSTHRD110 // Fire-and-forget for UI initialization
        LoadProjectsAsync();
#pragma warning restore CS4014, VSTHRD110
    }

    private async Task LoadProjectsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        IReadOnlyList<ProjectInfo> projects;
        if (ShowOnlyVsixProjects)
        {
            projects = await _solutionService.GetVsixProjectsAsync();
        }
        else
        {
            projects = await _solutionService.GetProjectsAsync();
        }

        ProjectListBox.ItemsSource = projects;
    }

    private void ProjectListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ProjectListBox.SelectedItem != null)
        {
            OkButton_Click(sender, e);
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectListBox.SelectedItem is ProjectInfo project)
        {
            SelectedProject = project;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
