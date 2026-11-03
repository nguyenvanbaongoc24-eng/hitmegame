"""Synthetic tests for independent frame/clip inventory."""

import json
import tempfile
import unittest
from pathlib import Path

from animation_audit import audit, clip_frames


class AnimationAuditTests(unittest.TestCase):
    def test_counts_sprite_curve_only(self):
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "Idle.anim"
            path.write_text("m_PPtrCurves:\n    - time: 0\n    - time: 0.1\n  m_SampleRate: 12\n    - time: 0.2\n", encoding="utf-8")
            self.assertEqual(2, clip_frames(path))

    def test_reports_missing_clips_and_png_without_editing_source(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            group = root / "src" / "NAM LÀNG" / "BÌNH THƯỜNG"
            group.mkdir(parents=True)
            (group / "metadata.json").write_text(json.dumps({"sprites": [{"file": "missing.png"}]}), encoding="utf-8")
            report = audit(root / "src", root / "unity")
            nam = report["characters"]["Nam Làng"]
            self.assertEqual(["missing.png"], nam["source_groups"]["BÌNH THƯỜNG"]["missing_png"])
            self.assertIn("Aim", nam["missing_clips"])
            self.assertEqual("no_verified_frame_sequence", report["characters"]["Quậy Boy"]["source_status"])


if __name__ == "__main__":
    unittest.main()
