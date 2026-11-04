using System.Xml;
using System.Xml.Linq;
using BKID.AutoCrop;
static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
string template = File.ReadAllText(args[0]);
var old = XDocument.Parse(template);
var macros = old.Root!.Element("macros")!;
var capture = macros.Elements("macro_item").Single(m => (string)m.Element("script") == "! _BKIDAutoCropCapture");
var resolution = macros.Elements("macro_item").Single(m => (string)m.Element("script") == "! _BKIDAutoCropResolution");
string rightId = (string)resolution.Attribute("guid")!;
string newLeftId = (string)capture.Attribute("guid")!;
capture.SetAttributeValue("guid", "654e9b41-09dc-51b6-8de2-4ee942d03864");
resolution.Remove();
var button = old.Descendants("tool_bar_item").First();
button.SetElementValue("left_macro_id", (string)capture.Attribute("guid")!);
button.SetElementValue("right_macro_id", (string)capture.Attribute("guid")!); // Actual pre-2.3 binding.
capture.Element("tooltip")!.SetElementValue("locale_1033", "좌클릭: 바로 저장 / 우클릭: ×2·×3·×4 해상도 선택");
var dock = old.Descendants("dock_bar_info").First();
dock.SetAttributeValue("floating", "False");
dock.SetAttributeValue("visible", "False");
dock.SetAttributeValue("dock_location", "bottom");
var originalDock = new XElement(dock);
var other = new XElement("tool_bar_item", new XAttribute("guid", Guid.NewGuid()), new XElement("left_macro_id", "unrelated"), new XElement("right_macro_id", "keep-this"));
button.Parent!.Add(other);
string originalOther = other.ToString();
string repaired = ToolbarBindings.Patch(old.ToString(), template);
Assert(ToolbarBindings.ContainsCaptureButton(old.ToString(), template), "Old collection was not recognized.");
Assert(ToolbarBindings.ContainsCaptureButton(template, template), "Correct RUI must still be eligible for UI-cache reload.");
Assert(!ToolbarBindings.ContainsCaptureButton("<RhinoUI><macros/><tool_bars/></RhinoUI>", template), "Unrelated collection must not be reloaded.");
Assert(repaired != null, "Old binding was not repaired.");
var result = XDocument.Parse(repaired!);
var resultButton = result.Descendants("tool_bar_item").First();
Assert((string)resultButton.Element("right_macro_id") == rightId, "Right-click not changed.");
Assert((string)resultButton.Element("left_macro_id") == newLeftId, "Left hover macro was not refreshed.");
Assert((string)resultButton.Element("left_macro_id") != (string)button.Element("left_macro_id"), "Old hover identity was reused.");
Assert((string)result.Descendants("macro_item").Single(m => (string)m.Attribute("guid") == newLeftId).Element("script") == "! _BKIDAutoCropCapture", "Left capture behavior changed.");
Assert(XNode.DeepEquals(result.Descendants("dock_bar_info").First(), originalDock), "Docking/visibility XML changed.");
Assert(result.Descendants("tool_bar_item").Last().ToString() == originalOther, "Unrelated button changed.");
Assert(ToolbarBindings.Patch(repaired!, template) == null, "Repair should be idempotent.");
Console.WriteLine("PASS: real old right-click binding repaired, left-click/docking/visibility/unrelated button preserved, repeat repair skipped.");

// Copying a native button into Default.rui can assign it new macro/button IDs.
var copied = XDocument.Parse(old.ToString());
var copiedCapture = copied.Descendants("macro_item").First();
string copiedId = Guid.NewGuid().ToString();
copiedCapture.SetAttributeValue("guid", copiedId);
var copiedButton = copied.Descendants("tool_bar_item").First();
copiedButton.SetAttributeValue("guid", Guid.NewGuid());
copiedButton.SetElementValue("left_macro_id", copiedId);
copiedButton.SetElementValue("right_macro_id", copiedId);
Assert(ToolbarBindings.Patch(copied.ToString(), template) != null, "Copied button not recognized by capture command.");
Assert(ToolbarBindings.Patch("<RhinoUI><macros/><tool_bars/></RhinoUI>", template) == null, "Unrelated collection changed.");
var external = new XDocument(new XElement("RhinoUI", new XElement("tool_bars", new XElement("tool_bar",
    new XElement("tool_bar_item", new XElement("left_macro_id", (string)capture.Attribute("guid")!),
                 new XElement("right_macro_id", (string)capture.Attribute("guid")!))))));
