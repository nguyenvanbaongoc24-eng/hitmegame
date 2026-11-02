# HIT ME – TRẠNG THÁI TIẾN ĐỘ THỰC HIỆN (STATUS)

*Cập nhật: 02/11/2026*

## 1. Phân chia nhiệm vụ Round 38 (hit-me-next-tasks.md & hit-me-antigravity-luot-2.md)

### Nhiệm vụ Codex (Backend / Server / QA)
| Mã | Nhiệm vụ | Trạng thái | Ghi chú |
|---|---|---|---|
| C1 | Đóng nốt H4/H5 round 37, lưu Evidence/round38/ | **ĐÃ XONG** | Test 139/139 pass (prep/codex) |
| C2 | Vòng đời phòng (1-6 người, mã phòng, từ chối bắt đầu khi 1 người) | **ĐÃ XONG** | FakeClient 3/3 matches pass |
| C3 | Tính trận server (authoritative, 5s window, đạn dừng tường/xuyên 1) | **ĐÃ XONG** | Golden test tất định seed |
| C4 | AFK / rớt mạng / reconnect / cấm đấu 2p | **ĐÃ XONG** | FileBanStore + token 30s |
| C5 | Lưu hồ sơ + vàng + hạng vào Supabase (chỉ top-3 thắng được cộng) | Đang chờ DB URL | ApprovedGoldByRoomSize |
| C6 | Chuẩn bị deploy WSS lên Render (Dockerfile/render.yaml) | Bản nháp | Cần tài khoản Render |
| C7 | Script đo tải measure_home_shown.py so sánh 8102 vs 8103 | **ĐÃ XONG** | Release Brotli 5.98s |

### Nhiệm vụ Antigravity (Unity Client / UI / UX) - Lượt 1 & Lượt 2
| Mã | Nhiệm vụ | Trạng thái | Ghi chú |
|---|---|---|---|
| A1 | Font + tiếng Việt: full dấu (BeVietnamPro-Bold SDF), sửa chữ mất dấu, nút chuyển VI/EN nhớ lựa chọn | **ĐÃ XONG** | Nút VI/EN trên Home + Settings, Refresh toàn bộ UI, 142/142 test pass (commit `272bde0`) |
| A2 | Màn Home đúng mockup: hồ sơ, vàng, kim cương, cài đặt, chọn nhân vật/vũ khí, Chơi nhanh, Tạo phòng, Vào phòng, Luyện với bot, tab Xếp hạng/Nhiệm vụ (khung Sắp ra mắt) | **ĐÃ XONG** | Khung Sắp ra mắt cho tab Nhiệm vụ, cycle nhân vật/vũ khí (commit `272bde0`) |
| A3 | Luồng phòng online: Tạo phòng (mã), Vào phòng (nhập), Lobby 1-6 người, nút SẴN SÀNG, chặn 1 người | **ĐÃ XONG** | Bàn phím in-game keypad 6 ký tự, roster 1-6, chặn nút Sẵn sàng/Bắt đầu khi 1 người |
| A4 | Màn Placing đúng luật v1.2: chạm đặt, trượt ngắm, chạm khác dời + reset Up, kéo < 4mm hủy ngắm, 5s timer, ngón đầu tính, chạm lên HUD không đặt | **ĐÃ XONG** | PlacementGestureV2 + chặn chạm HUD EventSystem, 15 tests pass (commit `272bde0`) |
| A5 | Màn Result chuyển cảnh mượt, trúng đòn + SFX, nút kết bạn sau trận | **ĐÃ XONG** | Smooth fade-in, hit/defeat SFX, win/lose SFX, nút Kết bạn (+ KẾT BẠN -> ✓ ĐÃ GỬI) (commit `46ff81e`) |
| A6 | Chat nhanh + emote trong trận theo mockup | **ĐÃ XONG** | Khay Emote/Chat nhanh in-game: 5 emoji + 3 câu mẫu, sfx_chat_pop, toast chat (commit `46ff81e`) |
| A7 | Build WebGL Release Brotli, chụp ảnh iPhone Safari (1170x2532), so sánh UI_DIFF.md | **ĐÃ XONG** | 7 ảnh chụp tại `Evidence/round38/screens/`, `Evidence/round38/UI_DIFF.md`, MIME type fix cổng 8103 (commit `db4e8f1`) |
| A8 | Lớp mạng client IGameClient, MockGameClient, WebSocketGameClient, WebGLWebSocketTransport, guest token | **ĐÃ XONG** | 6 test mới, 148/148 pass, sẵn sàng kết nối server Phase 3 (commit `bc33593`) |
| A9 | Tối ưu thời gian tải WebGL Release theo C11 (nén texture crunch, giảm kích thước) | **ĐÃ XONG** | Giảm data.br từ 12.12 MB xuống 5.96 MB (giảm 50.8%), HOME_SHOWN từ 10.64s xuống 5.98s (< 6.0s ĐẠT), `Evidence/round38/A9_load.md`, 148/148 pass (commit `aea0996`) |
| A10 | Rà soát chất lượng hình ảnh & chuyển động (5 người + 2 thú, flipX ném, thở không giật, quét tiếng Việt, sửa lệch UI_DIFF) | **ĐÃ XONG** | 100% tiếng Việt chuẩn Unicode, fix facing throw, fix idle stutter, thêm nút mũi tên tướng, viền vàng ngôn ngữ/emote, 148/148 pass, `Evidence/round38/A10_visual_qa.md` (commit `007e947`) |

