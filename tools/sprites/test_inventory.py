"""Synthetic screening tests; run: python -m unittest tools/sprites/test_inventory.py -v."""

import unittest
from pathlib import Path
from tempfile import TemporaryDirectory

import yaml
from PIL import Image, ImageDraw, ImageFont

from tools.sprites.inventory import classify, crop_unity, inventory


class InventoryTests(unittest.TestCase):
    def canvas(self):
        return Image.new("RGBA", (256, 256), (0, 0, 0, 0))

    def test_valid_single_figure(self):
        image = self.canvas()
        ImageDraw.Draw(image).ellipse((65, 26, 189, 226), fill="#3377bb")
        self.assertEqual([], classify(image, (256, 256))[0])

    def test_cut_edge(self):
        image = self.canvas()
        ImageDraw.Draw(image).ellipse((-40, 26, 100, 225), fill="#3377bb")
        self.assertIn("touches_frame_edge", classify(image)[0])

    def test_multiple_figures(self):
        image = self.canvas()
        draw = ImageDraw.Draw(image)
        draw.ellipse((20, 25, 95, 220), fill="#3377bb")
        draw.ellipse((155, 25, 230, 220), fill="#ee7744")
        self.assertIn("multiple_large_components", classify(image)[0])

    def test_text_or_ui(self):
        image = self.canvas()
        draw = ImageDraw.Draw(image)
        draw.ellipse((65, 85, 189, 225), fill="#3377bb")
        draw.text((38, 24), "HELLO UI TEXT", font=ImageFont.load_default(), fill="white")
        self.assertIn("possible_text_or_ui", classify(image)[0])

    def test_unity_bottom_origin_and_meta_inventory(self):
        with TemporaryDirectory() as tmp:
            source, out = Path(tmp) / "Unity", Path(tmp) / "out"
            source.mkdir()
            texture = Image.new("RGBA", (64, 128), (0, 0, 0, 0))
            ImageDraw.Draw(texture).rectangle((8, 8, 55, 55), fill="#4477aa")
            image = source / "sample.png"
            texture.save(image)
            meta = {"TextureImporter": {"spriteSheet": {"sprites": [
                {"name": "sample_Idle_00", "rect": {"x": 0, "y": 64, "width": 64, "height": 64}}
            ]}}}
            Path(str(image) + ".meta").write_text(yaml.safe_dump(meta), encoding="utf-8")
            self.assertEqual((68, 119, 170, 255), crop_unity(texture, meta["TextureImporter"]["spriteSheet"]["sprites"][0]["rect"]).getpixel((20, 20)))
            rows = inventory(source, out)
            self.assertEqual(1, len(rows))
            self.assertTrue((out / "frames" / "sample" / "000.png").exists())
            self.assertTrue((out / "inventory.json").exists())
            self.assertTrue((out / "contact_sheet.png").exists())
            self.assertFalse((source / "out").exists())


if __name__ == "__main__":
    unittest.main()
