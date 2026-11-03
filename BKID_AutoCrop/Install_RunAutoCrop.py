# -*- coding: utf-8 -*-
"""Installer 1.2.1. Install with Windows Rhino 6/8 _RunPythonScript (IronPython 2.7)."""
import base64
import os
import shutil
import sys
import tempfile
import traceback

# The release builder replaces this marker with the complete embedded payload.
EMBEDDED_FILES = None
MANAGED_FILES = ("BKID_AutoCrop.py", "BKID_AutoCrop.rui",
                 "assets/BKID_AutoCrop_Icon.svg")


def payload():
    if EMBEDDED_FILES is not None:
        return dict((name, base64.b64decode(EMBEDDED_FILES[name]))
                    for name in MANAGED_FILES)
    folder = os.path.dirname(os.path.abspath(__file__))
    result = {}
    for name in MANAGED_FILES:
        with open(os.path.join(folder, *name.split('/')), "rb") as stream:
            result[name] = stream.read()
    return result


def same_file(path, content):
    if not os.path.isfile(path):
        return False
    with open(path, "rb") as stream:
        return stream.read() == content


def close_toolbar(path, toolbar_files):
    """Use the exact file handle; verify state instead of trusting Close's return."""
    toolbar = toolbar_files.FindByPath(path)
    if toolbar is None:
        return
    toolbar.Close(False)
    if toolbar_files.FindByPath(path) is not None:
        raise RuntimeError(u"BKID 툴바 파일이 아직 열려 있습니다. Rhino를 다시 시작한 뒤 설치해 주세요.")


def install():
    if sys.platform != "cli" or sys.version_info[0] != 2:
        raise RuntimeError(u"Windows Rhino의 _RunPythonScript로 실행해 주세요 (IronPython 2.7).")
    import Rhino
    import System
    import rhinoscriptsyntax as rs
    if System.Environment.OSVersion.Platform != System.PlatformID.Win32NT:
        raise RuntimeError(u"Windows Rhino 전용 설치 파일입니다.")
    version = Rhino.RhinoApp.Version.Major
    if version not in (6, 8):
        raise RuntimeError(u"지원 대상은 Rhino 6 및 8입니다.")
    files = payload()  # Check the entire package before changing an existing install.
    folder = os.path.join(System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.ApplicationData),
        "BKID", "AutoCrop", "Rhino{0}".format(version))
    if not os.path.isdir(folder):
        os.makedirs(folder)
    rui = os.path.join(folder, "BKID_AutoCrop.rui")
    backup = tempfile.mkdtemp(prefix="BKID_AutoCrop_backup_")
    old_alias = rs.AliasMacro("RunAutoCrop") if rs.IsAlias("RunAutoCrop") else None
    old_collection = rs.IsToolbarCollection(rui)
    toolbar_changed = not same_file(rui, files["BKID_AutoCrop.rui"])
    closed = False
    written = []
    try:
        for name in MANAGED_FILES:
            target = os.path.join(folder, *name.split('/'))
            if os.path.isfile(target):
                saved = os.path.join(backup, *name.split('/'))
                parent = os.path.dirname(saved)
                if not os.path.isdir(parent):
                    os.makedirs(parent)
                shutil.copy2(target, saved)
        if old_collection and toolbar_changed:
            close_toolbar(rui, Rhino.RhinoApp.ToolbarFiles)
            closed = True
        for name in MANAGED_FILES:
            target = os.path.join(folder, *name.split('/'))
            if same_file(target, files[name]):
                continue
            parent = os.path.dirname(target)
            if not os.path.isdir(parent):
                os.makedirs(parent)
            written.append(name)
            with open(target, "wb") as stream:
                stream.write(files[name])
        macro = u'! _-RunPythonScript "{0}"'.format(os.path.join(folder, "BKID_AutoCrop.py"))
        if rs.IsAlias("RunAutoCrop"):
            rs.AliasMacro("RunAutoCrop", macro)
        else:
            rs.AddAlias("RunAutoCrop", macro)
        if rs.AliasMacro("RunAutoCrop") != macro:
            raise RuntimeError(u"RunAutoCrop 명령 등록에 실패했습니다.")
        collection = rs.IsToolbarCollection(rui) or rs.OpenToolbarCollection(rui)
        if not collection:
            raise RuntimeError(u"로고 툴바를 열지 못했습니다.")
        if not rs.ShowToolbar(collection, "BKID AutoCrop"):
            raise RuntimeError(u"BKID AutoCrop 툴바를 표시하지 못했습니다.")
        if not rs.IsToolbarVisible(collection, "BKID AutoCrop"):
            raise RuntimeError(u"툴바 표시 상태를 확인하지 못했습니다.")
    except Exception:
        # Restore only files and the alias this installer owns.
        try:
            current = rs.IsToolbarCollection(rui)
            if current and ("BKID_AutoCrop.rui" in written or closed):
                close_toolbar(rui, Rhino.RhinoApp.ToolbarFiles)
            for name in written:
                target = os.path.join(folder, *name.split('/'))
                saved = os.path.join(backup, *name.split('/'))
                if os.path.isfile(saved):
                    shutil.copy2(saved, target)
                elif os.path.isfile(target):
                    os.remove(target)
            if old_alias is None:
                if rs.IsAlias("RunAutoCrop"):
                    rs.DeleteAlias("RunAutoCrop")
            else:
                rs.AliasMacro("RunAutoCrop", old_alias)
            if old_collection and closed:
                restored = rs.OpenToolbarCollection(rui)
                if not restored:
                    raise RuntimeError("Cannot reopen original toolbar")
        except Exception:
            Rhino.RhinoApp.WriteLine(u"복구 오류. 이전 파일 백업: {0}".format(backup))
            Rhino.RhinoApp.WriteLine(traceback.format_exc())
            backup = None  # Retain backup for manual recovery.
        raise
    finally:
        if backup is not None:
            shutil.rmtree(backup)
    Rhino.RhinoApp.WriteLine(u"설치 완료 (1.2.1): 로고 버튼을 클릭하거나 RunAutoCrop을 입력하세요.")
    Rhino.RhinoApp.WriteLine(u"설치 위치: {0}".format(folder))
    Rhino.RhinoApp.WriteLine(u"다운로드한 설치 파일은 이제 이동하거나 삭제해도 됩니다.")
    return folder


if __name__ == "__main__":
    try:
        install()
    except Exception:
        import Rhino
        Rhino.RhinoApp.WriteLine(u"BKID AutoCrop 설치 실패:\n" + traceback.format_exc())
