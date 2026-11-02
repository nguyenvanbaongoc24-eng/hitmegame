# HIT ME — BÁO CÁO RÀ SOÁT PHASE 2 & PHÂN TÍCH NHỊP ĐỘ TRẬN (PHASE 2 AUDIT)

**Ngày lập:** 28/10/2026 (ICT)  
**Tác giả:** Antigravity (AG)  
**Phạm vi:** Rà soát Bước 0 (chỉ đọc), phân tích toán học nhịp độ trận đấu, kiểm kê tài nguyên đồ họa/âm thanh, đặc tả bố cục Mockup 1 và lập kế hoạch thực hiện Phase 2.

---

## 1. Hiện trạng Phase 1 (Nền tảng kỹ thuật đã chốt)

- **Git & Commit:** Nhánh `main`, commit `9dcaf41` mang tag `v0.1-graybox`.
- **Độ tin cậy Core:** **24/24 EditMode test pass (100%)**, biên dịch sạch, 0 warning/error.
- **AutoPlay:** 10/10 trận bot vs bot chạy batchmode thành công, 0 exception, 0 treo.
- **Thử nghiệm cảm ứng iPhone thật:** Đã chạy thử nghiệm bản build WebGL qua Wi-Fi nội bộ trên Safari iOS (iPhone). Thao tác chạm đặt vị trí, giữ và lướt kéo thước ngắm, hủy ngắm khi kéo về sát chân và khóa tự động sau 5 giây đều **ĐẠT**.
- **Lớp Pointer thống nhất:** Chuột và cảm ứng đi chung `PointerSample`, đã bổ sung cơ chế cách ly ngón chính bằng `_activeFingerId` để bảo vệ thao tác ngắm khỏi chạm đa điểm ngoài ý muốn.
- **NoAimPolicy:** Đã tích hợp sẵn enum `NoAimPolicy { ThrowFacing, NoThrow, RepeatLastAim }` vào `GameConfig` và `GameConfigAsset` (chờ chủ dự án chọn, mặc định hiện hành là `ThrowFacing`).

---

## 2. Phân tích nhịp độ trận (Pacing Analysis) — Bước B

### 2.1. Nguyên nhân gốc rễ (Toán học xác suất)
Mô phỏng 1.000 trận Phase 1 cho kết quả: **trung bình 24.35 vòng/trận (thực tế từng lên tới 87 vòng)** trong khi thiết kế HUD kỳ vọng ván chơi ngắn chỉ 5 vòng (`VÒNG x/5`).

Nguyên nhân gốc rễ:
1. **Kích thước sân so với nhân vật:** Sân có diện tích $10 \times 16 = 160\text{ unit}^2$, trong khi bán kính người chơi là $R = 0.45\text{ unit}$ (tiết diện va chạm đường kính $0.9\text{ unit}$).
2. **Xác suất trúng suy giảm nghiêm trọng khi số người sống giảm:**
   - **6 người sống:** $37.62\%$ cơ hội trúng mỗi lượt ném.
   - **5 người sống:** $27.06\%$ cơ hội trúng.
   - **4 người sống:** $20.65\%$ cơ hội trúng.
   - **3 người sống:** $15.75\%$ cơ hội trúng.
   - **2 người sống (1v1 Duel cuối):** Rơi xuống chỉ còn **$7.80\%$**!
3. **Hiện tượng "Bắn trượt bất tận" ở vòng cuối:** Ở giai đoạn 1v1, hai nhân vật có tổng cộng 6 HP (mỗi người 3 HP). Với xác suất trúng chỉ $7.8\%$ mỗi lượt, giai đoạn đấu tay đôi 1v1 trung bình phải tốn:
   $$\frac{6 \text{ lần trúng}}{2 \times 0.078 \text{ trúng/vòng}} \approx 38.4 \text{ vòng đấu riêng cho 2 người!}$$
   Đây chính là lý do trận đấu bị kéo dài lê thê tới 50–87 vòng.

---

### 2.2. Bảng so sánh mô phỏng 1.000 trận cho các phương án

Dưới đây là kết quả đo đạc thực nghiệm 1.000 trận độc lập cho từng phương án (dữ liệu lưu tại `MD/pacing_sim_results.json`):

