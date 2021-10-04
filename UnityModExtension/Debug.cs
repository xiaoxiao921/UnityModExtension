using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace UnityModExtension
{
    public static class Debug
    {
        internal static void Print(string message)
        {
            VsShellUtilities.ShowMessageBox(UnityModExtensionPackage.Instance,
                        "[UnityModExtension]" + message,
                        nameof(Debug),
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
