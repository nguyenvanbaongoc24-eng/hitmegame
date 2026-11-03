"""Synthetic evidence for screenshot regression heuristics."""

import tempfile
import unittest
from pathlib import Path

from PIL import Image, ImageDraw

from regression_from_screenshot import analyze


class ScreenshotRegressionTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = Path(self.temp.name) / "shot.png"

    def test_large_white_placeholder(self):
        image = Image.new("RGB", (120, 240), (40, 80, 100))
        ImageDraw.Draw(image).rectangle((20, 30, 100, 100), fill="white")
        image.save(self.path)
        result = analyze(self.path)
        self.assertTrue(result["flags"]["large_blank_white"])
        self.assertFalse(result["flags"]["top_black_band"])

    def test_solid_edge_bands_require_full_width(self):
        image = Image.new("RGB", (120, 240), (40, 80, 100))
        draw = ImageDraw.Draw(image)
        draw.rectangle((0, 0, 119, 24), fill="black")
        draw.rectangle((0, 215, 119, 239), fill="black")
        image.save(self.path)
        flags = analyze(self.path)["flags"]
        self.assertTrue(flags["top_black_band"])
        self.assertTrue(flags["bottom_black_band"])
        draw.rectangle((0, 0, 20, 24), fill=(40, 80, 100))
        image.save(self.path)
        self.assertFalse(analyze(self.path)["flags"]["top_black_band"])

    def test_clean_image_and_ocr_disclosure(self):
        Image.new("RGB", (120, 240), (40, 80, 100)).save(self.path)
        report = analyze(self.path)
        self.assertFalse(any(report["flags"].values()))
        self.assertFalse(report["ocr"]["supported"])


if __name__ == "__main__":
    unittest.main()
