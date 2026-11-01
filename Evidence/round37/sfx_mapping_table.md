# Bảng Ánh Xạ Sự Kiện → Tệp SFX (Cập Nhật Đợt 37)

| Sự kiện | Phương thức code | Tệp nguồn SFX (đường dẫn) | Trạng thái |
| :--- | :--- | :--- | :---: |
| Nhấn nút UI | `PlayClick()` | `Assets/Audio/sfx_ui_click.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Đặt dép (Place) | `PlayPlace()` | `Assets/Audio/sfx_tap_place.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Sẵn sàng (Ready) | `PlayReady()` | `Assets/Audio/sfx_ready.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Lộ diện (Reveal) | `PlayReveal()` | `Assets/Audio/sfx_reveal.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Ném dép (Throw) | `PlayThrow()` | `Assets/Audio/sfx_throw.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Trúng đòn (Hit) | `PlayHit()` | `Assets/Audio/sfx_hit_bop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Nhận sát thương (Damage) | `PlayDamage()` | `Assets/Audio/sfx_heart_break.wav` → fallback `sfx_hit_bop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Loại khỏi cuộc (Eliminate/Death) | `PlayDeath()` | `Assets/Audio/sfx_eliminate.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Đếm ngược (Tick) | `PlayTick()` / `PlayCountdownTick()` | `Assets/Audio/sfx_countdown_tick.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Đếm ngược cuối (Last) | `PlayCountdownLast()` | `Assets/Audio/sfx_countdown_last.wav` → fallback `sfx_countdown_tick.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Thắng (Win) | `PlayWin()` | `Assets/Audio/sfx_win.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Thua (Lose) | `PlayLose()` | `Assets/Audio/sfx_lose.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Mua sắm thành công | `PlayPurchase()` | `Assets/Audio/sfx_purchase.wav` | ✅ CÓ TỆP & ĐÃ GẮN (Shop) |
| Tiền xu / Phần thưởng | `PlayCoin()` | `Assets/Audio/sfx_coin.wav` | ✅ CÓ TỆP & ĐÃ GẮN (Shop/Reward) |
| Lỗi / Thao tác sai | `PlayError()` | `Assets/Audio/sfx_error.wav` | ✅ CÓ TỆP & ĐÃ GẮN (Error Toast/Shop) |
| Biểu cảm (Emote pop) | `PlayChatPop()` | `Assets/Audio/sfx_chat_pop.wav` | ✅ CÓ TỆP & ĐÃ GẮN (Emote/Online) |
| Đụng sàn / Tường | `PlayWallThud()` | `Assets/Audio/sfx_wall_thud.wav` | ✅ CÓ TỆP & ĐÃ GẮN (Weapon Miss) |
| Dép bay (Flight) | `PlayFlight()` | *Không có tệp nguồn* | ❌ TẠM (sfxFlightMissing = null) |
| BGM Menu | `PlayMenuBgm()` | `Assets/Audio/bgm_menu_loop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| BGM Trận đấu | `PlayMatchBgm()` | `Assets/Audio/bgm_match_loop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Tiếng ồn đám đông | `PlayAmbience()` | `Assets/Audio/amb_crowd_murmur_loop.wav` | ⚠️ BGM Loop nền |

## Tóm tắt R0
- **Dùng tệp thật từ SFX:** 17/17 sự kiện có tệp .wav thật đã được gắn vào mã nguồn.
- **TẠM (thiếu tệp nguồn):** 1 — `sfxFlightMissing` (dép bay) → `null`, giữ im lặng, không tự tạo âm tổng hợp.
- **Không có bất kỳ âm thanh nào được tạo bằng mã tổng hợp (synth).**
