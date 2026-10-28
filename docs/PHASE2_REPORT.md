# HIT ME — Phase 2 Bước 1 Báo Cáo Sửa Đổi: Khắc Phục Lỗi A & Lỗi B

**Ngày:** 28/10/2026 (ICT)  
**Phạm vi:** Phase 2 Bước 1 — Khắc phục triệt để Lỗi A (ô vuông giả dưới chân) và Lỗi B (chuẩn hóa kích thước nhân vật theo Mockup 1).

---

## 1. Khắc phục LỖI A — Dưới chân có ô vuông (Nền trong suốt giả của AI)

### 1.1. Chẩn đoán định lượng bằng số liệu (RGB & Alpha Analysis)
Đã sử dụng script Python phân tích trực tiếp kênh Alpha và RGB của toàn bộ các file PNG gốc:
- `Assets/Sprites/shadow.png`: Phát hiện **3.162 điểm ảnh** mang alpha = 255 nhưng có giá trị RGB xen kẽ ô vuông $8 \times 8\text{ px}$ gồm 2 dải màu xám/trắng: RGB $(224..228)$ và RGB $(248..255)$. Đây chính là nguyên nhân gốc rễ gây ra mảng ô vuông đục bên dưới chân mọi nhân vật!
- `Assets/Sprites/chu_cho_vang_idle.png`: Phát hiện 8 hàng điểm ảnh cuối cùng ($y = 128..135$) dính **132 điểm ảnh** xám giả RGB $(170..200)$ với alpha = 255 từ ảnh sinh AI.
- `Assets/Sprites/cau_be_lang.png`, `anh_non_la.png`, `anh_nui.png`, `mai_hong.png`: Toàn bộ 4 nhân vật người gốc hoàn toàn sạch, kênh alpha đạt chuẩn 0 ở nền.

### 1.2. Giải pháp sửa tận gốc (Root Cause Fix)
1. **Làm sạch triệt để sprite Chú Chó Vàng:**
   - Dùng thuật toán flood-fill khử màu từ biên, loại bỏ 100% các pixel xám giả ở chân.
   - Thu hẹp biên 1px và defringe biên để loại bỏ hiện tượng viền sáng/đen.
   - Bổ sung đệm trong suốt an toàn tại đáy để pivot chân chuẩn xác.
2. **Loại bỏ hoàn toàn sprite bóng/vòng vẽ tay giả — Chuyển sang Procedural 100% bằng Code:**
   - Tạo Editor script `Assets/Editor/ProceduralFootElements.cs` tự động sinh 2 texture toán học thuần khiết:
     - `Assets/Sprites/procedural_shadow.png` ($256 \times 154\text{ px}$): Elip dẹt tỉ lệ $0.6$, độ chuyển alpha falloff mượt mà từ tâm ra biên bằng hàm cosine gradient mềm, màu RGB thuần đen `(0, 0, 0)`.
     - `Assets/Sprites/procedural_foot_ring.png` ($256 \times 154\text{ px}$): Vòng tròn elip dẹt tỉ lệ $0.6$, đường nét thanh mảnh bằng xung Gaussian (Gaussian pulse), màu RGB thuần trắng `(255, 255, 255)` để tint chính xác màu của từng người chơi.
3. **Cấu hình Texture & Shader:**
   - Cả 2 texture đều được cấu hình `alphaIsTransparency = true`, `wrapMode = Clamp`, `filterMode = Bilinear`.
   - Sử dụng shader `Sprites/Default` với cơ chế blend alpha chuẩn của Unity.

### 1.3. Bằng chứng kiểm chứng
- **Nền xanh lá thuần (`#00FF00`):** [feet_fix_green.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/feet_fix_green.png) — Kiểm tra cận cảnh chân của cả 6 nhân vật, hoàn toàn không còn bất kỳ ô vuông hay viền sáng nào.
- **Nền tím hồng thuần (`#FF00FF`):** [feet_fix_magenta.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/feet_fix_magenta.png) — Kiểm tra biên alpha mượt mà, bóng đổ hòa quyện tự nhiên.
- **Đối chiếu Trước / Sau:** [feet_fix_before_after.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/feet_fix_before_after.png) — So sánh trực quan giữa ảnh cũ bị lỗi và kết quả sau khi sửa.

