using System.Threading.Tasks;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for querying MSBuild properties and targets.
/// </summary>
public interface IMsBuildQueryService
{
    /// <summary>
    /// Gets the value of an MSBuild property from a project file.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The property value, or null if not found.</returns>
    Task<string?> GetPropertyValueAsync(string projectPath, string propertyName);

    /// <summary>
    /// Checks if a project has a specific MSBuild target defined.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <param name="targetName">The name of the target to check for.</param>
    /// <returns>True if the target exists, false otherwise.</returns>
    Task<bool> HasTargetAsync(string projectPath, string targetName);
}
