namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents information about a project in the solution.
/// </summary>
public sealed class ProjectInfo
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path to the project file.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the project uses SDK-style format.
    /// </summary>
    public bool IsSdkStyle { get; set; }

    /// <summary>
    /// Gets or sets whether the project uses CodingWithCalvin.VsixSdk.
    /// </summary>
    public bool UsesVsixSdk { get; set; }

    /// <summary>
    /// Gets or sets whether the project is a VSIX project.
    /// </summary>
    public bool IsVsixProject { get; set; }

    /// <summary>
    /// Gets or sets the relative path from the manifest to this project.
    /// </summary>
    public string? RelativePath { get; set; }
}
