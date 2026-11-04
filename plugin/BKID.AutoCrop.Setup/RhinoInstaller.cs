using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BKID.AutoCrop.Setup
{
    internal static class RhinoInstaller
    {
        internal static string Find(IEnumerable<string> programFolders)
        {
            foreach (string root in programFolders)
            {
                if (string.IsNullOrWhiteSpace(root)) continue;
                foreach (string relative in new[] {
                    "McNeel/Rhino Installer Engine/x64/rhiexec.exe",
                    "McNeel/Rhino Installer Engine/x86/rhiexec.exe",
                    "Rhino 8/System/rhiexec.exe", "Rhino 6/System/rhiexec.exe" })
                {
                    string path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(path)) return path;
                }
            }
            throw new FileNotFoundException("Rhino Installer Engine (rhiexec.exe) was not found. Repair the Rhino installation and try again. Existing plug-in files have not been removed.");
        }

        internal static ProcessStartInfo StartInfo(string engine, string package)
        {
            if (package.IndexOf('"') >= 0) throw new ArgumentException("Invalid package path.");
            // Launch the known Rhino engine, never the Windows .rhi association.
            return new ProcessStartInfo(engine, "\"" + package + "\"") { UseShellExecute = false };
        }
    }
}
