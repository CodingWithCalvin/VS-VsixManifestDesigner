namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents an asset in a VSIX manifest.
/// </summary>
public sealed class Asset
{
    /// <summary>
    /// Gets or sets the asset type (e.g., Microsoft.VisualStudio.VsPackage).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source (Project or File).
    /// </summary>
    public string Source { get; set; } = "Project";

    /// <summary>
    /// Gets or sets the path or project output group reference.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name (design-time attribute).
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the full path to the referenced project file.
    /// Used for adding ProjectReference to the VSIX project.
    /// </summary>
    public string? ProjectFullPath { get; set; }

    /// <summary>
    /// Gets or sets the target path (design-time attribute).
    /// </summary>
    public string? TargetPath { get; set; }

    /// <summary>
    /// Gets or sets the VSIX sub-path.
    /// </summary>
    public string? VsixSubPath { get; set; }

    /// <summary>
    /// Gets or sets whether the asset is addressable.
    /// </summary>
    public bool Addressable { get; set; }
}

/// <summary>
/// Well-known asset types.
/// </summary>
public static class AssetTypes
{
    public const string VsPackage = "Microsoft.VisualStudio.VsPackage";
    public const string MefComponent = "Microsoft.VisualStudio.MefComponent";
    public const string Assembly = "Microsoft.VisualStudio.Assembly";
    public const string ToolboxControl = "Microsoft.VisualStudio.ToolboxControl";
    public const string ProjectTemplate = "Microsoft.VisualStudio.ProjectTemplate";
    public const string ItemTemplate = "Microsoft.VisualStudio.ItemTemplate";
    public const string Analyzer = "Microsoft.VisualStudio.Analyzer";
    public const string CodeLensComponent = "Microsoft.VisualStudio.CodeLensComponent";

    /// <summary>
    /// Gets all well-known asset types.
    /// </summary>
    public static string[] All => new[]
    {
        VsPackage,
        MefComponent,
        Assembly,
        ToolboxControl,
        ProjectTemplate,
        ItemTemplate,
        Analyzer,
        CodeLensComponent
    };

    /// <summary>
    /// Determines if the asset type is a template type.
    /// </summary>
    public static bool IsTemplate(string assetType)
    {
        return assetType == ProjectTemplate || assetType == ItemTemplate;
    }
}
