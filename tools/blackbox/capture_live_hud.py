"""Capture real Unity WebGL HUD on WebKit phone profiles, plus a short video.

Run: python tools/blackbox/capture_live_hud.py --url http://127.0.0.1:8093/next/
This is browser emulation evidence, not a physical iPhone test.
"""
import argparse
import json
import shutil
from pathlib import Path

from playwright.sync_api import sync_playwright


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--url', required=True)
    parser.add_argument('--output', type=Path, default=Path('Evidence/mobile/20261106/live_hud'))
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    rows = {}
    with sync_playwright() as playwright:
        for name, height in (('iphone15pro_safari', 695), ('iphone15pro_standalone', 852)):
            browser = playwright.webkit.launch(headless=True)
            context = browser.new_context(
                viewport={'width': 393, 'height': height}, device_scale_factor=3,
                is_mobile=True, has_touch=True, record_video_dir=str(args.output),
                record_video_size={'width': 393, 'height': height},
            )
            page = context.new_page()
            errors = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            try:
                page.goto(args.url.rstrip('/') + '/?e2e=1', wait_until='domcontentloaded', timeout=45000)
                page.wait_for_selector('[data-screen="home"]', timeout=90000)
                page.locator('[data-action="practice"]').click()
                page.locator('[data-action="start-practice"]').click()
                page.wait_for_function("() => window.__hitmeState?.screen === 'match'", timeout=15000)
                if page.locator('[data-action="tutorial-skip"]').count():
                    page.locator('[data-action="tutorial-skip"]').click()
                page.wait_for_timeout(900)
                page.screenshot(path=str(args.output / f'{name}_match.png'))
                rows[name] = {
                    'screen': page.evaluate('window.__hitmeState.screen'),
                    'unityReady': page.evaluate('!!window.unityInstance?.SendMessage'),
                    'hudCards': page.locator('.live-card').count(),
                    'settingsButtons': page.locator('.live-match [data-action="settings"]').count(),
                    'errors': errors,
                }
                page.wait_for_timeout(6000)
            finally:
                video_path = page.video.path()
                context.close()
                browser.close()
                shutil.move(video_path, args.output / f'{name}_match.webm')
    (args.output / 'results.json').write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding='utf-8')
    passed = sum(row['screen'] == 'match' and row['unityReady'] and row['hudCards'] >= 2
                 and row['settingsButtons'] == 1 and not row['errors'] for row in rows.values())
    print(f'Passed! {passed}/{len(rows)}')
    return 0 if passed == len(rows) else 1


if __name__ == '__main__':
    raise SystemExit(main())
