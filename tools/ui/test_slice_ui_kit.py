"""Synthetic regression tests for transparent/green UI kit extraction."""

import tempfile
import unittest
from pathlib import Path

from PIL import Image, ImageDraw

from nineslice_suggest import suggest
from slice_ui_kit import slice_kit


class UiKitTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def kit(self, count=3, touching=False, green=False):
        image = Image.new("RGB" if green else "RGBA", (180, 100),
                          (0, 255, 0) if green else (0, 0, 0, 0))
        draw = ImageDraw.Draw(image)
        color = (200, 30, 20) if green else (200, 30, 20, 255)
        for i in range(count):
            x = 10 + i * (35 if touching else 55)
            draw.rectangle((x, 25, x + 30, 60), fill=color)
            # Deliberately detached decoration within the merge gap.
            draw.rectangle((x + 12, 63, x + 18, 65), fill=color)
        path = self.root / "kit.png"
        image.save(path)
        return path

    def test_extracts_named_items_with_true_alpha_and_contact_sheet(self):
        records = slice_kit(self.kit(green=True), self.root / "out",
                            ["Nút Chơi", "Khung Phòng", "Vũ Khí"], 3, merge_gap=4)
        self.assertEqual(3, len(records))
        self.assertEqual(["Nút Chơi", "Khung Phòng", "Vũ Khí"], [r["name"] for r in records])
        self.assertTrue((self.root / "out" / "contact_sheet.png").exists())
        with Image.open(self.root / "out" / records[0]["file"]) as crop:
            self.assertEqual("RGBA", crop.mode)

    def test_missing_item_rejects_with_annotated_image(self):
        with self.assertRaisesRegex(ValueError, "Detected 2/3"):
            slice_kit(self.kit(count=2), self.root / "out", ["A", "B", "C"], 3)
        self.assertTrue((self.root / "out" / "rejected_annotated.png").exists())
        self.assertFalse((self.root / "out" / "items.json").exists())

    def test_touching_items_rejects_instead_of_guessing(self):
        with self.assertRaisesRegex(ValueError, "Detected 1/2"):
            slice_kit(self.kit(count=2, touching=True), self.root / "out", ["A", "B"], 2)

    def test_nineslice_warns_on_patterned_center(self):
        image = Image.new("RGBA", (80, 80), (20, 40, 60, 255))
        draw = ImageDraw.Draw(image)
        draw.rectangle((0, 0, 79, 79), outline=(230, 220, 30, 255), width=5)
        flat = self.root / "flat.png"
        image.save(flat)
        self.assertFalse(suggest(flat)["center_patterned"])
        for y in range(20, 60, 4):
            draw.line((20, y, 59, y), fill=(250, 10, 10, 255), width=2)
        patterned = self.root / "patterned.png"
        image.save(patterned)
        self.assertTrue(suggest(patterned)["center_patterned"])


if __name__ == "__main__":
    unittest.main()
