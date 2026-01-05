using System;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for adding or editing assets.
/// </summary>
public partial class AddAssetDialog : DialogWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _manifestFilePath;
    private ProjectInfo? _selectedProject;

    /// <summary>
    /// Gets the asset being edited.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAssetDialog"/> class for adding.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="manifestFilePath">The path to the manifest file being edited.</param>
    public AddAssetDialog(IServiceProvider serviceProvider, string? manifestFilePath = null)
        : this(serviceProvider, new Asset(), manifestFilePath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAssetDialog"/> class for editing.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="asset">The asset to edit.</param>
    /// <param name="manifestFilePath">The path to the manifest file being edited.</param>
    public AddAssetDialog(IServiceProvider serviceProvider, Asset asset, string? manifestFilePath = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _manifestFilePath = manifestFilePath;

        Asset = new Asset
        {
            Type = asset.Type,
            Source = asset.Source,
            Path = asset.Path,
            ProjectName = asset.ProjectName,
            TargetPath = asset.TargetPath,
            VsixSubPath = asset.VsixSubPath,
            Addressable = asset.Addressable
        };

        InitializeComponent();

        TypeComboBox.ItemsSource = AssetTypes.All;
        TypeComboBox.SelectedItem = string.IsNullOrEmpty(Asset.Type) ? AssetTypes.VsPackage : Asset.Type;

        foreach (ComboBoxItem item in SourceComboBox.Items)
        {
            if (item.Tag?.ToString() == Asset.Source)
            {
                SourceComboBox.SelectedItem = item;
                break;
            }
        }

        if (SourceComboBox.SelectedItem == null)
        {
            SourceComboBox.SelectedIndex = 0;
        }

        ProjectTextBox.Text = Asset.ProjectName ?? string.Empty;
        PathTextBox.Text = Asset.Path;
        VsixSubPathTextBox.Text = Asset.VsixSubPath ?? string.Empty;
        AddressableCheckBox.IsChecked = Asset.Addressable;

        UpdateSourceVisibility();
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateWarningVisibility();
    }

    private void SourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSourceVisibility();
        UpdateWarningVisibility();
    }

    private void UpdateSourceVisibility()
    {
        var isProjectSource = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "Project";

        ProjectLabel.Visibility = isProjectSource ? Visibility.Visible : Visibility.Collapsed;
        ProjectGrid.Visibility = isProjectSource ? Visibility.Visible : Visibility.Collapsed;
        PathLabel.Visibility = isProjectSource ? Visibility.Collapsed : Visibility.Visible;
        PathGrid.Visibility = isProjectSource ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateWarningVisibility()
    {
        var selectedType = TypeComboBox.SelectedItem as string;
        var isTemplateType = AssetTypes.IsTemplate(selectedType ?? string.Empty);
        var isProjectSource = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "Project";

        // Show warning if template asset type + project source + SDK-style project without VsixSdk
        if (isTemplateType && isProjectSource && _selectedProject != null)
        {
            if (_selectedProject.IsSdkStyle && !_selectedProject.UsesVsixSdk)
            {
                WarningBorder.Visibility = Visibility.Visible;
                return;
            }
        }

        WarningBorder.Visibility = Visibility.Collapsed;
    }

    private void BrowseProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProjectPickerDialog(_serviceProvider, _manifestFilePath);
        if (dialog.ShowDialog() == true && dialog.SelectedProject != null)
        {
            _selectedProject = dialog.SelectedProject;
            ProjectTextBox.Text = _selectedProject.Name;
            UpdateWarningVisibility();
        }
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Asset File",
            Filter = "All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            PathTextBox.Text = dialog.FileName;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Project";
        var isProjectSource = source == "Project";

        if (isProjectSource && string.IsNullOrWhiteSpace(ProjectTextBox.Text))
        {
            MessageBox.Show("Please select a project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!isProjectSource && string.IsNullOrWhiteSpace(PathTextBox.Text))
        {
            MessageBox.Show("Please specify a file path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Asset.Type = TypeComboBox.SelectedItem as string ?? AssetTypes.VsPackage;
        Asset.Source = source;

        if (isProjectSource)
        {
            Asset.ProjectName = ProjectTextBox.Text;
            Asset.Path = $"|{ProjectTextBox.Text}|";
        }
        else
        {
            Asset.Path = PathTextBox.Text;
            Asset.ProjectName = null;
        }

        Asset.VsixSubPath = string.IsNullOrWhiteSpace(VsixSubPathTextBox.Text) ? null : VsixSubPathTextBox.Text;
        Asset.Addressable = AddressableCheckBox.IsChecked ?? false;

        DialogResult = true;
        Close();
    }
}
