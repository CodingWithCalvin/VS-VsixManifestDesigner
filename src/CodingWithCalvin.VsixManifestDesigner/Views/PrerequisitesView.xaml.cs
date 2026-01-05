using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;

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

    private void AddPrerequisite_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ManifestViewModel vm)
        {
            // TODO: Show dialog to select VS component from known list
            // or allow manual entry
            vm.Prerequisites.Add(new Prerequisite());
        }
    }

    private void RemovePrerequisite_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement remove selected
    }
}
