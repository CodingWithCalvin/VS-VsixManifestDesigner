using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using CodingWithCalvin.VsixManifestDesigner.Services;
using CodingWithCalvin.VsixManifestDesigner.ViewModels;
using CodingWithCalvin.VsixManifestDesigner.Views;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CodingWithCalvin.VsixManifestDesigner.Editor;

/// <summary>
/// Editor pane that hosts the WPF VSIX manifest designer control.
/// Implements the document data interfaces for VS integration.
/// </summary>
[ComVisible(true)]
[Guid("f6a7b8c9-d0e1-2f3a-4b5c-6d7e8f9a0b1c")]
public sealed class VsixManifestEditorPane : WindowPane,
    IVsPersistDocData,
    IPersistFileFormat,
    IVsFileChangeEvents
{
    private const uint FileFormat = 0;
    private const string FileExtension = ".vsixmanifest";

    private readonly VsixManifestDesignerPackage _package;
    private readonly ManifestService _manifestService;
    private readonly ManifestDesignerControl _control;
    private readonly ManifestViewModel _viewModel;

    private string _filePath;
    private bool _isDirty;
    private bool _isLoading;
    private IVsFileChangeEx? _fileChangeService;
    private uint _fileChangeCookie;

    /// <summary>
    /// Initializes a new instance of the <see cref="VsixManifestEditorPane"/> class.
    /// </summary>
    /// <param name="package">The package that owns this pane.</param>
    /// <param name="filePath">The path to the manifest file.</param>
    public VsixManifestEditorPane(VsixManifestDesignerPackage package, string filePath)
        : base(package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

        _manifestService = new ManifestService();
        _viewModel = new ManifestViewModel(_manifestService);
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        _control = new ManifestDesignerControl { DataContext = _viewModel };
    }

    /// <summary>
    /// Gets the WPF content for this pane.
    /// </summary>
    public override object Content => _control;

    #region IVsPersistDocData

    /// <inheritdoc/>
    public int GetGuidEditorType(out Guid pClassID)
    {
        pClassID = new Guid(VsixManifestEditorFactory.GuidString);
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int IsDocDataDirty(out int pfDirty)
    {
        pfDirty = _isDirty ? 1 : 0;
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int SetUntitledDocPath(string pszDocDataPath)
    {
        return VSConstants.E_NOTIMPL;
    }

    /// <inheritdoc/>
    public int LoadDocData(string pszMkDocument)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return LoadFile(pszMkDocument);
    }

    /// <inheritdoc/>
    public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
    {
        pbstrMkDocumentNew = _filePath;
        pfSaveCanceled = 0;

        try
        {
            _manifestService.Save(_viewModel.Manifest!, _filePath);
            _isDirty = false;
            return VSConstants.S_OK;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving document: {ex}");
            return VSConstants.E_FAIL;
        }
    }

    /// <inheritdoc/>
    public int Close()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        UnadviseFileChange();
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
    {
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
    {
        _filePath = pszMkDocumentNew;
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int IsDocDataReloadable(out int pfReloadable)
    {
        pfReloadable = 1;
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int ReloadDocData(uint grfFlags)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return LoadFile(_filePath);
    }

    #endregion

    #region IPersistFileFormat

    /// <inheritdoc/>
    public int GetClassID(out Guid pClassID)
    {
        pClassID = new Guid(VsixManifestEditorFactory.GuidString);
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int IsDirty(out int pfIsDirty)
    {
        pfIsDirty = _isDirty ? 1 : 0;
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int InitNew(uint nFormatIndex)
    {
        return VSConstants.E_NOTIMPL;
    }

    /// <inheritdoc/>
    public int Load(string pszFilename, uint grfMode, int fReadOnly)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return LoadFile(pszFilename);
    }

    /// <inheritdoc/>
    public int Save(string pszFilename, int fRemember, uint nFormatIndex)
    {
        try
        {
            var targetPath = string.IsNullOrEmpty(pszFilename) ? _filePath : pszFilename;
            _manifestService.Save(_viewModel.Manifest!, targetPath);

            if (fRemember != 0)
            {
                _filePath = targetPath;
                _isDirty = false;
            }

            return VSConstants.S_OK;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving file: {ex}");
            return VSConstants.E_FAIL;
        }
    }

    /// <inheritdoc/>
    public int SaveCompleted(string pszFilename)
    {
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int GetCurFile(out string ppszFilename, out uint pnFormatIndex)
    {
        ppszFilename = _filePath;
        pnFormatIndex = FileFormat;
        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int GetFormatList(out string ppszFormatList)
    {
        ppszFormatList = $"VSIX Manifest (*{FileExtension})\n*{FileExtension}\n";
        return VSConstants.S_OK;
    }

    #endregion

    #region IVsFileChangeEvents

    /// <inheritdoc/>
    public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
    {
        if (_isLoading)
        {
            return VSConstants.S_OK;
        }

        for (var i = 0; i < cChanges; i++)
        {
            if (string.Equals(rgpszFile[i], _filePath, StringComparison.OrdinalIgnoreCase))
            {
                // File changed externally, reload (fire and forget)
#pragma warning disable VSSDK007 // Intentionally fire-and-forget for external file change notification
                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ReloadDocData(0);
                });
#pragma warning restore VSSDK007
                break;
            }
        }

        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int DirectoryChanged(string pszDirectory)
    {
        return VSConstants.S_OK;
    }

    #endregion

    private int LoadFile(string filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            _isLoading = true;
            _filePath = filePath;

            UnadviseFileChange();

            var manifest = _manifestService.Load(filePath);
            _viewModel.LoadManifest(manifest);
            _isDirty = false;

            AdviseFileChange();

            return VSConstants.S_OK;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading file: {ex}");
            return VSConstants.E_FAIL;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Only mark dirty for actual content changes, not UI state changes
        if (!_isLoading &&
            e.PropertyName != nameof(ManifestViewModel.Manifest) &&
            e.PropertyName != nameof(ManifestViewModel.SelectedTabIndex))
        {
            _isDirty = true;
        }
    }

    private void AdviseFileChange()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_fileChangeService == null)
        {
            _fileChangeService = _package.GetService<SVsFileChangeEx, IVsFileChangeEx>();
        }

        _fileChangeService?.AdviseFileChange(
            _filePath,
            (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size),
            this,
            out _fileChangeCookie);
    }

    private void UnadviseFileChange()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_fileChangeService != null && _fileChangeCookie != 0)
        {
            _fileChangeService.UnadviseFileChange(_fileChangeCookie);
            _fileChangeCookie = 0;
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                UnadviseFileChange();
            });
        }

        base.Dispose(disposing);
    }
}
