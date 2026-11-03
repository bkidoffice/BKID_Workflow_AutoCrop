using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace BKID.AutoCrop.Setup
{
    // Shared with the platform-independent transaction tests; no registry or UI access.
    internal sealed class ManagedFiles
    {
        private readonly string backup;
        private readonly List<Tuple<string, string>> moved = new List<Tuple<string, string>>();
        internal ManagedFiles(string backupFolder) { backup = backupFolder; }
        internal void Remove(string file)
        {
            if (!File.Exists(file) || moved.Any(item => string.Equals(item.Item1, file, StringComparison.OrdinalIgnoreCase))) return;
            CheckNoLinks(file);
            string target = Path.Combine(backup, "files", moved.Count.ToString("D3") + "_" + Path.GetFileName(file));
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.Copy(file, target);
            if (!Equal(File.ReadAllBytes(file), File.ReadAllBytes(target))) throw new IOException("Backup verification failed: " + file);
            File.AppendAllText(Path.Combine(backup, "files.txt"), target + " => " + file + Environment.NewLine);
            File.Delete(file);
            moved.Add(Tuple.Create(file, target));
        }
        internal void Restore()
        {
            foreach (var record in moved.AsEnumerable().Reverse())
            {
                if (File.Exists(record.Item1) && Equal(File.ReadAllBytes(record.Item1), File.ReadAllBytes(record.Item2))) continue;
                if (File.Exists(record.Item1)) throw new IOException("Cannot overwrite a new file during recovery: " + record.Item1);
                CheckNoLinks(record.Item1);
                Directory.CreateDirectory(Path.GetDirectoryName(record.Item1));
                File.Copy(record.Item2, record.Item1);
            }
        }
        private static void CheckNoLinks(string path)
        {
            while (!string.IsNullOrEmpty(path))
            {
                if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Refusing to change an installation through a link or junction: " + path);
                path = Path.GetDirectoryName(path);
            }
        }
        private static bool Equal(byte[] left, byte[] right)
        {
            using (var hash = SHA256.Create()) return hash.ComputeHash(left).SequenceEqual(hash.ComputeHash(right));
        }
    }
}
