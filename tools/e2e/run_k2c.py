#!/usr/bin/env python3
"""
HIT ME - K2-C Dedicated E2E Runner
Phòng 6 người TRỘN (3 người chơi thật Context WebGL + 3 Bot Script WebSocket).
Chơi 3 trận trọn vẹn (5 vòng mỗi trận cho tới màn hình Result).
"""

import os
import sys
import time
import subprocess
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")
from playwright.sync_api import sync_playwright

ROOT = Path("D:/HITME GAME")
SERVER_DLL = ROOT / "worktrees" / "codex-prep" / "server" / "bin" / "Debug" / "net8.0" / "HitMe.Server.dll"
WEB_URL = "http://127.0.0.1:8103/index.html"
SERVER_PORT = 8086
WS_URL = f"ws://127.0.0.1:{SERVER_PORT}/ws"
CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
SCREENS_DIR = ROOT / "unity-client" / "Evidence" / "round41" / "e2e" / "screens"
SCREENS_DIR.mkdir(parents=True, exist_ok=True)

IPHONE_CONFIG = {
    "viewport": {"width": 390, "height": 844},
    "user_agent": "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
    "device_scale_factor": 3,
    "is_mobile": True,
    "has_touch": True,
}

ANDROID_CONFIG = {
    "viewport": {"width": 360, "height": 800},
    "user_agent": "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Mobile Safari/537.36",
    "device_scale_factor": 2.75,
    "is_mobile": True,
    "has_touch": True,
}

class ServerProcess:
    def __init__(self, port=SERVER_PORT):
        self.port = port
        self.proc = None

    def start(self):
        env = os.environ.copy()
        env["PORT"] = str(self.port)
        env["ALLOWED_ORIGINS"] = f"http://localhost:8103,http://127.0.0.1:8103,http://localhost:{self.port},http://127.0.0.1:{self.port}"
        env["IDLE_TIMEOUT_SECONDS"] = "90"
        self.proc = subprocess.Popen(
            ["dotnet", str(SERVER_DLL)],
            env=env,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL
        )
        for _ in range(60):
            try:
                import urllib.request
                with urllib.request.urlopen(f"http://127.0.0.1:{self.port}/healthz", timeout=1) as res:
                    if res.status == 200:
                        return
            except Exception:
                time.sleep(0.1)
        raise RuntimeError(f"Server on port {self.port} failed to start")

    def stop(self):
        if self.proc:
            self.proc.terminate()
            try:
                self.proc.wait(timeout=5)
            except Exception:
                self.proc.kill()
            self.proc = None

def wait_for_state(page, predicate, timeout_sec=15.0):
    start = time.time()
    while time.time() - start < timeout_sec:
        try:
            st = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : null")
            if st and predicate(st):
                return st
        except Exception:
            pass
        time.sleep(0.15)
    cur = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : null")
    raise TimeoutError(f"Predicate not met in {timeout_sec}s. Current state: {cur}")

def create_client(browser, name, device_cfg):
    context = browser.new_context(**device_cfg)
    page = context.new_page()
    token = f"guest:{name}_{int(time.time()*1000)%10000}"
    url = f"{WEB_URL}?e2e=1&online=1&server={WS_URL}"
    page.goto(url, wait_until="domcontentloaded")
    page.wait_for_selector("#unity-loading", state="detached", timeout=35000)
    wait_for_state(page, lambda s: s.get("screen") == "Home", timeout_sec=15.0)
    return context, page, token

