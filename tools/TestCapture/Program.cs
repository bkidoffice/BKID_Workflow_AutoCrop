using System.Diagnostics;
using BKID.AutoCrop;

if (args.Length != 1) throw new ArgumentException("Pass the Python executable path.");
var folder = Path.Combine(Path.GetTempPath(), "BKID 배율 테스트 " + Guid.NewGuid());
Directory.CreateDirectory(folder);
try
{
    string capture = Path.Combine(folder, "BKID_AutoCrop.py");
    string source = "import os\nCAPTURE_SCALE = 1\ndef run(capture_scale=None):\n    with open(os.path.join(os.path.dirname(__file__), 'result.txt'), 'w') as f:\n        f.write(str(capture_scale) + '|' + str(CAPTURE_SCALE))\n";
    File.WriteAllText(capture, source);
    foreach (int scale in new[] { 2, 3, 4 })
    {
        string driver = Path.Combine(folder, "capture.py");
        File.WriteAllText(driver, CaptureRunner.Build(scale));
        var start = new ProcessStartInfo(args[0]) { UseShellExecute = false, RedirectStandardError = true };
        start.ArgumentList.Add(driver);
        using (var process = Process.Start(start)!)
        {
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new Exception(error);
        }
        if (File.ReadAllText(Path.Combine(folder, "result.txt")) != scale + "|1")
            throw new Exception("Scale was not passed per call or changed the module default.");
        if (File.ReadAllText(capture) != source) throw new Exception("Capture source was modified.");
    }
    foreach (int invalid in new[] { -1, 0, 1, 5 })
    {
        bool rejected = false;
        try { CaptureRunner.Build(invalid); } catch (ArgumentOutOfRangeException) { rejected = true; }
        if (!rejected) throw new Exception("Invalid high-resolution factor accepted.");
    }
    Console.WriteLine("PASS: actual generated runners pass x2/x3/x4 to Python, preserve defaults/source, support Korean paths and reject invalid factors.");
}
finally { Directory.Delete(folder, true); }
