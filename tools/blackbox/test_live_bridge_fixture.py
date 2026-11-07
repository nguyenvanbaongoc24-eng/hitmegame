"""README: python tools/blackbox/test_live_bridge_fixture.py

Exercise the production UI event contract with a local bridge fixture. This is
not evidence that the deployed Unity build has passed end-to-end tests.
"""
from __future__ import annotations

import json
import threading
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlsplit

from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
DIST = ROOT / 'web-ui' / 'dist'
OUT = ROOT / 'web-ui' / 'evidence' / 'bridge_fixture'


class Handler(SimpleHTTPRequestHandler):
    def translate_path(self, path: str) -> str:
        requested = unquote(urlsplit(path).path)
        relative = requested.removeprefix('/next/')
        candidate = (DIST / (relative or 'index.html')).resolve()
        return str(candidate if requested.startswith('/next/') and candidate.is_relative_to(DIST) else DIST / '404')

    def log_message(self, *_args: object) -> None:
        pass


def run() -> int:
    OUT.mkdir(parents=True, exist_ok=True)
    server = ThreadingHTTPServer(('127.0.0.1', 0), partial(Handler, directory=str(DIST)))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    results = {}
    with sync_playwright() as playwright:
        for engine in ('webkit', 'chromium'):
            browser = getattr(playwright, engine).launch(headless=True, **({'channel': 'msedge'} if engine == 'chromium' else {}))
            context = browser.new_context(viewport={'width': 393, 'height': 695}, device_scale_factor=3, is_mobile=True, has_touch=True)
            page = context.new_page()
            page.add_init_script("window.__sent=[];window.unityInstance={SendMessage:(o,m,p)=>window.__sent.push({m,p:JSON.parse(p)})}")
            errors = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            page.goto(f'http://127.0.0.1:{server.server_port}/next/?e2e=1', wait_until='networkidle')
            checks = []
            def check(ok, label):
                if not ok: checks.append(label)
            check(page.locator('[data-screen="home"]').is_visible(), 'home visible')
            check(page.locator('[data-action="online-locked"]').count() == 3, 'online gated')
            page.wait_for_function("() => document.querySelector('.hero')?.naturalWidth > 0")
            check(page.locator('.hero').is_visible(), 'hero image visible')
            for button in ('.home .blue', '.home .green'):
                check(page.locator(button).evaluate('e => e.scrollWidth <= e.clientWidth'),
                      f'{button} label fits')
            page.evaluate("hitmeUi.onUnity({type:'ready',version:'1.2',roster:[],weapons:[]})")
            check(any(x['m']=='SetLayout' for x in page.evaluate('window.__sent')), 'SetLayout sent')
            page.locator('[data-action="practice"]').click()
            check(page.locator('[data-screen="bot"]').is_visible(), 'real bot picker')
            page.locator('[data-id="co_ba"]').click()
            page.locator('[data-action="start-practice"]').click()
            check(any(x['m']=='StartPractice' and x['p']['characterId']=='co_ba' for x in page.evaluate('window.__sent')), 'StartPractice uses selection')
            page.evaluate("hitmeUi.onUnity({type:'match',round:1,maxRounds:0,phase:'placing',timeLeft:4.2,aliveCount:2,weaponName:'Dép Tổ Ong',players:[{id:'player',name:'Bạn',characterId:'co_ba',slot:0,hp:3,maxHp:3,alive:true,isLocal:true},{id:'bot1',name:'Bot 1',characterId:'nam_lang',slot:1,hp:2,maxHp:3,alive:true,isLocal:false}]})")
            check(page.locator('.live-card').count() == 2, 'real player cards')
            check('5 GIÂY' in page.locator('.live-summary').inner_text(), 'real countdown')
            check('VÒNG 1' in page.locator('.live-summary').inner_text() and '/5' not in page.locator('.live-summary').inner_text(), 'v1.3 uncapped round label')
            check('2 người còn sống' in page.locator('.live-progress').inner_text(), 'v1.3 alive count')
            page.evaluate("hitmeUi.onUnity({type:'match',round:26,maxRounds:0,tiebreak:true,tiebreakCount:1,phase:'placing',timeLeft:4.2,aliveCount:2,weaponName:'Dép Tổ Ong',players:[{id:'player',name:'Bạn',characterId:'co_ba',slot:0,hp:1,maxHp:3,alive:true,isLocal:true},{id:'bot1',name:'Bot 1',characterId:'nam_lang',slot:1,hp:1,maxHp:3,alive:true,isLocal:false}]})")
            check('HIỆP PHỤ 1' in page.locator('.live-summary').inner_text(), 'v1.3 tiebreak label')
            page.evaluate("hitmeUi.onUnity({type:'event',kind:'hit',sourceId:'player',targetId:'bot1'})")
            page.evaluate("hitmeUi.onUnity({type:'event',kind:'ko',sourceId:'player',targetId:'bot1'})")
            check(page.locator('.live-round-events span').count() == 2, 'round event strip')
            check('Trúng đòn' in page.locator('.live-toast').inner_text(), 'toast queue starts at first event')
            check(page.locator('.live-tutorial').is_visible(), 'first time tutorial')
            page.evaluate("""() => {const c=document.querySelector('#unity-canvas');c.dispatchEvent(new PointerEvent('pointerdown',{pointerId:7,clientX:180,clientY:330,bubbles:true}));c.dispatchEvent(new PointerEvent('pointerup',{pointerId:7,clientX:180,clientY:330,bubbles:true}));}""")
            check('2/3' in page.locator('.live-tutorial').inner_text(), 'placing tap advances tutorial')
            page.evaluate("""() => {const c=document.querySelector('#unity-canvas');c.dispatchEvent(new PointerEvent('pointerdown',{pointerId:7,clientX:180,clientY:330,bubbles:true}));c.dispatchEvent(new PointerEvent('pointerup',{pointerId:7,clientX:220,clientY:330,bubbles:true}));}""")
            check('3/3' in page.locator('.live-tutorial').inner_text(), 'aim drag advances tutorial')
            page.locator('[data-action="tutorial-skip"]').click()
            page.screenshot(path=str(OUT / f'{engine}_match_fixture.png'))
            page.locator('.live-summary [data-action="settings"]').click()
            page.locator('[data-action="pause"]').click()
            check(any(x['m']=='Pause' for x in page.evaluate('window.__sent')), 'pause sent')
            page.locator('[data-action="resume"]').click()
            check(any(x['m']=='Resume' for x in page.evaluate('window.__sent')), 'resume sent')
            page.wait_for_timeout(1300)  # Let rapid button SFX drain before the result jingle.
            page.evaluate("hitmeUi.onUnity({type:'result',round:5,rankings:[{id:'player',name:'Bạn',rank:1,hits:3,survived:true},{id:'bot1',name:'Bot 1',rank:2,hits:1,survived:false}]})")
            page.evaluate("hitmeUi.onUnity({type:'screen',name:'result'})")
            check(page.locator('.live-rank').count() == 2, 'actual rankings')
            audio_log=page.evaluate('window.__hitmeAudioLog')
            audio_available=page.evaluate('Boolean(window.AudioContext || window.webkitAudioContext)')
            if audio_available: check(any(x['id']=='win' for x in audio_log), f'result win audio {audio_log}')
            page.screenshot(path=str(OUT / f'{engine}_result_fixture.png'))
            page.locator('[data-action="rematch"]').click()
            check(any(x['m']=='Rematch' for x in page.evaluate('window.__sent')), 'Rematch sent')
            page.locator('.live-summary [data-action="settings"]').click()
            page.locator('[data-action="exit-confirm"]').click()
            check(not any(x['m']=='ExitMatch' for x in page.evaluate('window.__sent')), 'exit asks confirmation')
            page.locator('[data-action="exit-match"]').click()
            check(any(x['m']=='ExitMatch' for x in page.evaluate('window.__sent')), 'ExitMatch sent')
            check(not errors, f'javascript errors: {errors}')
            results[engine] = {'status':'PASS' if not checks else 'FAIL', 'failures':checks,
                               'audio':'checked' if audio_available else 'unavailable in headless engine'}
            context.close(); browser.close()
    server.shutdown()
    (OUT/'results.json').write_text(json.dumps(results,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    passed=sum(x['status']=='PASS' for x in results.values())
    print(f'Passed! {passed}/{len(results)}')
    for engine, result in results.items():
        if result['failures']: print(engine, result['failures'])
    return 0 if passed==len(results) else 1


if __name__=='__main__':
    raise SystemExit(run())
