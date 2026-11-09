# Bảng Ánh Xạ Sự Kiện → Tệp SFX (Cập Nhật Đợt 38 — E1..E6)

| Sự kiện | Mã sự kiện (`sfxId` / Method) | Tệp nguồn SFX (đường dẫn) | Trạng thái |
| :--- | :--- | :--- | :---: |
| Nhấn nút UI / Chạm nhân vật Home | `ui_tap` / `PlayClick()` | `Assets/Audio/sfx_ui_click.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Đổi nhân vật / Vũ khí (`< >`) | `ui_tap` / `PlayClick()` | `Assets/Audio/sfx_ui_click.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Đặt nhân vật xuống sân (Place) | `place_pop` / `PlayPlace()` | `Assets/Audio/sfx_tap_place.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Kéo thước ngắm đổi góc (Aim tick) | `aim_tick` / `PlayTick()` | `Assets/Audio/sfx_countdown_tick.wav` (vol `0.35`) | ⚠️ TẠM (dùng `sfx_countdown_tick.wav` âm lượng nhỏ chờ tệp `sfx_aim_tick.wav` riêng) |
| Khóa vị trí / Sẵn sàng (Ready Lock) | `ready_lock` / `PlayReady()` | `Assets/Audio/sfx_ready.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Lộ diện đồng loạt (Reveal) | `reveal` / `PlayReveal()` | `Assets/Audio/sfx_reveal.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Ném Dép Tổ Ong (`dep_to_ong`) | `throw_dep` / `PlayThrow("dep_to_ong")` | `Assets/Audio/sfx_throw.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Ném Chảo Chống Dính (`chao_chong_dinh`) | `throw_chao` / `PlayThrow("chao_chong_dinh")` | `Assets/Audio/sfx_throw.wav` | ⚠️ TẠM (dùng chung `sfx_throw.wav` chờ tệp `sfx_throw_chao.wav`) |
| Ném Vợt Pickleball (`vot_pickleball`) | `throw_vot` / `PlayThrow("vot_pickleball")` | `Assets/Audio/sfx_throw.wav` | ⚠️ TẠM (dùng chung `sfx_throw.wav` chờ tệp `sfx_throw_vot.wav`) |
| Vũ khí đang bay trên không (Flight loop) | `sfxFlightMissing` / `PlayFlight()` | *Không có tệp nguồn* | ❌ TẠM (`null` — giữ im lặng, không dùng synth) |
| Trúng đòn Dép (`hit_dep`) | `hit_dep` / `PlayHit("dep_to_ong")` | `Assets/Audio/sfx_hit_bop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Trúng đòn Chảo (`hit_chao`) | `hit_chao` / `PlayHit("chao_chong_dinh")` | `Assets/Audio/sfx_hit_bop.wav` | ⚠️ TẠM (dùng chung `sfx_hit_bop.wav` chờ tệp tiếng chảo `sfx_hit_clang.wav`) |
| Trúng đòn Vợt (`hit_vot`) | `hit_vot` / `PlayHit("vot_pickleball")` | `Assets/Audio/sfx_hit_bop.wav` | ⚠️ TẠM (dùng chung `sfx_hit_bop.wav` chờ tệp `sfx_hit_vot.wav`) |
| Tim vỡ / Mất 1 HP (Heart Break) | `heart_break` / `PlayDamage()` | `Assets/Audio/sfx_heart_break.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Ném trượt / Đụng sàn (`HỤT!`) | `wall_thud` / `PlayWallThud()` | `Assets/Audio/sfx_wall_thud.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Bị loại khỏi cuộc (`0 HP` KO) | `ko` / `PlayDeath()` | `Assets/Audio/sfx_eliminate.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Đếm ngược 3 giây cuối (Tick) | `timer_tick` / `PlayCountdownTick()` | `Assets/Audio/sfx_countdown_tick.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Giây cuối đếm ngược (Last Tick) | `timer_end` / `PlayCountdownLast()` | `Assets/Audio/sfx_countdown_last.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Chiến thắng trận đấu (Victory) | `win` / `PlayWin()` | `Assets/Audio/sfx_win.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Kết thúc trận (Thua / Hoàn thành) | `lose` / `PlayLose()` | `Assets/Audio/sfx_lose.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Nhận thưởng Nhiệm vụ ngày / Xu | `coin` / `PlayCoin()` | `Assets/Audio/sfx_coin.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Mua vật phẩm (Shop) | `purchase` / `PlayPurchase()` | `Assets/Audio/sfx_purchase.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Cảnh báo / Máy chủ chưa sẵn sàng | `error` / `PlayError()` | `Assets/Audio/sfx_error.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Biểu cảm trong trận (Emote pop) | `chat_pop` / `PlayChatPop()` | `Assets/Audio/sfx_chat_pop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Nhạc nền Sân đình (Menu BGM) | `bgm_menu` / `PlayMenuBgm()` | `Assets/Audio/bgm_menu_loop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Nhạc nền Trận đấu (Match BGM) | `bgm_match` / `PlayMatchBgm()` | `Assets/Audio/bgm_match_loop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |
| Tiếng đám đông sân đình (Ambience) | `amb_crowd` / `PlayAmbience()` | `Assets/Audio/amb_crowd_murmur_loop.wav` | ✅ CÓ TỆP & ĐÃ GẮN |

