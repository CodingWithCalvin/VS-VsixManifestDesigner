using System.Threading.Tasks;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for integrating manifest changes with the VSIX project file.
/// Handles adding/removing ProjectReferences and file items.
/// </summary>
public interface IProjectIntegrationService
{
    /// <summary>
    /// Adds a ProjectReference to the VSIX project with appropriate metadata.
    /// </summary>
    /// <param name="vsixProjectPath">Path to the VSIX project file (.csproj).</param>
    /// <param name="referencedProjectPath">Path to the project being referenced.</param>
    /// <param name="assetType">The asset type being added (affects output groups).</param>
    /// <returns>True if the reference was added; false if it already existed.</returns>
    Task<bool> AddProjectReferenceAsync(string vsixProjectPath, string referencedProjectPath, string? assetType = null);

    /// <summary>
    /// Removes a ProjectReference from the VSIX project.
    /// </summary>
    /// <param name="vsixProjectPath">Path to the VSIX project file (.csproj).</param>
    /// <param name="referencedProjectPath">Path to the project being dereferenced.</param>
    /// <returns>True if the reference was removed; false if it didn't exist.</returns>
    Task<bool> RemoveProjectReferenceAsync(string vsixProjectPath, string referencedProjectPath);

    /// <summary>
    /// Checks if a ProjectReference exists in the VSIX project.
    /// </summary>
    /// <param name="vsixProjectPath">Path to the VSIX project file (.csproj).</param>
    /// <param name="referencedProjectPath">Path to the project to check.</param>
    /// <returns>True if the reference exists.</returns>
    Task<bool> HasProjectReferenceAsync(string vsixProjectPath, string referencedProjectPath);

    /// <summary>
    /// Adds a file to the VSIX project with Include in VSIX enabled.
    /// </summary>
    /// <param name="vsixProjectPath">Path to the VSIX project file (.csproj).</param>
    /// <param name="sourceFilePath">Path to the source file to add.</param>
    /// <param name="includeInVsix">Whether to set IncludeInVSIX=true.</param>
    /// <returns>The project-relative path of the added file.</returns>
    Task<string?> AddFileToProjectAsync(string vsixProjectPath, string sourceFilePath, bool includeInVsix = true);

    /// <summary>
    /// Removes a file from the VSIX project.
    /// </summary>
    /// <param name="vsixProjectPath">Path to the VSIX project file (.csproj).</param>
    /// <param name="projectRelativePath">The project-relative path of the file to remove.</param>
    /// <param name="deleteFromDisk">Whether to also delete the file from disk.</param>
    /// <returns>True if the file was removed.</returns>
    Task<bool> RemoveFileFromProjectAsync(string vsixProjectPath, string projectRelativePath, bool deleteFromDisk = false);

    /// <summary>
    /// Gets the VSIX project path from a manifest file path.
    /// </summary>
    /// <param name="manifestFilePath">Path to the .vsixmanifest file.</param>
    /// <returns>Path to the containing .csproj file, or null if not found.</returns>
    string? GetVsixProjectPath(string manifestFilePath);
}
