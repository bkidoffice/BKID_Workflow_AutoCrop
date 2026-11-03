# Native toolbar and deletion-first installer — 2.2.0

## Build

Install .NET SDK 8 and run from the repository root:

```sh
python3 tools/build_rhi.py
```

An SDK outside PATH can be passed with `--dotnet /path/to/dotnet`. Locked dependencies compile a RhinoCommon 6.35 / .NET Framework 4.5 RHP and an x64 Windows setup EXE. The builder verifies metadata, embedded capture/logo/image bytes, RHI contents and shared managed-file deletion tests. `python3 -m unittest discover -s tests -v` additionally runs legacy installer and native RUI checks (Pillow required).

Outputs: `installers/BKID_AutoCrop_Setup_2.2.0.exe` and `installers/BKID_AutoCrop_2.2.0.rhi`. Host SDK DLLs are not distributed. The EXE is not Authenticode signed; a previously reported security-product threat detection remains undiagnosed.

## Deletion-first setup

The EXE requires Rhino to be closed and serializes setup instances with a named mutex. It discovers candidate files and validates the plugin GUID, preflights links/read-only files and current-user registry write access, then deletes only selected managed files and exact plugin registry keys. It does not copy existing files, export registry keys, create backups or roll back. Existing historical backups are not removed. The embedded RHI is extracted to a unique temporary directory and cleaned up after the installer exits.

The native Rhino Installer still completes the new installation. Exit code zero alone is insufficient: installed RHP/RUI bytes must match the package. Errors remain errors; cancellation/failure can leave the old version removed. This consequence is displayed before installation and in failure messages.

Scope is standard current-user Rhino 6/8 Plug-ins directories, the plugin's GUID registry keys, and known legacy BKID AppData files. HKLM installations are not removed. Arbitrary user sources, capture output, models and other plugins are outside the deletion plan. Shared `ManagedFiles.cs` is linked into `tools/TestSetup` to exercise real deletion/preflight code, without registry/UI mocks.

## Show the native toolbar only once

`LoadTime` remains AtStartup to make commands available. `OnLoad` checks Rhino plugin setting `NativeToolbarInitialized` (default false). Only uninitialized profiles subscribe to Idle and show the native toolbar. A successful display records true and unsubscribes. Failed initialization leaves the flag false and can retry on a later launch.

On later starts the plugin does not open or force-show the RUI. Rhino owns docking, placement and visibility persistence. `BKIDAutoCrop` explicitly opens the toolbar on demand and also marks initialization complete. The flag is in Rhino's per-plugin user settings, independent of assembly version. It is not forcibly reset on an update.

The RUI has the same basename as the RHP, a stable GUID namespace and a native `BKID Workflowboost` group. Its initial floating/visible XML state applies to initial creation; runtime docking persistence is owned by Rhino. The button invokes ScriptRunner command `BKIDAutoCropCapture`, using the embedded original IronPython capture script.

## Artwork and sources

`BKID_AutoCrop/assets/WizardImage.png` is the unchanged 2500×3334 original supplied in `Workflowboost_download.zip`. The setup panel has zero padding and its width follows the image aspect ratio at the current client height. Embedded artwork bytes are verified against the source. Toolbar artwork is the separate AutoCrop icon.

References inspected: McNeel's official [toolbar deployment guide](https://developer.rhino3d.com/guides/rhinocommon/create-deploy-plugin-toolbar/), [Installer Engine guide](https://developer.rhino3d.com/guides/general/rhino-installer-engine/), RhinoCommon lifecycle source and pinned NuGet reference assemblies/XML. RUI structure was compared with `Ennead-Architects-LLP/EA_Dist`'s actual classic RUI files; no scripts, artwork or application code were copied from that project.

Windows real-host checks still required: deletion/reinstallation, registry handling, initial toolbar appearance, docking and hiding followed by restart without forced reopening, manual reopen, capture and security-product detection diagnosis. Linux build and file checks do not validate these host behaviors.
