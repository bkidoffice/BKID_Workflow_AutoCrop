using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Commands;
using Command = Rhino.Commands.Command;
using Rhino.PlugIns;
using Rhino.UI;

namespace BKID.AutoCrop
{
    [Guid("3881C10B-30A8-4FE5-BB48-9E53A6FEA1F4")]
    public sealed class AutoCropPlugin : PlugIn
    {
        private bool waitingForStartup;
        private int startupAttempts;
        public static AutoCropPlugin Instance { get; private set; }

        public AutoCropPlugin() { Instance = this; }
        public override PlugInLoadTime LoadTime { get { return PlugInLoadTime.AtStartup; } }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            waitingForStartup = true;
            RhinoApp.Idle += OnStartupIdle;
            return LoadReturnCode.Success;
        }

        private void OnStartupIdle(object sender, EventArgs e)
        {
            // AtStartup can run before Rhino has a document and an owner window.
            if (RhinoDoc.ActiveDoc == null || RhinoEtoApp.MainWindow == null) return;
            startupAttempts++;
            try
            {
                if (startupAttempts == 1) RemoveLegacyRegistration();
                ShowToolbar();
                StopWaitingForStartup();
            }
            catch (Exception error)
            {
                // Rhino 8 may finish importing a sidecar on a later UI idle cycle.
                if (startupAttempts >= 5)
                {
                    StopWaitingForStartup();
                    RhinoApp.WriteLine("BKID AutoCrop toolbar startup: " + error);
                    RhinoApp.WriteLine("Run BKIDAutoCrop to retry opening the toolbar.");
                }
            }
        }

        private void StopWaitingForStartup()
        {
            if (!waitingForStartup) return;
            RhinoApp.Idle -= OnStartupIdle;
            waitingForStartup = false;
        }

        private void RemoveLegacyRegistration()
        {
            foreach (var alias in new[] { "RunAutoCrop", "ShowAutoCrop" })
            {
                var macro = Rhino.ApplicationSettings.CommandAliasList.GetMacro(alias);
                if (macro != null && (macro.IndexOf("BKID_AutoCrop.py", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      macro.IndexOf("Show_AutoCrop.py", StringComparison.OrdinalIgnoreCase) >= 0))
                    Rhino.ApplicationSettings.CommandAliasList.Delete(alias);
            }
            var major = RhinoApp.Version.Major.ToString();
            var legacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                      "BKID", "AutoCrop", "Rhino" + major, "BKID_AutoCrop.rui");
            var old = RhinoApp.ToolbarFiles.FindByPath(legacy);
            if (old != null) old.Close(false);
        }

        internal void ShowToolbar()
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BKID.AutoCrop.rui");
            if (!File.Exists(path)) throw new FileNotFoundException("Reinstall BKID AutoCrop: toolbar sidecar missing.", path);
            var toolbar = RhinoApp.ToolbarFiles.FindByPath(path) ?? RhinoApp.ToolbarFiles.Open(path);
            if (toolbar == null) throw new InvalidOperationException("Rhino could not open toolbar: " + path);
            var group = toolbar.GetGroup("BKID AutoCrop");
            if (group == null)
            {
                var names = new System.Collections.Generic.List<string>();
                for (int i = 0; i < toolbar.GroupCount; i++) names.Add(toolbar.GetGroup(i).Name);
                throw new InvalidOperationException("BKID AutoCrop toolbar group unavailable; groups: " + string.Join(", ", names));
            }
            group.Visible = true;
            if (!group.Visible) throw new InvalidOperationException("Rhino did not display the AutoCrop toolbar group.");
        }

        internal string PrepareCaptureScript()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                      "BKID", "AutoCrop", "runtime", "2.1.0");
            Directory.CreateDirectory(folder);
            var target = Path.Combine(folder, "BKID_AutoCrop.py");
            using (var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("BKID.AutoCrop.Capture.py"))
            {
                if (input == null) throw new InvalidOperationException("Missing embedded capture script.");
                using (var output = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.Read))
                    input.CopyTo(output);
            }
            return target;
        }

        protected override void OnShutdown()
        {
            StopWaitingForStartup();
            base.OnShutdown();
        }
    }

    [Guid("E63167B6-42F8-4B81-86B3-31565BCB239E")]
    public sealed class AutoCropToolbarCommand : Command
    {
        public override string EnglishName { get { return "BKIDAutoCrop"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try { AutoCropPlugin.Instance.ShowToolbar(); return Result.Success; }
            catch (Exception error) { RhinoApp.WriteLine("BKID AutoCrop: " + error); return Result.Failure; }
        }
    }

    [Guid("BB6FF93A-344E-4027-BAA4-A8AC780418C7")]
    [CommandStyle(Style.ScriptRunner)]
    public sealed class AutoCropCaptureCommand : Command
    {
        public override string EnglishName { get { return "BKIDAutoCropCapture"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (doc == null) return Result.Failure;
            try
            {
                var path = AutoCropPlugin.Instance.PrepareCaptureScript();
                return RhinoApp.RunScript("_-RunPythonScript \"" + path + "\"", false)
                    ? Result.Success : Result.Failure;
            }
            catch (Exception error) { RhinoApp.WriteLine("BKID AutoCrop: " + error); return Result.Failure; }
        }
    }
}
