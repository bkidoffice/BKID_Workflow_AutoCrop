import importlib.util
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[1]


class CaptureScaleTests(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location('capture_under_test', ROOT / 'BKID_AutoCrop/BKID_AutoCrop.py')
        self.capture = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.capture)

    def test_dimensions_and_fixed_margin_for_each_scale(self):
        for factor in (2, 3, 4):
            with self.subTest(factor=factor):
                self.assertEqual(self.capture.capture_dimensions(160, 90, factor), (160 * factor, 90 * factor))
                self.assertEqual(self.capture.output_size((0, 0, 10 * factor - 1, 6 * factor - 1)),
                                 (10 * factor + 2, 6 * factor + 2))
        self.assertEqual(self.capture.CAPTURE_SCALE, 1.0)
        self.assertEqual(self.capture.capture_dimensions(160, 90), (160, 90))

    def test_invalid_scale_fails_before_capture(self):
        for factor in (0, -1, float('nan'), float('inf'), -float('inf')):
            with self.subTest(factor=factor), self.assertRaises(ValueError):
                self.capture.capture_dimensions(160, 90, factor)

    def host(self, fail=False):
        selected = SimpleNamespace(value=True)
        obj = SimpleNamespace(IsSelected=lambda unused: selected.value,
                              GetSelectedSubObjects=lambda: [],
                              Select=lambda value: setattr(selected, 'value', value))
        doc = SimpleNamespace(Objects=SimpleNamespace(GetSelectedObjects=lambda *unused: [obj]))
        redraws = []
        plane = SimpleNamespace(ShowZAxis=True)
        view = SimpleNamespace(ActiveViewport=SimpleNamespace(Size=SimpleNamespace(Width=160, Height=90), GetConstructionPlane=lambda: plane, SetConstructionPlane=lambda value: None),
                               Redraw=lambda: redraws.append(True))
        records = []
        class ViewCapture:
            def CaptureToBitmap(settings, actual_view):
                self.assertIs(actual_view, view)
                self.assertFalse(selected.value)
                self.assertFalse(plane.ShowZAxis)
                records.append(settings)
                if fail: raise RuntimeError('capture failed')
                return 'bitmap'
        return doc, view, selected, redraws, records, SimpleNamespace(Display=SimpleNamespace(ViewCapture=ViewCapture))

    def test_scale_changes_render_dimensions_and_next_default_remains_one(self):
        doc, view, selected, redraws, records, rhino = self.host()
        with patch.dict(sys.modules, {'Rhino': rhino}):
            for factor in (2, 3, 4, None):
                self.assertEqual(self.capture.capture(doc, view, factor), 'bitmap')
                self.assertTrue(selected.value)
                self.assertTrue(view.ActiveViewport.GetConstructionPlane().ShowZAxis)
        self.assertEqual([(item.Width, item.Height) for item in records],
                         [(320, 180), (480, 270), (640, 360), (160, 90)])
        self.assertTrue(all(item.TransparentBackground for item in records))
        self.assertTrue(all(not item.DrawGrid for item in records))
        self.assertEqual(len(redraws), 8)

    def test_capture_error_still_restores_selection(self):
        doc, view, selected, redraws, records, rhino = self.host(fail=True)
        with patch.dict(sys.modules, {'Rhino': rhino}), self.assertRaisesRegex(RuntimeError, 'capture failed'):
            self.capture.capture(doc, view, 4)
        self.assertTrue(selected.value)
        self.assertTrue(view.ActiveViewport.GetConstructionPlane().ShowZAxis)
        self.assertEqual(len(redraws), 2)
