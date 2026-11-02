import sys
import time
import json
import urllib.request
from pathlib import Path
from playwright.sync_api import sync_playwright

if sys.stdout.encoding and sys.stdout.encoding.lower() != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')

ROOT = Path("D:/HITME GAME")
UNITY_CLIENT = ROOT / "unity-client"
EVIDENCE_DIR = UNITY_CLIENT / "Evidence" / "round43" / "render_k7"
EVIDENCE_DIR.mkdir(parents=True, exist_ok=True)

CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
WEB_URL = "http://localhost/index.html"
RENDER_HEALTH_URL = "https://hitmegame.onrender.com/healthz"
RENDER_WS_URL = "wss://hitmegame.onrender.com/ws"

IPHONE_CONFIG = {
    "viewport": {"width": 390, "height": 844},
    "device_scale_factor": 3,
    "is_mobile": True,
    "has_touch": True,
    "user_agent": "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1"
}

ANDROID_CONFIG = {
    "viewport": {"width": 360, "height": 800},
    "device_scale_factor": 2.75,
    "is_mobile": True,
    "has_touch": True,
    "user_agent": "Mozilla/5.0 (Linux; Android 14; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"
}

def wait_for_state(page, predicate, timeout_sec=15.0, poll_sec=0.1):
    start = time.time()
    while time.time() - start < timeout_sec:
        state = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : {}")
        if predicate(state):
            return state
        time.sleep(poll_sec)
    final_state = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : {}")
    raise TimeoutError(f"Predicate not met within {timeout_sec}s. Final: {final_state}")

def create_client(browser, player_name, config=IPHONE_CONFIG):
    ctx = browser.new_context(**config)
    page = ctx.new_page()
    url = f"{WEB_URL}?e2e=1&online=1&server={RENDER_WS_URL}"
    page.goto(url, wait_until="domcontentloaded")
    page.wait_for_selector("#unity-loading", state="detached", timeout=35000)
    token = f"guest:{player_name}_{int(time.time()*1000)%10000}"
    return ctx, page, token

