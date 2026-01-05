namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents an installation target in a VSIX manifest.
/// </summary>
public sealed class InstallationTarget
{
    /// <summary>
    /// Gets or sets the target identifier (e.g., Microsoft.VisualStudio.Community).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version range (e.g., [17.0,19.0)).
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product architecture (amd64, arm64).
    /// </summary>
    public string? ProductArchitecture { get; set; }
}
