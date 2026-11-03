"""README: python -m unittest discover -s tools/ui -p 'test_*.py'"""

import tempfile
import unittest
from pathlib import Path

from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen

from glyph_coverage import audit, vietnamese_characters


def fake_font(path, chars):
    builder = FontBuilder(1000, isTTF=True)
    names = [".notdef"] + [f"uni{ord(ch):04X}" for ch in sorted(set(chars))]
    builder.setupGlyphOrder(names)
    builder.setupCharacterMap({ord(ch): f"uni{ord(ch):04X}" for ch in set(chars)})
    pen = TTGlyphPen(None)
    glyph = pen.glyph()
    builder.setupGlyf({name: glyph for name in names})
    builder.setupHorizontalMetrics({name: (500, 0) for name in names})
    builder.setupHorizontalHeader(ascent=800, descent=-200)
    builder.setupNameTable({"familyName": "Fake", "styleName": "Regular"})
    builder.setupOS2(sTypoAscender=800, sTypoDescender=-200, usWinAscent=800, usWinDescent=200)
    builder.setupPost()
    builder.setupMaxp()
    builder.save(path)


class GlyphCoverageTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def test_missing_glyph_reports_file_and_line(self):
        font = self.root / "fake.ttf"
        fake_font(font, "Nha?t")
        source = self.root / "UiStrings.cs"
        source.write_text('class X {\n const string Title = "Nhân vật";\n}', encoding="utf-8")
        report = audit(font, [source])
        self.assertIn("â", report["missing"])
        self.assertIn("ậ", report["missing"])
        self.assertIn(f"{source}:2", report["missing"]["â"])

    def test_complete_fake_font_passes_json_and_md(self):
        font = self.root / "fake.ttf"
        fake_font(font, "Nhân vật Đẹp 012")
        (self.root / "items.json").write_text('{"name":"Đẹp"}', encoding="utf-8")
        (self.root / "screen.md").write_text("Nhân vật\n```\nΩ\n```", encoding="utf-8")
        self.assertEqual(0, audit(font, [self.root])["missingCount"])

    def test_vietnamese_set_includes_precomposed_tones(self):
        letters = vietnamese_characters()
        for character in "ắằẳẵặớờởỡợỷỵĐđ":
            self.assertIn(character, letters)
        font = self.root / "fake.ttf"
        fake_font(font, "a")
        self.assertIn("ắ", audit(font, [], True)["missing"])


if __name__ == "__main__":
    unittest.main()
