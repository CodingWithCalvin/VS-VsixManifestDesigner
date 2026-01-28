using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
using Microsoft.VisualStudio.PlatformUI;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for adding an installation target.
/// </summary>
public partial class AddInstallationTargetDialog : DialogWindow
{
    private static readonly string[] AllArchitectures = { "amd64", "arm64" };

    /// <summary>
    /// Initializes a new instance of the <see cref="AddInstallationTargetDialog"/> class.
    /// </summary>
    public AddInstallationTargetDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the installation targets created by this dialog.
    /// When "All architectures" is selected, this returns one target per architecture.
    /// </summary>
    public List<InstallationTarget> InstallationTargets { get; } = new();

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var edition = (EditionComboBox.SelectedItem as ComboBoxItem)?.Tag as string ?? string.Empty;
        var version = VersionTextBox.Text?.Trim() ?? string.Empty;
        var architecture = (ArchitectureComboBox.SelectedItem as ComboBoxItem)?.Tag as string;

        if (string.IsNullOrEmpty(edition))
        {
            MessageBox.Show("Please select a Visual Studio edition.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(version))
        {
            MessageBox.Show("Please enter a version range.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        InstallationTargets.Clear();

        if (string.IsNullOrEmpty(architecture))
        {
            // "All architectures" selected - create one target per architecture
            foreach (var arch in AllArchitectures)
            {
                InstallationTargets.Add(new InstallationTarget
                {
                    Id = edition,
                    Version = version,
                    ProductArchitecture = arch
                });
            }
        }
        else
        {
            // Specific architecture selected
            InstallationTargets.Add(new InstallationTarget
            {
                Id = edition,
                Version = version,
                ProductArchitecture = architecture
            });
        }

        DialogResult = true;
        Close();
    }
}