Assert(ToolbarBindings.Patch(external.ToString(), template) != null, "Cross-collection capture reference without local macros was not repaired.");
// Existing correct bindings must also receive the newly separated descriptions.
var shared = XDocument.Parse(template);
foreach (var macro in shared.Root!.Element("macros")!.Elements("macro_item"))
{
    macro.Element("tooltip")!.SetElementValue("locale_1033", "old shared description");
    macro.Element("help_text")!.SetElementValue("locale_1033", "old shared description");
}
string updatedDescriptions = ToolbarBindings.Patch(shared.ToString(), template);
Assert(updatedDescriptions != null, "Correct bindings with old descriptions were not refreshed.");
var described = XDocument.Parse(updatedDescriptions!);
var describedMacros = described.Root!.Element("macros")!.Elements("macro_item").ToList();
Assert(describedMacros[0].Element("tooltip")!.Value.Contains("1배"), "Left description not updated.");
Assert(describedMacros[1].Element("tooltip")!.Value.Contains("×2"), "Right description not updated.");
Assert(describedMacros[0].Element("tooltip")!.Value != describedMacros[1].Element("tooltip")!.Value, "Descriptions are still identical.");
Assert(ToolbarBindings.Patch(updatedDescriptions!, template) == null, "Description migration should be idempotent.");
Console.WriteLine("PASS: existing macros with correct bindings get distinct left/right descriptions without requiring rebinding.");
bool dtdRejected = false;
try { ToolbarBindings.Patch("<!DOCTYPE RhinoUI [<!ENTITY x SYSTEM 'file:///never-read'>]><RhinoUI/>", template); }
catch (XmlException) { dtdRejected = true; }
Assert(dtdRejected, "DTD must not be read.");
Console.WriteLine("PASS: copied native button recognized, unrelated collection skipped and external-entity XML rejected.");


// The real display policy is exercised against a host that counts opens/shows.
var fileId = ToolbarBindings.CollectionId(template);
var groupId = ToolbarBindings.GroupId(template);
var autoGroup = new FakeGroup(groupId, true, false);
var autoCollection = new FakeCollection(fileId, autoGroup);
var autoHost = new FakeHost(autoCollection);
ToolbarDisplay.Show(autoHost, "different-installed-path.rui", fileId, groupId);
ToolbarDisplay.Show(autoHost, "different-installed-path.rui", fileId, groupId);
Assert(autoHost.Opens == 0 && autoGroup.Shows == 0, "Auto-loaded toolbar must be reused without reopening or re-showing.");

FakeHost.FileId = fileId;
FakeHost.GroupId = groupId;
var fresh = new FakeHost();
ToolbarDisplay.Show(fresh, "new.rui", fileId, groupId);
ToolbarDisplay.Show(fresh, "new.rui", fileId, groupId);
Assert(fresh.Opens == 1 && fresh.Created!.Shows == 0, "First import must occur once and already-visible group must not be re-shown.");

var hidden = new FakeGroup(groupId, false, true);
ToolbarDisplay.Show(new FakeHost(new FakeCollection(fileId, hidden)), "x.rui", fileId, groupId);
Assert(hidden.Shows == 1 && hidden.IsDocked, "Hidden toolbar should be shown once without losing docking.");

