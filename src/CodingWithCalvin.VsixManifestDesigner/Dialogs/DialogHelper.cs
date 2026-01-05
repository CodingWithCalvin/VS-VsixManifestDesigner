using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodingWithCalvin.VsixManifestDesigner.Dialogs;

/// <summary>
/// Helper class for dialog operations.
/// </summary>
internal static class DialogHelper
{
    /// <summary>
    /// Sets the owner of a WPF window to the Visual Studio main window.
    /// </summary>
    /// <param name="window">The window to set the owner for.</param>
    public static void SetOwner(Window window)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
        if (uiShell != null && uiShell.GetDialogOwnerHwnd(out var hwnd) == 0)
        {
            var helper = new WindowInteropHelper(window)
            {
                Owner = hwnd
            };
        }
    }

    /// <summary>
    /// Shows a dialog with proper VS ownership.
    /// </summary>
    /// <param name="window">The window to show.</param>
    /// <returns>The dialog result.</returns>
    public static bool? ShowDialogWithOwner(Window window)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        SetOwner(window);
        return window.ShowDialog();
    }
}
