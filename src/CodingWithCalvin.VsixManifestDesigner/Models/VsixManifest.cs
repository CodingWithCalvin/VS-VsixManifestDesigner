using System.Collections.ObjectModel;

namespace CodingWithCalvin.VsixManifestDesigner.Models;

/// <summary>
/// Represents a complete VSIX manifest (source.extension.vsixmanifest).
/// </summary>
public sealed class VsixManifest
{
    /// <summary>
    /// Gets or sets the manifest schema version.
    /// </summary>
    public string Version { get; set; } = "2.0.0";

    /// <summary>
    /// Gets or sets the metadata section.
    /// </summary>
    public ManifestMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the installation targets.
    /// </summary>
    public ObservableCollection<InstallationTarget> InstallationTargets { get; set; } = new();

    /// <summary>
    /// Gets or sets the dependencies.
    /// </summary>
    public ObservableCollection<Dependency> Dependencies { get; set; } = new();

    /// <summary>
    /// Gets or sets the prerequisites.
    /// </summary>
    public ObservableCollection<Prerequisite> Prerequisites { get; set; } = new();

    /// <summary>
    /// Gets or sets the assets.
    /// </summary>
    public ObservableCollection<Asset> Assets { get; set; } = new();

    /// <summary>
    /// Gets or sets the content declarations (for templates).
    /// </summary>
    public ObservableCollection<Content> Contents { get; set; } = new();
}

/// <summary>
/// Represents the metadata section of a VSIX manifest.
/// </summary>
public sealed class ManifestMetadata
{
    /// <summary>
    /// Gets or sets the extension identity.
    /// </summary>
    public ExtensionIdentity Identity { get; set; } = new();

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the more info URL.
    /// </summary>
    public string? MoreInfo { get; set; }

    /// <summary>
    /// Gets or sets the license file path or SPDX expression.
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// Gets or sets the getting started guide URL.
    /// </summary>
    public string? GettingStartedGuide { get; set; }

    /// <summary>
    /// Gets or sets the release notes URL or file path.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Gets or sets the icon path.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the preview image path.
    /// </summary>
    public string? PreviewImage { get; set; }

    /// <summary>
    /// Gets or sets the tags (comma-separated).
    /// </summary>
    public string? Tags { get; set; }
}

/// <summary>
/// Represents the extension identity.
/// </summary>
public sealed class ExtensionIdentity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Gets or sets the publisher name.
    /// </summary>
    public string Publisher { get; set; } = string.Empty;
}