var docked = new FakeCollection(fileId, new FakeGroup(groupId, true, true));
var duplicate = new FakeCollection(fileId, new FakeGroup(groupId, true, false));
ToolbarDisplay.Show(new FakeHost(duplicate, docked), "x.rui", fileId, groupId);
Assert(duplicate.Closed && !docked.Closed, "Existing duplicate must be closed while the docked collection is retained.");
var foreign = new FakeCollection(Guid.NewGuid(), new FakeGroup(groupId, true, false));
ToolbarDisplay.Show(new FakeHost(docked, foreign), "x.rui", fileId, groupId);
Assert(!foreign.Closed && !foreign.Groups[0].Visible, "Only the duplicate owned group should be hidden in a shared collection.");
var remainHidden = new FakeGroup(groupId, false, true);
var hiddenHost = new FakeHost(new FakeCollection(fileId, remainHidden));
ToolbarDisplay.Show(hiddenHost, "x.rui", fileId, groupId, false);
Assert(!remainHidden.Visible && remainHidden.Shows == 0 && hiddenHost.Opens == 0, "Startup cleanup must preserve hidden state.");
var absentHost = new FakeHost();
ToolbarDisplay.Show(absentHost, "x.rui", fileId, groupId, false);
Assert(absentHost.Opens == 0, "Startup cleanup must not import a deliberately closed collection.");

var importing = new FakeHost(new FakeCollection(fileId));
bool deferred = false;
try { ToolbarDisplay.Show(importing, "x.rui", fileId, groupId); }
catch (InvalidOperationException) { deferred = true; }
Assert(deferred && importing.Opens == 0, "An unfinished auto-import must not trigger another open.");
var gate = new StartupGate();
Assert(gate.TryEnter() && !gate.TryEnter(), "Nested UI idle must be rejected.");
gate.Exit();
Assert(gate.TryEnter(), "Startup retry should be possible after leaving.");
gate.Exit();
var nestedHost = new FakeHost();
var nestedGate = new StartupGate();
Action initialize = null!;
initialize = () =>
{
    if (!nestedGate.TryEnter()) return;
    try { ToolbarDisplay.Show(nestedHost, "x.rui", fileId, groupId); }
    finally { nestedGate.Exit(); }
};
nestedHost.BeforeOpen = initialize; // Simulates Open pumping another Rhino idle event.
initialize();
Assert(nestedHost.Opens == 1, "Nested import must not create a second toolbar.");
Console.WriteLine("PASS: single import/show, auto-loaded identity reuse, hidden/docked behavior, scoped duplicate cleanup and nested-idle protection.");

sealed class FakeGroup : IToolbarGroupView
{
    private bool visible;
    public Guid Id { get; }
    public bool IsDocked { get; }
    public int Shows { get; private set; }
    public bool Visible { get => visible; set { if (value) Shows++; visible = value; } }
    public FakeGroup(Guid id, bool shown, bool docked) { Id = id; visible = shown; IsDocked = docked; }
}
sealed class FakeCollection : IToolbarCollectionView
{
    public Guid Id { get; }
    public IList<IToolbarGroupView> Groups { get; }
    public bool IsDedicated => Groups.Count == 1;
    public bool Closed { get; private set; }
    public FakeCollection(Guid id, params IToolbarGroupView[] groups) { Id = id; Groups = groups.ToList(); }
    public void SaveAndClose() { Closed = true; }
}
sealed class FakeHost : IToolbarDisplayHost
{
    public IList<IToolbarCollectionView> Loaded { get; }
    public int Opens { get; private set; }
    public FakeGroup Created { get; private set; }
    public Action BeforeOpen { get; set; }
    public FakeHost(params IToolbarCollectionView[] loaded) { Loaded = loaded.ToList(); }
    public IToolbarCollectionView Open(string path)
    {
        Opens++;
        BeforeOpen?.Invoke();
        // Opening the native sidecar auto-shows its group.
        Created = new FakeGroup(GroupId, true, false);
        var file = new FakeCollection(FileId, Created);
        Loaded.Add(file);
        return file;
    }
    public static Guid FileId;
    public static Guid GroupId;
}
