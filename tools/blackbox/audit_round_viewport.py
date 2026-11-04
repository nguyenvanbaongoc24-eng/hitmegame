"""Measure the shipped Unity WebGL UI in browser-sized viewports without changing it.

README: python tools/blackbox/audit_round_viewport.py --url https://hitmegame-iota.vercel.app/?e2e=1
Evidence includes screenshots, Unity bridge dumps, and sampled color statistics.
Unknown scene-dependent defects remain UNKNOWN; a screenshot alone never proves a fix.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
PROFILES = Path(__file__).with_name("profiles.json")
DEFAULT_OUT = ROOT / "Evidence" / "blackbox" / "20261104_round_viewport_live"


def colored_fraction(image: Image.Image, box: dict, dpr: float) -> dict:
    left = max(0, round(box["x"] * dpr))
    top = max(0, round(box["y"] * dpr))
    right = min(image.width, round((box["x"] + box["w"]) * dpr))
    bottom = min(image.height, round((box["y"] + box["h"]) * dpr))
    if right <= left or bottom <= top:
        return {"valid": False}
    pixels = np.asarray(image.crop((left, top, right, bottom)).convert("RGB"), dtype=np.float32)
    maximum = pixels.max(axis=2)
    minimum = pixels.min(axis=2)
    saturation = np.where(maximum > 0, (maximum - minimum) / np.maximum(maximum, 1), 0)
    bright_neutral = (minimum > 205) & (saturation < .18)
    colored = (maximum > 70) & (saturation > .35)
    yellow = (pixels[:, :, 0] > 150) & (pixels[:, :, 1] > 100) & (pixels[:, :, 2] < 130)
    blue = (pixels[:, :, 2] > pixels[:, :, 0] * 1.2) & (pixels[:, :, 2] > pixels[:, :, 1] * 1.05)
    green = (pixels[:, :, 1] > pixels[:, :, 0] * 1.2) & (pixels[:, :, 1] > pixels[:, :, 2] * 1.05)
    return {"valid": True, "brightNeutralFraction": round(float(bright_neutral.mean()), 3),
            "coloredFraction": round(float(colored.mean()), 3),
            "yellowFraction": round(float(yellow.mean()), 3),
            "blueFraction": round(float(blue.mean()), 3),
            "greenFraction": round(float(green.mean()), 3),
            "meanRgb": [round(float(v), 1) for v in pixels.mean(axis=(0, 1))]}


def rect_for(dump: list, name: str) -> dict | None:
    for item in dump:
        if item.get("name") == name:
            return item.get("rect")
    return None


def match_edge_metrics(image: Image.Image) -> dict:
    pixels = np.asarray(image.convert("RGB"), dtype=np.float32)
    height, width = pixels.shape[:2]
    left = pixels[int(.37 * height):int(.72 * height), :max(4, int(.03 * width))]
    right = pixels[int(.37 * height):int(.72 * height), width - max(4, int(.03 * width)):]
    bottom = pixels[int(.94 * height):height - 4, int(.10 * width):int(.28 * width)]
    horizontal = float(np.abs(np.diff(bottom, axis=1)).mean())
    vertical = float(np.abs(np.diff(bottom, axis=0)).mean())
    return {"leftMean": round(float(left.mean()), 2), "leftStd": round(float(left.std()), 2),
            "rightMean": round(float(right.mean()), 2), "rightStd": round(float(right.std()), 2),
            "bottomHorizontalGradient": round(horizontal, 2),
            "bottomVerticalGradient": round(vertical, 2)}


def overlap_fraction(a: dict, b: dict) -> float:
    width = max(0.0, min(a["x"] + a["w"], b["x"] + b["w"]) - max(a["x"], b["x"]))
    height = max(0.0, min(a["y"] + a["h"], b["y"] + b["h"]) - max(a["y"], b["y"]))
    return width * height / max(1.0, min(a["w"] * a["h"], b["w"] * b["h"]))


def run(url: str, output: Path, selected_profile: str | None = None) -> dict:
    profiles = json.loads(PROFILES.read_text(encoding="utf-8"))
    if selected_profile:
        profiles = {selected_profile: profiles[selected_profile]}
    output.mkdir(parents=True, exist_ok=True)
    result = {}
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        for name, profile in profiles.items():
            context = browser.new_context(viewport={"width": profile["width"], "height": profile["height"]},
                                          device_scale_factor=profile["dpr"], is_mobile=True, has_touch=True)
            page = context.new_page()
            errors = []
            page.on("pageerror", lambda error: errors.append(str(error)))
            row = {"profile": profile, "url": url, "status": "UNKNOWN"}
            try:
                page.goto(url, wait_until="domcontentloaded", timeout=45000)
                page.wait_for_function("""() => {
                  const loading=document.getElementById('unity-loading');
                  const error=document.getElementById('webgl-error-modal');
                  return (!loading || getComputedStyle(loading).display==='none') &&
                         (!error || getComputedStyle(error).display==='none');
                }""", timeout=90000)
                page.wait_for_timeout(1500)
                path = output / f"{name}_home.png"
                page.screenshot(path=str(path), scale="device")
                image = Image.open(path)
                dump = page.evaluate("() => typeof __hitmeUiDump === 'function' ? __hitmeUiDump() : null")
                state = page.evaluate("() => window.__hitmeState || null")
                row["bridge"] = "available" if isinstance(dump, list) and isinstance(state, dict) else "missing"
                row["visualViewport"] = page.evaluate("""() => ({innerWidth,innerHeight,
                    width:visualViewport?.width,height:visualViewport?.height,
                    offsetTop:visualViewport?.offsetTop,dpr:devicePixelRatio})""")
                row["dump"] = dump
                row["state"] = state
                if isinstance(dump, list):
                    row["samples"] = {}
                    for item_name in ("BtnQuickMatch", "BtnCreateRoom", "BtnJoinRoom", "BtnBotTraining",
                                      "HomeLogo", "CharCard", "WeaponCard", "BtnHomeSettings", "BtnHomeProfileHeader"):
                        box = rect_for(dump, item_name)
                        if box:
                            row["samples"][item_name] = {"rect": box, **colored_fraction(image, box, profile["dpr"])}
                    clipped = [item["name"] for item in dump if item.get("visible") and item.get("rect") and
                               (item["rect"]["x"] < -1 or item["rect"]["y"] < -1 or
                                item["rect"]["x"] + item["rect"]["w"] > profile["width"] + 1 or
                                item["rect"]["y"] + item["rect"]["h"] > profile["height"] + 1)]
                    row["outOfViewport"] = clipped
                    samples = row["samples"]
                    findings = []
                    profile_header = samples.get("BtnHomeProfileHeader")
                    if profile_header:
                        header = profile_header["rect"]
                        avatar = {"x": header["x"] + header["h"] * .3,
                                  "y": header["y"] + header["h"] * .18,
                                  "w": header["h"] * .66, "h": header["h"] * .66}
                        row["avatarSample"] = colored_fraction(image, avatar, profile["dpr"])
                        if (row["avatarSample"]["brightNeutralFraction"] > .3 and
                                row["avatarSample"]["coloredFraction"] < .12):
                            findings.append("avatar_monochrome_placeholder")
                    quick = samples.get("BtnQuickMatch")
                    if quick and quick["brightNeutralFraction"] > .35:
                        findings.append("white_primary_button")
                    for button, color in (("BtnCreateRoom", "blue"), ("BtnJoinRoom", "green"),
                                          ("BtnBotTraining", "blue")):
                        sample = samples.get(button)
                        if sample and sample[f"{color}Fraction"] < .12:
                            findings.append(f"{button}_missing_{color}_fill")
                    for button in ("BtnQuickMatch", "BtnCreateRoom", "BtnJoinRoom", "BtnBotTraining"):
                        sample = samples.get(button)
                        if sample and sample["rect"]["h"] < (44 if button != "BtnQuickMatch" else 56):
                            findings.append(f"{button}_undersized")
                    # The shipped Unity logo has no bridge rect. Inspect its central word band.
                    logo_region = {"x": profile["width"] * .28, "y": profile["height"] * .165,
                                   "w": profile["width"] * .44, "h": profile["height"] * .072}
                    row["logoSample"] = colored_fraction(image, logo_region, profile["dpr"])
                    if row["logoSample"]["brightNeutralFraction"] > .4:
                        findings.append("logo_ghost_white_letters")
                    row["findings"] = findings
                    if name == "iphone15pro_safari":
                        bot = rect_for(dump, "BtnBotTraining")
                        if bot:
                            page.touchscreen.tap(bot["x"] + bot["w"] / 2, bot["y"] + bot["h"] / 2)
                            page.wait_for_timeout(900)
                            next_dump = page.evaluate("() => typeof __hitmeUiDump === 'function' ? __hitmeUiDump() : null")
                            next_state = page.evaluate("() => window.__hitmeState || null")
                            select_path = output / "iphone15pro_safari_after_bot_tap.png"
                            page.screenshot(path=str(select_path), scale="device")
                            row["afterBotTap"] = {"state": next_state, "dump": next_dump}
                            if isinstance(next_dump, list) and next_state and next_state.get("screen") == "select":
                                back, start = rect_for(next_dump, "BtnBack"), rect_for(next_dump, "BtnStartBot")
                                if back and back["y"] < 0:
                                    findings.append("bot_back_clipped_top")
                                if start and start["y"] + start["h"] > profile["height"]:
                                    findings.append("bot_start_clipped_bottom")
                                select_image = Image.open(select_path)
                                for card_name in ("CharCard_0", "CharCard_4"):
                                    card = rect_for(next_dump, card_name)
                                    if card:
                                        icon_box = {"x": card["x"] + 2, "y": card["y"] + 2,
                                                    "w": card["h"] * .73, "h": card["h"] - 4}
                                        sample = colored_fraction(select_image, icon_box, profile["dpr"])
                                        row.setdefault("selectCardSamples", {})[card_name] = sample
                                        if sample["brightNeutralFraction"] > .45:
                                            findings.append(f"{card_name}_white_placeholder")
                                if start and start["y"] < profile["height"] - 3:
                                    # Tap the small portion that remains visible to a real user.
                                    page.touchscreen.tap(start["x"] + start["w"] / 2,
                                                         min(profile["height"] - 3, start["y"] + 5))
                                    page.wait_for_timeout(1200)
                                    row["afterStartTap"] = page.evaluate("() => window.__hitmeState || null")
                                    row["afterStartTapDump"] = page.evaluate("() => typeof __hitmeUiDump === 'function' ? __hitmeUiDump() : null")
                                    page.screenshot(path=str(output / "iphone15pro_safari_after_start_tap.png"), scale="device")
                                    skip = rect_for(row["afterStartTapDump"] or [], "BtnSkip")
                                    if skip:
                                        page.touchscreen.tap(skip["x"] + skip["w"] / 2, skip["y"] + skip["h"] / 2)
                                        page.wait_for_timeout(400)
                                        match_path = output / "iphone15pro_safari_match_placing.png"
                                        page.screenshot(path=str(match_path), scale="device")
                                        match_image = Image.open(match_path)
                                        match_dump = page.evaluate("() => typeof __hitmeUiDump === 'function' ? __hitmeUiDump() : null")
                                        row["afterTutorialSkip"] = {"state": page.evaluate("() => window.__hitmeState || null"),
                                                                    "dump": match_dump}
                                        edge = match_edge_metrics(match_image)
                                        row["matchEdgeMetrics"] = edge
                                        if edge["leftMean"] < 55 and edge["leftStd"] < 18 and edge["rightMean"] < 55 and edge["rightStd"] < 18:
                                            findings.append("arena_dark_side_bands")
                                        if edge["bottomVerticalGradient"] < .6 and edge["bottomHorizontalGradient"] > 1.2:
                                            findings.append("arena_stretched_bottom_streaks")
                                        emote = rect_for(match_dump or [], "BtnInGameEmote")
                                        if emote:
                                            sample = colored_fraction(match_image, emote, profile["dpr"])
                                            row["matchEmoteSample"] = sample
                                            center = {"x": emote["x"] + emote["w"] * .25,
                                                      "y": emote["y"] + emote["h"] * .25,
                                                      "w": emote["w"] * .5, "h": emote["h"] * .5}
                                            row["matchEmoteCenterSample"] = colored_fraction(match_image, center, profile["dpr"])
                                            if row["matchEmoteCenterSample"]["brightNeutralFraction"] > .55:
                                                findings.append("match_emote_white_placeholder")
                                        for _ in range(20):
                                            page.wait_for_timeout(500)
                                            phase_state = page.evaluate("() => window.__hitmeState || null")
                                            phase = (phase_state or {}).get("match", {}).get("phase")
                                            if phase not in ("reveal", "throwing"):
                                                continue
                                            row["firstPublicPhase"] = phase_state
                                            page.screenshot(path=str(output / f"iphone15pro_safari_match_{phase}.png"), scale="device")
                                            alive = [player for player in phase_state["match"]["players"] if player.get("alive") and isinstance(player.get("rect"), dict)]
                                            collisions = []
                                            for i, first in enumerate(alive):
                                                for second in alive[i + 1:]:
                                                    ratio = overlap_fraction(first["rect"], second["rect"])
                                                    if ratio > .2:
                                                        collisions.append({"first": first["id"], "second": second["id"], "fraction": round(ratio, 3)})
                                            row["visualSpriteOverlaps"] = collisions
                                            if collisions:
                                                findings.append("revealed_characters_overlap")
                                            break
                row["status"] = "MEASURED"
            except Exception as error:
                row["error"] = str(error)
            row["pageErrors"] = errors
            result[name] = row
            context.close()
        browser.close()
    (output / "audit.json").write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    for name, row in result.items():
        print(name, row["status"], "bridge=" + row.get("bridge", "?"),
              "findings=" + str(row.get("findings", [])), "error=" + row.get("error", ""))
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="https://hitmegame-iota.vercel.app/?e2e=1")
    parser.add_argument("--output", type=Path, default=DEFAULT_OUT)
    parser.add_argument("--profile", choices=json.loads(PROFILES.read_text(encoding="utf-8")).keys())
    args = parser.parse_args()
    result = run(args.url, args.output, args.profile)
    if not all(row["status"] == "MEASURED" for row in result.values()):
        raise SystemExit(1)
    raise SystemExit(2 if any(row.get("findings") for row in result.values()) else 0)
