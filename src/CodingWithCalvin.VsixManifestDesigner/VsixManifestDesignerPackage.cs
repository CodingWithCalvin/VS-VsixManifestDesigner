using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CodingWithCalvin.Otel4Vsix;
using CodingWithCalvin.VsixManifestDesigner.Editor;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodingWithCalvin.VsixManifestDesigner;

/// <summary>
/// The main package class for the VSIX Manifest Designer extension.
/// Registers the custom editor factory for .vsixmanifest files.
/// </summary>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(VsixInfo.DisplayName, VsixInfo.Description, VsixInfo.Version)]
[Guid(PackageGuids.PackageGuidString)]
[ProvideEditorFactory(typeof(VsixManifestEditorFactory), 110, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
[ProvideEditorExtension(typeof(VsixManifestEditorFactory), ".vsixmanifest", 100)]
[ProvideEditorLogicalView(typeof(VsixManifestEditorFactory), VSConstants.LOGVIEWID.Designer_string)]
[ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.EmptySolution_string, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class VsixManifestDesignerPackage : AsyncPackage
{
    /// <summary>
    /// Initializes the package asynchronously.
    /// </summary>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);

        // Switch to main thread for UI operations
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        // Initialize telemetry
        var builder = VsixTelemetry.Configure()
            .WithServiceName(VsixInfo.DisplayName)
            .WithServiceVersion(VsixInfo.Version)
            .WithVisualStudioAttributes(this)
            .WithEnvironmentAttributes();

#if !DEBUG
        builder
            .WithOtlpHttp("https://api.honeycomb.io")
            .WithHeader("x-honeycomb-team", HoneycombConfig.ApiKey);
#endif

        builder.Initialize();

        // Register the editor factory
        var editorFactory = new VsixManifestEditorFactory(this);
        RegisterEditorFactory(editorFactory);
    }

    /// <summary>
    /// Disposes of the package resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            VsixTelemetry.Shutdown();
        }

        base.Dispose(disposing);
    }
}
