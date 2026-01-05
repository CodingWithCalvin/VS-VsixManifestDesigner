using CodingWithCalvin.VsixManifestDesigner.Models;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for loading and saving VSIX manifest files.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Loads a VSIX manifest from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the manifest file.</param>
    /// <returns>The parsed manifest.</returns>
    VsixManifest Load(string filePath);

    /// <summary>
    /// Saves a VSIX manifest to the specified file path.
    /// </summary>
    /// <param name="manifest">The manifest to save.</param>
    /// <param name="filePath">The path to save to.</param>
    void Save(VsixManifest manifest, string filePath);
}
