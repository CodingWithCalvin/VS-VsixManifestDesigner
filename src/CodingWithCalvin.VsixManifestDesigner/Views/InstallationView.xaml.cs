using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Dialogs;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing installation targets.
/// </summary>
public partial class InstallationView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallationView"/> class.
    /// </summary>
    public InstallationView()
    {
        InitializeComponent();
    }

    private void AddTarget_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddInstallationTargetDialog();

        if (dialog.ShowDialog() == true && dialog.InstallationTargets.Count > 0)
        {
            if (DataContext is ManifestViewModel vm)
            {
                foreach (var target in dialog.InstallationTargets)
                {
                    vm.InstallationTargets.Add(target);
                }
            }
        }
    }

    private void RemoveTarget_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            // Get the DataGrid from the visual tree
            var dataGrid = FindDataGrid();
            if (dataGrid?.SelectedItem is Models.InstallationTarget target)
            {
                vm.InstallationTargets.Remove(target);
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
