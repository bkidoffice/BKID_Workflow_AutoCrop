using BKID.AutoCrop.Setup;

static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
var root = Path.Combine(Path.GetTempPath(), "BKID 설치 테스트 " + Guid.NewGuid());
Directory.CreateDirectory(root);
try
{
    var backup = Path.Combine(root, "backup");
    var previous = Path.Combine(root, "plugin", "BKID.AutoCrop.rhp");
    Directory.CreateDirectory(Path.GetDirectoryName(previous)!);
    File.WriteAllBytes(previous, new byte[] { 1, 2, 3, 4 });
    var userFile = Path.Combine(root, "plugin", "사용자 메모.txt");
    File.WriteAllText(userFile, "preserve");
    var transaction = new ManagedFiles(backup);
    transaction.Remove(previous);
    Assert(!File.Exists(previous), "Old installed plugin was not removed before installing.");
    Assert(File.ReadAllBytes(Path.Combine(backup, "files", "000_BKID.AutoCrop.rhp")).SequenceEqual(new byte[] { 1, 2, 3, 4 }), "Backup differs.");
    transaction.Remove(previous); // Repeat/missing-file path is harmless.
    Assert(Directory.GetFiles(Path.Combine(backup, "files")).Length == 1, "Duplicate backup.");
    transaction.Restore(); // Simulate a cancelled or failed native installer.
    transaction.Restore(); // Recovery is repeatable.
    Assert(File.ReadAllBytes(previous).SequenceEqual(new byte[] { 1, 2, 3, 4 }), "Previous plugin not restored.");
    Assert(File.ReadAllText(userFile) == "preserve", "Unrelated user file changed.");
    Console.WriteLine("PASS: verified backup, removal, repeat/missing input, cancel recovery, repeated recovery, unrelated files and Korean paths.");

    var conflict = new ManagedFiles(Path.Combine(root, "backup-conflict"));
    conflict.Remove(previous);
    File.WriteAllText(previous, "new file from another installer");
    bool refused = false;
    try { conflict.Restore(); } catch (IOException) { refused = true; }
    Assert(refused, "Recovery should refuse to overwrite a new file.");
    Assert(File.ReadAllText(previous) == "new file from another installer", "New file was overwritten.");
    Assert(File.Exists(Path.Combine(root, "backup-conflict", "files", "000_BKID.AutoCrop.rhp")), "Recovery backup lost.");
    Console.WriteLine("PASS: conflicting new file preserved and old backup retained.");
    var linked = Path.Combine(root, "linked.rhp");
    File.CreateSymbolicLink(linked, previous);
    bool refusedLink = false;
    try { new ManagedFiles(Path.Combine(root, "backup-link")).Remove(linked); }
    catch (IOException) { refusedLink = true; }
    Assert(refusedLink && File.Exists(linked) && File.Exists(previous), "Linked installation should remain untouched.");
    Console.WriteLine("PASS: linked installation is not removed.");

}
finally { Directory.Delete(root, true); }
