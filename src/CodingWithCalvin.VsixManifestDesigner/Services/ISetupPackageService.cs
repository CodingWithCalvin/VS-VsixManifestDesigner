using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Represents information about a VS setup package.
/// </summary>
public class SetupPackageInfo
{
    /// <summary>
    /// Gets or sets the package ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Service for querying installed Visual Studio setup packages.
/// </summary>
public interface ISetupPackageService
{
    /// <summary>
    /// Gets the list of installed setup packages.
    /// </summary>
    /// <returns>A list of installed setup packages.</returns>
    Task<IReadOnlyList<SetupPackageInfo>> GetInstalledPackagesAsync();
}
