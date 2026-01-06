using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Dialogs;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.Services;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using Microsoft.VisualStudio.Shell;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing dependencies.
/// </summary>
public partial class DependenciesView : UserControl
{
    private readonly IProjectIntegrationService _projectIntegrationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependenciesView"/> class.
    /// </summary>
    public DependenciesView()
    {
        _projectIntegrationService = new ProjectIntegrationService();
        InitializeComponent();
    }

    private IServiceProvider GetServiceProvider()
    {
        return ServiceProvider.GlobalProvider;
    }

    private void AddDependency_Click(object sender, RoutedEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (DataContext is ManifestViewModel vm)
        {
            var dialog = new AddDependencyDialog(GetServiceProvider(), vm.ManifestFilePath);
            if (DialogHelper.ShowDialogWithOwner(dialog) == true)
            {
                var dependency = dialog.Dependency;
                vm.Dependencies.Add(dependency);

                // Handle project integration
                _ = HandleDependencyAddedAsync(vm.ManifestFilePath, dependency);
            }
        }
    }

    private void EditDependency_Click(object sender, RoutedEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (DataContext is ManifestViewModel vm)
        {
            if (DependenciesGrid.SelectedItem is Dependency selectedDependency)
            {
                var originalDependency = new Dependency
                {
                    Id = selectedDependency.Id,
                    DisplayName = selectedDependency.DisplayName,
                    Version = selectedDependency.Version,
                    Source = selectedDependency.Source,
                    Location = selectedDependency.Location,
                    ProjectFullPath = selectedDependency.ProjectFullPath
                };

                var dialog = new AddDependencyDialog(GetServiceProvider(), selectedDependency, vm.ManifestFilePath);
                if (DialogHelper.ShowDialogWithOwner(dialog) == true)
                {
                    var index = vm.Dependencies.IndexOf(selectedDependency);
                    if (index >= 0)
                    {
                        vm.Dependencies[index] = dialog.Dependency;

                        // Handle project integration changes
                        _ = HandleDependencyEditedAsync(vm, originalDependency, dialog.Dependency);
                    }
                }
            }
        }
    }

    private void RemoveDependency_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            if (DependenciesGrid.SelectedItem is Dependency selectedDependency)
            {
                vm.Dependencies.Remove(selectedDependency);

                // Handle project integration
                _ = HandleDependencyRemovedAsync(vm, selectedDependency);
            }
        }
    }

    private async System.Threading.Tasks.Task HandleDependencyAddedAsync(string? manifestFilePath, Dependency dependency)
    {
        if (string.IsNullOrEmpty(manifestFilePath))
        {
            return;
        }

        var vsixProjectPath = _projectIntegrationService.GetVsixProjectPath(manifestFilePath);
        if (string.IsNullOrEmpty(vsixProjectPath))
        {
            return;
        }

        if (dependency.Source == "Project" && !string.IsNullOrEmpty(dependency.ProjectFullPath))
        {
            await _projectIntegrationService.AddProjectReferenceAsync(
                vsixProjectPath,
                dependency.ProjectFullPath);
        }
    }

    private async System.Threading.Tasks.Task HandleDependencyEditedAsync(ManifestViewModel vm, Dependency originalDependency, Dependency newDependency)
    {
        if (string.IsNullOrEmpty(vm.ManifestFilePath))
        {
            return;
        }

        var vsixProjectPath = _projectIntegrationService.GetVsixProjectPath(vm.ManifestFilePath);
        if (string.IsNullOrEmpty(vsixProjectPath))
        {
            return;
        }

        // If project reference changed, handle the transition
        if (originalDependency.Source == "Project" && originalDependency.ProjectFullPath != newDependency.ProjectFullPath)
        {
            // Remove old reference if no other dependencies or assets use it
            if (!string.IsNullOrEmpty(originalDependency.ProjectFullPath))
            {
                var stillUsedByDependencies = vm.Dependencies.Any(d =>
                    d != newDependency &&
                    d.Source == "Project" &&
                    d.ProjectFullPath == originalDependency.ProjectFullPath);

                var stillUsedByAssets = vm.Assets.Any(a =>
                    a.Source == "Project" &&
                    a.ProjectFullPath == originalDependency.ProjectFullPath);

                if (!stillUsedByDependencies && !stillUsedByAssets)
                {
                    await _projectIntegrationService.RemoveProjectReferenceAsync(
                        vsixProjectPath,
                        originalDependency.ProjectFullPath);
                }
            }
        }

        // Add new project reference if needed
        if (newDependency.Source == "Project" && !string.IsNullOrEmpty(newDependency.ProjectFullPath))
        {
            await _projectIntegrationService.AddProjectReferenceAsync(
                vsixProjectPath,
                newDependency.ProjectFullPath);
        }
    }

    private async System.Threading.Tasks.Task HandleDependencyRemovedAsync(ManifestViewModel vm, Dependency removedDependency)
    {
        if (string.IsNullOrEmpty(vm.ManifestFilePath))
        {
            return;
        }

        var vsixProjectPath = _projectIntegrationService.GetVsixProjectPath(vm.ManifestFilePath);
        if (string.IsNullOrEmpty(vsixProjectPath))
        {
            return;
        }

        if (removedDependency.Source == "Project" && !string.IsNullOrEmpty(removedDependency.ProjectFullPath))
        {
            // Check if any other dependencies or assets still reference this project
            var stillUsedByDependencies = vm.Dependencies.Any(d =>
                d.Source == "Project" &&
                d.ProjectFullPath == removedDependency.ProjectFullPath);

            var stillUsedByAssets = vm.Assets.Any(a =>
                a.Source == "Project" &&
                a.ProjectFullPath == removedDependency.ProjectFullPath);

            if (!stillUsedByDependencies && !stillUsedByAssets)
            {
                await _projectIntegrationService.RemoveProjectReferenceAsync(
                    vsixProjectPath,
                    removedDependency.ProjectFullPath);
            }
        }
    }
}
