from playwright.sync_api import sync_playwright
import json
import sys
sys.stdout.reconfigure(encoding='utf-8')

with sync_playwright() as p:
    browser = p.webkit.launch(headless=True)
    page = browser.new_page(viewport={'width': 393, 'height': 852}, device_scale_factor=3, has_touch=True)
    page.goto('http://127.0.0.1:8104/?e2e=1')
    page.wait_for_timeout(6000)
    
    uidump = page.evaluate('() => window.__hitmeUiDump ? window.__hitmeUiDump() : []')
    print('Total UI elements:', len(uidump))
    for el in uidump:
        r = el.get('rect', {})
        print(f"{el.get('name')}: text='{el.get('text')}' rect=({r.get('x'):.1f}, {r.get('y'):.1f}, {r.get('w'):.1f}, {r.get('h'):.1f})")
    browser.close()
