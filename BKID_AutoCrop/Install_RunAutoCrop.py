# -*- coding: utf-8 -*-
"""Run once per Rhino version with RunPythonScript; keep both files together."""
import os
import rhinoscriptsyntax as rs


def install():
    folder = os.path.dirname(os.path.abspath(__file__))
    target = os.path.join(folder, "BKID_AutoCrop.py")
    if not os.path.isfile(target):
        raise RuntimeError(u"같은 폴더에 BKID_AutoCrop.py가 있어야 합니다.")
    macro = u'! _-RunPythonScript "{0}"'.format(target)
    if rs.IsAlias("RunAutoCrop"):
        rs.AliasMacro("RunAutoCrop", macro)
    else:
        rs.AddAlias("RunAutoCrop", macro)
    if rs.AliasMacro("RunAutoCrop") != macro:
        raise RuntimeError(u"RunAutoCrop 명령 등록에 실패했습니다.")
    print(u"등록 완료. 이제 RunAutoCrop 입력 후 Enter로 캡처합니다.")
    print(u"Rhino 6과 8에서 각각 한 번 등록하세요. 이 폴더는 이동하지 마세요.")


if __name__ == "__main__":
    install()
