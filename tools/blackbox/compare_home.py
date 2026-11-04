"""Measure the web Home against the original mockup without relaxing tolerances.

Usage: python tools/blackbox/compare_home.py --url http://127.0.0.1:8765/?mock=1
Outputs a 1179x2556 WebKit screenshot, normalized reference, side-by-side image,
annotated layout, and JSON metrics. A FAIL is evidence, not a reason to edit limits.
"""
from __future__ import annotations

import argparse
import json
import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
LAYOUT = ROOT / "web-ui" / "home_layout.json"
DEFAULT_REFERENCE = ROOT.parents[1] / "asset" / "HOME_MOCKUP_TARGET.png"


def rgb_to_lab(color):
    rgb = [v / 255 for v in color[:3]]
    rgb = [((v + .055) / 1.055) ** 2.4 if v > .04045 else v / 12.92 for v in rgb]
    x = (rgb[0] * .4124564 + rgb[1] * .3575761 + rgb[2] * .1804375) / .95047
    y = rgb[0] * .2126729 + rgb[1] * .7151522 + rgb[2] * .0721750
    z = (rgb[0] * .0193339 + rgb[1] * .1191920 + rgb[2] * .9503041) / 1.08883
    def f(v): return v ** (1 / 3) if v > .008856 else (7.787 * v + 16 / 116)
    return (116 * f(y) - 16, 500 * (f(x) - f(y)), 200 * (f(y) - f(z)))


def delta_e(a, b):
    aa, bb = rgb_to_lab(a), rgb_to_lab(b)
    return math.sqrt(sum((x-y)**2 for x, y in zip(aa, bb)))


def compare(url: str, reference_path: Path, output: Path, viewport=(393, 852)) -> dict:
    output.mkdir(parents=True, exist_ok=True)
    layout = json.loads(LAYOUT.read_text(encoding="utf-8"))
    width, height = viewport
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        context = browser.new_context(viewport={"width": width, "height": height},
                                      device_scale_factor=3, is_mobile=True, has_touch=True)
        page = context.new_page()
        page.goto(url, wait_until="networkidle", timeout=30000)
        page.wait_for_selector('[data-name="quick-play"]')
        page.evaluate("document.fonts.ready")
        page.screenshot(path=str(output / "home.png"), scale="device")
        rects = page.locator("[data-name]").evaluate_all("els => Object.fromEntries(els.map(el => {const r=el.getBoundingClientRect();return [el.dataset.name,{x:r.x,y:r.y,w:r.width,h:r.height}]}))")
        browser.close()
    source = Image.open(reference_path).convert("RGB")
    box = layout["referencePixels"]
    target = source.crop((box["x"], box["y"], box["x"]+box["w"], box["y"]+box["h"]))
    target = target.resize((width*3, height*3), Image.Resampling.LANCZOS)
    target.save(output / "mockup_normalized.png")
    actual = Image.open(output / "home.png").convert("RGB")
    annotated = target.copy()
    draw = ImageDraw.Draw(annotated)
    try: font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 25)
    except OSError: font = ImageFont.load_default()
    metrics = {}
    center_limit = layout["tolerance"]["centerHeightFraction"]
    size_limit = layout["tolerance"]["sizeFraction"]
    for name, norm in layout["rects"].items():
        expected = {key: value * factor for key, value, factor in zip(("x","y","w","h"), norm, (width,height,width,height))}
        got = rects.get(name)
        if got is None:
            metrics[name] = {"status":"FAIL", "reason":"missing element"}
            continue
        center_error = math.hypot((got["x"]+got["w"]/2 - expected["x"]-expected["w"]/2)/width,
                                  (got["y"]+got["h"]/2 - expected["y"]-expected["h"]/2)/height)
        vertical_center_error = abs(got["y"]+got["h"]/2-expected["y"]-expected["h"]/2)/height
        width_error = abs(got["w"]-expected["w"])/max(expected["w"],1)
        height_error = abs(got["h"]-expected["h"])/max(expected["h"],1)
        status = "PASS" if vertical_center_error <= center_limit and width_error <= size_limit and height_error <= size_limit else "FAIL"
        metrics[name] = {"status":status,"verticalCenterError":round(vertical_center_error,4),
                         "normalizedCenterError":round(center_error,4),"widthError":round(width_error,4),"heightError":round(height_error,4),"actual":got,"expected":expected}
        x,y,w,h=(round(expected[k]*3) for k in ("x","y","w","h"))
        draw.rectangle((x,y,x+w,y+h),outline="red" if status=="FAIL" else "#00ffff",width=4)
        draw.text((x+3,y+3),name,fill="white",stroke_width=2,stroke_fill="black",font=font)
    annotated.save(output / "mockup_annotated.png")
    samples = {}
    for name in ("quick-play","create-room","join-room","practice"):
        norm=layout["rects"][name]
        sx=round((norm[0]+norm[2]*.12)*width*3)
        sy=round((norm[1]+norm[3]*.75)*height*3)
        a=actual.getpixel((sx,sy)); b=target.getpixel((sx,sy))
        samples[name]={"actualRGB":a,"targetRGB":b,"deltaE76":round(delta_e(a,b),2),
                       "status":"PASS" if delta_e(a,b)<=layout["tolerance"]["buttonDeltaE"] else "FAIL"}
    combined=Image.new("RGB",(width*6,height*3));combined.paste(target,(0,0));combined.paste(actual,(width*3,0));combined.save(output/"side_by_side.png")
    result={"url":url,"viewport":viewport,"layout":metrics,"buttonColors":samples,
            "passedLayout":sum(v["status"]=="PASS" for v in metrics.values()),"totalLayout":len(metrics),
            "passedColors":sum(v["status"]=="PASS" for v in samples.values()),"totalColors":len(samples)}
    (output/"metrics.json").write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding="utf-8")
    return result


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--url',default='http://127.0.0.1:8765/?mock=1')
    parser.add_argument('--reference',type=Path,default=DEFAULT_REFERENCE)
    parser.add_argument('--output',type=Path,default=ROOT/'web-ui'/'evidence'/'compare_home')
    args=parser.parse_args()
    result=compare(args.url,args.reference,args.output)
    print(f"Layout {result['passedLayout']}/{result['totalLayout']}; button colors {result['passedColors']}/{result['totalColors']}; {args.output}")
    return 0 if result['passedLayout']==result['totalLayout'] and result['passedColors']==result['totalColors'] else 1


if __name__=='__main__': raise SystemExit(main())
