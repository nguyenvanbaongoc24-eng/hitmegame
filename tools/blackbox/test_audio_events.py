"""Exercise the Web audio event table in Chromium; save the real browser event log.

README: python tools/blackbox/test_audio_events.py
Headless WebKit has no AudioContext here; this is not a real iPhone listening test.
"""
from __future__ import annotations

import json
from pathlib import Path
from playwright.sync_api import sync_playwright

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "web-ui" / "evidence" / "audio_events.json"
IDS = [
    "ui_tap", "ui_confirm", "ui_back", "ui_denied", "ui_toggle", "ui_slider_tick",
    "ui_tab", "ui_sheet_open", "ui_sheet_close", "ui_toast", "ui_error",
    "ui_select_char", "ui_select_weapon", "ui_copy", "room_join", "room_leave",
    "round_start", "place_pop", "aim_tick", "ready_lock", "timer_tick", "timer_end",
    "reveal", "throw_dep", "throw_chao", "throw_vot", "hit_dep", "hit_chao",
    "hit_vot", "ko", "win", "lose", "round_result", "emo_pop", "chat_pop",
    "jingle_win", "jingle_lose"
]


def run() -> int:
    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(headless=True, channel="msedge")
        page = browser.new_page(viewport={"width":393,"height":852},is_mobile=True,has_touch=True)
        errors=[]
        page.on("pageerror",lambda error: errors.append(str(error)))
        page.goto("http://127.0.0.1:8765/?mock=1&e2e=1",wait_until="networkidle")
        page.locator('[data-action="profile"]').first.click()
        page.wait_for_timeout(700)
        state = page.evaluate("""async () => {
          const {audio}=await import('./audio.js'); return audio.context?.state;
        }""")
        if state != "running": errors.append(f"AudioContext {state}")
        for event_id in IDS:
            page.evaluate("id=>window.hitmeUi.onUnity({type:'sfx',id})",event_id)
            page.wait_for_timeout(110)
        page.evaluate("window.hitmeUi.onUnity({type:'music',id:'bgm_match',action:'play'})")
        page.wait_for_timeout(800)
        page.evaluate("window.hitmeUi.onUnity({type:'music',id:'bgm_match',action:'stop'})")
        log = page.evaluate("window.__hitmeAudioLog")
        missing = [event_id for event_id in IDS if not any(row["id"]==event_id for row in log)]
        if missing: errors.append(f"Missing IDs {missing}")
        if not any(row["id"]=="bgm_match" for row in log): errors.append("Missing match music")
        report={"status":"PASS" if not errors else "FAIL", "contextState":state,
                "eventsChecked":len(IDS), "missing":missing, "errors":errors,
                "fileEvents":sum(row.get("source")=="file" for row in log),"log":log,
                "limitation":"Headless Chromium only; iPhone Safari sound not verified"}
        OUT.write_text(json.dumps(report,indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
        print(f"Passed! {len(IDS)-len(missing)}/{len(IDS)}" if not errors else f"FAIL {errors}")
        browser.close()
        return 0 if not errors else 1


if __name__ == "__main__":
    raise SystemExit(run())
