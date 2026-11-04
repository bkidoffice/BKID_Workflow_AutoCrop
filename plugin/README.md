# Native toolbar and deletion-first installer — 2.5.4

## Build

Install .NET SDK 8 and run from the repository root:

```sh
python3 tools/build_rhi.py
```

An SDK outside PATH can be passed with `--dotnet /path/to/dotnet`. Locked dependencies compile a RhinoCommon 6.0.18016.23451 / .NET Framework 4.5 RHP and an x64 Windows setup EXE. The builder verifies metadata, embedded capture/logo/image bytes, RHI contents and shared managed-file deletion tests. `python3 -m unittest discover -s tests -v` additionally runs legacy installer and native RUI checks (Pillow required).

Outputs: `installers/BKID_AutoCrop_Setup_2.5.4.exe` and `installers/BKID_AutoCrop_2.5.4.rhi`. Host SDK DLLs are not distributed. The EXE is not Authenticode signed; a previously reported security-product threat detection remains undiagnosed.

## Deletion-first setup

The EXE requires Rhino to be closed and serializes setup instances with a named mutex. It discovers candidate files and validates the plugin GUID, preflights links/read-only files and current-user registry write access, then deletes only selected managed files and exact plugin registry keys. It does not copy existing files, export registry keys, create backups or roll back. Existing historical backups are not removed. The embedded RHI is extracted to a unique temporary directory and cleaned up after the installer exits.

The native Rhino Installer still completes the new installation. Neither zero nor a nonzero installer exit code alone determines success. The installed new RHP must match the package hash; the RUI must retain the expected collection ID and required command definitions. Runtime docking/schema/formatting edits are accepted. Missing or old files remain failures; cancellation/failure can leave the old version removed. This consequence is displayed before installation and in failure messages.

Scope is standard current-user Rhino 6/8 Plug-ins directories, the plugin's GUID registry keys, and known legacy BKID AppData files. HKLM installations are not removed. Arbitrary user sources, capture output, models and other plugins are outside the deletion plan. Shared `ManagedFiles.cs` is linked into `tools/TestSetup` to exercise real deletion/preflight code, without registry/UI mocks.

## Show the native toolbar only once

`LoadTime` remains AtStartup to make commands available. `OnLoad` checks Rhino plugin setting `NativeToolbarInitialized` (default false). Only uninitialized profiles subscribe to Idle and show the native toolbar. A successful display records true and unsubscribes. Failed initialization leaves the flag false and can retry on a later launch.

On later starts the plugin does not open or force-show the RUI. Rhino owns docking, placement and visibility persistence. `BKIDAutoCrop` explicitly opens the toolbar on demand and also marks initialization complete. The flag is in Rhino's per-plugin user settings, independent of assembly version. It is not forcibly reset on an update.

The RUI has the same basename as the RHP, a stable GUID namespace and a native `BKID Workflowboost` group. Its initial floating/visible XML state applies to initial creation; runtime docking persistence is owned by Rhino. The button invokes ScriptRunner command `BKIDAutoCropCapture`, using the embedded original IronPython capture script.

## Artwork and sources

`BKID_AutoCrop/assets/WizardImage.png` is the unchanged 2500×3334 original supplied in `Workflowboost_download.zip`. The setup panel has zero padding and its width follows the image aspect ratio at the current client height. Embedded artwork bytes are verified against the source. Toolbar artwork is the separate AutoCrop icon.

References inspected: McNeel's official [toolbar deployment guide](https://developer.rhino3d.com/guides/rhinocommon/create-deploy-plugin-toolbar/), [Installer Engine guide](https://developer.rhino3d.com/guides/general/rhino-installer-engine/), RhinoCommon lifecycle source and pinned NuGet reference assemblies/XML. RUI structure was compared with `Ennead-Architects-LLP/EA_Dist`'s actual classic RUI files; no scripts, artwork or application code were copied from that project.

Windows real-host checks still required: deletion/reinstallation, registry handling, initial toolbar appearance, docking and hiding followed by restart without forced reopening, manual reopen, capture and security-product detection diagnosis. Linux build and file checks do not validate these host behaviors.

## Capture scale selection (2.3)

The native button's left macro still invokes `BKIDAutoCropCapture`. The right macro now invokes `BKIDAutoCropResolution`, a separate ScriptRunner command. Its Eto `Dialog<int>` has x2/x3/x4 buttons; choosing one closes the modal before capture. AbortButton, Esc and window close return cancellation. Nothing is extracted or captured on cancellation. Eto Dialog.Close(T), ShowModal(Control) and AbortButton were checked in the pinned Rhino 6 SDK's Eto XML and compiled reference assemblies.

The original capture command uses the default 1x script. High-resolution capture writes a small relative-path runner built by `CaptureRunner.cs`, loads the capture module with standard-library runpy and calls `run(capture_scale=N)`. It does not rewrite CAPTURE_SCALE or persist the chosen scale. The capture engine passes the per-call factor to ViewCapture.Width/Height before rendering. PNG crop/alpha verification and 1px margins remain unchanged.

`tools/TestCapture` links the real C# runner generator and executes its outputs under Python at Korean/space-containing paths, checking x2/x3/x4 argument delivery, unchanged source/defaults and invalid factor rejection. Python capture tests exercise scaled ViewCapture settings under a mock Rhino host, default 1x after high-resolution calls, finite-scale validation, fixed margins and selection restoration on errors. These do not execute IronPython, System.Drawing, actual Rhino rendering or the Eto dialog. All require Windows Rhino 6/8 host validation separately.

## 2.4.1: confirmation and saved-toolbar migration

