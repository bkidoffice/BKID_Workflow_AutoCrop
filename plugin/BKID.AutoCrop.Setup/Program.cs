using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BKID.AutoCrop.Setup
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool first;
            using (var mutex = new System.Threading.Mutex(true, @"Local\BKID.AutoCrop.Setup", out first))
            {
                if (!first)
                {
                    MessageBox.Show("BKID 설치 프로그램이 이미 실행 중입니다.");
                    return;
                }
                try { Application.Run(new SetupWindow()); }
                finally { mutex.ReleaseMutex(); }
            }
        }
    }

    internal sealed class SetupWindow : Form
    {
        private readonly Button install = new Button { Text = "설치 / 다시 설치", Dock = DockStyle.Bottom, Height = 42 };
        private readonly Label status = new Label { Dock = DockStyle.Fill, AutoSize = false };
        private readonly Image logo;
        private bool busy;
        public SetupWindow()
        {
            Text = "BKID Workflowboost 2.1 설치";
            ClientSize = new Size(760, 420);
            Padding = new Padding(0);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BKID.Setup.Logo.png"))
            using (var decoded = new Bitmap(stream))
                logo = new Bitmap(decoded);
            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
            var title = new Label
            {
                Text = "BKID Workflowboost 설치",
                Dock = DockStyle.Top,
                Height = 54,
                Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 16, FontStyle.Bold)
            };
            content.Controls.Add(status);
            content.Controls.Add(title);
            content.Controls.Add(install);
            var artwork = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(6, 8, 14)
            };
            artwork.Controls.Add(new PictureBox
            {
                Image = logo,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            });
            Controls.Add(content);
            Controls.Add(artwork);
            status.Text = "Rhino를 종료한 뒤 설치하세요.\r\n기존 BKID 설치를 백업·제거하고 새 툴바 버전을 설치합니다.\r\nRhino Installer에서는 현재 사용자 설치를 선택하세요.";
            install.Click += InstallClicked;
            FormClosing += (sender, args) => { if (busy) args.Cancel = true; };
            FormClosed += (sender, args) => logo.Dispose();
        }

        private async void InstallClicked(object sender, EventArgs args)
        {
            busy = true;
            install.Enabled = false;
            status.Text = "기존 설치를 확인하고 있습니다…\r\nRhino Installer가 열리면 설치를 진행해 주세요.";
            try
            {
                string backup = await Task.Run(() => new Reinstall().Run());
                status.Text = "설치 완료. Rhino를 실행하면 BKID Workflowboost 툴바가 표시됩니다.\r\n이전 설치 백업:\r\n" + backup;
                install.Text = "설치 완료";
            }
            catch (Exception error)
            {
                status.Text = "설치하지 못했습니다. 아래 오류를 확인하세요.";
                MessageBox.Show(this, error.Message, "BKID AutoCrop 설치", MessageBoxButtons.OK, MessageBoxIcon.Error);
                install.Enabled = true;
            }
            finally { busy = false; }
        }
    }

    internal sealed class Reinstall
    {
        private const string PluginId = "3881C10B-30A8-4FE5-BB48-9E53A6FEA1F4";
        private readonly string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private ManagedFiles managed;
        private readonly List<Tuple<string, string>> registry = new List<Tuple<string, string>>();
        private string backup;

        internal string Run()
        {
            if (Process.GetProcessesByName("Rhino").Length != 0)
                throw new InvalidOperationException("열려 있는 Rhino를 모두 종료한 뒤 다시 설치하세요. 작업은 아직 변경하지 않았습니다.");
            backup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BKID", "AutoCrop", "InstallBackups",
                                  DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(backup);
            managed = new ManagedFiles(backup);
            string package = Path.Combine(backup, "BKID_AutoCrop_2.1.0.rhi");
            using (var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("BKID.Setup.Package.rhi"))
            using (var output = File.Create(package)) input.CopyTo(output);
            byte[] expectedPlugin;
            byte[] expectedToolbar;
            using (var stream = File.OpenRead(package))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                expectedPlugin = Read(zip.GetEntry("BKID.AutoCrop.rhp"));
                expectedToolbar = Read(zip.GetEntry("BKID.AutoCrop.rui"));
            }
            // Discover and validate all candidates before removing any installation.
            var oldPlugins = FindInstalledPlugins().ToList();
            foreach (var version in new[] { "6.0", "8.0" })
            foreach (var id in new[] { PluginId, "{" + PluginId + "}" })
            {
                string keyPath = @"Software\McNeel\Rhinoceros\" + version + @"\Plug-ins\" + id;
                using (var machine = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    var machineFile = machine == null ? null : machine.GetValue("FileName") as string;
                    if (!string.IsNullOrEmpty(machineFile) && File.Exists(machineFile))
                        throw new InvalidOperationException("모든 사용자용 BKID 설치가 있습니다. Rhino에서 해당 설치를 제거한 뒤 현재 사용자용으로 다시 설치하세요.");
                }
                using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
                {
                    if (key == null) continue;
                    string export = Path.Combine(backup, "registry_" + registry.Count + ".reg");
                    RunReg("export \"HKCU\\" + keyPath + "\" \"" + export + "\" /y");
                    registry.Add(Tuple.Create(keyPath, export));
                }
            }
            try
            {
                foreach (var file in oldPlugins)
                {
                    managed.Remove(file);
                    managed.Remove(Path.ChangeExtension(file, ".rui"));
                }
                foreach (var version in new[] { "6", "8" })
                foreach (var name in new[] { "BKID_AutoCrop.py", "Show_AutoCrop.py", "BKID_AutoCrop.rui", "assets/BKID_AutoCrop_Icon.svg", "assets/icon_32.png" })
                    managed.Remove(Path.Combine(roaming, "BKID", "AutoCrop", "Rhino" + version, name.Replace('/', Path.DirectorySeparatorChar)));
                managed.Remove(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BKID", "AutoCrop", "runtime", "2.0.0", "BKID_AutoCrop.py"));
                foreach (var record in registry) Registry.CurrentUser.DeleteSubKeyTree(record.Item1, false);
                var start = new ProcessStartInfo(package) { UseShellExecute = true };
                using (var installer = Process.Start(start))
                {
                    if (installer != null) installer.WaitForExit();
                    if (installer != null && installer.ExitCode != 0) throw new InvalidOperationException("Rhino Installer가 설치를 완료하지 않았습니다 (" + installer.ExitCode + ").");
                }
                // Do not treat a zero installer exit code or an old file as success.
                bool complete = false;
                for (int attempt = 0; attempt < 80 && !complete; attempt++)
                {
                    complete = FindInstalledPlugins().Any(file => Equal(File.ReadAllBytes(file), expectedPlugin) &&
                        File.Exists(Path.ChangeExtension(file, ".rui")) && Equal(File.ReadAllBytes(Path.ChangeExtension(file, ".rui")), expectedToolbar));
                    if (!complete) System.Threading.Thread.Sleep(250);
                }
                if (!complete) throw new InvalidOperationException("새 플러그인과 툴바 설치를 확인하지 못했습니다. 설치 취소 또는 다른 설치 범위인지 확인하세요.");
                File.WriteAllText(Path.Combine(backup, "result.txt"), "Installed BKID AutoCrop 2.1.0. Previous managed files are retained in this backup.");
                return backup;
            }
            catch (Exception original)
            {
                try
                {
                    managed.Restore();
                    foreach (var record in registry) RunReg("import \"" + record.Item2 + "\"");
                }
                catch (Exception restore)
                {
                    throw new InvalidOperationException(original.Message + "\r\n자동 복구를 완료하지 못했습니다: " + restore.Message + "\r\n백업: " + backup, original);
                }
                throw new InvalidOperationException(original.Message + "\r\n이전 설치 파일과 등록 정보를 복원했습니다.\r\n백업: " + backup, original);
            }
        }

        private IEnumerable<string> FindInstalledPlugins()
        {
            foreach (var version in new[] { "6.0", "8.0" })
            {
                string root = Path.Combine(roaming, "McNeel", "Rhinoceros", version, "Plug-ins");
                if (!Directory.Exists(root)) continue;
                foreach (var file in EnumeratePluginFiles(root))
                {
                    var assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(file));
                    var guid = assembly.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType.FullName == typeof(System.Runtime.InteropServices.GuidAttribute).FullName);
                    if (guid != null && string.Equals((string)guid.ConstructorArguments[0].Value, PluginId, StringComparison.OrdinalIgnoreCase)) yield return file;
                }
            }
        }

        private static IEnumerable<string> EnumeratePluginFiles(string root)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                string directory = pending.Pop();
                if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0) continue;
                foreach (var file in Directory.EnumerateFiles(directory, "BKID.AutoCrop.rhp"))
                    if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0) yield return file;
                foreach (var child in Directory.EnumerateDirectories(directory)) pending.Push(child);
            }
        }

        private static byte[] Read(ZipArchiveEntry entry)
        {
            if (entry == null) throw new InvalidDataException("설치 패키지 파일이 누락되었습니다.");
            using (var stream = entry.Open())
            using (var memory = new MemoryStream()) { stream.CopyTo(memory); return memory.ToArray(); }
        }
        private static bool Equal(byte[] left, byte[] right)
        {
            using (var hash = SHA256.Create()) return hash.ComputeHash(left).SequenceEqual(hash.ComputeHash(right));
        }
        private static void RunReg(string arguments)
        {
            using (var process = Process.Start(new ProcessStartInfo("reg.exe", arguments) { UseShellExecute = false, CreateNoWindow = true }))
            {
                process.WaitForExit();
                if (process.ExitCode != 0) throw new IOException("플러그인 등록 정보 백업/복원 실패.");
            }
        }
    }
}
