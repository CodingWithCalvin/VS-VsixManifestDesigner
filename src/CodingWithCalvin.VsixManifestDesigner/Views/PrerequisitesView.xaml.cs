using System;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Dialogs;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using Microsoft.VisualStudio.Shell;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing prerequisites.
/// </summary>
public partial class PrerequisitesView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrerequisitesView"/> class.
    /// </summary>
    public PrerequisitesView()
    {
        InitializeComponent();
    }

    private IServiceProvider GetServiceProvider()
    {
        return ServiceProvider.GlobalProvider;
    }

    private void AddPrerequisite_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddPrerequisiteDialog(GetServiceProvider());

        if (dialog.ShowDialog() == true && dialog.Prerequisite != null)
        {
            if (DataContext is ManifestViewModel vm)
            {
                vm.Prerequisites.Add(dialog.Prerequisite);
            }
        }
    }

    private void RemovePrerequisite_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            // Get the DataGrid from the visual tree
            var dataGrid = FindDataGrid();
            if (dataGrid?.SelectedItem is Models.Prerequisite prerequisite)
            {
                vm.Prerequisites.Remove(prerequisite);
            }
        }
    }

    private DataGrid? FindDataGrid()
    {
        // Find the DataGrid in the visual tree
        return FindChild<DataGrid>(this);
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
            {
                return result;
            }

            var descendant = FindChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }
}
