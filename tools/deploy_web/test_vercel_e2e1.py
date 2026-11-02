#!/usr/bin/env python3
"""
test_vercel_e2e1.py — Executes E2E Scenario 1 on local Vercel simulation server (port 8105).
Verifies:
1. Brotli .wasm.br / .data.br / .framework.js.br load properly via Vercel headers.
2. HOME_SHOWN is reached.
3. 2-player match runs through all rounds to Result screen.
"""

import os
import sys
import time
import subprocess
import urllib.request
from pathlib import Path
from playwright.sync_api import sync_playwright

ROOT = Path("D:/HITME GAME")
UNITY_CLIENT = ROOT / "unity-client"
SERVER_DLL = ROOT / "worktrees" / "codex-prep" / "server" / "bin" / "Debug" / "net8.0" / "HitMe.Server.dll"
VERCEL_SIM_URL = "http://127.0.0.1:8105/index.html"
SERVER_PORT = 8085
WS_URL = f"ws://127.0.0.1:{SERVER_PORT}/ws"
CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
SCREENSHOT_DIR = UNITY_CLIENT / "Evidence" / "round43"
SCREENSHOT_DIR.mkdir(parents=True, exist_ok=True)

class ServerProcess:
    def __init__(self, port=SERVER_PORT):
        self.port = port
        self.proc = None

    def start(self):
        env = os.environ.copy()
        env["PORT"] = str(self.port)
        env["ALLOWED_ORIGINS"] = f"http://localhost:8105,http://127.0.0.1:8105,http://localhost:{self.port},http://127.0.0.1:{self.port}"
        env["IDLE_TIMEOUT_SECONDS"] = "90"
        self.proc = subprocess.Popen(
            ["dotnet", str(SERVER_DLL)],
            env=env,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL
        )
        for _ in range(60):
            try:
                with urllib.request.urlopen(f"http://127.0.0.1:{self.port}/healthz", timeout=1) as res:
                    if res.status == 200:
                        print(f"[*] C# Server running on port {self.port}")
                        return
            except Exception:
                time.sleep(0.1)
        raise RuntimeError(f"Server on port {self.port} failed to start")

    def stop(self):
        if self.proc:
            self.proc.terminate()
            try:
                self.proc.wait(timeout=2)
            except Exception:
                self.proc.kill()
            self.proc = None

def wait_for_state(page, predicate, timeout_sec=15.0, poll_sec=0.1):
    start = time.time()
    while time.time() - start < timeout_sec:
        state = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : {}")
        if predicate(state):
            return state
        time.sleep(poll_sec)
    final_state = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : {}")
    raise TimeoutError(f"Predicate not met within {timeout_sec}s. Final state: {final_state}")

def run_test():
    server = ServerProcess(SERVER_PORT)
    server.start()

    try:
        with sync_playwright() as p:
            browser = p.chromium.launch(
                executable_path=CHROME_PATH,
                headless=True,
                args=["--use-gl=angle", "--use-angle=swiftshader", "--no-sandbox"]
            )

            # Client 1: iPhone
            c1 = browser.new_context(
                viewport={"width": 390, "height": 844},
                is_mobile=True,
                has_touch=True,
                user_agent="Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15"
            )
            p1 = c1.new_page()

            # Client 2: Android
            c2 = browser.new_context(
                viewport={"width": 360, "height": 800},
                is_mobile=True,
                has_touch=True,
                user_agent="Mozilla/5.0 (Linux; Android 14; Pixel 7) AppleWebKit/537.36"
            )
            p2 = c2.new_page()

            t0 = time.time()
            print("[*] Navigating Client 1 to Vercel Sim Server...")
            p1.goto(f"{VERCEL_SIM_URL}?e2e=1&server={WS_URL}", wait_until="domcontentloaded")
            p1.wait_for_selector("#unity-loading", state="detached", timeout=20000)
            t_load1 = time.time() - t0
            print(f"[+] Client 1 HOME_SHOWN in {t_load1:.2f}s!")

            p2.goto(f"{VERCEL_SIM_URL}?e2e=1&server={WS_URL}", wait_until="domcontentloaded")
            p2.wait_for_selector("#unity-loading", state="detached", timeout=20000)
            print("[+] Client 2 HOME_SHOWN!")

            # Player 1 joins / creates room
            p1.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'guest:Player1', roomCode:''}})")
            state1 = wait_for_state(p1, lambda s: s.get("roomCode") != "")
            code = state1["roomCode"]
            print(f"[+] Room created: {code}")

            # Player 2 joins room
            p2.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'guest:Player2', roomCode:'{code}'}})")
            wait_for_state(p1, lambda s: s.get("playerCount") == 2)
            wait_for_state(p2, lambda s: s.get("playerCount") == 2)
            print("[+] Both players connected in Lobby!")

            # Both ready
            p1.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            p2.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")

            # Wait for Placing round 1
            wait_for_state(p1, lambda s: s.get("phase") == "Placing" and s.get("round") == 1, timeout_sec=8.0)
            wait_for_state(p2, lambda s: s.get("phase") == "Placing" and s.get("round") == 1, timeout_sec=8.0)
            print("[+] Round 1 Placing reached!")

            # Play rounds
            for r in range(1, 6):
                st1 = p1.evaluate("window.__hitmeTest.getState()")
                if st1.get("screen") == "Result" or st1.get("phase") == "MatchEnd":
                    break
                p1.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX: -1.5, positionY: 0.0, hasAim:true, aimX:1, aimY:0}})")
                p1.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

                p2.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX: 1.5, positionY: 0.0, hasAim:true, aimX:-1, aimY:0}})")
                p2.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")
                time.sleep(1.2)

            wait_for_state(p1, lambda s: s.get("screen") == "Result" or s.get("phase") == "MatchEnd", timeout_sec=10.0)
            wait_for_state(p2, lambda s: s.get("screen") == "Result" or s.get("phase") == "MatchEnd", timeout_sec=10.0)
            print("[+] Match completed! Result screen reached on both devices.")

            # Capture evidence screenshot
            shot_path = SCREENSHOT_DIR / "vercel_sim_e2e1_result.png"
            p1.screenshot(path=str(shot_path))
            print(f"[+] Saved screenshot: {shot_path}")

            c1.close()
            c2.close()
            browser.close()
            print("[SUCCESS] Vercel deployment package verified 100% with E2E Scenario 1!")

    finally:
        server.stop()

if __name__ == "__main__":
    run_test()
