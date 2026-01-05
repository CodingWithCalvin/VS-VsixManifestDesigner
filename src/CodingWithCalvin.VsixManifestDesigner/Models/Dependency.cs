namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents a dependency in a VSIX manifest.
/// </summary>
public sealed class Dependency
{
    /// <summary>
    /// Gets or sets the dependency identifier.
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

    /// <summary>
    /// Gets or sets the source type (Manual, Installed, File, Project).
    /// </summary>
    public string Source { get; set; } = "Manual";

    /// <summary>
    /// Gets or sets the location (URL or relative path).
    /// </summary>
    public string? Location { get; set; }
}
