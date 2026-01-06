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
/// View for editing assets.
/// </summary>
public partial class AssetsView : UserControl
{
    private readonly IProjectIntegrationService _projectIntegrationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetsView"/> class.
    /// </summary>
    public AssetsView()
    {
        _projectIntegrationService = new ProjectIntegrationService();
        InitializeComponent();
    }

    private IServiceProvider GetServiceProvider()
    {
        return ServiceProvider.GlobalProvider;
    }

    private void AddAsset_Click(object sender, RoutedEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (DataContext is ManifestViewModel vm)
        {
            var dialog = new AddAssetDialog(GetServiceProvider(), vm.ManifestFilePath);
            if (DialogHelper.ShowDialogWithOwner(dialog) == true)
            {
                var asset = dialog.Asset;
                vm.Assets.Add(asset);

                // Handle project integration
                _ = HandleAssetAddedAsync(vm.ManifestFilePath, asset);
            }
        }
    }

    private void EditAsset_Click(object sender, RoutedEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (DataContext is ManifestViewModel vm)
        {
            var dataGrid = FindName("AssetsGrid") as DataGrid;
            if (dataGrid?.SelectedItem is Asset selectedAsset)
            {
                var originalAsset = new Asset
                {
                    Type = selectedAsset.Type,
                    Source = selectedAsset.Source,
                    Path = selectedAsset.Path,
                    ProjectName = selectedAsset.ProjectName,
                    ProjectFullPath = selectedAsset.ProjectFullPath,
                    TargetPath = selectedAsset.TargetPath,
                    VsixSubPath = selectedAsset.VsixSubPath,
                    Addressable = selectedAsset.Addressable
                };

                var dialog = new AddAssetDialog(GetServiceProvider(), selectedAsset, vm.ManifestFilePath);
                if (DialogHelper.ShowDialogWithOwner(dialog) == true)
                {
                    var index = vm.Assets.IndexOf(selectedAsset);
                    if (index >= 0)
                    {
                        vm.Assets[index] = dialog.Asset;

                        // Handle project integration changes
                        _ = HandleAssetEditedAsync(vm, originalAsset, dialog.Asset);
                    }
                }
            }
        }
    }

    private void RemoveAsset_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            var dataGrid = FindName("AssetsGrid") as DataGrid;
            if (dataGrid?.SelectedItem is Asset selectedAsset)
            {
                vm.Assets.Remove(selectedAsset);

                // Handle project integration
                _ = HandleAssetRemovedAsync(vm, selectedAsset);
            }
        }
    }

    private async System.Threading.Tasks.Task HandleAssetAddedAsync(string? manifestFilePath, Asset asset)
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

        if (asset.Source == "Project" && !string.IsNullOrEmpty(asset.ProjectFullPath))
        {
            await _projectIntegrationService.AddProjectReferenceAsync(
                vsixProjectPath,
                asset.ProjectFullPath,
                asset.Type);
        }
        else if (asset.Source == "File" && !string.IsNullOrEmpty(asset.Path))
        {
            await _projectIntegrationService.AddFileToProjectAsync(
                vsixProjectPath,
                asset.Path,
                includeInVsix: true);
        }
    }

    private async System.Threading.Tasks.Task HandleAssetEditedAsync(ManifestViewModel vm, Asset originalAsset, Asset newAsset)
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
        if (originalAsset.Source == "Project" && originalAsset.ProjectFullPath != newAsset.ProjectFullPath)
        {
            // Remove old reference if no other assets use it
            if (!string.IsNullOrEmpty(originalAsset.ProjectFullPath))
            {
                var stillUsed = vm.Assets.Any(a =>
                    a != newAsset &&
                    a.Source == "Project" &&
                    a.ProjectFullPath == originalAsset.ProjectFullPath);

                if (!stillUsed)
                {
                    await _projectIntegrationService.RemoveProjectReferenceAsync(
                        vsixProjectPath,
                        originalAsset.ProjectFullPath);
                }
            }
        }

        // Add new project reference if needed
        if (newAsset.Source == "Project" && !string.IsNullOrEmpty(newAsset.ProjectFullPath))
        {
            await _projectIntegrationService.AddProjectReferenceAsync(
                vsixProjectPath,
                newAsset.ProjectFullPath,
                newAsset.Type);
        }
    }

    private async System.Threading.Tasks.Task HandleAssetRemovedAsync(ManifestViewModel vm, Asset removedAsset)
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

        if (removedAsset.Source == "Project" && !string.IsNullOrEmpty(removedAsset.ProjectFullPath))
        {
            // Check if any other assets still reference this project
            var stillUsed = vm.Assets.Any(a =>
                a.Source == "Project" &&
                a.ProjectFullPath == removedAsset.ProjectFullPath);

            if (!stillUsed)
            {
                await _projectIntegrationService.RemoveProjectReferenceAsync(
                    vsixProjectPath,
                    removedAsset.ProjectFullPath);
            }
        }
        else if (removedAsset.Source == "File" && !string.IsNullOrEmpty(removedAsset.Path))
        {
            // Prompt user about file removal
            var result = MessageBox.Show(
                $"Do you also want to remove the file '{removedAsset.Path}' from the project?",
                "Remove File",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _projectIntegrationService.RemoveFileFromProjectAsync(
                    vsixProjectPath,
                    removedAsset.Path,
                    deleteFromDisk: false);
            }
        }
    }
}
