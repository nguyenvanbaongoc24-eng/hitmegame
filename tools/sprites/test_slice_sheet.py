"""README: python -m unittest discover -s tools/sprites -p test_slice_sheet.py"""

import json
import tempfile
import unittest
from pathlib import Path

from PIL import Image, ImageDraw

from slice_sheet import slice_sheet
from validate_pack import validate_gp_directory
from pack_sprites import pack


class SliceSheetTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def sheet(self, rows=2, missing=None, merged=False, fringe=False, uneven=False):
        image = Image.new("RGB", (400 * 4, 300 * rows), (0, 255, 0))
        pen = ImageDraw.Draw(image)
        for row in range(rows):
            for col in range(4):
                if (row, col) == missing:
                    continue
                x, y = col * 400, row * 300
                pen.ellipse((x + 160, y + 55, x + 240, y + 245), fill=(100, 50, 30))
                if fringe:
                    pen.rectangle((x + 165, y + 52, x + 185, y + 54), fill=(20, 205, 20))
                if uneven:
                    pen.rectangle((x, y, x + 399, y + 1), fill=(10, 240, 5))
        if merged:
            pen.rectangle((390, 130, 410, 140), fill=(100, 50, 30))
        path = self.root / "source.png"
        image.save(path)
        return path

    def test_valid_four_by_two_has_shared_scale_foot_line_and_manifest(self):
        paths, manifest = slice_sheet(self.sheet(), self.root / "out", "nam_lang", ["hit", "ko"])
        self.assertEqual(8, len(paths))
        self.assertEqual("gp_nam_lang_hit_down_1.png", paths[0].name)
        self.assertEqual(8, len(json.loads(manifest.read_text())["frames"]))
        with Image.open(paths[0]) as frame:
            self.assertEqual((256, 256), frame.size)
            self.assertEqual(253, frame.getchannel("A").getbbox()[3] - 1)
        checked, issues = validate_gp_directory(self.root / "out")
        self.assertEqual(8, len(checked))
        self.assertEqual([], issues)
        atlas, packed = pack(self.root / "out", self.root / "atlas")
        self.assertTrue(atlas.exists())
        self.assertEqual(8, len(json.loads(packed.read_text())["frames"]))

    def test_valid_four_by_three(self):
        paths, _ = slice_sheet(self.sheet(3), self.root / "out", "nam_lang", ["aim", "hit", "ko"])
        self.assertEqual(12, len(paths))

    def test_missing_frame_rejected_and_annotated(self):
        with self.assertRaisesRegex(ValueError, "missing frame"):
            slice_sheet(self.sheet(missing=(1, 2)), self.root / "out", "nam_lang", ["hit", "ko"])
        self.assertTrue((self.root / "out" / "rejected_annotated.png").exists())
        self.assertEqual([], list((self.root / "out").glob("gp_*.png")))

    def test_merged_cells_rejected(self):
        with self.assertRaisesRegex(ValueError, "touches cell edge"):
            slice_sheet(self.sheet(merged=True), self.root / "out", "nam_lang", ["hit", "ko"])

    def test_green_fringe_rejected(self):
        with self.assertRaisesRegex(ValueError, "green fringe"):
            slice_sheet(self.sheet(fringe=True), self.root / "out", "nam_lang", ["hit", "ko"])

    def test_uneven_background_rejected(self):
        with self.assertRaisesRegex(ValueError, "uneven green background"):
            slice_sheet(self.sheet(uneven=True), self.root / "out", "nam_lang", ["hit", "ko"])


if __name__ == "__main__":
    unittest.main()
