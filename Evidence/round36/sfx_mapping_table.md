# Bảng Ánh Xạ Sự Kiện → Tệp SFX

| Sự kiện | Phương thức code | Tệp nguồn SFX (đường dẫn) | Trạng thái |
| :--- | :--- | :--- | :---: |
| Nhấn nút UI | `PlayClick()` | `Assets/Audio/sfx_ui_click.wav` | ✅ CÓ TỆP |
| Đặt dép (Place) | `PlayPlace()` | `Assets/Audio/sfx_tap_place.wav` | ✅ CÓ TỆP |
| Sẵn sàng (Ready) | `PlayReady()` | `Assets/Audio/sfx_ready.wav` | ✅ CÓ TỆP |
| Lộ diện (Reveal) | `PlayReveal()` | `Assets/Audio/sfx_reveal.wav` | ✅ CÓ TỆP |
| Ném dép (Throw) | `PlayThrow()` | `Assets/Audio/sfx_throw.wav` | ✅ CÓ TỆP |
| Trúng đòn (Hit) | `PlayHit()` | `Assets/Audio/sfx_hit_bop.wav` | ✅ CÓ TỆP |
| Nhận sát thương (Damage) | `PlayDamage()` | `Assets/Audio/sfx_heart_break.wav` → fallback `sfx_hit_bop.wav` | ✅ CÓ TỆP |
| Loại khỏi cuộc (Eliminate/Death) | `PlayDeath()` | `Assets/Audio/sfx_eliminate.wav` | ✅ CÓ TỆP |
| Đếm ngược (Tick) | `PlayTick()` / `PlayCountdownTick()` | `Assets/Audio/sfx_countdown_tick.wav` | ✅ CÓ TỆP |
| Đếm ngược cuối (Last) | `PlayCountdownLast()` | `Assets/Audio/sfx_countdown_last.wav` → fallback `sfx_countdown_tick.wav` | ✅ CÓ TỆP |
| Thắng (Win) | `PlayWin()` | `Assets/Audio/sfx_win.wav` | ✅ CÓ TỆP |
| Thua (Lose) | `PlayLose()` | `Assets/Audio/sfx_lose.wav` | ✅ CÓ TỆP |
| Mua sắm (Purchase) | *(chưa gọi trong code)* | `Assets/Audio/sfx_purchase.wav` | ⚠️ CÓ TỆP, CHƯA DÙNG |
| Tiếng xu (Coin) | *(chưa gọi trong code)* | `Assets/Audio/sfx_coin.wav` | ⚠️ CÓ TỆP, CHƯA DÙNG |
| Lỗi (Error) | *(chưa gọi trong code)* | `Assets/Audio/sfx_error.wav` | ⚠️ CÓ TỆP, CHƯA DÙNG |
| Chat pop | *(chưa gọi trong code)* | `Assets/Audio/sfx_chat_pop.wav` | ⚠️ CÓ TỆP, CHƯA DÙNG |
| Đụng tường (Wall thud) | *(chưa gọi trong code)* | `Assets/Audio/sfx_wall_thud.wav` | ⚠️ CÓ TỆP, CHƯA DÙNG |
| Dép bay (Flight) | `PlayFlight()` | *Không có tệp nguồn* | ❌ TẠM (sfxFlightMissing = null) |
| BGM Menu | `PlayMenuBgm()` | `Assets/Audio/bgm_menu_loop.wav` | ✅ CÓ TỆP |
| BGM Trận đấu | `PlayMatchBgm()` | `Assets/Audio/bgm_match_loop.wav` | ✅ CÓ TỆP |
| Tiếng ồn đám đông | *(chưa gọi trong code)* | `Assets/Audio/amb_crowd_murmur_loop.wav` | ⚠️ CÓ TỆP, CHƯA DÙNG |

## Tóm tắt
- **Dùng tệp thật từ SFX:** 12/12 sự kiện có nguồn .wav
- **TẠM (thiếu tệp nguồn):** 1 — `sfxFlightMissing` (dép bay) → `null`, không phát âm thanh, không tự tổng hợp
- **Có tệp nhưng chưa dùng:** 5 — `sfx_purchase.wav`, `sfx_coin.wav`, `sfx_error.wav`, `sfx_chat_pop.wav`, `sfx_wall_thud.wav`
- **Không có AudioClip nào được tạo bằng mã tổng hợp (synth).** Tất cả đều trỏ đến tệp .wav thật hoặc null.

## Danh sách Thiếu
| ID | Sự kiện | Ghi chú |
| :--- | :--- | :--- |
| MISSING-01 | Dép bay (Flight SFX) | Cần tệp âm thanh dép xoay bay trong không khí. Hiện `sfxFlightMissing = null`. |
