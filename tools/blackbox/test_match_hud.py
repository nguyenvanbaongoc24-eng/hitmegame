"""Inspect W2 match HUD with a synthetic Unity bridge, not a gameplay certification.

Run: python tools/blackbox/test_match_hud.py --root "D:/HITME GAME/worktrees/web-dist-mobile"
"""
import argparse
import json
import threading
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from playwright.sync_api import sync_playwright


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', required=True, type=Path)
    parser.add_argument('--output', type=Path, default=Path('Evidence/mobile/20261106/match_hud'))
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)

    class Handler(SimpleHTTPRequestHandler):
        def __init__(self, *a, **kw):
            super().__init__(*a, directory=str(args.root), **kw)

        def log_message(self, *_):
            pass

    server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    results = {}
    try:
        with sync_playwright() as playwright:
            for name, height in [('iphone15pro_safari', 695), ('iphone15pro_standalone', 852)]:
                browser = playwright.webkit.launch(headless=True)
                context = browser.new_context(viewport={'width': 393, 'height': height},
                                              device_scale_factor=3, is_mobile=True, has_touch=True)
                page = context.new_page()
                page.route('**/*.loader.js*', lambda route: route.fulfill(status=200, content_type='text/javascript', body='window.createUnityInstance=()=>new Promise(()=>{});'))
                page.add_init_script("window.__sent=[];window.unityInstance={SendMessage:(o,m,p)=>window.__sent.push({m,p:JSON.parse(p)})}")
                errors = []
                page.on('pageerror', lambda error: errors.append(str(error)))
                page.goto(f'http://127.0.0.1:{server.server_port}/next/?e2e=1', wait_until='networkidle')
                page.evaluate("document.querySelector('#unity-loading')?.remove()")
                page.evaluate("hitmeUi.onUnity({type:'ready',version:'1.2',roster:[],weapons:[]})")
                page.evaluate("hitmeUi.onUnity({type:'screen',name:'match'})")
                page.evaluate("hitmeUi.onUnity({type:'match',round:2,maxRounds:5,phase:'placing',timeLeft:4.2,weaponName:'Dép Tổ Ong',players:[{id:'player',name:'Bạn',characterId:'nam_lang',hp:3,maxHp:3,alive:true,isLocal:true},{id:'bot1',name:'Bot 1',characterId:'co_ba',hp:2,maxHp:3,alive:true}]})")
                if page.locator('[data-action="tutorial-skip"]').count():
                    page.locator('[data-action="tutorial-skip"]').click()
                page.screenshot(path=str(args.output / f'{name}.png'))
                checks = {}
                checks['HUD-12_one_settings'] = page.locator('.live-match [data-action="settings"]').count() == 1
                checks['HUD-13_one_phase'] = page.locator('.live-match .live-phase').count() == 1
                checks['HUD-14_svg_emote'] = page.locator('.live-emote-trigger svg').count() == 1 and '☺' not in page.locator('.live-emote-trigger').inner_text()
                checks['HUD-15_countdown_progress'] = ('5 GIÂY' in page.locator('.live-summary').inner_text() and page.locator('.live-progress i').count() == 5)
                checks['HUD-16_ready_button'] = page.locator('.live-ready').is_visible()
                page.locator('.live-ready').click()
                checks['HUD-16_ready_sends'] = any(x['m'] == 'Ready' for x in page.evaluate('window.__sent'))
                page.evaluate("hitmeUi.onUnity({type:'event',kind:'throw'})")
                page.evaluate("hitmeUi.onUnity({type:'match',round:2,maxRounds:5,phase:'round_result',timeLeft:0,players:[{id:'player',name:'Bạn',characterId:'nam_lang',hp:3,maxHp:3,alive:true,isLocal:true}]})")
                page.evaluate("hitmeUi.onUnity({type:'event',kind:'round_result'})")
                checks['HUD-13_no_stale_phase_toast'] = ('Đang ném' not in page.locator('.live-top').inner_text()
                                                    and page.locator('.live-phase').count() == 1)
                page.locator('.live-emote-trigger').click()
                checks['EMO-07_two_tabs'] = page.locator('.live-emote-tabs button').count() == 2
                page.locator('[data-action="emote-tab"][data-tab="phrase"]').click()
                checks['EMO-07_six_phrases'] = page.locator('.live-emotes.phrase button').count() == 6
                page.locator('.live-emotes.phrase button').first.click()
                checks['EMO-07_card_bubble'] = page.locator('.live-card[data-player="player"] .live-card-bubble').is_visible()
                checks['EMO-07_bridge_send'] = any(x['m'] == 'SendEmote' for x in page.evaluate('window.__sent'))
                checks['no_js_error'] = not errors
                results[name] = {'checks': checks, 'errors': errors}
                context.close()
                browser.close()
    finally:
        server.shutdown()
    (args.output / 'results.json').write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding='utf-8')
    passed = sum(all(v['checks'].values()) for v in results.values())
    print(f'Passed! {passed}/{len(results)}')
    for name, row in results.items():
        failed = [key for key, ok in row['checks'].items() if not ok]
        if failed:
            print(name, failed, row['errors'])
    return 0 if passed == len(results) else 1


if __name__ == '__main__':
    raise SystemExit(main())