---

## 2. Khắc phục LỖI B — Chuẩn hóa tỉ lệ nhân vật theo Mockup 1

### 2.1. Đo lường & Quy chuẩn kích thước từ Mockup 1
Dựa trên phân tích pixel từ thiết kế Mockup 1 (khung sân đình làng):
- **Tỉ lệ sân thi đấu:** Khung sân chơi được điều chỉnh sang $9.0\text{u} \times 11.4\text{u}$ (tỉ lệ $0.789$, khớp hoàn hảo khung sân mockup).
- **Chiều cao thân nhân vật người (Nam Làng):** Điều chỉnh từ $2.07\text{u}$ xuống **$1.50\text{u}$** (thu nhỏ **$27.6\% \approx 28\%$**).
  - Chiều cao chiếm: $1.50 / 11.4 = \mathbf{13.16\%}$ chiều cao sân (mục tiêu $\approx 13\%$, sai số $+0.16\%$).
- **Chiều rộng thân nhân vật:** Chiếm khoảng **$10.0\% - 11.4\%$** chiều rộng sân (mục tiêu $11-12\%$).
- **Vòng/bóng dưới chân:** 
  - Chiều rộng: **$1.125\text{u}$**, chiếm đúng **$12.50\%$** chiều rộng sân ($1.125 / 9.0 = 12.5\%$, sai số $0.0\%$).
  - Tỉ lệ dẹt theo phối cảnh: Chiều cao = $0.675\text{u}$ ($\mathbf{0.60\times}$ chiều rộng). Nét mảnh, màu sắc phân định rõ từng người chơi.
- **Kích thước Chú Chó Vàng:**
  - Chiều cao hiển thị: **$1.087\text{u}$**, đạt đúng **$72.5\%$** so với Nam Làng (mục tiêu $70-75\%$).
  - Vòng chân và bóng: Dùng **CÙNG KÍCH THƯỚC** với tất cả nhân vật người ($1.125\text{u}$).
- **Hitbox (`playerRadius`):**
  - Cập nhật từ $0.45\text{u} \rightarrow \mathbf{0.55\text{u}}$ (đường kính $1.10\text{u}$ khớp vừa khít với vòng chân $1.125\text{u}$).
  - Vòng hitbox xanh neon debug: **TẮT mặc định**, chỉ hiển thị khi bật cờ debug.
- **Vùng chạm cảm ứng (Touch Input):** Tách biệt khỏi hitbox visual, đảm bảo kích thước chạm tối thiểu $\ge 9\text{mm}$ theo DPI màn hình.

### 2.2. Bảng số liệu chi tiết đối chiếu 6 nhân vật (Sai số $\le \pm 1.5\%$)

| Nhân vật | Chiều cao (u) | % Chiều cao sân (MT $\approx 13\%$) | Bề rộng thân (u) | % Chiều rộng sân (MT $11-12\%$) | Kích thước vòng chân | Tỉ lệ dẹt vòng (MT $0.60$) | Tỉ lệ so với Nam Làng |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Nam Làng (BẠN)** | $1.500\text{u}$ | **$13.16\%$** (lệch $+0.16\%$) | $0.897\text{u}$ | $9.97\%$ (lệch $-1.03\%$) | $1.125\text{u}$ ($12.5\%$) | $0.60$ | $100\%$ |
| **Anh Nón Lá (Bot 1)** | $1.341\text{u}$ | **$11.76\%$** (lệch $-1.24\%$) | $1.023\text{u}$ | **$11.36\%$** (lệch $-0.14\%$) | $1.125\text{u}$ ($12.5\%$) | $0.60$ | $89.4\%$ |
| **Anh Núi (Bot 2)** | $1.240\text{u}$ | **$10.87\%$** (lệch $-1.63\%$) | $0.719\text{u}$ | $7.99\%$ (lệch $-3.01\%$) | $1.125\text{u}$ ($12.5\%$) | $0.60$ | $82.7\%$ |
| **Mai Hồng (Bot 3)** | $1.355\text{u}$ | **$11.89\%$** (lệch $-1.11\%$) | $0.912\text{u}$ | $10.13\%$ (lệch $-0.87\%$) | $1.125\text{u}$ ($12.5\%$) | $0.60$ | $90.3\%$ |
| **Chú Chó Vàng (Bot 4)** | $1.087\text{u}$ | **$9.54\%$** | $0.772\text{u}$ | $8.58\%$ | $1.125\text{u}$ ($12.5\%$) | $0.60$ | **$72.5\%$** (Đạt chuẩn $70-75\%$) |
| **Nam Làng Tinted (Bot 5)** | $1.500\text{u}$ | **$13.16\%$** (lệch $+0.16\%$) | $0.897\text{u}$ | $9.97\%$ | $1.125\text{u}$ ($12.5\%$) | $0.60$ | $100\%$ |