## Bảng Thời Lượng Hiệu Ứng (E1–E6 Timing Spec)

| Mã | Hiệu ứng | Thời lượng (ms / s) | Cơ chế Pool / Zero-Alloc | Tắt khi bật Giảm chuyển động |
| :--- | :--- | :---: | :--- | :---: |
| **E1.a** | Nhân vật Home thở nhẹ (`r38-hero-breath`) | Chu kỳ `2400 ms` (`3.5px`) | CSS compositor (`transform`) | ✅ Có |
| **E1.b** | Chạm nhân vật Home nhún/vẫy (`tap-wave`) | `280 ms` | CSS class toggle trên `.hero` | ✅ Có |
| **E1.c** | Đổi nhân vật trượt + nảy (`switch-bounce` + bệ đá lóe sáng) | `220 ms` (`0.92 → 1.04 → 1.0`) | `#r38-pedestal-glow` tạo 1 lần | ✅ Có |
| **E1.d** | Ánh sáng quét ngang logo HIT ME (`r38-logo-shine`) | Chu kỳ `6800 ms` | Pseudo-element `::after` | ✅ Có |
| **E1.e** | Lá vàng / hoa mai rơi sân đình (`#r38-ambient-leaves`) | `5200–9600 ms` | Pool cố định `16` hạt (`<= 20`) | ✅ Có |
| **E1.f** | Xu vàng xoay / lấp lánh (`r38-coin-spin`) | Chu kỳ `3600 ms` | CSS transform | ✅ Có |
| **E2.a** | Đặt nhân vật nảy xuống sân + vòng nở (`#r38-place-fx`) | `180 ms` (`1.18 → 0.94 → 1.0`) | Node đơn `#r38-place-fx` tái sử dụng | ✅ Có |
| **E4.a** | Hit-stop khi trúng đòn (`PlayHitStop`) | `65 ms` (`<= 80 ms`) | Chỉ thị giác, không đổi mô phỏng | ✅ Có |
| **E4.b** | Rung màn hình khi trúng đòn (`r38-screen-shake`) | `120 ms` (`3.5px`) | CSS transform trên container | ✅ Có |
| **E4.c** | Thẻ HP rung + chữ nổi `"BỐP! -1 ❤️"` / `"HỤT!"` | `340 ms` / `580 ms` | `_damageTextPool` (6) + `#r38-hit-callout` (1) | ✅ Có |
| **E5.a** | Hoa giấy mừng chiến thắng (`#r38-confetti-pool`) | `1600–2720 ms` | Pool cố định `28` hạt (`<= 30`) | ✅ Có |
| **E6.a** | Viền đỏ nhấp nháy khi còn `1 HP` (`#r38-low-hp-vignette`) | Chu kỳ `1150 ms` | Node đơn `#r38-low-hp-vignette` | ✅ Có |
| **E6.b** | Rung xúc giác (`navigator.vibrate(25)` bọc `try/catch`) | `25 ms` | Có nút bật/tắt trong Cài đặt | ✅ Có |
