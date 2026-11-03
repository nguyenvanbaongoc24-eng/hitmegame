"""Self-tests: python -m unittest discover tools/blackbox -v"""
from __future__ import annotations

import tempfile
import unittest
from pathlib import Path

from PIL import Image, ImageDraw

from golden import compare
from run import image_content, probe


def fake_page(path: Path, overlap: bool = False) -> None:
    second_x = 50 if overlap else 210
    path.write_text(f"""<!doctype html><meta name=viewport content='width=device-width,initial-scale=1'>
<style>html,body{{height:100%;}}body{{margin:0;background:linear-gradient(90deg,#63490a,#21a9ca);}}
button{{position:absolute;top:120px;width:100px;height:60px;}}
#a{{left:50px}} #b{{left:{second_x}px}}</style>
<button id=a>CHƠI NHANH</button><button id=b>TẠO PHÒNG</button>
<script>
window.__hitmeState={{screen:'home',roster:[],weapons:[],match:null}};
window.__hitmeUiDump=()=>[
{{name:'CHƠI NHANH',text:'CHƠI NHANH',visible:true,interactable:true,rect:{{x:50,y:120,w:100,h:60}}}},
{{name:'TẠO PHÒNG',text:'TẠO PHÒNG',visible:true,interactable:true,rect:{{x:{second_x},y:120,w:100,h:60}}}}];
</script>""", encoding="utf-8")


class BlackboxTests(unittest.TestCase):
    def test_fake_home_passes_layout_and_coverage(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            fake_page(root / "page.html")
            result = probe((root / "page.html").as_uri(), root / "evidence")
            self.assertEqual(result["T1"]["status"], "PASS")
            self.assertEqual(result["T6"]["status"], "PASS")

    def test_overlapping_buttons_fail(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            fake_page(root / "page.html", overlap=True)
            result = probe((root / "page.html").as_uri(), root / "evidence")
            self.assertEqual(result["T1"]["status"], "FAIL")

    def test_uniform_screen_is_not_visual_pass(self):
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "blank.png"
            Image.new("RGB", (1179, 2556), "#26170e").save(path)
            self.assertTrue(image_content(path)["blank_or_uniform"])

    def test_loading_page_is_never_visual_pass(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "page.html").write_text("<div id='unity-loading'>Downloading assets... 0%</div>", encoding="utf-8")
            result = probe((root / "page.html").as_uri(), root / "evidence", load_timeout_ms=100)
            self.assertEqual(result["T6"]["status"], "SKIP")

    def test_golden_requires_reference(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            actual = root / "actual.png"
            Image.new("RGB", (1179, 2556), "blue").save(actual)
            self.assertEqual(compare(actual, root / "missing.png", root / "out")["status"], "NO_GOLDEN")

    def test_golden_detects_change(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            golden, actual = root / "golden.png", root / "actual.png"
            a = Image.new("RGB", (300, 400), "#394eea")
            ImageDraw.Draw(a).rectangle((0, 0, 100, 100), fill="red")
            a.save(golden)
            a.save(actual)
            self.assertEqual(compare(actual, golden, root / "same")["status"], "PASS")
            Image.new("RGB", (300, 400), "white").save(actual)
            self.assertEqual(compare(actual, golden, root / "changed")["status"], "FAIL")


if __name__ == "__main__":
    unittest.main()
