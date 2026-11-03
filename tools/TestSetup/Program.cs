using BKID.AutoCrop.Setup;

static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
var root = Path.Combine(Path.GetTempPath(), "BKID 삭제 설치 테스트 " + Guid.NewGuid());
Directory.CreateDirectory(root);
try
{
    var previous = Path.Combine(root, "BKID.AutoCrop.rhp");
    var userFile = Path.Combine(root, "사용자 메모.txt");
    File.WriteAllBytes(previous, new byte[] { 1, 2, 3, 4 });
    File.WriteAllText(userFile, "preserve");
    var managed = new ManagedFiles();
    managed.Validate(new[] { previous });
    managed.Remove(previous);
    managed.Remove(previous); // Repeat/missing-file deletion is harmless.
    Assert(!File.Exists(previous), "Previous plugin was not deleted.");
    Assert(File.ReadAllText(userFile) == "preserve", "Unrelated file changed.");
    Assert(Directory.GetFiles(root).Length == 1 && Directory.GetDirectories(root).Length == 0, "Unexpected backup created.");
    Console.WriteLine("PASS: deletion without backup, repeat/missing input, Korean paths and unrelated file preservation.");

    File.WriteAllBytes(previous, new byte[] { 5, 6 });
    var linked = Path.Combine(root, "linked.rhp");
    File.CreateSymbolicLink(linked, previous);
    bool refused = false;
    try { managed.Validate(new[] { previous, linked }); }
    catch (IOException) { refused = true; }
    Assert(refused && File.Exists(previous) && File.Exists(linked), "Preflight should reject a link before any file removal.");
    Console.WriteLine("PASS: linked installation rejected before deletion.");

    File.SetAttributes(previous, FileAttributes.ReadOnly);
    refused = false;
    try { managed.Validate(new[] { previous }); }
    catch (IOException) { refused = true; }
    Assert(refused && File.Exists(previous), "Read-only file should fail preflight without deletion.");
    File.SetAttributes(previous, FileAttributes.Normal);
    Console.WriteLine("PASS: read-only installation rejected before deletion.");
}
finally { Directory.Delete(root, true); }
