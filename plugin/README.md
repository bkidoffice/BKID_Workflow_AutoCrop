# Native toolbar and replacement installer, 2.1.0

2.1 removes the modeless Eto capture window. The RHP now opens a native Rhino RUI toolbar containing one logo button. A Windows WinForms setup EXE backs up/removes known prior user installations, then invokes the standard Rhino Installer for the embedded RHI. The setup GUI is only an installation interface; it is not opened by Rhino.

## Build and validate

Install .NET SDK 8, then from the repository root:

```sh
python3 tools/build_rhi.py
```

For an SDK outside PATH, pass `--dotnet /path/to/dotnet`. Dependencies are pinned by each `packages.lock.json` and restored in locked mode. No Windows runtime is claimed by the Linux build.

The builder generates native RUI, compiles the RHP against RhinoCommon 6.35 / .NET Framework 4.5, validates assembly identity and embedded resources, packages RHP + matching-name RUI, compiles the .NET Framework Windows setup EXE with embedded RHI and logo, verifies its embedded package, and executes the shared file transaction tests on .NET 8. Host SDK binaries are not redistributed.

Outputs committed for download:

- `installers/BKID_AutoCrop_Setup_2.1.0.exe` — recommended removal/reinstallation workflow.
- `installers/BKID_AutoCrop_2.1.0.rhi` — underlying standard package, without pre-removal logic.

Run `python3 -m unittest discover -s tests -v` (Pillow required) for legacy installer tests and native RUI/package checks. `tools/TestSetup` links the actual `ManagedFiles.cs` used by setup, so backup/removal/rollback checks run on the real file transaction code rather than a mock.

## Toolbar lifecycle

`LoadTime` is AtStartup. The idle callback waits for a document/main owner window, removes known legacy aliases/old toolbar registration, and shows the native group. Up to five UI idle cycles allow Rhino 8 to complete sidecar import. There is no separate capture Form or substitute logo window. `BKIDAutoCrop` retries showing the same group. The toolbar calls `BKIDAutoCropCapture`, a ScriptRunner command using the embedded original IronPython capture engine.

The RUI has a fresh stable file/group GUID namespace to avoid cached legacy definitions. Its `bitmap_id` is an attribute on the macro/toolbar; the group item and active toolbar identifiers resolve to the toolbar, and floating/docking state is explicit. It ships beside an identically named RHP, following Rhino's official automatic toolbar loading contract. Rhino owns placement persistence; real-host testing is required.

## Reinstall transaction

The EXE is x64 (Windows Rhino 6/8 are x64). It requires Rhino to be closed, serializes concurrent setup processes via a named mutex, discovers all candidate managed RHP files before changing them, validates the assembly GUID, and backs up exact current-user registry keys. HKLM installations are not removed automatically. Reparse points are not traversed for discovery and file operations reject links/junctions.

Only known managed plugin/script files are removed, after verifying byte-equivalent backups. Other files and capture output are preserved. The EXE then opens the native RHI installer, waits and checks installed RHP/RUI bytes; cancelling or incomplete installation triggers file and registry recovery. A conflicting newly created file is never overwritten during recovery; backups remain available. External user source paths are not recursively deleted.

The cleanup covers standard per-user Rhino 6/8 Plug-ins folders and previous BKID AppData paths. It does not promise to remove arbitrary manually copied source trees, custom package-manager installs or machine-wide installations. The default native installer route must use current-user scope. The EXE is not Authenticode signed; Windows distribution policy and the real installer workflow need Windows validation.

## Sources inspected

- [Official toolbar deployment guide](https://developer.rhino3d.com/guides/rhinocommon/create-deploy-plugin-toolbar/): an RUI with exactly the RHP basename, in the same directory, is automatically opened on first plugin load.
- [Official Rhino Installer Engine guide](https://developer.rhino3d.com/guides/general/rhino-installer-engine/): RHI copies/registers the newest compatible plugin identified by assembly GUID/version/SDK, but does not execute custom cleanup. That is why pre-removal belongs in the separate setup EXE. RHI is a legacy distribution format; future PackageManager distribution would use Yak.
- RhinoCommon plugin lifecycle source and pinned NuGet reference assemblies/XML: assembly GUID discovery, AtStartup, Idle, ToolbarFiles, group Visible, command aliases, ScriptRunner.
- Structure comparison only: `Ennead-Architects-LLP/EA_Dist`, `Apps/_rhino/EnneadTab_For_Rhino_Classic.rui` and `Installation/EnneadTab_For_Rhino_Installer.rui`. Actual files confirmed RUI 3 attributes, bitmap mapping order and docking/group state. No source, scripts or artwork from that project are included; no redistribution license was established for its application files.

## Remaining real-host checks

Windows Rhino 6 and 8: replace an existing installation through EXE, native installer cancel/rollback, registry export/import, actual native group and logo rendering, button capture, repeated install without duplicates, toolbar docking/placement after restart. Builds, metadata checks and filesystem tests passed on Linux; these Windows/Rhino checks remain unexecuted.

## Wizard artwork

`BKID_AutoCrop/assets/WizardImage.png` is the unmodified PNG provided in `Workflowboost_download.zip` (2500 × 3334). Setup embeds it and displays it edge to edge in the left artwork panel. The panel has zero padding and its width follows the source aspect ratio at the current client height; StretchImage fills the remaining subpixel rounding gap without cropping the artwork. The metadata verifier compares embedded artwork bytes against this source. The toolbar icon remains the separate AutoCrop icon. The actual Windows wizard appearance remains untested on this Linux machine.