---

## 2. Nhiệm vụ Phase 3 (hit-me-phase3-ag.md) – Nối Client Với Server Thật

| Mã | Nhiệm vụ | Trạng thái | Ghi chú |
|---|---|---|---|
| P3-1 | Kết nối thật cục bộ: WebSocketGameClient nối `ws://localhost:8080/ws`, tạo phòng, vào phòng, lobby 1-6 người, từ chối 1 người | **ĐÃ XONG** | Tích hợp ProductionUIController, NetConfig.IsOnlineActive, hỗ trợ chạy với server thật & FakeClient (commit `428d174`) |
| P3-2 | Chơi trọn trận qua mạng: 5s window, commit (x,y,hasAim), lock_ready, reveal, round_result | **ĐÃ XONG** | Khớp Wire Protocol v1, server authoritative, 3/3 FakeClient matches pass |
| P3-3 | Tình huống lỗi: rớt mạng, session_replaced, banned (120s), reconnect token 30s | **ĐÃ XONG** | 3 test mới NUnit, 14 mã lỗi bản địa hóa tiếng Việt, 151/151 test pass (commit `428d174`) |
| P3-4 | Chat, emote, kết bạn qua mạng & lập bảng PROTOCOL_MISMATCH.md | **ĐÃ XONG** | Gửi emote 5 emoji chuẩn wire v1, lưu giữ local feedback cho chat & kết bạn, lập file `MD/PROTOCOL_MISMATCH.md` |
| P3-5 | Hướng dẫn thử LAN điện thoại, mixed content http/ws vs https/wss | **ĐÃ XONG** | Ghi rõ trong PROTOCOL_MISMATCH và README |
| P3-6 | Dọn các điểm lệch mockup còn mở: Mũi tên đầu dép trên thước ngắm + Hào quang xoay sau Cúp chiến thắng | **ĐÃ XONG** | Vẽ mũi tên 5 điểm trên LineRenderer thước ngắm, xoay liên tục VictoryHaloGlow trên Result Screen (commit `428d174`) |

---

## 3. Phase 3B: Tự Kiểm Tra Tự Động E2E (hit-me-phase3b-selftest.md)