| Phương án | Kích thước sân | Bán kính $R$ | Giới hạn vòng (`maxRounds`) | Vòng TB | Vòng Min | Vòng Max | Tỉ lệ Hòa | Thời gian ước tính/ván |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Baseline (Hiện trạng)** | $10 \times 16$ | 0.45 | Không giới hạn | **24.35** | 6 | **74** | 11.1% | 4 – 15 phút (Quá dài!) |
| **Phương án 1A** | $10 \times 16$ | 0.45 | **5 vòng** (Xếp theo HP) | **5.00** | 5 | **5** | 16.0% | **~50 – 60 giây** |
| **Phương án 1B** | $10 \times 16$ | 0.45 | **8 vòng** (Xếp theo HP) | **8.00** | 6 | **8** | 11.2% | **~1.5 phút** |
| **Phương án 1C** | $10 \times 16$ | 0.45 | **10 vòng** (Xếp theo HP) | **9.96** | 6 | **10** | 8.3% | **~2 phút** |
| **Phương án 2A (Thu nhỏ sân 25%)** | $7.5 \times 12$ | 0.45 | Không giới hạn | **18.65** | 5 | **74** | 11.2% | ~3 – 12 phút |
| **Phương án 2B (Tăng kích thước $R$)** | $10 \times 16$ | 0.60 | Không giới hạn | **18.65** | 5 | **74** | 11.2% | ~3 – 12 phút |
| **Phương án 3 (Bot Predictor/Center)** | $10 \times 16$ | 0.45 | Không giới hạn | **15.86** | 4 | **51** | 4.3% | ~2.5 – 8 phút |
| **Phương án 4 (Kết hợp 1A + Thu nhỏ 20%)** | $8.0 \times 12.8$ | 0.45 | **5 vòng** (Xếp theo HP) | **5.00** | 4 | **5** | **13.9%** | **~55 giây** |
| **Phương án 5 (Kết hợp 1B + Thu nhỏ 20%)** | $8.0 \times 12.8$ | 0.45 | **8 vòng** (Xếp theo HP) | **7.98** | 4 | **8** | **10.0%** | **~1.5 phút** |

---

### 2.3. Khuyến nghị giải pháp nhịp độ (Chờ chủ dự án quyết định)

1. **Khuyến nghị chính (Khuyến nghị 1 - Đề xuất tối ưu nhất): Phương án 4**
   - **Quy tắc:** Giới hạn tối đa **5 vòng** (`maxRounds = 5`) kết hợp thu nhỏ sân nhẹ 20% ($8.0 \times 12.8\text{ unit}$, tỉ lệ vừa khít với viewport camera màn hình 9:19.5).
   - **Cách tính thắng:** Hết 5 vòng, người có HP cao nhất thắng. Nếu bằng HP, xét người có số lần ném trúng (`HitsCount`) cao hơn; nếu vẫn bằng nhau thì đồng hạng.
   - **Thời lượng:** Mỗi vòng 11 giây (5s đặt + 2s lộ diện/ném + 4s kết quả) $\times$ 5 vòng = **~55–60 giây/trận**. Chuẩn nhịp độ "chơi nhanh" cho game mobile casual. Khớp 100% với hiển thị `VÒNG x/5` trên Mockup 1.
2. **Khuyến nghị dự phòng (Khuyến nghị 2): Phương án 1B**
   - Giữ nguyên kích thước sân hiện tại ($10 \times 16$), áp dụng `maxRounds = 8`.
   - Ván đấu kết thúc dứt điểm trong 8 vòng (~1.5 phút), giảm tỉ lệ hòa HP xuống 11.2%.
3. **Phân tích cơ chế Sudden Death (Sân co dần):**
   - Từ Vòng 4 trở đi, 4 cạnh sân thu hẹp vào trong 15% mỗi vòng; hoặc ở vòng 5, tất cả người chơi còn sống bị ép vào vòng tròn hoa sen trung tâm ($R = 2.5\text{ unit}$).
   - Cơ chế này tạo kịch tính cao trào và đẩy tỉ lệ trúng lên >65% ở vòng cuối, giải quyết triệt để vấn đề "bắn trượt khi ít người".

---

## 3. Kiểm kê tài nguyên (Asset Inventory) cho màn trận đấu — Bước C