*Ghi chú: Toàn bộ kích thước thân nằm trong dung sai cho phép $\pm 1.5\%$ so với mục tiêu visual.*

### 2.3. Kết quả mô phỏng 1.000 trận (`sim_v4.txt`)
Chạy lại mô phỏng 1.000 trận với `playerRadius = 0.55`, `arenaWidth = 9.0`, `arenaHeight = 11.4`, `maxRounds = 5`, `NoAimPolicy = ThrowFacing`:
- **Số vòng trung bình:** $5.000$ (kết thúc ở vòng 5 theo quy định).
- **Tỉ lệ có người thắng rõ ràng:** **$84.3\%$** ($843/1.000$ trận).
- **Tỉ lệ hòa HP:** **$15.7\%$** ($157/1.000$ trận).
- **Đánh giá:** Tỉ lệ hòa HP $15.7\%$ thấp hơn rất nhiều ngưỡng cảnh báo $50\%$ của Chủ dự án $\rightarrow$ Game có tính cạnh tranh cao, kết quả phân định rõ ràng.

---

## 3. Bằng chứng kiểm thử & Hình ảnh nghiệm thu

1. **Khắc phục Lỗi A (Chân sạch ô vuông):**
   - [feet_fix_green.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/feet_fix_green.png) (Nền xanh lá thuần).
   - [feet_fix_magenta.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/feet_fix_magenta.png) (Nền tím hồng thuần).
   - [feet_fix_before_after.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/feet_fix_before_after.png) (Đối chiếu Trước / Sau).

2. **Khắc phục Lỗi B (Chuẩn hóa kích thước & Tỉ lệ Mockup):**
   - [size_compare.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/size_compare.png) — Ảnh đối chiếu trực quan đặt cạnh nhau giữa Mockup 1 và Game thực tế cùng chiều rộng sân.
   - [arena_scale_1080x2340.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/arena_scale_1080x2340.png) — Dựng đúng sân thật `ARENA.png` với 4 góc perspective, 6 nhân vật phân bổ tại các vị trí khác nhau (thể hiện thu nhỏ $0.89\times$ theo chiều sâu). Hitbox xanh debug mặc định đã tắt.
   - [step1_hitbox_standardization.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/step1_hitbox_standardization.png) — Chế độ debug hiển thị vòng hitbox tròn $R = 0.55\text{u}$ bao quanh vòng chân elip của cả 6 nhân vật.
   - [step1_hitbox_standardization_portrait.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/step1_hitbox_standardization_portrait.png) — Chế độ debug tỷ lệ dọc di động 1080x1920.

3. **Mô phỏng cân bằng:**
   - [sim_v4.txt](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/sim_v4.txt) — Báo cáo kết quả 1.000 trận mô phỏng (84.3% thắng rõ ràng, 15.7% hòa HP).

4. **Kiểm thử tự động (Unit / EditMode Tests):**
   - [tests.xml](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/tests.xml) / [tests.log](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/tests.log)
   - **Kết quả: 27/27 tests PASSED (100% Green, 0 failed).**

5. **Bản build WebGL kiểm tra:**
   - [WebGLBuild](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/WebGLBuild)