def run_k2c():
    print(f"Khởi động game server trên cổng {SERVER_PORT}...")
    server = ServerProcess(SERVER_PORT)
    server.start()
    print("Server sẵn sàng!")

    with sync_playwright() as p:
        browser = p.chromium.launch(
            executable_path=CHROME_PATH,
            headless=True,
            args=[
                "--use-gl=angle",
                "--use-angle=swiftshader",
                "--enable-webgl",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu-watchdog"
            ]
        )

        print("\n[K2-C] Khởi tạo 3 client WebGL THẬT (1 Host iPhone + 2 Peer Android)...")
        c_m1, p_m1, t_m1 = create_client(browser, "K2C_P1_Host", IPHONE_CONFIG)
        c_m2, p_m2, t_m2 = create_client(browser, "K2C_P2_Peer", ANDROID_CONFIG)
        c_m3, p_m3, t_m3 = create_client(browser, "K2C_P3_Peer", ANDROID_CONFIG)
        print("  Cả 3 client WebGL thật đã khởi động và hiển thị Home Screen!")

        matches_passed = 0
        for match_idx in range(1, 4):
            print(f"\n==========================================")
            print(f"  BẮT ĐẦU TRẬN TRỘN {match_idx}/3 (6 người: 3 thật + 3 script)")
            print(f"==========================================")

            if match_idx > 1:
                for p in [p_m1, p_m2, p_m3]:
                    p.evaluate("""async () => {
                        const old = window.__hitmeActiveSocket;
                        if (old) { old.onclose = null; try { old.close(); } catch(e) {} }
                        const ws = window.__hitmeTest.connect();
                        await new Promise((res, rej) => {
                            ws.addEventListener('open', res, { once: true });
                            setTimeout(() => rej(new Error('ws open timeout')), 5000);
                        });
                        window.__hitmeActiveSocket = ws;
                        if (window.__hitmeTestState) {
                            window.__hitmeTestState.connected = true;
                            window.__hitmeTestState.roomCode = '';
                            window.__hitmeTestState.phase = '';
                            window.__hitmeTestState.screen = '';
                            window.__hitmeTestState.round = 0;
                            window.__hitmeTestState.playerCount = 0;
                            window.__hitmeTestState.lastError = '';
                            window.__hitmeTestState.winners = [];
                            window.__hitmeTestState.receivedMessages = [];
                            window.__hitmeTestState.sentMessages = [];
                        }
                    }""")
                time.sleep(0.3)

            p_m1.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t_m1}_{match_idx}', roomCode:''}})")
            mix_code = wait_for_state(p_m1, lambda s: s.get("roomCode") != "")["roomCode"]
            print(f"  Phòng mới được tạo: Mã {mix_code}")

            p_m2.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t_m2}_{match_idx}', roomCode:'{mix_code}'}})")
            p_m3.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t_m3}_{match_idx}', roomCode:'{mix_code}'}})")

            # Connect 3 script bots via raw WebSockets
            p_m1.evaluate(f"""async ([code, matchIdx]) => {{
                window.__mixedSockets = [];
                const RawWS = window.__OrigWebSocket || WebSocket;
                for (let i = 4; i <= 6; i++) {{
                    const ws = new RawWS('{WS_URL}');
                    await new Promise(r => ws.onopen = r);
                    ws.send(JSON.stringify({{v: 1, type: 'join', token: 'guest:MixedBot_' + i + '_' + matchIdx + '_' + Date.now(), roomCode: code}}));
                    let hasReadied = false;
                    ws.onmessage = (e) => {{
                        try {{
                            const msg = JSON.parse(e.data);
                            if (msg.type === 'room_state' && !hasReadied) {{
                                hasReadied = true;
                                ws.send(JSON.stringify({{v: 1, type: 'room_ready', ready: true}}));
                            }} else if (msg.type === 'round_start') {{
                                ws.send(JSON.stringify({{v: 1, type: 'commit', roundId: msg.roundId, positionX: 0.3 * i, positionY: 0, hasAim: false}}));
                                ws.send(JSON.stringify({{v: 1, type: 'lock_ready', ready: true}}));
                            }}
                        }} catch(err) {{}}
                    }};
                    window.__mixedSockets.push(ws);
                }}
            }}""", [mix_code, match_idx])

            wait_for_state(p_m1, lambda s: s.get("playerCount") == 6, timeout_sec=10.0)
            wait_for_state(p_m2, lambda s: s.get("playerCount") == 6, timeout_sec=10.0)
            wait_for_state(p_m3, lambda s: s.get("playerCount") == 6, timeout_sec=10.0)
            print("  Lobby đã đủ 6/6 người chơi (3 thật + 3 script)!")

            # 3 real players ready up
            p_m2.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            p_m3.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            time.sleep(0.2)
            p_m1.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")

            wait_for_state(p_m1, lambda s: s.get("phase") == "Placing", timeout_sec=8.0)
            print("  Trận đấu bắt đầu vào vòng 1 (Placing)!")

            for r in range(1, 6):
                st1 = p_m1.evaluate("window.__hitmeTest.getState()")
                if st1.get("screen") == "Result" or st1.get("phase") == "MatchEnd":
                    print(f"  Trận đấu kết thúc sớm ở vòng {r}!")
                    break

                print(f"    Vòng {r}/5: Tất cả 6 người chơi commit hành động & lock ready...")
                p_m1.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX:-1.5, positionY:0, hasAim:true, aimX:1, aimY:0}})")
                p_m1.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

                p_m2.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX:0, positionY:1.5, hasAim:true, aimX:0, aimY:-1}})")
                p_m2.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

                p_m3.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX:1.5, positionY:0, hasAim:true, aimX:-1, aimY:0}})")
                p_m3.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

                time.sleep(1.2)

            wait_for_state(p_m1, lambda s: s.get("screen") == "Result" or s.get("phase") == "MatchEnd", timeout_sec=12.0)
            matches_passed += 1
            print(f"  Trận trộn {match_idx}/3 hoàn tất trọn vẹn đến màn hình Result!")

            # Clean up sockets
            p_m1.evaluate("""() => {
                if (window.__mixedSockets) {
                    window.__mixedSockets.forEach(ws => { try { ws.close(); } catch(e){} });
                    window.__mixedSockets = [];
                }
            }""")
            time.sleep(0.5)

        screen_path = SCREENS_DIR / "12_k2c_6mixed_players_result.png"
        p_m1.screenshot(path=str(screen_path))
        print(f"\nĐã chụp ảnh màn hình Result K2-C: {screen_path}")
        print(f"KẾT QUẢ K2-C: {matches_passed}/3 TRẬN TRỘN HOÀN THÀNH XUẤT SẮC!")

        c_m1.close()
        c_m2.close()
        c_m3.close()
        browser.close()
        server.stop()

if __name__ == "__main__":
    run_k2c()
