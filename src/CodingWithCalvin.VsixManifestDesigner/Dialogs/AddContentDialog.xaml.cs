using System.Windows;
using System.Windows.Forms;
using CodingWithCalvin.VsixManifestDesigner.Models;
using Microsoft.VisualStudio.PlatformUI;
using ContentModel = CodingWithCalvin.VsixManifestDesigner.Models.Content;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Dialog for adding or editing content entries.
/// </summary>
public partial class AddContentDialog : DialogWindow
{
    /// <summary>
    /// Gets the content being edited.
    /// </summary>
    public ContentModel ContentItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddContentDialog"/> class for adding.
    /// </summary>
    public AddContentDialog() : this(new ContentModel())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddContentDialog"/> class for editing.
    /// </summary>
    /// <param name="content">The content to edit.</param>
    public AddContentDialog(ContentModel content)
    {
        ContentItem = new ContentModel
        {
            Type = content.Type,
            Path = content.Path
        };

        InitializeComponent();

        TypeComboBox.ItemsSource = ContentTypes.All;
        TypeComboBox.SelectedItem = ContentItem.Type;
        PathTextBox.Text = ContentItem.Path;

        DataContext = ContentItem;
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Template Folder",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ContentItem.Path = dialog.SelectedPath;
            PathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ContentItem.Path))
        {
            System.Windows.MessageBox.Show("Please specify a path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }
}