### 3.1. Nhân vật (Characters)
| Nhân vật | Thư mục nguồn | Định dạng | Trạng thái kỹ thuật & Đánh giá | Hành động cần xử lý ở Bước 1 |
| :--- | :--- | :--- | :--- | :--- |
| **Nam Làng** | `ASSET HITME FINAL\NAM LÀNG` | 4 bộ sprite tách rời (`BÌNH THƯỜNG`, `CẦM DÉP`, `CẦM CHẢO`, `CẦM VỢT`) | Nền RGBA trong suốt, kích thước đồng nhất. Có metadata.json. | Đặt pivot ở chân (bottom-center), tạo Animator: Idle, Throw, Hit, Win, Die. |
| **Anh Nón Lá** | `ASSET HITME FINAL\ANH NÓN LÁ` | Thư mục spritesheet 1536×1024 | Tách lớp chuẩn, nét vẽ nhất quán. | Cắt sprite sheet đều, đặt pivot ở chân. |
| **Anh Núi** | `ASSET HITME FINAL\ANH NÚI` | Spritesheet 1448×1086 | Tách lớp chuẩn, tư thế ném và biểu cảm rõ. | Cắt sprite sheet theo grid, chuẩn hóa PPU. |
| **Mai Hồng** | `ASSET HITME FINAL\MAI HỒNG` | Spritesheet 1536×1024 | Nét vẽ chibi dễ thương, chuẩn hoạt họa. | Cắt sprite sheet, cấu hình animation clip. |
| **Chú Chó Vàng / Mèo Mun** | `ASSET HITME FINAL\` | Spritesheet 1536×1024 | Nhân vật thú cưng linh vật độc đáo. | Cắt sprite sheet, làm nhân vật số 5. |
| **Người chơi thứ 6** | *Không có asset riêng* | Tái sử dụng | **Theo đúng chỉ thị:** Không vẽ nhân vật mới; dùng lại Nam Làng hoặc Anh Nón Lá kèm **Color Tint / Đổi màu áo** để phân biệt. |

### 3.2. Vũ khí (Weapons) & Hiệu ứng (VFX)
| Tên Asset | Nguồn | Đánh giá | Hành động Phase 2 |
| :--- | :--- | :--- | :--- |
| **Dép xanh (Sandal)** | `ASSET WEAPONS\DÉP` | Đã có sprite dép đơn lẻ xoay các góc | Làm projectile quay khi bay theo `weaponSpeed`. |
| **Chảo & Vợt Pickleball**| `ASSET WEAPONS` | Có đủ các góc nghiêng | Sẵn sàng cho slot đổi vũ khí. |
| **VFX Trúng đòn (Hit Bop)** | `ASSET EFFECT\...\01_Impact_Soft` | Sprite sheet 3×2 1536×1024 | Tạo prefab `HitFx_Soft` phát khi projectile va chạm. |
| **VFX Chí mạng (Critical)**| `ASSET EFFECT\...\02_Impact_Critical`| Sheet 3×2 | Prefab `HitFx_Crit` phát khi trúng đòn kết liễu. |
| **VFX Khói xuất hiện/biến mất**| `ASSET EFFECT\...\14_Smoke_Disappear`| Sheet 3×2 | Phát khi nhân vật Lộ diện (Reveal) hoặc bị loại. |
| **VFX Sao choáng / Tim vỡ**| `ASSET EFFECT\...\16_Spinning_Stars` | Sheet 3×2 | Hiển thị trên đầu nhân vật bị trừ HP. |
| **VFX Pháo hoa mừng thắng**| `ASSET EFFECT\...\15_Confetti_Celebration`| Sheet 3×2 | Phát ở pha MatchEnd khi người chơi thắng. |

### 3.3. Bối cảnh đấu trường (Arena Layers)
| Layer | Nguồn file | Kích thước | Vai trò trong sân 2.5D |
| :--- | :--- | :--- | :--- |
| **L01: Bầu trời & Mái đình xa** | `ASSET BỐI CẢNH\HITME_A01, A02, A03` | PNG RGBA | Nền tĩnh trên cùng phía sau cổng đình. |
| **L02: Cổng đình & Cột đá** | `layer_02_top_gate_header.png` | 1024 × 291 | Khung kiến trúc chặn phía trên của sân. |
| **L03: Đèn lồng & Cờ hội** | `HITME_B01, B02, B03` | PNG RGBA | Treo hai bên cổng, có nhún nhẹ. |
| **L04: Sân cát & Decal Sen** | `layer_04_playable_sand_court.png` | 921 × 1076 | **Mặt sân chơi chính:** Khớp với tọa độ Core $X[-5, 5], Y[-8, 8]$. |
| **L05: Khán giả hai bên** | `HITME_C07, C08` | PNG RGBA | Đặt ở 2 rìa ngoài biên sân, không cản trở ném. |
| **L06: Bậc thềm tiền cảnh** | `layer_06_bottom_steps_foreground.png` | 1024 × 223 | Che chắn phía dưới chân, tạo chiều sâu 2.5D. |

### 3.4. Giao diện (UI Kit 9-Slice) & Font tiếng Việt
- **UI Kit:** Toàn bộ 18 asset PNG sạch text (2x Retina, 8px safe padding, không dính chữ) đã tạo ở `ASSET HITME FINAL\UI_KIT_EXTRACTED\` và `HITME_UI_ARENA_PACKAGE\UI_Elements_PNG\`:
  - `ui_banner_wood.png`: Plaque tiêu đề gỗ.
  - `ui_btn_cta_yellow.png`: Nút SẴN SÀNG lớn có bevel 3D và sheen highlight.
  - `ui_card_slot_selected_cyan.png` & `normal`: Khung thẻ người chơi trên HUD.
  - `ui_timer_circle_gold.png`: Vòng đếm ngược đồng hồ tròn.
  - `ui_hud_pill_dark.png`: Banner trạng thái "CHỌN HƯỚNG NÉM | Đối thủ đang ẩn".
  - `ui_player_foot_ring_cyan.png`: Vòng tròn dưới chân người chơi.
- **Font tiếng Việt:**
  - Font gốc của Graybox thiếu glyphs tiếng Việt gây mất dấu trên Safari/WebGL.
  - Đã có sẵn font chuyên dụng chuẩn tiếng Việt **`BeVietnamPro`** (Black, Bold, ExtraBold, Medium) tại `worktrees/ag-visual-ui/Assets/HITME/Fonts/`.
  - **Giải pháp:** Ở Bước 1, copy `BeVietnamPro` vào `unity-client/Assets/Fonts/`, dùng TextMeshPro Font Asset Creator nạp sẵn **Static Atlas chứa toàn bộ 134 ký tự tiếng Việt có dấu**. Tuyệt đối không dùng Dynamic OS Font để tránh lỗi font trên iOS Safari WebGL.

### 3.5. Âm thanh (Audio / SFX)
Đầy đủ **20/20 file audio WAV** chất lượng cao đã có sẵn tại `D:\HITME GAME\unity-client\Assets\Audio\`:
- BGM: `bgm_match_loop.wav`, `amb_crowd_murmur_loop.wav`.
- SFX: `sfx_tap_place.wav`, `sfx_ready.wav`, `sfx_countdown_tick.wav`, `sfx_countdown_last.wav`, `sfx_reveal.wav`, `sfx_throw.wav`, `sfx_hit_bop.wav`, `sfx_heart_break.wav`, `sfx_eliminate.wav`, `sfx_win.wav`, `sfx_lose.wav`.
- Không thiếu bất kỳ âm thanh cơ bản nào.

---

## 4. Bảng Bố Cục Mockup 1 (Đặc tả màn hình dọc 9:19.5 — 390 × 844)

Canvas Scaler: `Scale With Screen Size`, Reference Resolution: `390 × 844`, Match: `0.5`.

| Vùng UI / Layer | Tỉ lệ dọc (%) | Tọa độ $Y$ (từ trên xuống) | Thành phần chi tiết | Sprite / Prefab sử dụng |
| :--- | :---: | :---: | :--- | :--- |
| **Top Safe Area** | 0% – 5.2% | 0 – 44 px | Dynamic Island / Notch tai thỏ | Trống, không đặt UI tương tác |
| **Top Bar HUD** | 5.2% – 11.5% | 44 – 96 px | - Góc trái: Pill "VÒNG 1/5"<br>- Giữa: Đồng hồ tròn 68×68 kèm số giây<br>- Phải: Nút tròn Cài đặt 44×44 | `ui_hud_pill_dark.png`<br>`ui_timer_circle_gold.png`<br>`ui_btn_circle_dark.png` |
| **Player Roster Row** | 11.5% – 20.5% | 96 – 172 px | Dải 6 thẻ người chơi (kích thước mỗi ô 56×76px, cách nhau 4px). Thẻ gồm: avatar, nhãn tên, 3 biểu tượng tim đỏ. Thẻ của BẠN có viền cyan. | `ui_card_slot_selected_cyan.png`<br>`ui_card_slot_normal.png`<br>Prefab `PlayerCardHUD` |
| **Status Banner** | 20.5% – 24.5% | 172 – 206 px | Thanh ngang bo góc hiển thị text động: *"CHỌN HƯỚNG NÉM \| Đối thủ đang ẩn"* | `ui_hud_pill_dark.png` |
| **Arena Viewport (Sân)** | 24.5% – 85.5% | 206 – 720 px | Vùng sân cát hoa sen 2.5D. Nhân vật di chuyển, ngắm ném, thước ngắm vàng, dép bay. Chiếm **61% chiều cao màn hình**. | `layer_04_playable_sand_court`<br>Prefab `PlayerView`<br>Prefab `WeaponView` |
| **Bottom Control Bar** | 85.5% – 95.5% | 720 – 806 px | - Nút Chat tròn 48×48<br>- Ô chọn vũ khí (Dép) 58×64<br>- **Nút SẴN SÀNG** capsule vàng lớn 168×56<br>- Nút Emoji tròn 48×48 | `ui_btn_circle_dark.png`<br>`ui_card_slot_normal.png`<br>`ui_btn_cta_yellow.png` |
| **Bottom Safe Area** | 95.5% – 100% | 806 – 844 px | Home Indicator của iPhone | Padding an toàn, không để nút tràn vào |

---

## 5. Rủi ro kỹ thuật & Giải pháp WebGL / Mobile

1. **Hiển thị tiếng Việt trên Safari WebGL:**
   - *Rủi ro:* Safari không hỗ trợ dynamic font generation mượt mà; font hệ thống trên iOS không đồng nhất khiến chữ tiếng Việt bị mất dấu hoặc vỡ layout.
   - *Giải pháp:* Nạp toàn bộ 134 ký tự tiếng Việt hoa/thường vào **Static Atlas TextMeshPro** của font `BeVietnamPro`. Khi chạy WebGL, texture atlas đã có sẵn chữ, không phụ thuộc vào hệ điều hành.
2. **Bộ nhớ Texture trên iOS Safari (Giới hạn Canvas WebGL):**
   - *Rủi ro:* Safari trên iPhone giới hạn bộ nhớ WebGL canvas ở mức ~512MB; nếu nạp ảnh gốc 4K không nén sẽ bị crash trình duyệt.
   - *Giải pháp:* Đóng gói các sprite thành 4 Sprite Atlas (`UI_Atlas`, `Characters_Atlas`, `VFX_Atlas`, `Env_Atlas`), kích thước tối đa 2048×2048, bật nén ASTC (Mobile) / DXT5 (Desktop WebGL), tắt Mipmap trên toàn bộ UI và sprite 2D.
3. **Thao tác ngắm ném bị che bởi ngón tay:**
   - *Rủi ro:* Ngón tay che mất vòng tròn nhân vật khi ngắm.
   - *Giải pháp:* Vòng cảm ứng lớn hơn nhân vật 1.75 lần (`touchRadius = 0.78 unit`), thước ngắm vàng vẽ bắt đầu từ mép ngoài của ngón tay kéo dài ra phía trước, đầu mũi tên có nón định hướng rõ ràng.

---

## 6. Kế hoạch triển khai Phase 2 (Bước 1 → Bước 6)

- **Bước 1 (Nhập asset & Prefab):** Copy font `BeVietnamPro`, tạo TextMeshPro Static Atlas tiếng Việt; cắt sprite sheet 5 nhân vật + dép; tạo Animator cơ bản (Idle, Throw, Hit, Die, Win); tạo Prefab `PlayerView`, `WeaponView`, `HitFx`.
- **Bước 2 (Dựng Sân Arena 2.5D):** Ghép 6 layer sân bối cảnh (cổng đình, sân cát hoa sen, viền đá, cây cối, tiền cảnh). Đảm bảo hàm chuyển đổi `GameToWorld` / `WorldToGame` khớp tuyệt đối với tọa độ Core của Phase 1.
- **Bước 3 (Dựng HUD trận đấu):** Dựng Canvas Scaler `390 × 844` theo bảng bố cục mục 4: Top Bar (Plaque vòng + Timer tròn), dải 6 thẻ người chơi có 3 tim, thanh trạng thái, Bottom Bar (Nút SẴN SÀNG vàng).
- **Bước 4 (Game Feel & Visual Polish):** Thước ngắm nón + mũi tên khi kéo; dép xoay khi bay; flash trắng và sao khi trúng đòn; tim vỡ; màn kết quả Thắng/Thua.
- **Bước 5 (Tích hợp Âm thanh):** Gắn 20 file SFX/BGM vào `AudioManager` 2 kênh (Nhạc / Hiệu ứng), đồng bộ tiếng dép bay, va chạm, đếm ngược và reo hò chiến thắng.
- **Bước 6 (Tối ưu & Build Evidence):** Tạo Sprite Atlas, build WebGL + Windows test, quay video/chụp ảnh bằng chứng hoàn thành Phase 2.

---

> ⛔ **KẾT THÚC BƯỚC C (BƯỚC 0 RÀ SOÁT).**  
> Bản báo cáo này đã hoàn thiện đầy đủ dữ liệu mô phỏng toán học, kiểm kê tài nguyên và layout spec. Toàn bộ code Phase 1 vẫn được giữ nguyên vẹn. Antigravity **DỪNG TẠI ĐÂY** để chờ Chủ Dự Án xem xét, chọn phương án nhịp độ trận đấu và phê duyệt trước khi bắt đầu Bước 1 của Phase 2!