The setup uses `InstallWorkflow.RunAsync`: detection is read-only, the Yes/No question resumes on the WinForms UI context, and the destructive install action is called only after Yes when an existing installation is present. No returns without calling the install action. A fresh install skips the question, but the backend rechecks before deletion and rejects a newly discovered existing installation unless replacement was approved. The question defaults to No. Shared workflow tests exercise real file preservation/replacement and the no-consent guard.

The reported symptom—right-click immediately performing the old capture—indicates a stale saved button binding rather than a displayed scale dialog. Existing toolbar collections can retain old left/right IDs even when the installed RUI has changed. On the first UI-idle pass for the saved binding revision (currently `rhino6-tooltips-2`), the plugin identifies loaded collections containing our capture command, saves the current layout, patches only the AutoCrop right macro, and reopens the collection to refresh cached controls. It also recognizes copied buttons with new IDs by their exact capture command. Group docking XML, visibility, left click and unrelated buttons are preserved; collections with no AutoCrop button are not saved/reopened. After successful migration the revision is persisted, so this reload is not repeated on every launch. A correct on-disk RUI still gets a one-time reload if it contains our button, because in-memory controls can be stale.

`BKIDAutoCropRefresh` manually repeats this scoped refresh without intentionally floating/showing the toolbar. No new standalone capture window is added. RUI save/reload and actual docking preservation need Windows Rhino validation; XML-level tests do not prove native UI behavior.

## 2.4: Rhino 6 compatibility checks and separate tooltips

The prior build used RhinoCommon 6.35 and a RUI 3 header. C# and capture API probes also compile against Rhino 6.0.18016.23451, so production now targets that initial stable SDK and ships the backward-compatible RUI 2 schema. The NuGet lock retains package integrity metadata. `tools/CheckRhino6` adds compile-time probes for the APIs dynamically called from IronPython, including transparent ViewCapture and selection restoration. These checks establish API availability, not working Windows rendering or a confirmed cause for the user's reported Rhino 6 failure.

Startup waits for `RhinoApp.MainWindowHandle()` rather than relying on an Eto owner window. Legacy cleanup and saved-RUI migration failures are logged independently and cannot suppress initial native toolbar display. First-display persistence and subsequent docking behavior remain as before. The user's exact Rhino 6 failure stage/F2 output is still needed; do not call host compatibility validated from compilation alone.

Source macro tooltip/help text now differs for left and right click. Migration updates only those descriptions for recognized AutoCrop macros, retaining their IDs and other button/layout data. It also updates descriptions when the right-click binding is already correct; tests cover that previously missed case. A new binding revision reloads existing cached controls once to apply these descriptions.

Binary metadata comparison confirmed that 2.3.1 referenced RhinoCommon/Rhino.UI 6.35.21222.17000 and Eto 2.5.0.0. The 2.4 build references RhinoCommon/Rhino.UI 6.0.18016.23451 and Eto 2.3.6591.18824; `VerifyPlugin` enforces those baseline references to catch accidental SDK upgrades. This is a compatibility correction, not confirmation of the exact user error without its message.

## 2.4.1: installed evidence and hover identities

The former wrapper threw immediately on nonzero Rhino Installer exits, before checking output files. It now verifies output after every exit, including 403. Registered plugin paths are checked first, then the standard user plugin tree, with bounded retries for delayed copies. Tests use the actual verification class to cover 403 with valid output, zero with missing/old output, delayed valid output, and changed RUI layout/schema. This fixes false error reporting without calling an unverified installation successful. Windows Installer execution remains untested here.

Left/right hover descriptions now contain only their respective action, not the same combined sentence. New description macro IDs (`capture-hover-v2`, `resolution-hover-v2`) avoid retaining older cached macro text. The migration recognizes the prior capture ID and command-based copied buttons, rebinds those buttons to the new description macros and preserves their capture actions, toolbar/button/group IDs, docking and unrelated buttons. Unused legacy macro definitions are left intact for other references. The binding revision is `distinct-hover-3`. This intentionally replaces description-macro identities; older notes about retaining those identities describe previous releases.


2.4.3: 2.4.1 기반으로 캡처 중 작업평면 Z축 표시를 숨겨 가장자리 판정에서 제외합니다. 성공·실패 후 원래 축 표시 상태를 복구합니다. 실제 Windows Rhino 실행은 미검증입니다.


2.5.1: 툴바 우클릭 → **저장 폴더 변경…**에서 PNG 저장 폴더를 지정합니다. `BKIDAutoCropFolder` 명령으로도 변경할 수 있습니다. 선택한 폴더는 Rhino 사용자 설정에 저장되어 재시작 후에도 좌클릭 1배·우클릭 모든 배율에 적용됩니다. 폴더 선택 취소는 기존 경로를 유지합니다. 폴더만 바꾸려면 배율을 고르지 않고 창을 닫으세요. Z축 캡처 제외 수정도 유지합니다. 실제 Rhino UI는 미검증입니다.


2.5.2: 캡처 가장자리 감지 시 오류 추적 대신 “Display 패널에서 Z축(Z axis)을 끄고 다시 시도해 주세요.”를 표시하고 저장 없이 종료합니다. 저장 폴더 설정 기능은 유지합니다.


2.5.3: Z-axis guidance is now ASCII English: "Turn off Z axis in the Display panel, then try again."


2.5.4: Windows .rhi 연결 프로그램 대신 설치된 Rhino Installer Engine의 rhiexec.exe를 직접 실행합니다. 엔진을 찾지 못하면 기존 파일 삭제 전에 중단합니다. 파일 연결 설정은 변경하지 않습니다. 실제 Windows 실행은 미검증입니다.