| Mã | Nhiệm vụ | Trạng thái | Ghi chú |
|---|---|---|---|
| B1 | Bộ kiểm thử E2E Playwright (Python), Chrome Headless, WebGL Release Brotli (:8103), Server C# Kestrel (:8085), bridge `?e2e=1` | **ĐÃ XONG** | `tools/e2e/e2e_suite.py`, giả lập iPhone 390×844 @3x & Android 360×800 @2.75x |
| B2 | 10 kịch bản tự động: 2p, 6p+reject, solo block, reconnect, session_replaced, AFK ban, server shutdown, bad token, emote, 3G throttle | **ĐÃ XONG** | **10/10 PASS**, bằng chứng tại `Evidence/round41/e2e/screens/` |
| B3 | Soi lỗi giao diện responsive + quét console trình duyệt | **ĐÃ XONG** | 0 lỗi JS/WebGL, layout ĐẠT cả 2 viewport, `Evidence/round41/UI_DIFF_E2E.md` |
| B4 | 20 trận liên tiếp stability loop (2-6 người luân phiên) | **ĐÃ XONG** | **20/20 PASS**, JS Heap 15.6–17.0 MB (ổn định, 0 rò rỉ) |
| B5 | Chạy lại trên server Render (wss://...) | **Chờ** | Đã kiểm tra `hitmegame.onrender.com`: DNS & port 443 OK, HTTP /healthz timeout >90s (service chưa phản hồi) |
| B6 | Hồ sơ E2E_SUMMARY.md | **ĐÃ XONG** | `Evidence/round41/E2E_SUMMARY.md` |

---

## 4. Phase 3C (Siết Bằng Chứng) & Phase 4 (Nội Dung) — hit-me-phase3c-phase4.md

| Mã | Nhiệm vụ | Trạng thái | Ghi chú |
|---|---|---|---|
| K1 | Minh bạch hóa E2E: thêm cột người thật/script, giải trình SC6 & SC10, khôi phục SC10 2 client thật trọn trận | **ĐÃ XONG** | `Evidence/round41/E2E_SUMMARY.md` cập nhật chi tiết |
| K2 | Bằng chứng người thật: 2 người thật trọn trận (SC1, SC10), 3 người thật trọn trận (K2-A), đo giới hạn (K2-B: 4/6 context) | **ĐÃ XONG** | K2-A (3 client WebGL thật) PASS, K2-B PARTIAL (4/6 context đạt ngưỡng máy SwiftShader CPU) |
| K3 | Điều tra lỗi `room_ready` lặp: xác định nguyên nhân và chứng minh client Unity | **ĐÃ XONG** | Client Unity sạch sẽ (chỉ gửi 1 lần khi click); nguyên nhân do bot script loop. Test `Test_RoomReady_IsSentExactlyOnce_NotRepeatedOnRoomState` PASS |
| K4 | Đo bộ nhớ thật: Wasm Heap (`Module.HEAPU32`) + JS Heap qua 10 trận với 2 context thật | **ĐÃ XONG** | Wasm Heap cố định 98.0 MB (0% rò rỉ), JS Heap dao động 15.5–16.4 MB (<6%) |
| K5 | Dọn mã kiểm thử khỏi bản phát hành: không lọt biến `__hitme*` khi không có `?e2e=1` | **ĐÃ XONG** | `tools/e2e/test_release_hygiene.py` PASS, log tại `Evidence/round41/e2e/logs/release_hygiene.log` |
| K6 | Protocol v2 (chat theo id, rankings+vàng trong match_end, kết bạn) | **Chặn** | Nhánh `prep/codex` chưa bàn giao `MD/PROTOCOL_V2_CHANGES.md` |
| K7 | B5 Render: chạy lại suite qua `wss://hitmegame.onrender.com/ws` | **Chờ** | DNS/Port 443 thông suốt; HTTP GET /healthz timeout >90s (chờ container Render hoàn tất khởi động) |
| P4-1 | Dữ liệu Cửa hàng Việt Nam `Assets/Data/Items.json` + `ItemCatalog.cs` + danh sách thiếu asset | **ĐÃ XONG** | 7 vũ khí đúng công thức giá (199 -> ×1.3, x3); `Evidence/round42/MISSING_ASSETS.md`; 7 tests NUnit PASS (159/159 EditMode tests PASS) |

---

## 5. Phase 4, Lượt 2 (Giao Diện & Kịch Bản Trộn) — hit-me-phase4-luot-2.md

| Mã | Nhiệm vụ | Trạng thái | Ghi chú |
|---|---|---|---|
| K2-C | Phòng 6 người trộn: 3 client WebGL thật (1 Host iPhone + 2 Peer Android) + 3 bot script WebSocket | **ĐÃ XONG** | Chơi 3 trận trọn vẹn (5 vòng mỗi trận) đến Result; minh bạch hóa trong `Evidence/round41/E2E_SUMMARY.md`; ảnh chụp `12_k2c_6mixed_players_result.png` |
| K6 | Protocol v2: kiểm tra `MD/PROTOCOL_V2_CHANGES.md` | **Chặn** | Nhánh `prep/codex` vẫn ở commit `1c447e2`, chưa bàn giao protocol v2 |
| K7 | Render deployment verification | **Chờ** | HTTP `/healthz` vẫn timeout >90s (chờ phản hồi / khởi động của container Render) |
| P4-2 | Cửa hàng + Túi đồ hoàn thiện (cục bộ): mua bằng vàng, trang bị, vũ khí hiển thị trong trận, VFX theo độ hiếm | **ĐÃ XONG** | Đọc `Items.json`, kiểm tra số dư, không mua trùng, hiển thị "THIẾU ẢNH" cho các sprite chưa có, VFX HitFx scale & color theo độ hiếm (Common/Rare/Epic/Legendary/Mythic) không đổi chỉ số |
| P4-3 | Hồ sơ, Đăng nhập, Cài đặt: Khách + Google/FB/Apple ("Sắp ra mắt"); Profile tên, avatar, rank, vàng, số trận; đổi tên lần 1 free, lần 2 giá 99G | **ĐÃ XONG** | Tích hợp `AccountConfig.json`, hiển thị số trận & bậc rank, đổi tên 1st free / 2nd 99 gold; nút mạng xã hội hiện toast "Sắp ra mắt trong Phase 5!"; cài đặt âm thanh tách kênh & ngôn ngữ VI/EN |
| P4-4 | Rank + Nhiệm vụ ngày (giao diện + logic cục bộ): tab Bảng xếp hạng + tab Nhiệm vụ (3 nhiệm vụ ngày, nhận thưởng, reset 00:00 VN) | **ĐÃ XONG** | `RankConfig.json` (5 bậc Đồng->Kim Cương) + `QuestsConfig.json` (Chơi 3 trận, Trúng 5 đòn, Thắng 1 trận), tự động cập nhật tiến độ sau trận, nút NHẬN / ĐÃ NHẬN |
| P4-5 | Âm thanh: BGM menu + gameplay (transition crossfade mượt), SFX giao diện (bấm nút, mua đồ, nhận thưởng), tách kênh âm nhạc/hiệu ứng | **ĐÃ XONG** | Thêm `TransitionBgm`, `DuckMusic`, `PlayClaimQuest`, `PlayBuyItem` trong `AudioManager.cs` |

---

## 6. TIẾN ĐỘ PHASE 4 – LƯỢT 3 (ANTIGRAVITY)

**Commit HEAD:** `5f029f2` trên nhánh `feature/ui-ux-production`  
**Tổng số EditMode tests:** **177/177 tests PASS (100% xanh)**  
**Báo cáo hiệu năng:** `Evidence/round43/L2_perf.md`  
**Gói triển khai Vercel:** `Tools/deploy_web/` & `MD/DEPLOY_VERCEL.md`  
**Hợp đồng hồ sơ & ví:** `MD/PROFILE_CONTRACT.md` & `MD/PROTOCOL_MISMATCH.md`  

| Hạng mục | Nội dung thực hiện | Trạng thái | Ghi chú & Bằng chứng |
|---|---|---|---|
| **L1** | Dọn chuỗi giao diện: sửa toast "Sắp ra mắt trong Phase 5!" thành "Sắp ra mắt" (VI/EN). Che giấu bảng xếp hạng top 5 và ví vàng giả khi ở chế độ Online nếu server chưa trả dữ liệu (hiện "Đang tải..." / "Chưa có dữ liệu"). | **ĐÃ XONG** | 167/167 tests pass (`Phase4Luot3DataSanitizationTests.cs`, `test-results-l1.xml`). Commit `350ad6c`. |
| **L3** | Gói triển khai Vercel: Tạo `Tools/deploy_web/` gồm `vercel.json` (header Brotli `Content-Encoding: br`, MIME type chuẩn, cache dài cho file hash, no-cache index), `config.json` runtime, script `package_deploy.py`, server giả lập `serve_vercel_sim.py`, chạy thử E2E Scenario 1 trên đó thành công. Viết `MD/DEPLOY_VERCEL.md`. | **ĐÃ XONG** | Bundle tĩnh 10.14 MB tại `dist/`. E2E Scenario 1 chạy qua server giả lập đạt HOME_SHOWN 1.63s, kết quả trận 2 người đồng bộ (`Evidence/round43/vercel_sim_e2e1_result.png`). Commit `b256e49`. |
| **L2** | Ngân sách hiệu năng máy yếu: Đo Playwright CDP throttle CPU 4x và 6x, mạng 4G. Đo FPS trung vị/1% thấp tại Placing, Reveal/Throw, Result; đo HOME_SHOWN; Wasm heap. | **ĐÃ XONG** | Báo cáo `Evidence/round43/L2_perf.md`. FPS trung vị 59.9 FPS (vượt xa chỉ tiêu ≥30 FPS ở 4x và ≥20 FPS ở 6x); Wasm Heap 16 MB; Standard 4G đạt HOME_SHOWN trong 4.18s. Commit `0650fbd`. |
| **L6** | Sẵn sàng nối dữ liệu server: Hoàn thiện `IAccountService` với `FakeAccountService` và `ServerAccountService`. Viết `MD/PROFILE_CONTRACT.md`. Khớp với Codex R5 `MD/NET_PROTOCOL.md` và cập nhật `MD/PROTOCOL_MISMATCH.md`. Mua/trang bị online chờ server xác nhận, không trừ vàng cục bộ. | **ĐÃ XONG** | 171/171 tests pass (`ServerAccountServiceTests.cs`, `test-results-l6.xml`). Commit `08603ee`. |
| **L4** | Hướng dẫn tân thủ & Bot: Hướng dẫn cử chỉ 3 bước mở lần đầu hoặc khi click, có cờ lưu `HitMe_TutorialCompleted`. Cấu hình bot 3 mức (Dễ/Thường/Khó) tại `BotConfig.json`; Dễ nhắm lệch, Khó đoán vị trí từ lịch sử, không biết vị trí thật vòng hiện tại; test tất định bằng seed. | **ĐÃ XONG** | 175/175 tests pass (`Phase4BotDifficultyTests.cs`, `test-results-l4.xml`). Commit `3f28448`. |
| **L5** | Cảm giác chơi (Game feel): Rung nhẹ camera khi trúng đòn, hit-stop 40ms, chữ sát thương bay (-1 HP 💔), dừng va chạm biên, nảy ngã knockout. Mọi hiệu ứng tắt được qua `ReducedMotion` trong Cài đặt. | **ĐÃ XONG** | 177/177 tests pass (`GameFeelTests.cs`, `test-results-l5.xml`). Ảnh kiểm chứng tại `Evidence/round43/L5/`. Commit `5f029f2`. |



---

## 6. Phase 4, Lượt 4 (M1 – M8 Hoàn Tất) — hit-me-phase4-luot-4-ag.md

| Mã | Nhiệm vụ | Trạng thái | Ghi chú & Bằng chứng |
|:---:|---|:---:|---|
| **M1** | Chẩn đoán Git push, tạo bản sao lưu cục bộ và đẩy lên GitHub | **ĐÃ XONG** | Tạo `git bundle` (595.59 MB) tại `D:\HITME GAME\_backups\hitme_ui_ux_production_20261102.bundle`; tăng buffer `http.postBuffer 524288000` và push thành công nhánh `feature/ui-ux-production` lên GitHub remote; tài liệu `MD/GIT_PUSH_HELP.md`. |
| **M2** | Hồi quy toàn bộ E2E trên bản Release mới + Kịch bản vòng đời Tutorial | **ĐÃ XONG** | Chạy toàn bộ 10 kịch bản + K2-A (3 client WebGL thật) + K2-C (6 người trộn) + B4 (10 trận liên tiếp) + Kịch bản mới Tutorial Lifecycle (xóa cờ `HitMe_TutorialCompleted` -> hiện -> skip/done -> mở lại không hiện). 100% PASS, báo cáo `Evidence/round43/E2E_REGRESSION_REPORT.md`. |
| **M3** | Giảm dung lượng tải cho mạng yếu & UX màn tải tiến độ | **ĐÃ XONG** | Kiểm kê tài nguyên trong `data.br` (8.69 MB âm thanh WAV chưa nén); cập nhật màn tải hiển thị song song % và số MB đã tải (`10.1 MB`); thông báo tự động "Mạng đang chậm, vui lòng đợi thêm giây lát…" sau 15 giây; phân tích lộ trình tại `Evidence/round43/M3_size.md`. |
| **M4** | Làm rõ giới hạn số đo hiệu năng (GPU phần mềm vs thật) | **ĐÃ XONG** | Đo trực tiếp qua `WEBGL_debug_renderer_info`: SwiftShader Subzero Vulkan (CPU) vs AMD Radeon 780M Direct3D11 (GPU thật). Bổ sung mục 4 vào `Evidence/round43/L2_perf.md` phân định rõ chỉ số nào suy ra được và chỉ số nào bắt buộc thử trên điện thoại thật. |
| **M5** | Bộ đặc tả ảnh còn thiếu cho nghệ sĩ / AI | **ĐÃ XONG** | Lập `MD/ASSET_REQUEST_SHEET.md` quy chuẩn từng dòng: tên, item ID, kích thước 256×256, số pose/khung, nền trong suốt, góc nhìn 2.5D, hướng bay, viền đổ bóng và prompt Midjourney/SD mẫu. |
| **M6** | Chọn sân (Map) theo dữ liệu từ mockup | **ĐÃ XONG** | Tạo `Assets/Resources/Maps.json` gồm 2 sân đấu: `map_street_vietnam` (Phố Cổ Nắng Gắt) và `map_village_lotus` (Sân Đình Ao Sen); tích hợp `MapManager.cs` và nút chọn sân đổi mượt trong Lobby; kích thước sân và luật chơi giữ nguyên (9×11.4); tài liệu giao thức tại `MD/PROTOCOL_MISMATCH.md`. |
| **M7** | Báo lỗi trong game (Settings Bug Report) & Bộ đệm vòng | **ĐÃ XONG** | Nút "Báo lỗi" và nhãn phiên bản build trong Cài đặt; tích hợp `RingBufferLogger.cs` lưu 50 dòng log gần nhất, lọc sạch token, mật khẩu, bearer, reconnectToken và ID khách; tự động sao chép Markdown report vào clipboard; 182/182 EditMode tests PASS. |
| **M8** | Kiểm tra toàn diện trên máy chủ Cloud Render (HTTPS + WSS) | **ĐÃ XONG** | Chạy 5 kịch bản trọng yếu K7 trực tiếp trên `https://hitmegame.onrender.com` / `wss://hitmegame.onrender.com/ws`: Cold start 0.49s (HTTP 200), độ trễ trung vị vòng 0.25s, tỷ lệ thành công 5/5 (100% PASS), báo cáo `Evidence/round43/M8_RENDER_REPORT.md`. |
