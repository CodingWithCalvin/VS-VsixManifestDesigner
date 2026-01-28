using System.IO;
using System.Windows;
using System.Windows.Controls;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using Microsoft.Win32;

namespace CodingWithCalvin.VsixManifestDesigner.Views;

/// <summary>
/// View for editing manifest metadata.
/// </summary>
public partial class MetadataView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataView"/> class.
    /// </summary>
    public MetadataView()
    {
        InitializeComponent();
    }

    private ManifestViewModel? ViewModel => DataContext as ManifestViewModel;

    private string? GetManifestDirectory()
    {
        var filePath = ViewModel?.ManifestFilePath;
        return string.IsNullOrEmpty(filePath) ? null : Path.GetDirectoryName(filePath);
    }

    private string? BrowseForFile(string title, string filter, string? currentValue)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true
        };

        var manifestDir = GetManifestDirectory();
        if (!string.IsNullOrEmpty(manifestDir))
        {
            dialog.InitialDirectory = manifestDir;
        }

        if (!string.IsNullOrEmpty(currentValue) && File.Exists(currentValue))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(currentValue);
            dialog.FileName = Path.GetFileName(currentValue);
        }

        if (dialog.ShowDialog() == true)
        {
            var selectedPath = dialog.FileName;

            // Try to make the path relative to the manifest directory
            if (!string.IsNullOrEmpty(manifestDir) && selectedPath.StartsWith(manifestDir, System.StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = selectedPath.Substring(manifestDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return relativePath;
            }

            return selectedPath;
        }

        return null;
    }

    private void BrowseIcon_Click(object sender, RoutedEventArgs e)
    {
        var result = BrowseForFile(
            "Select Icon",
            "Image Files (*.ico;*.png)|*.ico;*.png|Icon Files (*.ico)|*.ico|PNG Files (*.png)|*.png|All Files (*.*)|*.*",
            ViewModel?.Icon);

        if (result != null && ViewModel != null)
        {
            ViewModel.Icon = result;
        }
    }

    private void BrowsePreviewImage_Click(object sender, RoutedEventArgs e)
    {
        var result = BrowseForFile(
            "Select Preview Image",
            "Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*",
            ViewModel?.PreviewImage);

        if (result != null && ViewModel != null)
        {
            ViewModel.PreviewImage = result;
        }
    }

    private void BrowseLicense_Click(object sender, RoutedEventArgs e)
    {
        var result = BrowseForFile(
            "Select License File",
            "All Files (*.*)|*.*|Text Files (*.txt;*.md;*.rtf)|*.txt;*.md;*.rtf|License Files (LICENSE*)|LICENSE*",
            ViewModel?.License);

        if (result != null && ViewModel != null)
        {
            ViewModel.License = result;
        }
    }

    private void BrowseGettingStarted_Click(object sender, RoutedEventArgs e)
    {
        var result = BrowseForFile(
            "Select Getting Started Guide",
            "HTML Files (*.htm;*.html)|*.htm;*.html|Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            ViewModel?.GettingStartedGuide);

        if (result != null && ViewModel != null)
        {
            ViewModel.GettingStartedGuide = result;
        }
    }

    private void BrowseReleaseNotes_Click(object sender, RoutedEventArgs e)
    {
        var result = BrowseForFile(
            "Select Release Notes",
            "HTML Files (*.htm;*.html)|*.htm;*.html|Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            ViewModel?.ReleaseNotes);

        if (result != null && ViewModel != null)
        {
            ViewModel.ReleaseNotes = result;
        }
    }
}
