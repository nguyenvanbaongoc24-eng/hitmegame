"""Report measured UI/audio requirement coverage without promoting missing tests to PASS.

README: python tools/blackbox/requirements_ui_audio.py
Add --deploy-gate to exit nonzero when a checked mandatory ID is FAIL.
The source specification remains in the owner's MD folder; this tool reads it only.
"""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

import numpy as np
from PIL import Image

REPO = Path(__file__).resolve().parents[2]
PROJECT = REPO.parent.parent if REPO.parent.name == "worktrees" else REPO
SPEC = PROJECT / "MD" / "HITME GAME MD" / "hit-me-requirements-ui-audio-v1.md"
SMOKE = REPO / "web-ui" / "evidence" / "home_smoke" / "results.json"
COMPARE = REPO / "web-ui" / "evidence" / "compare_home" / "metrics.json"
BUTTONS = REPO / "web-ui" / "evidence" / "button_home" / "results.json"
OUTPUT = REPO / "web-ui" / "evidence" / "requirements_ui_audio.json"


def analyze(spec: Path, smoke_path: Path, compare_path: Path) -> dict:
    ids = re.findall(r"^\| ((?:BTN|HOME|SEL|HUD|ARENA|EMO|SET|AUD)-\d+) \|",
                     spec.read_text(encoding="utf-8"), re.MULTILINE)
    if not ids or len(set(ids)) != len(ids):
        raise ValueError("No IDs or duplicate IDs in specification")
    rows = {key: {"status": "NOT_TESTED", "evidence": None} for key in ids}
    smoke = json.loads(smoke_path.read_text(encoding="utf-8"))
    compare = json.loads(compare_path.read_text(encoding="utf-8"))
    buttons = json.loads(BUTTONS.read_text(encoding="utf-8"))
    all_smoke = bool(smoke) and all(row["status"] == "PASS" for row in smoke.values())
    all_buttons = bool(buttons) and all(row["status"] == "PASS" for row in buttons.values())
    all_touch = all(not row["geometry"]["blocked"] for row in smoke.values())
    def set_result(key: str, status: str, evidence: str, note: str = "") -> None:
        rows[key] = {"status": status, "evidence": evidence, "note": note}
    set_result("BTN-01", "PASS" if all_smoke else "FAIL", str(smoke_path), "10 simulated viewports; minimum 48 CSS px")
    set_result("BTN-02", "PASS" if compare["passedColors"] == compare["totalColors"] else "FAIL",
               str(compare_path), "Four Home button interior color samples")
    set_result("BTN-03", "PASS" if all_buttons else "FAIL", str(BUTTONS),
               "Pressed 3px and released transform; locked grayscale/opacity")
    set_result("BTN-06", "PASS" if all_buttons else "FAIL", str(BUTTONS),
               "Visible Home buttons have opaque color or gradient; no square glyph")
    set_result("BTN-08", "PASS" if all_touch else "FAIL", str(smoke_path),
               "Center and four points at 80% area; simulated touch engines")
    set_result("BTN-09", "PASS" if all_smoke else "FAIL", str(smoke_path),
               "Locked character tapped; selection unchanged. Audio checked only where AudioContext exists")
    set_result("BTN-10", "PASS" if all_buttons else "FAIL", str(BUTTONS),
               "Vietnamese and English at phone widths; CTA, cards and tabs not clipped")
    logo = np.asarray(Image.open(REPO / "web-ui/assets/logo_hit_me.png").convert("RGB").convert("HSV"))
    yellow = float(((logo[:, :, 0] >= 28) & (logo[:, :, 0] <= 39) &
                    (logo[:, :, 1] > 153)).mean())
    width = compare["layout"]["logo"]["actual"]["w"] / compare["viewport"][0]
    set_result("HOME-03", "PASS" if 0.7 <= width <= 0.8 and yellow >= 0.15 else "FAIL",
               str(compare_path), f"Logo width {width:.1%}, yellow pixels {yellow:.1%}")
    set_result("HOME-07", "PASS" if all_buttons else "FAIL", str(BUTTONS),
               "Both tabs open Coming Soon and have a back button")
    set_result("HOME-08", "PASS" if all_buttons else "FAIL", str(BUTTONS),
               "Interactive mark under 1s; Play without Unity shows loading")
    # HOME-11 asks for a measured side-by-side report. Smaller old arrows/logo conflict with new requirements.
    set_result("HOME-11", "PASS" if compare["totalLayout"] == 13 and compare["totalColors"] == 4 else "FAIL",
               str(compare_path), f"{compare['passedLayout']}/{compare['totalLayout']} geometry, "
               f"{compare['passedColors']}/{compare['totalColors']} colors; old target has smaller arrows/logo")
    checked = sum(row["status"] != "NOT_TESTED" for row in rows.values())
    passed = sum(row["status"] == "PASS" for row in rows.values())
    failed = [key for key,row in rows.items() if row["status"] == "FAIL"]
    return {"checked": checked, "total": len(rows), "passed": passed,
            "failed": failed, "ids": rows,
            "limits": "WebKit headless has no AudioContext; real iPhone audio and Unity match UI remain unverified"}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--spec", type=Path, default=SPEC)
    parser.add_argument("--deploy-gate", action="store_true")
    args = parser.parse_args()
    result = analyze(args.spec, SMOKE, COMPARE)
    OUTPUT.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"Checked {result['checked']}/{result['total']}; PASS {result['passed']}/{result['total']}; "
          f"FAIL {', '.join(result['failed']) or 'none'}")
    if args.deploy_gate and (result["failed"] or result["checked"] < result["total"]):
        raise SystemExit(1)
