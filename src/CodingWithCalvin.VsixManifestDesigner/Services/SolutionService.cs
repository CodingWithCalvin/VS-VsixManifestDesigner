using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CodingWithCalvin.VsixManifestDesigner.Models;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for querying solution projects using CPS-compatible APIs.
/// Uses IVsSolution.GetProjectEnum instead of DTE extenders.
/// </summary>
public sealed class SolutionService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SolutionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Gets all projects in the current solution.
    /// </summary>
    /// <returns>A list of project information.</returns>
    public async Task<IReadOnlyList<ProjectInfo>> GetProjectsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
        if (solution == null)
        {
            return Array.Empty<ProjectInfo>();
        }

        var projects = new List<ProjectInfo>();
        var guid = Guid.Empty;

        if (solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out var enumerator) != VSConstants.S_OK)
        {
            return projects;
        }

        var hierarchy = new IVsHierarchy[1];
        while (enumerator.Next(1, hierarchy, out var fetched) == VSConstants.S_OK && fetched == 1)
        {
            var projectInfo = await GetProjectInfoAsync(hierarchy[0]);
            if (projectInfo != null)
            {
                projects.Add(projectInfo);
            }
        }

        return projects;
    }

    /// <summary>
    /// Gets all VSIX projects in the current solution.
    /// </summary>
    /// <returns>A list of VSIX project information.</returns>
    public async Task<IReadOnlyList<ProjectInfo>> GetVsixProjectsAsync()
    {
        var allProjects = await GetProjectsAsync();
        var vsixProjects = new List<ProjectInfo>();

        foreach (var project in allProjects)
        {
            if (project.IsVsixProject)
            {
                vsixProjects.Add(project);
            }
        }

        return vsixProjects;
    }

    private async Task<ProjectInfo?> GetProjectInfoAsync(IVsHierarchy hierarchy)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        // Get the project path
        if (hierarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out var projectPath) != VSConstants.S_OK)
        {
            return null;
        }

        if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
        {
            return null;
        }

        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        var projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;

        // Check if SDK-style by looking for IVsBrowseObjectContext (CPS indicator)
        var isSdkStyle = hierarchy is IVsBrowseObjectContext;

        // Check if VSIX project by looking for .vsixmanifest file
        var isVsixProject = File.Exists(Path.Combine(projectDir, "source.extension.vsixmanifest")) ||
                           File.Exists(Path.Combine(projectDir, $"{projectName}.vsixmanifest"));

        // Check for VsixSdk usage
        var usesVsixSdk = await CheckUsesVsixSdkAsync(projectPath);

        return new ProjectInfo
        {
            Name = projectName,
            FullPath = projectPath,
            IsSdkStyle = isSdkStyle,
            UsesVsixSdk = usesVsixSdk,
            IsVsixProject = isVsixProject
        };
    }

    private async Task<bool> CheckUsesVsixSdkAsync(string projectPath)
    {
        await Task.Yield(); // Ensure async context

        try
        {
            // Simple check: read the project file and look for VsixSdk reference
            var content = File.ReadAllText(projectPath);
            return content.Contains("CodingWithCalvin.VsixSdk", StringComparison.OrdinalIgnoreCase) ||
                   content.Contains("UsingCodingWithCalvinVsixSdk", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the relative path from a base path to a target path.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The relative path.</returns>
    public static string GetRelativePath(string basePath, string targetPath)
    {
        var baseUri = new Uri(basePath);
        var targetUri = new Uri(targetPath);

        if (baseUri.Scheme != targetUri.Scheme)
        {
            return targetPath;
        }

        var relativeUri = baseUri.MakeRelativeUri(targetUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        if (targetUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        return relativePath;
    }
}
