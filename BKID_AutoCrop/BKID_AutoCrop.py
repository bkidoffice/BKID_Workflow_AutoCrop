# -*- coding: utf-8 -*-
"""BKID AutoCrop 1.3 - Windows Rhino 6 and 8, IronPython 2.7.
Run with _RunPythonScript. No third-party Python packages required.
"""
import math
import os
import sys
import traceback

CAPTURE_SCALE = 1.0  # Capture resolution BEFORE crop; margin is always 1 pixel.
OUTPUT_FOLDER = None  # None = Windows Desktop/BKID_Captures
MARGIN = 1


def initialize_runtime():
    if sys.platform != "cli" or sys.version_info[0] != 2:
        raise RuntimeError(u"Rhino 6/8 명령창의 RunPythonScript로 실행해 주세요. "
                           u"ScriptEditor에서는 Python 2 (IronPython)를 선택하세요.")
    import clr
    clr.AddReference("System.Drawing")
    import System
    import Rhino
    if System.Environment.OSVersion.Platform != System.PlatformID.Win32NT:
        raise RuntimeError(u"이 버전은 Windows 전용입니다.")
    Rhino.RhinoApp.WriteLine("BKID AutoCrop 1.3 | Rhino {0} | IronPython {1}".format(
        Rhino.RhinoApp.Version, sys.version.split()[0]))


def verify_bitmap_runtime():
    """Exercise actual .NET interop before touching Rhino selection state."""
    from System.Drawing import Bitmap, Color, Graphics
    from System.Drawing.Imaging import PixelFormat, ImageFormat
    from System.IO import MemoryStream
    sample = Bitmap(5, 5, PixelFormat.Format32bppArgb)
    stream = MemoryStream()
    reopened = None
    try:
        graphics = Graphics.FromImage(sample)
        try:
            graphics.Clear(Color.Transparent)
        finally:
            graphics.Dispose()
        if bitmap_bounds(sample) is not None:
            raise RuntimeError("Empty alpha self-test failed")
        sample.SetPixel(1, 1, Color.FromArgb(1, 255, 255, 255))
        sample.SetPixel(3, 3, Color.FromArgb(255, 0, 0, 0))
        sample.Save(stream, ImageFormat.Png)
        stream.Position = 0
        reopened = Bitmap(stream)
        if bitmap_bounds(reopened) != (1, 1, 3, 3):
            raise RuntimeError("PNG alpha / Marshal.Copy self-test failed")
    finally:
        if reopened is not None:
            reopened.Dispose()
        stream.Dispose()
        sample.Dispose()


def alpha_bounds(rows, width, height):
    """Rows contain BGRA bytes. Retain every alpha > 0, including AA pixels."""
    left, top, right, bottom = width, height, -1, -1
    for y, row in enumerate(rows):
        for x in range(width):
            if row[4 * x + 3] > 0:
                left = min(left, x)
                right = max(right, x)
                top = min(top, y)
                bottom = max(bottom, y)
    if right < 0:
        return None
    return (left, top, right, bottom)


def output_size(bounds):
    left, top, right, bottom = bounds
    return (right - left + 1 + 2 * MARGIN,
            bottom - top + 1 + 2 * MARGIN)


def touches_frame(bounds, width, height):
    left, top, right, bottom = bounds
    return left == 0 or top == 0 or right == width - 1 or bottom == height - 1


