using System;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Dialogs;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using Microsoft.VisualStudio.Shell;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing assets.
/// </summary>
public partial class AssetsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetsView"/> class.
    /// </summary>
    public AssetsView()
    {
        InitializeComponent();
    }

    private IServiceProvider GetServiceProvider()
    {
        return ServiceProvider.GlobalProvider;
    }

    private void AddAsset_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            var dialog = new AddAssetDialog(GetServiceProvider());
            if (dialog.ShowDialog() == true)
            {
                vm.Assets.Add(dialog.Asset);
            }
        }
    }

    private void EditAsset_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            var dataGrid = FindName("AssetsGrid") as DataGrid;
            if (dataGrid?.SelectedItem is Asset selectedAsset)
            {
                var dialog = new AddAssetDialog(GetServiceProvider(), selectedAsset);
                if (dialog.ShowDialog() == true)
                {
                    var index = vm.Assets.IndexOf(selectedAsset);
                    if (index >= 0)
                    {
                        vm.Assets[index] = dialog.Asset;
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
            }
        }
    }
}
