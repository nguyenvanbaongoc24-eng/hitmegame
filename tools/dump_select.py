from playwright.sync_api import sync_playwright
import time
import sys
sys.stdout.reconfigure(encoding='utf-8')

with sync_playwright() as p:
    browser = p.webkit.launch(headless=True)
    page = browser.new_page(viewport={'width': 393, 'height': 852}, device_scale_factor=3, has_touch=True)
    page.goto('http://127.0.0.1:8104/?e2e=1')
    page.wait_for_timeout(6000)
    page.touchscreen.tap(196.6, 750.1)
    time.sleep(1.5)
    
    uidump = page.evaluate('() => window.__hitmeUiDump ? window.__hitmeUiDump() : []')
    print('Select screen UI elements:', len(uidump))
    for el in uidump:
        r = el.get('rect', {})
        sx = 393.0 / 983.0
        sy = 852.0 / 2130.0
        cx = (r.get('x', 0) + r.get('w', 0) / 2) * sx
        cy = (r.get('y', 0) + r.get('h', 0) / 2) * sy
        print(f"{el.get('name')}: text='{el.get('text')}' center=({cx:.1f}, {cy:.1f})")
    browser.close()
