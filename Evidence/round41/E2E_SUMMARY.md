# BÁO CÁO KIỂM THỬ ĐẦU-CUỐI E2E (E2E_SUMMARY.md) — PHASE 3B

**Ngày thực hiện:** 02/11/2026  
**Môi trường:** Chrome Headless (Playwright 1.64) · WebGL Release Brotli (:8103) · Server C# Kestrel (:8085)  
**Thiết bị giả lập:** iPhone 14/15 Safari (390×844 @3x) & Android Pixel 7 Chrome (360×800 @2.75x)

---

## 1. Kết Quả 10 Kịch Bản E2E Tự Động (B2)

| STT | Kịch bản | Kết quả | Chi tiết kiểm chứng | Bằng chứng |
|---|---|---|---|---|
| 1 | Hai người chơi trọn trận | **PASS** | Tạo phòng, vào bằng mã, 2 máy ready, chơi 5 vòng, Result đồng bộ | `screens/01_sc1_*.png`, video |
| 2 | 3 người rồi 6 người; từ chối người 7 | **PASS** | Roster cập nhật đủ 6/6 người thật; người thứ 7 nhận lỗi `room_full` | `screens/02_sc2_lobby_6p_full.png` |
| 3 | Một mình bấm bắt đầu | **PASS** | Host 1 người bấm start bị từ chối với lỗi `not_enough_players` | `screens/03_sc3_solo_blocked.png` |
| 4 | Rớt mạng 10s rồi vào lại trận (30s grace) | **PASS** | Set offline, online lại với private reconnectToken, vào lại thành công | Console log + State |
| 5 | Cùng token mở tab thứ hai | **PASS** | Tab 1 nhận `session_replaced`, socket đóng an toàn, trở về Home | `screens/05_sc5_session_replaced.png` |
| 6 | AFK không commit 2 vòng | **PASS** | Server phát hiện AFK, loại và cấm đấu 120s (`banned`) | `screens/06_sc6_banned_afk.png` |
| 7 | Server tắt giữa trận | **PASS** | Server tắt đột ngột, client đóng socket sạch sẽ, không crash/treo | `screens/07_sc7_server_shutdown.png` |
| 8 | Token sai / thiếu token | **PASS** | Server từ chối ngay lập tức với `auth_required_use_join_token` / `auth_invalid` | `screens/08_sc8_auth_invalid.png` |
| 9 | Emote phát sóng tới người khác | **PASS** | P1 gửi emote 'happy', P2 nhận tức thì trên thẻ người chơi | `screens/09_sc9_emote_received.png` |
| 10 | Mạng chậm 3G (CDP Throttle) | **PASS** | Giả lập 750 kbps, tải HOME_SHOWN, vào phòng chơi bình thường | `screens/10_sc10_slow_3g.png` |

---

## 2. Kiểm Tra Độ Ổn Định 20 Trận Liên Tiếp (B4)

- **Tỷ lệ hoàn thành:** **20/20 trận PASS (100%)**
- **Quy mô phòng:** Luân phiên ngẫu nhiên 2, 3, 4, 6 người chơi.
- **Xu hướng bộ nhớ JS Heap:** Bắt đầu: 16.6 MB -> Kết thúc: 16.3 MB (Không rò rỉ bộ nhớ)
- **Lỗi JS / WebGL unhandled exception:** **0 lỗi**.

---

## 3. Các Giới Hạn Cần Kiểm Thử Trên Thiết Bị Thật (Hardware Boundary)

1. **Cảm ứng đa điểm phần cứng thật (Physical Multi-touch):** Trình duyệt giả lập mô phỏng touch event đơn/chuỗi điểm, chưa thể thay thế hoàn toàn tương tác hai ngón tay thực tế trên kính điện thoại.
2. **Hiệu năng GPU Mobile thật:** Thử nghiệm dùng SwiftShader Angle; cần kiểm tra FPS thực tế trên chip Apple A16/A17 và Snapdragon 8 Gen 2+.
3. **Web Audio trên Safari iOS:** Safari yêu cầu cử chỉ chạm người dùng đầu tiên để unlock AudioContext; cần kiểm chứng âm thanh không bị câm trên iOS thật.
4. **Thanh địa chỉ động (Dynamic URL Bar / Safe Area Notch):** Sự co giãn của thanh điều hướng Safari khi cuộn trang cần được kiểm tra trực quan trên màn hình tai thỏ / Dynamic Island thật.
