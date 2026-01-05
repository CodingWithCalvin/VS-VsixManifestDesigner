using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace CodingWithCalvin.VsixManifestDesigner.Editor;

/// <summary>
/// Editor factory GUID for the VSIX Manifest Designer.
/// </summary>
[Guid(GuidString)]
public sealed class VsixManifestEditorFactory : IVsEditorFactory, IDisposable
{
    /// <summary>
    /// The GUID string for this editor factory.
    /// </summary>
    public const string GuidString = "e5f9a3b2-4c6d-5e8f-a0b1-2c3d4e5f6a78";

    private readonly VsixManifestDesignerPackage _package;
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="VsixManifestEditorFactory"/> class.
    /// </summary>
    /// <param name="package">The package that owns this factory.</param>
    public VsixManifestEditorFactory(VsixManifestDesignerPackage package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
    }

    /// <inheritdoc/>
    public int SetSite(IOleServiceProvider psp)
    {
        _serviceProvider = new ServiceProvider(psp);
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int MapLogicalView(ref Guid rguidLogicalView, out string? pbstrPhysicalView)
    {
        pbstrPhysicalView = null;

        // Support the Designer logical view
        if (rguidLogicalView == VSConstants.LOGVIEWID.Designer_guid ||
            rguidLogicalView == VSConstants.LOGVIEWID.Primary_guid)
        {
            return VSConstants.S_OK;
        }

        return VSConstants.E_NOTIMPL;
    }

    /// <inheritdoc/>
    public int CreateEditorInstance(
        uint grfCreateDoc,
        string pszMkDocument,
        string pszPhysicalView,
        IVsHierarchy pvHier,
        uint itemid,
        IntPtr punkDocDataExisting,
        out IntPtr ppunkDocView,
        out IntPtr ppunkDocData,
        out string? pbstrEditorCaption,
        out Guid pguidCmdUI,
        out int pgrfCDW)
    {
        ppunkDocView = IntPtr.Zero;
        ppunkDocData = IntPtr.Zero;
        pbstrEditorCaption = string.Empty;
        pguidCmdUI = Guid.Empty;
        pgrfCDW = 0;

        // Validate we're not trying to create a secondary view on an existing document
        if ((grfCreateDoc & (uint)(__VSCREATEEDITORFLAGS.CEF_OPENFILE | __VSCREATEEDITORFLAGS.CEF_SILENT)) == 0)
        {
            return VSConstants.E_INVALIDARG;
        }

        // If there's existing document data, we need to verify it's compatible
        if (punkDocDataExisting != IntPtr.Zero)
        {
            return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
        }

        try
        {
            // Create the editor pane (which hosts our WPF control)
            var editorPane = new VsixManifestEditorPane(_package, pszMkDocument);

            ppunkDocView = Marshal.GetIUnknownForObject(editorPane);
            ppunkDocData = Marshal.GetIUnknownForObject(editorPane);
            pbstrEditorCaption = " [Design]";
            pguidCmdUI = new Guid(GuidString);

            return VSConstants.S_OK;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating editor instance: {ex}");
            return VSConstants.E_FAIL;
        }
    }

    /// <inheritdoc/>
    public int Close()
    {
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
    }
}
