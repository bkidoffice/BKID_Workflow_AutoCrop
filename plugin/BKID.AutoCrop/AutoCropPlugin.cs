using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
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
        internal const string StartupKey = "ShowOnStartup";
        private Form window;
        private bool waitingForStartup;
        public static AutoCropPlugin Instance { get; private set; }

        public AutoCropPlugin() { Instance = this; }
        public override PlugInLoadTime LoadTime { get { return PlugInLoadTime.AtStartup; } }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            if (Settings.GetBool(StartupKey, true))
            {
                waitingForStartup = true;
                RhinoApp.Idle += OnStartupIdle;
            }
            return LoadReturnCode.Success;
        }

        private void OnStartupIdle(object sender, EventArgs e)
        {
            // AtStartup can run before Rhino has a document and an owner window.
            if (RhinoDoc.ActiveDoc == null || RhinoEtoApp.MainWindow == null) return;
            StopWaitingForStartup();
            try { ShowWindow(); }
            catch (Exception error) { RhinoApp.WriteLine("BKID AutoCrop startup: " + error); }
        }

        private void StopWaitingForStartup()
        {
            if (!waitingForStartup) return;
            RhinoApp.Idle -= OnStartupIdle;
            waitingForStartup = false;
        }

        internal bool ShowOnStartup
        {
            get { return Settings.GetBool(StartupKey, true); }
            set { Settings.SetBool(StartupKey, value); }
        }

        internal void ShowWindow()
        {
            if (window != null)
            {
                window.BringToFront();
                return;
            }
            Stream imageStream = null;
            Bitmap image = null;
            Form created = null;
            try
            {
                imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BKID.AutoCrop.Icon.png");
                if (imageStream == null) throw new InvalidOperationException("Missing embedded AutoCrop logo.");
                image = new Bitmap(imageStream);
                created = new Form
                {
                    Title = "BKID AutoCrop",
                    Owner = RhinoEtoApp.MainWindow,
                    Resizable = false,
                    Maximizable = false,
                    Minimizable = false,
                    ShowInTaskbar = false,
                    ClientSize = new Size(270, 210)
                };
                var logo = new ImageView { Image = image, Size = new Size(64, 64) };
                var caption = new Label { Text = "현재 뷰를 투명 PNG로 저장합니다.", TextAlignment = TextAlignment.Center };
                var capture = new Button { Text = "PNG 저장", Height = 36 };
                capture.Click += (sender, args) =>
                {
                    if (!RhinoApp.RunScript("! _BKIDAutoCropCapture", false))
                        RhinoApp.WriteLine("BKID AutoCrop: capture command did not complete. See F2.");
                };
                var autoStart = new CheckBox { Text = "Rhino 시작 시 창 열기", Checked = ShowOnStartup };
                autoStart.CheckedChanged += (sender, args) => { ShowOnStartup = autoStart.Checked == true; };
                var layout = new DynamicLayout { Padding = new Padding(16), Spacing = new Size(8, 8) };
                layout.AddRow(null, logo, null);
                layout.AddRow(caption);
                layout.AddRow(capture);
                layout.AddRow(autoStart);
                created.Content = layout;
                var ownedImage = image;
                var ownedStream = imageStream;
                created.Closed += (sender, args) =>
                {
                    if (ReferenceEquals(window, sender)) window = null;
                    ownedImage.Dispose();
                    ownedStream.Dispose();
                    ((Form)sender).Dispose();
                };
                window = created;
                created.Show();
            }
            catch
            {
                window = null;
                if (created != null) created.Dispose();
                if (image != null) image.Dispose();
                if (imageStream != null) imageStream.Dispose();
                throw;
            }
        }

        internal string PrepareCaptureScript()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                      "BKID", "AutoCrop", "runtime", "2.0.0");
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
            if (window != null)
            {
                var closing = window;
                closing.Close();
                closing.Dispose();
                window = null;
            }
            base.OnShutdown();
        }
    }

    [Guid("E63167B6-42F8-4B81-86B3-31565BCB239E")]
    public sealed class AutoCropWindowCommand : Command
    {
        public override string EnglishName { get { return "BKIDAutoCrop"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try { AutoCropPlugin.Instance.ShowWindow(); return Result.Success; }
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