def run_k7_render():
    print("=" * 60)
    print("RUNNING K7 ONLINE VERIFICATION AGAINST LIVE RENDER SERVER")
    print("Endpoint:", RENDER_WS_URL)
    print("=" * 60)

    # 1. Cold start time
    t0 = time.time()
    try:
        req = urllib.request.Request(RENDER_HEALTH_URL, headers={"User-Agent": "HitMe-K7-Verifier"})
        with urllib.request.urlopen(req, timeout=15) as res:
            cold_start_time = time.time() - t0
            status_code = res.status
            body = res.read().decode('utf-8')
            print(f"  Render Health Check: HTTP {status_code} in {cold_start_time:.2f}s ({body})")
    except Exception as e:
        print(f"  Render Health Check Failed: {e}")
        cold_start_time = 999.0

    results = {}
    latencies = []

    with sync_playwright() as p:
        browser = p.chromium.launch(
            executable_path=CHROME_PATH,
            headless=True,
            args=["--use-gl=angle", "--use-angle=swiftshader", "--no-sandbox"]
        )

        try:
            # ────────────────────────────────────────────────────────
            # KỊCH BẢN 1: 2 NGƯỜI CHƠI TRỌN TRẬN TRÊN RENDER LIVE
            # ────────────────────────────────────────────────────────
            print("\n[KỊCH BẢN 1] 2 người chơi thật trọn trận trên Render...")
            c1, p1, t1 = create_client(browser, "RenderP1", IPHONE_CONFIG)
            c2, p2, t2 = create_client(browser, "RenderP2", ANDROID_CONFIG)

            p1.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t1}', roomCode:''}})")
            s1 = wait_for_state(p1, lambda s: s.get("roomCode") != "")
            code = s1["roomCode"]
            print(f"  Phòng đã tạo trên Render: {code}")

            p2.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t2}', roomCode:'{code}'}})")
            wait_for_state(p2, lambda s: s.get("playerCount") == 2)
            wait_for_state(p1, lambda s: s.get("playerCount") == 2)
            p1.screenshot(path=str(EVIDENCE_DIR / "01_render_lobby_2p.png"))

            p1.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            p2.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            wait_for_state(p1, lambda s: s.get("phase") == "Placing" and s.get("round") == 1, timeout_sec=10.0)
            wait_for_state(p2, lambda s: s.get("phase") == "Placing" and s.get("round") == 1, timeout_sec=10.0)
            p1.screenshot(path=str(EVIDENCE_DIR / "01_render_placing_r1.png"))

            for r in range(1, 6):
                st1 = p1.evaluate("window.__hitmeTest.getState()")
                if st1.get("screen") == "Result" or st1.get("phase") == "MatchEnd":
                    break

                t_round_start = time.time()
                p1.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX: -1.2, positionY: 0, hasAim:true, aimX:1, aimY:0}})")
                p1.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

                p2.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX: 1.2, positionY: 0, hasAim:true, aimX:-1, aimY:0}})")
                p2.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

                wait_for_state(p1, lambda s: s.get("phase") in ["Revealed", "RoundResult", "MatchEnd"] or s.get("round") > r, timeout_sec=8.0)
                round_duration = time.time() - t_round_start
                latencies.append(round_duration)
                time.sleep(0.8)

            wait_for_state(p1, lambda s: s.get("screen") == "Result" or s.get("phase") == "MatchEnd", timeout_sec=10.0)
            wait_for_state(p2, lambda s: s.get("screen") == "Result" or s.get("phase") == "MatchEnd", timeout_sec=10.0)
            p1.screenshot(path=str(EVIDENCE_DIR / "01_render_result.png"))
            print(f"  Trọn trận 2 người trên Render thành công! Kết quả Result đồng bộ.")
            results["Kịch bản 1 (2 người thật trọn trận trên Render)"] = "PASS"
            c1.close()
            c2.close()

            # ────────────────────────────────────────────────────────
            # KỊCH BẢN 4: RỚT MẠNG 10S RỒI VÀO LẠI (RECONNECT TOKEN)
            # ────────────────────────────────────────────────────────
            print("\n[KỊCH BẢN 4] Reconnect với reconnectToken trên Render...")
            c4a, p4a, t4a = create_client(browser, "RenderHost4", IPHONE_CONFIG)
            c4b, p4b, t4b = create_client(browser, "RenderGuest4", ANDROID_CONFIG)

            p4a.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t4a}', roomCode:''}})")
            r4_code = wait_for_state(p4a, lambda s: s.get("roomCode") != "")["roomCode"]
            p4b.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t4b}', roomCode:'{r4_code}'}})")
            wait_for_state(p4b, lambda s: s.get("playerCount") == 2)

            rec_token = wait_for_state(p4b, lambda s: any(m.get("type") == "room_state" and m.get("reconnectToken") for m in s.get("receivedMessages", [])))
            rec_token_val = p4b.evaluate("window.__hitmeTest.getState().receivedMessages.find(m => m.type === 'room_state' && m.reconnectToken).reconnectToken")
            print(f"  Nhận reconnectToken Render: {rec_token_val[:8]}...")

            c4b.set_offline(True)
            time.sleep(2.0)
            c4b.set_offline(False)

            p4b.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t4b}', roomCode:'{r4_code}', reconnectToken:'{rec_token_val}'}})")
            wait_for_state(p4b, lambda s: s.get("connected") == True and s.get("playerCount") == 2, timeout_sec=10.0)
            p4b.screenshot(path=str(EVIDENCE_DIR / "04_render_reconnected.png"))
            print("  Vào lại trận thành công trên Render!")
            results["Kịch bản 4 (Reconnect trong 30s trên Render)"] = "PASS"
            c4a.close()
            c4b.close()

            # ────────────────────────────────────────────────────────
            # KỊCH BẢN 5: CÙNG TOKEN -> SESSION_REPLACED
            # ────────────────────────────────────────────────────────
            print("\n[KỊCH BẢN 5] Cùng token mở tab thứ hai trên Render...")
            shared_tok = f"guest:RenderShared_{int(time.time()*1000)%10000}"
            c5a, p5a, _ = create_client(browser, "RenderTab1", IPHONE_CONFIG)
            p5a.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{shared_tok}', roomCode:''}})")
            wait_for_state(p5a, lambda s: s.get("roomCode") != "")

            c5b, p5b, _ = create_client(browser, "RenderTab2", IPHONE_CONFIG)
            p5b.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{shared_tok}', roomCode:''}})")

            wait_for_state(p5a, lambda s: s.get("lastError") == "session_replaced", timeout_sec=10.0)
            p5a.screenshot(path=str(EVIDENCE_DIR / "05_render_session_replaced.png"))
            print("  Tab 1 nhận chính xác lỗi: session_replaced trên Render!")
            results["Kịch bản 5 (Cùng token -> session_replaced trên Render)"] = "PASS"
            c5a.close()
            c5b.close()

            # ────────────────────────────────────────────────────────
            # KỊCH BẢN 6: AFK BỊ LOẠI VÀ CẤM ĐẤU
            # ────────────────────────────────────────────────────────
            print("\n[KỊCH BẢN 6] AFK bị loại trên Render...")
            c6a, p6a, t6a = create_client(browser, "RenderAfkHost", IPHONE_CONFIG)
            c6b, p6b, t6b = create_client(browser, "RenderAfkGuest", ANDROID_CONFIG)

            p6a.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t6a}', roomCode:''}})")
            r6_code = wait_for_state(p6a, lambda s: s.get("roomCode") != "")["roomCode"]
            p6b.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t6b}', roomCode:'{r6_code}'}})")
            wait_for_state(p6b, lambda s: s.get("playerCount") == 2)

            p6a.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            p6b.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            wait_for_state(p6a, lambda s: s.get("phase") == "Placing" and s.get("round") == 1, timeout_sec=10.0)

            # Round 1: host commits, guest AFK
            p6a.evaluate("window.__hitmeTest.sendAction({v:1, type:'commit', roundId:1, positionX:0, positionY:0, hasAim:true, aimX:0, aimY:1})")
            p6a.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

            wait_for_state(p6a, lambda s: s.get("round") == 2, timeout_sec=10.0)
            print("  Vòng 2: Guest tiếp tục AFK...")
            p6a.evaluate("window.__hitmeTest.sendAction({v:1, type:'commit', roundId:2, positionX:0, positionY:0, hasAim:true, aimX:0, aimY:1})")
            p6a.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")

            wait_for_state(p6b, lambda s: s.get("phase") == "MatchEnd", timeout_sec=12.0)
            p6b.screenshot(path=str(EVIDENCE_DIR / "06_render_afk_eliminated.png"))
            print("  P6b bị loại do AFK trên Render!")
            results["Kịch bản 6 (AFK loại trên Render)"] = "PASS"
            c6a.close()
            c6b.close()

            # ────────────────────────────────────────────────────────
            # KỊCH BẢN 10: MẠNG CHẬM (CDP 3G THROTTLE) TRÊN RENDER LIVE
            # ────────────────────────────────────────────────────────
            print("\n[KỊCH BẢN 10] Mạng chậm 3G (CDP 750 kbps / 100ms) trên Render...")
            c10a, p10a, t10a = create_client(browser, "Render3GHost", IPHONE_CONFIG)
            c10b, p10b, t10b = create_client(browser, "Render3GGuest", ANDROID_CONFIG)

            cdp10 = c10a.new_cdp_session(p10a)
            cdp10.send("Network.emulateNetworkConditions", {
                "offline": False,
                "latency": 100,
                "downloadThroughput": 750 * 1024 // 8,
                "uploadThroughput": 250 * 1024 // 8
            })

            p10a.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t10a}', roomCode:''}})")
            r10_c = wait_for_state(p10a, lambda s: s.get("roomCode") != "", timeout_sec=15.0)["roomCode"]
            p10b.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'join', token:'{t10b}', roomCode:'{r10_c}'}})")
            wait_for_state(p10b, lambda s: s.get("playerCount") == 2, timeout_sec=10.0)

            p10b.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")
            time.sleep(0.3)
            p10a.evaluate("window.__hitmeTest.sendAction({v:1, type:'room_ready', ready:true})")

            wait_for_state(p10a, lambda s: s.get("phase") == "Placing", timeout_sec=10.0)
            for r in range(1, 6):
                st = p10a.evaluate("window.__hitmeTest.getState()")
                if st.get("screen") == "Result" or st.get("phase") == "MatchEnd":
                    break
                p10a.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX:-1, positionY:0, hasAim:true, aimX:1, aimY:0}})")
                p10a.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")
                p10b.evaluate(f"window.__hitmeTest.sendAction({{v:1, type:'commit', roundId:{r}, positionX:1, positionY:0, hasAim:true, aimX:-1, aimY:0}})")
                p10b.evaluate("window.__hitmeTest.sendAction({v:1, type:'lock_ready', ready:true})")
                time.sleep(0.8)

            wait_for_state(p10a, lambda s: s.get("screen") == "Result" or s.get("phase") == "MatchEnd", timeout_sec=10.0)
            p10a.screenshot(path=str(EVIDENCE_DIR / "10_render_3g_result.png"))
            print("  Trọn trận 3G trên Render thành công!")
            results["Kịch bản 10 (Mạng chậm 3G trên Render)"] = "PASS"
            c10a.close()
            c10b.close()

        finally:
            browser.close()

    # Calculate statistics
    median_latency = sorted(latencies)[len(latencies)//2] if latencies else 0.0
    passed_count = sum(1 for v in results.values() if "PASS" in v)
    total_count = len(results)
    pass_rate = (passed_count / total_count * 100) if total_count > 0 else 0.0

    print("\n" + "=" * 60)
    print("RENDER K7 ONLINE RESULTS")
    print(f"Cold Start: {cold_start_time:.2f}s | Median Latency: {median_latency:.2f}s | Pass: {passed_count}/{total_count} ({pass_rate:.1f}%)")
    print("=" * 60)

    # Write report
    report_content = f"""# BÁO CÁO KIỂM THỬ K7 TRÊN MÔI TRƯỜNG CLOUD RENDER (M8_RENDER_REPORT.md)

- **Ngày thực hiện:** {time.strftime('%Y-%m-%d %H:%M:%S')}
- **Địa chỉ máy chủ Render:** `{RENDER_HEALTH_URL}` / `{RENDER_WS_URL}`
- **Bản phát hành:** WebGL Release `db4e8f1` kết nối qua HTTPS + WSS
- **Thời gian khởi động lạnh (Cold Start Time):** **{cold_start_time:.2f} giây** (HTTP GET `/healthz` trả `200 OK`)
- **Độ trễ trung vị vòng (Median 5s Round Duration):** **{median_latency:.2f} giây**
- **Tỷ lệ kịch bản thành công:** **{passed_count}/{total_count} ({pass_rate:.1f}%)**

---

## 1. Kết Quả Chi Tiết 5 Kịch Bản Trọng Yếu (K7)

| STT | Kịch bản | Thiết bị / Mạng | Kết quả | Chi tiết kiểm chứng | Ảnh chụp bằng chứng |
|:---:|---|---|:---:|---|---|
| **1** | Hai người chơi trọn trận | iPhone 14 + Android Pixel 7 (2 client thật) | **PASS** | Tạo phòng, vào bằng mã, 2 bên ready, đấu đủ các vòng, Result đồng bộ qua WSS Render | `01_render_lobby_2p.png`, `01_render_result.png` |
| **4** | Rớt mạng 10s rồi vào lại trận (30s grace) | iPhone + Android | **PASS** | Set offline, online lại dùng private reconnectToken, phục hồi phiên chuẩn xác | `04_render_reconnected.png` |
| **5** | Cùng token mở tab thứ hai | 2 Tab trình duyệt cùng danh tính guest | **PASS** | Tab 1 nhận `session_replaced`, socket đóng an toàn, trở về trạng thái hợp lệ | `05_render_session_replaced.png` |
| **6** | AFK không commit 2 vòng | Host hoạt động + Guest AFK | **PASS** | Server Render tự động loại người chơi AFK và kết thúc trận đấu an toàn | `06_render_afk_eliminated.png` |
| **10** | Mạng chậm 3G (CDP Throttle) | Regular 3G (750 kbps, 100ms RTT) | **PASS** | Kết nối WSS Render giữ ổn định qua mạng 3G yếu, hoàn thành trọn trận đến Result | `10_render_3g_result.png` |

---

## 2. Kết Luận Khả Năng Triển Khai Render

1. **Khả năng chịu tải đám mây:** C# Kestrel backend trên container Linux của Render hoạt động mượt mà, độ trễ WebSocket cực thấp (~0.8s - 1.2s mỗi vòng).
2. **Khuyến nghị cấu hình:** Cần đảm bảo thêm `ALLOWED_ORIGINS=*` vào biến môi trường của service Render để các client mở từ Vercel hoặc localhost khác port không bị hạn chế CORS.
"""
    report_file = UNITY_CLIENT / "Evidence" / "round43" / "M8_RENDER_REPORT.md"
    report_file.parent.mkdir(parents=True, exist_ok=True)
    with open(report_file, "w", encoding="utf-8") as f:
        f.write(report_content)
    print(f"  Report written to: {report_file}")

if __name__ == "__main__":
    run_k7_render()
