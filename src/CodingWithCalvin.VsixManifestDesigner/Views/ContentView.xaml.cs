using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Dialogs;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using Microsoft.VisualStudio.Shell;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing content declarations (templates).
/// </summary>
public partial class ContentView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentView"/> class.
    /// </summary>
    public ContentView()
    {
        InitializeComponent();
    }

    private void AddContent_Click(object sender, RoutedEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (DataContext is ManifestViewModel vm)
        {
            var dialog = new AddContentDialog();
            if (DialogHelper.ShowDialogWithOwner(dialog) == true)
            {
                vm.Contents.Add(dialog.ContentItem);
            }
        }
    }

    private void EditContent_Click(object sender, RoutedEventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (DataContext is ManifestViewModel vm && ContentsGrid.SelectedItem is Content selectedContent)
        {
            var dialog = new AddContentDialog(selectedContent);
            if (DialogHelper.ShowDialogWithOwner(dialog) == true)
            {
                var index = vm.Contents.IndexOf(selectedContent);
                if (index >= 0)
                {
                    vm.Contents[index] = dialog.ContentItem;
                }
            }
        }
    }

    private void RemoveContent_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm && ContentsGrid.SelectedItem is Content selectedContent)
        {
            vm.Contents.Remove(selectedContent);
        }
    }
}
