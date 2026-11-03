"""Run with python -m unittest discover -s tools/harness_lib -p test_*.py -v."""

import unittest
from pathlib import Path
from tempfile import TemporaryDirectory

from PIL import Image, ImageDraw

from tools.harness_lib import head_ratio, layout_check, report, visual_diff


class HarnessTests(unittest.TestCase):
    def test_layout_valid(self):
        dump = {"safeArea": [0, 0, 200, 200], "elements": [
            {"name": "A", "rect": [0, 0, 50, 50], "clickable": True, "text": "A", "textRect": [3, 3, 20, 20]},
            {"name": "B", "rect": [70, 0, 50, 50], "clickable": True}]}
        self.assertTrue(layout_check.check(dump)["passed"])

    def test_layout_overlap_and_small_target(self):
        dump = {"safeArea": [0, 0, 200, 200], "elements": [
            {"name": "A", "rect": [0, 0, 50, 50], "clickable": True},
            {"name": "B", "rect": [20, 0, 30, 50], "clickable": True}]}
        failures = layout_check.check(dump)["failures"]
        self.assertTrue(any("clickable_overlap" in x for x in failures))
        self.assertTrue(any("touch_target_below_44px" in x for x in failures))

    def test_layout_safe_area_and_text(self):
        result = layout_check.check({"safeArea": [10, 10, 100, 100], "elements": [
            {"name": "Title", "rect": [0, 0, 30, 30], "text": "HIT ME", "textRect": [0, 0, 90, 30]}]})
        self.assertEqual(2, len(result["failures"]))

    def test_head_ratio_pass_and_fail(self):
        anatomy = {"points": {"headTop": [0, 0], "chin": [0, 25], "hip": [0, 60], "heel": [0, 100]},
                   "expectedHeadFraction": 0.25, "tolerance": 0.02}
        self.assertTrue(head_ratio.check(anatomy)["passed"])
        anatomy["points"]["chin"][1] = 40
        self.assertFalse(head_ratio.check(anatomy)["passed"])

    def test_visual_identical_and_shifted(self):
        with TemporaryDirectory() as tmp:
            root = Path(tmp)
            original = Image.new("RGB", (100, 100), "black")
            ImageDraw.Draw(original).rectangle((10, 30, 39, 69), fill="red")
            original.save(root / "reference.png")
            original.save(root / "actual.png")
            spec = {"regions": [{"name": "actor", "rect": [0, 0, 100, 100],
                                 "minSsim": 0.9, "maxChangedFraction": 0.05,
                                 "maxCentroidShift": 3}]}
            self.assertTrue(visual_diff.compare(root / "actual.png", root / "reference.png", spec)["passed"])
            shifted = Image.new("RGB", (100, 100), "black")
            ImageDraw.Draw(shifted).rectangle((50, 30, 79, 69), fill="red")
            shifted.save(root / "actual.png")
            result = visual_diff.compare(root / "actual.png", root / "reference.png", spec, root / "overlay.png")
            self.assertFalse(result["passed"])
            self.assertGreater(result["regions"][0]["centroid_shift_px"], 3)
            self.assertTrue((root / "overlay.png").exists())

    def test_report(self):
        output = report.render({"layout": {"passed": False, "failures": ["overlap"]},
                                "head": {"passed": True}})
        self.assertIn("| layout | FAIL | overlap |", output)
        self.assertIn("| head | PASS |", output)


if __name__ == "__main__":
    unittest.main()
