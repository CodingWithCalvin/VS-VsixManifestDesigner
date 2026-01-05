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

    private IServiceProvider? GetServiceProvider()
    {
        return Package.GetGlobalService(typeof(SVsServiceProvider)) as IServiceProvider;
    }

    private void AddDependency_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            var serviceProvider = GetServiceProvider();
            if (serviceProvider == null)
            {
                return;
            }

            var dialog = new AddDependencyDialog(serviceProvider);
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
            var serviceProvider = GetServiceProvider();
            if (serviceProvider == null)
            {
                return;
            }

            if (DependenciesGrid.SelectedItem is Dependency selectedDependency)
            {
                var dialog = new AddDependencyDialog(serviceProvider, selectedDependency);
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
