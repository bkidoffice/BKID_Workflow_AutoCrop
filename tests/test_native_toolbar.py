import base64
import io
from pathlib import Path
import unittest
import xml.etree.ElementTree as E
import zipfile
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
RUI = ROOT / 'plugin/BKID.AutoCrop/Toolbar/BKID.AutoCrop.rui'


class NativeToolbarTests(unittest.TestCase):
    def test_native_group_and_logo_references(self):
        root = E.parse(RUI).getroot()
        group = root.find('tool_bar_groups/tool_bar_group')
        bar = root.find('tool_bars/tool_bar')
        macro = root.find('macros/macro_item')
        self.assertEqual(group.findtext('text/locale_1033'), 'BKID Workflowboost')
        self.assertEqual(group.get('active_tool_bar_group'), bar.get('guid'))
        self.assertEqual(group.find('tool_bar_group_item').get('guid'), bar.get('guid'))
        self.assertEqual(group.findtext('tool_bar_group_item/tool_bar_id'), bar.get('guid'))
        self.assertEqual(group.find('dock_bar_info').get('floating'), 'True')
        self.assertEqual(bar.findtext('tool_bar_item/left_macro_id'), macro.get('guid'))
        self.assertEqual(macro.findtext('script'), '! _BKIDAutoCropCapture')
        # Native RUI stores bitmap_id as an attribute, not a child element.
        self.assertIsNone(macro.find('bitmap_id'))
        for size, name in [(16, 'small'), (24, 'normal'), (32, 'large')]:
            bitmap = root.find('bitmaps/' + name + '_bitmap')
            self.assertEqual(bitmap.find('bitmap_item').get('guid'), macro.get('bitmap_id'))
            with Image.open(io.BytesIO(base64.b64decode(bitmap.findtext('bitmap')))) as png:
                self.assertEqual(png.size, (size, size))
                self.assertEqual(png.getpixel((0, 0))[3], 0)

    def test_rhi_contains_matching_named_plugin_and_toolbar(self):
        with zipfile.ZipFile(ROOT / 'installers/BKID_AutoCrop_2.2.0.rhi') as archive:
            self.assertIsNone(archive.testzip())
            self.assertEqual(set(archive.namelist()), {'BKID.AutoCrop.rhp', 'BKID.AutoCrop.rui'})
            self.assertTrue(archive.read('BKID.AutoCrop.rhp').startswith(b'MZ'))
            self.assertEqual(archive.read('BKID.AutoCrop.rui'), RUI.read_bytes())