def bitmap_bounds(bitmap):
    from System import Array, Byte, IntPtr
    from System.Runtime.InteropServices import Marshal
    from System.Drawing import Rectangle
    from System.Drawing.Imaging import ImageLockMode, PixelFormat
    data = bitmap.LockBits(Rectangle(0, 0, bitmap.Width, bitmap.Height),
                           ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
    try:
        def rows():
            row = Array[Byte]([0] * (bitmap.Width * 4))
            for y in range(bitmap.Height):
                pointer = IntPtr(data.Scan0.ToInt64() + y * data.Stride)
                Marshal.Copy(pointer, row, 0, row.Length)
                yield row
        return alpha_bounds(rows(), bitmap.Width, bitmap.Height)
    finally:
        bitmap.UnlockBits(data)


def resolve_capture_scale(capture_scale=None):
    scale = float(CAPTURE_SCALE if capture_scale is None else capture_scale)
    if scale <= 0 or math.isnan(scale) or math.isinf(scale):
        raise ValueError("Capture scale must be a finite number greater than zero")
    return scale


def capture_dimensions(width, height, capture_scale=None):
    scale = resolve_capture_scale(capture_scale)
    return (max(1, int(round(width * scale))),
            max(1, int(round(height * scale))))


def capture(doc, view, capture_scale=None):
    import Rhino
    width, height = capture_dimensions(view.ActiveViewport.Size.Width,
                                       view.ActiveViewport.Size.Height,
                                       capture_scale)
    selected = list(doc.Objects.GetSelectedObjects(False, False))
    states = [(obj, obj.IsSelected(False), list(obj.GetSelectedSubObjects() or []))
              for obj in selected]
    viewport = view.ActiveViewport
    construction_plane = viewport.GetConstructionPlane()
    show_z_axis = construction_plane.ShowZAxis
    try:
        # Rhino manages the construction-plane Z axis separately from DrawGridAxes.
        construction_plane.ShowZAxis = False
        viewport.SetConstructionPlane(construction_plane)
        for obj, state, components in states:
            for component in components:
                obj.SelectSubObject(component, False, True)
            if state:
                obj.Select(False)
        view.Redraw()
        settings = Rhino.Display.ViewCapture()
        settings.Width = width
        settings.Height = height
        settings.ScaleScreenItems = False
        settings.DrawAxes = False
        settings.DrawGrid = False
        settings.DrawGridAxes = False
        settings.TransparentBackground = True
        return settings.CaptureToBitmap(view)
    finally:
        construction_plane.ShowZAxis = show_z_axis
        viewport.SetConstructionPlane(construction_plane)
        for obj, state, components in states:
            if state:
                obj.Select(True)
            for component in components:
                obj.SelectSubObject(component, True, True)
        view.Redraw()


def resolve_output_folder(output_folder=None):
    folder = OUTPUT_FOLDER if output_folder is None else output_folder
    if folder is None:
        import System
        folder = os.path.join(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "BKID_Captures")
    if not folder or not folder.strip():
        raise ValueError(u"PNG 저장 폴더를 선택해 주세요.")
    return os.path.abspath(folder)


def run(capture_scale=None, output_folder=None):
    scale = resolve_capture_scale(capture_scale)
    initialize_runtime()
    verify_bitmap_runtime()
    import Rhino
    import System
    from System.Drawing import Bitmap, Color, Graphics, Rectangle
    from System.Drawing.Drawing2D import CompositingMode
    from System.Drawing.Imaging import PixelFormat, ImageFormat
    from System.IO import File, FileMode, FileAccess, FileShare, FileStream

    doc = Rhino.RhinoDoc.ActiveDoc
    view = doc.Views.ActiveView if doc else None
    if view is None:
        raise RuntimeError(u"활성 모델 뷰포트가 없습니다.")
    if isinstance(view, Rhino.Display.RhinoPageView):
        raise RuntimeError(u"Layout 대신 모델 뷰포트에서 실행해 주세요.")
    folder = resolve_output_folder(output_folder)
    if not os.path.isdir(folder):
        os.makedirs(folder)
    name = "BKID_{0}_{1}.png".format(
        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
        System.Guid.NewGuid().ToString("N")[:8])
    final_path = os.path.join(folder, name)
    temporary_path = final_path + ".part"
    source = cropped = output = check = None
    try:
        source = capture(doc, view, scale)
        if source is None:
            raise RuntimeError(u"Rhino가 캡처 이미지를 반환하지 않았습니다.")
        bounds = bitmap_bounds(source)
        if bounds is None:
            raise RuntimeError(u"저장할 픽셀이 없습니다. 오브젝트의 표시 상태를 확인해 주세요.")
        if touches_frame(bounds, source.Width, source.Height):
            Rhino.RhinoApp.WriteLine(
                u"Display 패널에서 Z축(Z axis)을 끄고 다시 시도해 주세요.")
            return None
        left, top, right, bottom = bounds
        width, height = output_size(bounds)
        cropped = source.Clone(Rectangle(left, top, right-left+1, bottom-top+1),
                               PixelFormat.Format32bppArgb)
        output = Bitmap(width, height, PixelFormat.Format32bppArgb)
        graphics = Graphics.FromImage(output)
        try:
            graphics.Clear(Color.Transparent)
            graphics.CompositingMode = CompositingMode.SourceCopy
            graphics.DrawImageUnscaled(cropped, MARGIN, MARGIN)
        finally:
            graphics.Dispose()
        # CreateNew prevents accidental overwrite. Verify the encoded PNG before commit.
        stream = FileStream(temporary_path, FileMode.CreateNew, FileAccess.Write, getattr(FileShare, "None"))
        try:
            output.Save(stream, ImageFormat.Png)
        finally:
            stream.Dispose()
        check = Bitmap(temporary_path)
        actual = bitmap_bounds(check)
        expected = (MARGIN, MARGIN, width-MARGIN-1, height-MARGIN-1)
        if check.Width != width or check.Height != height or actual != expected:
            raise RuntimeError(u"저장 후 투명도/1px 여백 검증에 실패했습니다.")
        check.Dispose()
        check = None
        File.Move(temporary_path, final_path)
        Rhino.RhinoApp.WriteLine(u"저장 완료: {0} × {1}px | 사방 1px 투명 여백".format(width, height))
        Rhino.RhinoApp.WriteLine(final_path)
        return final_path
    finally:
        for bitmap in (check, output, cropped, source):
            if bitmap is not None:
                bitmap.Dispose()
        if File.Exists(temporary_path):
            File.Delete(temporary_path)


if __name__ == "__main__":
    try:
        run()
    except Exception as error:
        import Rhino
        Rhino.RhinoApp.WriteLine(u"BKID AutoCrop: {0}".format(error))
        Rhino.RhinoApp.WriteLine(traceback.format_exc())
