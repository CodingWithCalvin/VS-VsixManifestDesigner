using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodingWithCalvin.VsixManifestDesigner.Services;

/// <summary>
/// Service for querying installed Visual Studio setup packages.
/// </summary>
public sealed class SetupPackageService : ISetupPackageService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupPackageService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SetupPackageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Gets the list of installed setup packages.
    /// </summary>
    /// <returns>A list of installed setup packages.</returns>
    public async Task<IReadOnlyList<SetupPackageInfo>> GetInstalledPackagesAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var packages = new List<SetupPackageInfo>();

        try
        {
            var setupService = _serviceProvider.GetService(typeof(SVsSetupCompositionService)) as IVsSetupCompositionService;
            if (setupService != null)
            {
                // First call with null to get the count
                setupService.GetSetupPackagesInfo(0, null, out var count);

                if (count > 0)
                {
                    var installedPackages = new IVsSetupPackageInfo[count];
                    setupService.GetSetupPackagesInfo(count, installedPackages, out _);

                    foreach (var package in installedPackages)
                    {
                        if (package != null)
                        {
                            packages.Add(new SetupPackageInfo
                            {
                                Id = package.PackageId ?? string.Empty,
                                Title = package.Title ?? string.Empty,
                                Version = package.Version ?? string.Empty
                            });
                        }
                    }
                }
            }
        }
        catch
        {
            // If VS service is unavailable, return empty list
        }

        // Sort by title for easier browsing
        packages.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));

        return packages;
    }
}
