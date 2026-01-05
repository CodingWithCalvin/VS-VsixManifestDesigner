using System.Collections.Generic;
using System.Threading.Tasks;
using CodingWithCalvin.VsixManifestDesigner.Models;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for querying solution projects using CPS-compatible APIs.
/// </summary>
public interface ISolutionService
{
    /// <summary>
    /// Gets all projects in the current solution.
    /// </summary>
    /// <returns>A list of project information.</returns>
    Task<IReadOnlyList<ProjectInfo>> GetProjectsAsync();

    /// <summary>
    /// Gets all VSIX projects in the current solution.
    /// </summary>
    /// <returns>A list of VSIX project information.</returns>
    Task<IReadOnlyList<ProjectInfo>> GetVsixProjectsAsync();
}
