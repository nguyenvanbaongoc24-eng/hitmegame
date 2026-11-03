"""Run: python -m unittest discover -s tools/sprites -p test_sprites.py -v.

All fixtures are generated in temporary directories; no real asset is edited.
"""

from __future__ import annotations

import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path

from PIL import Image, ImageDraw

sys.path.insert(0, str(Path(__file__).resolve().parent))
from pack_sprites import pack
from validate_pack import DIRECTIONS, POSES, inspect_sprite, validate_directory


def sprite(path: Path, *, box=(100, 55, 156, 253), mode="RGBA",
           size=(256, 256), ring=False, shadow=False) -> None:
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    if ring:
        draw.ellipse((35, 230, 220, 251), fill=(0, 230, 235, 90))
    if shadow:
        draw.ellipse((35, 230, 220, 251), fill=(10, 10, 10, 90))
    draw.rectangle(box, fill=(180, 90, 40, 255))
    if mode == "RGB":
        image = image.convert("RGB")
    image.save(path)


class SpriteToolTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "sprites"
        self.root.mkdir()
        self.path = self.root / "char_namlang_idle_down.png"
        sprite(self.path)

    def errors(self):
        return inspect_sprite(self.path).issues

    def complete_set(self):
        for pose in POSES:
            for direction in DIRECTIONS:
                path = self.root / f"char_namlang_{pose}_{direction}.png"
                if not path.exists():
                    sprite(path)

    def test_valid_frame_and_foot_pivot(self):
        result = inspect_sprite(self.path)
        self.assertEqual(result.issues, [])
        self.assertEqual(result.foot_anchor, [128, 253])

    def test_invalid_name(self):
        self.path.rename(self.root / "Nam Lang Idle.png")
        results, _ = validate_directory(self.root)
        self.assertIn("invalid_name", results[0].issues[0])

    def test_wrong_dimensions(self):
        sprite(self.path, size=(128, 256), box=(40, 55, 90, 253))
        self.assertTrue(any("wrong_size" in error for error in self.errors()))

    def test_fake_transparency_or_missing_alpha(self):
        sprite(self.path, mode="RGB")
        self.assertIn("missing_rgba_alpha", self.errors())

    def test_opaque_border(self):
        with Image.open(self.path) as image:
            image.putpixel((0, 100), (255, 255, 255, 255))
            image.save(self.path)
        self.assertIn("opaque_top_or_side_border", self.errors())

    def test_foot_off_center(self):
        sprite(self.path, box=(118, 55, 174, 253))
        self.assertTrue(any("foot_anchor_off_center" in error for error in self.errors()))

    def test_foot_above_bottom(self):
        sprite(self.path, box=(100, 55, 156, 245))
        self.assertTrue(any("feet_too_far_from_bottom" in error for error in self.errors()))

    def test_character_height(self):
        sprite(self.path, box=(100, 100, 156, 253))
        self.assertTrue(any("character_height" in error for error in self.errors()))

    def test_baked_cyan_ring(self):
        sprite(self.path, ring=True)
        self.assertTrue(any("possible_baked_cyan_ring" in error for error in self.errors()))

    def test_baked_dark_shadow(self):
        sprite(self.path, shadow=True)
        self.assertTrue(any("possible_baked_dark_shadow" in error for error in self.errors()))

    def test_missing_frames_and_duplicate(self):
        _, issues = validate_directory(self.root)
        self.assertEqual(sum(issue.startswith("missing_frame") for issue in issues), 17)
        (self.root / "nested").mkdir()
        sprite(self.root / "nested" / self.path.name)
        _, issues = validate_directory(self.root)
        self.assertTrue(any(issue.startswith("duplicate_frame") for issue in issues))

    def test_pack_refuses_invalid_set_and_source_output(self):
        with self.assertRaisesRegex(ValueError, "validation"):
            pack(self.root, Path(self.temp.name) / "out")
        self.complete_set()
        with self.assertRaisesRegex(ValueError, "outside"):
            pack(self.root, self.root / "atlas")

    def test_pack_manifest_is_lossless_and_deterministic(self):
        self.complete_set()
        source_bytes = {path.name: hashlib.sha256(path.read_bytes()).digest()
                        for path in self.root.glob("*.png")}
        atlas_path, manifest_path = pack(self.root, Path(self.temp.name) / "out")
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        self.assertEqual(len(manifest["frames"]), 18)
        self.assertEqual((manifest["width"], manifest["height"]), (1536, 768))
        with Image.open(atlas_path) as atlas:
            for entry in manifest["frames"]:
                rect = entry["rect"]
                frame = atlas.crop((rect["x"], rect["y"], rect["x"] + 256, rect["y"] + 256))
                with Image.open(self.root / entry["name"]) as source:
                    self.assertEqual(frame.tobytes(), source.tobytes())
                self.assertEqual(entry["pivotPixelsTopLeft"], {"x": 128, "y": 253})
        self.assertEqual(source_bytes, {path.name: hashlib.sha256(path.read_bytes()).digest()
                                        for path in self.root.glob("*.png")})
        with self.assertRaises(FileExistsError):
            pack(self.root, Path(self.temp.name) / "out")


if __name__ == "__main__":
    unittest.main()
