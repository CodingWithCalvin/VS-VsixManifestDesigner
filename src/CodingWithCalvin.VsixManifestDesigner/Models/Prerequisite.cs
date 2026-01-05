namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents a prerequisite in a VSIX manifest.
/// </summary>
public sealed class Prerequisite
{
    /// <summary>
    /// Gets or sets the prerequisite identifier (VS component ID).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version range.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
