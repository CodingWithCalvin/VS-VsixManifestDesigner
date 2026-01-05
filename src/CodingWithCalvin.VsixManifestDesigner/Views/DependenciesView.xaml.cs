using System;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Dialogs;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using Microsoft.VisualStudio.Shell;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing dependencies.
/// </summary>
public partial class DependenciesView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependenciesView"/> class.
    /// </summary>
    public DependenciesView()
    {
        InitializeComponent();
    }

    private IServiceProvider GetServiceProvider()
    {
        return ServiceProvider.GlobalProvider;
    }

    private void AddDependency_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            var dialog = new AddDependencyDialog(GetServiceProvider());
            if (dialog.ShowDialog() == true)
            {
                vm.Dependencies.Add(dialog.Dependency);
            }
        }
    }

    private void EditDependency_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            if (DependenciesGrid.SelectedItem is Dependency selectedDependency)
            {
                var dialog = new AddDependencyDialog(GetServiceProvider(), selectedDependency);
                if (dialog.ShowDialog() == true)
                {
                    var index = vm.Dependencies.IndexOf(selectedDependency);
                    if (index >= 0)
                    {
                        vm.Dependencies[index] = dialog.Dependency;
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
            }
        }
    }
}
