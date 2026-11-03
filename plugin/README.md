# Native Rhino host, version 2.0.0

The native C# host provides Windows installation, a persistent startup preference, a modeless Eto window and two Rhino commands. The existing IronPython capture algorithm is embedded unchanged. The minimum SDK target is RhinoCommon 6.35 / .NET Framework 4.5; intended hosts are Windows Rhino 6 and 8. Host compatibility still needs real Windows validation.

## Build

Install .NET SDK 8. From the repository root:

```sh
python3 tools/build_rhi.py
```

For an SDK outside PATH:

```sh
python3 tools/build_rhi.py --dotnet /path/to/dotnet
```

The build restores dependencies in locked mode, compiles the RHP, verifies its assembly metadata/resources and creates `installers/BKID_AutoCrop_2.0.0.rhi`. Commit the RHI when changing distributed plugin code. It contains only the compiled RHP; scripts and logos are embedded resources. RhinoCommon, Rhino.UI and Eto are supplied by the Rhino host and are not copied into the installer.

`tools/VerifyPlugin` uses .NET PE metadata, so verification does not require or pretend to run the Windows Rhino host. It checks the real installer GUID, assembly version, host references, and exact capture/logo resource bytes. The package builder checks the archive and its RHP contents.

The original logo is in `BKID_AutoCrop/assets/BKID_AutoCrop_Icon.svg`. The native window uses `icon_64.png`, rendered from that SVG with CairoSVG 2.8.2. To regenerate it:

```sh
python3 -m pip install CairoSVG==2.8.2
python3 -c "import cairosvg; cairosvg.svg2png(url='BKID_AutoCrop/assets/BKID_AutoCrop_Icon.svg', write_to='BKID_AutoCrop/assets/icon_64.png', output_width=64, output_height=64)"
```

Legacy installer tests remain available via `python3 -m unittest discover -s tests -v` (Pillow required). These do not validate the native plugin's startup lifecycle.

## Lifecycle

- `AutoCropPlugin.LoadTime` returns `PlugInLoadTime.AtStartup`.
- `OnLoad` subscribes once to `RhinoApp.Idle` when the saved startup preference is true (default).
- The idle callback waits for a model document and Rhino owner window, unsubscribes, then opens one Eto Form. Errors are printed in command history.
- `BKIDAutoCrop` opens or brings forward the same window. Closing disposes the logo stream/image and clears the reference.
- The checkbox uses Rhino's plugin `Settings` for persistence. `OnShutdown` removes the idle callback and closes the window.
- `BKIDAutoCropCapture` is marked `CommandStyle(Style.ScriptRunner)` so it can invoke `_RunPythonScript`. It extracts the embedded script to `%LOCALAPPDATA%/BKID/AutoCrop/runtime/2.0.0` and runs it with Rhino's IronPython engine. Capture errors are reported by the original script in F2; a successful script command dispatch is not a guarantee of saved PNG output.

The startup preference defaults to enabled without modifying global Rhino startup macros, user aliases or other plugin settings.

## Sources inspected

- McNeel's [Windows plugin installer guide](https://developer.rhino3d.com/guides/rhinocommon/plugin-installers-windows/) (official Markdown source): an RHI is a ZIP containing compiled RHP files; the installer inspects assemblies, not folder names. The installer engine is a legacy distribution route; McNeel recommends Package Manager/Yak for future managed distribution. This task uses RHI for a local double-click installation without publishing a package feed.
- [RhinoCommon plugin lifecycle source](https://github.com/mcneel/rhino3dm/blob/main/src/dotnet/rhino/rhinosdkplugin.cs): assembly-level GUID/title discovery, `PlugInLoadTime`, `OnLoad`, `OnShutdown`.
- McNeel `rhino-developer-samples`: `rhinocommon/cs/SampleCsEto/Commands/SampleCsEtoModelessFormCommand.cs` and `rhinopython/SampleEtoModelessForm.py`: Rhino-owned modeless forms, single-window lifetime, button-triggered commands. MIT sample patterns were consulted; no third-party sample files are distributed.
- The pinned RhinoCommon 6.35 NuGet XML and actual reference assemblies were used to check and compile the invoked APIs.

## Real-host checks still required

Use Windows Rhino 6 and 8 separately. Verify RHI installation and plugin recognition, automatic window after restart, logo rendering, click-to-capture, closing/reopening, checkbox persistence across restart, and Korean user paths. This Linux environment cannot execute Rhino or its Installer Engine. A successful build and package check does not prove those host behaviors.
