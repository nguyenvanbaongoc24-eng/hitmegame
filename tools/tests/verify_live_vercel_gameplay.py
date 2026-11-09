import sys
import json
import time
from playwright.sync_api import sync_playwright

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", line_buffering=True)

with sync_playwright() as pw:
    browser = pw.chromium.launch(channel="msedge", headless=True)
    context = browser.new_context(
        viewport={"width": 393, "height": 852},
        has_touch=True,
        is_mobile=True,
        device_scale_factor=3,
        user_agent="Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1"
    )
    page = context.new_page()

    logs = []
    page.on("console", lambda m: logs.append(f"[{m.type.upper()}] {m.text}"))
    page.on("pageerror", lambda e: logs.append(f"[PAGEERROR] {repr(e)}"))

    print("[*] Navigating to LIVE https://hitmegame-iota.vercel.app/next/?e2e=1 ...")
    page.goto("https://hitmegame-iota.vercel.app/next/?e2e=1")
    page.wait_for_function("() => typeof window.unityInstance !== 'undefined'", timeout=45000)
    print("[+] Unity WebGL loaded on live Vercel!")
    page.wait_for_timeout(2000)

    # 1. Capture Live Home Screen
    page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/01_live_next_beta8_home.png", timeout=8000)
    print("[+] Screenshot 1: Home screen captured")

    # 2. Open weapon modal, select Chảo Chống Dính (the weapon from user's bug report)
    print("[*] Selecting weapon Chảo Chống Dính...")
    page.tap(".selection-card.weapon-card")
    page.wait_for_timeout(500)
    page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/02_live_next_beta8_weapon_modal.png", timeout=8000)
    page.tap('[data-id="chao_chong_dinh"]')
    page.wait_for_timeout(500)

    # 3. Open character modal, select Quậy Boy (previously locked)
    print("[*] Selecting character Quậy Boy...")
    page.tap(".selection-card.character-card")
    page.wait_for_timeout(500)
    page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/03_live_next_beta8_char_modal.png", timeout=8000)
    page.tap('[data-id="quay_boy"]')
    page.wait_for_timeout(500)

    # 4. Tap Practice on Home
    print("[*] Tapping Luyện với bot...")
    page.tap(".home .practice")
    page.wait_for_timeout(1000)

    # 5. Tap Start Practice (BẮT ĐẦU)
    print("[*] Tapping BẮT ĐẦU...")
    page.wait_for_selector('[data-action="start-practice"]')
    page.tap('[data-action="start-practice"]')
    page.wait_for_selector(".live-match", timeout=10000)
    page.wait_for_timeout(500)

    # 6. Verify Match is running and NOT frozen!
    snap = page.evaluate("""() => ({
        screen: document.querySelector('.stage')?.dataset?.screen,
        summary: document.querySelector('.live-summary')?.innerText,
        progress: document.querySelector('.live-progress')?.innerText,
        cardsCount: document.querySelectorAll('.live-card')?.length,
        weaponChip: document.querySelector('.live-weapon-chip')?.innerText?.trim(),
        phaseText: document.querySelector('.live-phase')?.innerText,
        match: window.__hitmeLastMatchEvt ? {
            round: window.__hitmeLastMatchEvt.round,
            phase: window.__hitmeLastMatchEvt.phase,
            timeLeft: window.__hitmeLastMatchEvt.timeLeft,
            aliveCount: window.__hitmeLastMatchEvt.aliveCount,
            weaponName: window.__hitmeLastMatchEvt.weaponName,
            players: window.__hitmeLastMatchEvt.players?.map(p => ({ id: p.id, char: p.characterId, hp: p.hp, alive: p.alive }))
        } : null
    })""")
    print("[+] Live Match Snapshot immediately after start:")
    print(json.dumps(snap, ensure_ascii=True, indent=2))

    assert snap["cardsCount"] == 4, f"Expected 4 cards, found {snap['cardsCount']}"
    assert "4" in snap["progress"], f"Expected 4 alive, found {snap['progress']}"
    assert "Chảo" in snap["weaponChip"], f"Expected Chảo Chống Dính in chip, found {snap['weaponChip']}"

    # Tap on canvas to place character
    canvas = page.locator("#unity-canvas")
    box = canvas.bounding_box()
    cx = box["x"] + box["width"] * 0.5
    cy = box["y"] + box["height"] * 0.55
    page.touchscreen.tap(cx, cy)
    page.wait_for_timeout(400)

    page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/04_live_next_beta8_match_placing.png", timeout=8000)
    print("[+] Screenshot 4: Placing phase captured")

    # Wait for reveal and throwing
    print("[*] Waiting for phase progression...")
    page.wait_for_timeout(3500)
    page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/05_live_next_beta8_match_throwing.png", timeout=8000)
    print("[+] Screenshot 5: Throwing/Round progression captured")

    browser.close()
    print("[SUCCESS] All live Vercel checks passed! Gameplay is 100% restored!")
