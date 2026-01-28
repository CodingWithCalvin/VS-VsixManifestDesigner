using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
using CodingWithCalvin.VsixManifestDesigner.Services;
using Microsoft.VisualStudio.PlatformUI;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for adding a prerequisite.
/// </summary>
public partial class AddPrerequisiteDialog : DialogWindow
{
    private readonly IServiceProvider? _serviceProvider;
    private IReadOnlyList<SetupPackageInfo>? _installedPackages;
    private bool _packagesLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPrerequisiteDialog"/> class.
    /// </summary>
    public AddPrerequisiteDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPrerequisiteDialog"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for loading installed packages.</param>
    public AddPrerequisiteDialog(IServiceProvider serviceProvider) : this()
    {
        _serviceProvider = serviceProvider;
        _ = LoadInstalledPackagesAsync();
    }

    private async System.Threading.Tasks.Task LoadInstalledPackagesAsync()
    {
        if (_packagesLoaded || _serviceProvider == null)
        {
            return;
        }

        _packagesLoaded = true;

        try
        {
            var service = new SetupPackageService(_serviceProvider);
            _installedPackages = await service.GetInstalledPackagesAsync();

            // Clear existing items except the first "(Custom)" item
            while (CommonComboBox.Items.Count > 1)
            {
                CommonComboBox.Items.RemoveAt(1);
            }

            // Add loaded packages
            foreach (var package in _installedPackages)
            {
                CommonComboBox.Items.Add(new ComboBoxItem
                {
                    Content = package.Title,
                    Tag = package.Id
                });
            }
        }
        catch
        {
            // Keep the static fallback items if loading fails
        }
    }

    /// <summary>
    /// Gets the prerequisite created by this dialog.
    /// </summary>
    public Prerequisite? Prerequisite { get; private set; }

    private void CommonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CommonComboBox.SelectedItem is ComboBoxItem item)
        {
            var componentId = item.Tag as string;
            if (!string.IsNullOrEmpty(componentId))
            {
                IdTextBox.Text = componentId;
                DisplayNameTextBox.Text = item.Content?.ToString() ?? string.Empty;
            }
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var id = IdTextBox.Text?.Trim() ?? string.Empty;
        var displayName = DisplayNameTextBox.Text?.Trim() ?? string.Empty;
        var version = VersionTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(id))
        {
            MessageBox.Show("Please enter a Component ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Prerequisite = new Prerequisite
        {
            Id = id,
            DisplayName = displayName,
            Version = version
        };

        DialogResult = true;
        Close();
    }
}
