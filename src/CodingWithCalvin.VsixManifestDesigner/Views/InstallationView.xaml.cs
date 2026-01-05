using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
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
        if (DataContext is ManifestViewModel vm)
        {
            // TODO: Show dialog to allow user to customize installation target details
            // (VS edition, version range, architecture)
            vm.InstallationTargets.Add(new InstallationTarget());
        }
    }

    private void RemoveTarget_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement remove selected
    }
}
