namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents a content entry in a VSIX manifest.
/// Used for project templates and item templates.
/// </summary>
public sealed class Content
{
    /// <summary>
    /// Gets or sets the content type (ProjectTemplates or ItemTemplates).
    /// </summary>
    public string Type { get; set; } = ContentTypes.ProjectTemplates;

    /// <summary>
    /// Gets or sets the path to the template(s).
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Well-known content types.
/// </summary>
public static class ContentTypes
{
    public const string ProjectTemplates = "ProjectTemplates";
    public const string ItemTemplates = "ItemTemplates";

    /// <summary>
    /// Gets all well-known content types.
    /// </summary>
    public static string[] All => new[]
    {
        ProjectTemplates,
        ItemTemplates
    };
}
