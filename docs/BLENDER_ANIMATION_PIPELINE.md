# HIT ME VN — QUY TRÌNH KỸ THUẬT BLENDER 5.2 ANIMATION PIPELINE
## Lead Technical Artist & 2.5D Mobile Pipeline Guide
**Dự án:** HIT ME VN (Rebuild 2026)  
**Phiên bản Blender:** Blender 5.2.2 LTS (Build hash `d13f752e3b9c`)  
**Công cụ tự động hóa:** `_AG_VISUAL_WORK/tools/hm_blender.py` & `_AG_VISUAL_WORK/tools/post_frames.py`  
**Đích xuất xưởng:** Unity 6 Sprite Sequence Animator (`Assets/HITME/Art/Animation/`)

---

## 1. TỔNG QUAN KIẾN TRÚC PIPELINE

Để đảm bảo hiệu năng tối đa cho nền tảng WebGL và Mobile (iPhone 15 portrait), HIT ME VN áp dụng giải pháp **Pre-rendered 2.5D Sprite Sequences**:
1. **Dựng hình & Biến dạng trong Blender 5.2 LTS:** Mô phỏng vật lý vải (cloth wave) và chuyển động tán cây (vegetation harmonic sway) bằng toán học giải tích chính xác.
2. **Cycles Batchmode Rendering:** Kết xuất trực tiếp ra chuỗi frame RGBA 32-bit (với kênh Alpha chuẩn, Transparent Background) không cần mở GUI.
3. **Post-Processing & Defringing:** Cân bằng sắc độ (color gain match), tẩy viền tối (alpha bleeding / defringe), và xác minh độ mượt của mối nối chu kỳ lặp (seamless loop seam check).
4. **Metadata & Schema Binding:** Tự động phát sinh file mô tả JSON (`*_anim.json`) để Unity `SpriteSequenceAnimator` nạp động mà không sinh rác bộ nhớ (Zero-GC).

```
[2D Source PNG / Mesh Plane]
           │
           ▼
[Blender 5.2 Python Script (Batchmode)] ──► Mesh Subdivision & Constraint Weight
           │
           ├─► Sine Wave / Directional Harmonic Displacement (Analytical Math)
           ├─► Sun Lamp Warm Rig (Energy 1.5, Procedural Noise Bump)
           └─► Cycles Engine (16 Samples, Denoise OIDN, RGBA PNG)
           │
           ▼
[Post-Processing Script]
           │
           ├─► Channel Color Matching (vs. Source Palette)
           ├─► Edge Alpha Defringing (Bleed outward 2px)
           └─► Seam Step Error Metric (< 1.5% max pixel delta)
           │
           ▼
[Unity Worktree: Assets/HITME/Art/Animation/]
           ├─► Frames: frame_0000.png ... frame_0015.png
           ├─► Metadata: *_anim.json
           └─► SpriteSequenceAnimator.cs (Unity 6 WebGL Zero-GC)
```

---

## 2. THIẾT LẬP KẾT XUẤT BLENDER 5.2 CYCLES

Tất cả các lệnh render đều được thực thi tự động qua CLI với cờ vô hình:
```powershell
& "D:\App\BLENDER\blender.exe" -b --python "tools/hm_blender.py" -- <animation_target>
```

### Cấu hình Engine Cycles chuẩn:
- **Engine:** `CYCLES`
- **Device:** `CPU` / `GPU` (tự động fallback nếu không có OptiX/CUDA)
- **Samples:** `16` samples (với OpenImageDenoise kích hoạt, chất lượng khử răng cưa tương đương 256 samples truyền thống nhưng thời gian render chỉ ~0.4s/frame).
- **Film Transparent:** `True` (nền trong suốt 100%, không bị ám đen môi trường).
- **Color Management:** `sRGB`, View Transform: `Standard` (giữ nguyên độ rực màu gốc của 2D concept, không dùng AgX/Filmic để tránh làm bạc màu tranh vẽ).

### Thiết lập Chiếu sáng (Lighting Rig):
- **Sun Light:** Góc xiên 45° từ trên xuống bên trái (`rot = (55°, 15°, -45°)`), cường độ `energy = 1.5` (đã hiệu chỉnh từ 2.4 để triệt tiêu hiện tượng lóa sáng specular blowout trên bệ hoa sen).
- **Ambient Light:** Đèn Sky mềm màu lam nhạt (`#E8F2FF`), cường độ `0.35` để nâng sáng các vùng bóng tối.

---

## 3. TOÁN HỌC BIẾN DẠNG MÔI TRƯỜNG (DEFORMATION MATH)

### A. Lá cờ hội bay (`flag_wave`)
- **Độ phân giải:** 256 × 192 px, 16 frames @ 14 FPS (Chu kỳ lặp 1.14s).
- **Yêu cầu kỹ thuật:** Cạnh bên trái (gắn vào cán cờ) phải hoàn toàn đứng yên; sóng vải lan truyền dần về phía đuôi cờ với biên độ tăng dần.
- **Công thức biến dạng đỉnh (Vertex Displacement):**
  Cho đỉnh có tọa độ chuẩn hóa $u \in [0, 1]$ dọc theo chiều dài cờ (với $u=0$ tại cán cờ, $u=1$ tại đuôi cờ), tại frame $t \in [0, N-1]$:
  $$\Delta z(u, v, t) = u^{1.35} \cdot A \cdot \left[ \sin\left( 2\pi \left( \frac{t}{N} - 2.0 u \right) \right) + 0.35 \sin\left( 4\pi \left( \frac{t}{N} - 3.5 u \right) \right) \right]$$
  $$\Delta y(u, v, t) = 0.15 \cdot \Delta z(u, v, t) \cdot \cos(2\pi u)$$
  - Thừa số $u^{1.35}$ đóng vai trò neo cứng cạnh gắn cán cờ ($\Delta z = 0$ khi $u=0$).
  - Thành phần tần số kép $2\pi$ và $4\pi$ tạo cảm giác nếp gấp vải lụa tự nhiên, không bị đơn điệu hình học.
  - Pha thời gian $\frac{t}{N}$ đảm bảo tính tuần hoàn khép kín: frame $N$ đồng nhất hoàn toàn với frame $0$.

### B. Tán cây đung đưa (`tree_canopy_left` & `tree_canopy_right`)
- **Độ phân giải:** 384 × 384 px, 16 frames @ 12 FPS (Chu kỳ lặp 1.33s).
- **Yêu cầu kỹ thuật:** Gốc thân cây cắm đất cố định, phần tán lá xòe dao động đu đưa theo phương ngang kèm co giãn nhẹ theo phương thẳng đứng.
- **Công thức biến dạng đỉnh:**
  Cho đỉnh có tọa độ chuẩn hóa $(x_n, y_n)$ với $y_n \in [0, 1]$ từ gốc cây lên ngọn:
  $$W_{\text{trunk}}(y_n) = y_n^{1.8}$$
  $$\Delta x(x_n, y_n, t) = W_{\text{trunk}}(y_n) \cdot B_x \cdot \sin\left( 2\pi \frac{t}{N} + \phi_{\text{branch}}(x_n) \right)$$
  $$\Delta y(x_n, y_n, t) = - | \Delta x | \cdot 0.08$$
  - Trọng số $W_{\text{trunk}}$ giữ cho gốc cây hoàn toàn ổn định trên nền sân đấu.
  - Độ lệch pha $\phi_{\text{branch}}(x_n) = 0.8 x_n$ tạo sự chuyển động trễ pha giữa cành bên trái và cành bên phải, mô phỏng khối lượng lá cây thực tế.

### C. Bệ đá hoa sen 3D (`podium_lotus`)
- **Quy cách hình học:** Khối trụ đá tròn 3 tầng giật cấp, chiều cao tỉ lệ $0.18$, mặt trên được khắc hoa văn đóa sen Việt Nam.
- **Vật liệu đá cổ (Procedural Weathered Stone):**
  - Base Color: `#D8D2C4` pha trộn Noise Texture tỉ lệ scale 12.0 để tạo đốm rêu phong.
  - Roughness: `0.78` (chống phản xạ sắc cạnh).
  - Normal Bump: Procedural Voronoi + Perlin Noise tạo vết rỗ sỏi đá tự nhiên.

---

## 4. QUY CHUẨN ĐẶT TÊN VÀ FILE MÔ TẢ JSON (`*_anim.json`)

Mỗi chuỗi animation được đóng gói trong một thư mục chuyên biệt tại `Assets/HITME/Art/Animation/<AnimationName>/`:
```
Assets/HITME/Art/Animation/
├── FlagWave/
│   ├── flag_wave_00.png
│   ├── flag_wave_01.png
│   ...
│   ├── flag_wave_15.png
│   └── flag_wave_anim.json
├── TreeLeft/
│   ├── tree_left_00.png
│   ...
│   └── tree_left_anim.json
└── TreeRight/
    ├── tree_right_00.png
    ...
    └── tree_right_anim.json
```

### Cấu trúc Schema chuẩn (`*_anim.json`):
```json
{
  "clipName": "flag_wave",
  "frameCount": 16,
  "frameRate": 14.0,
  "isLooping": true,
  "seamless": true,
  "pivot": [0.08, 0.50],
  "pixelDimensions": {
    "width": 256,
    "height": 192
  },
  "sourcePath": "D:/hitme game/ASSET BỐI CẢNH/HITME_A/A_FLAG.png",
  "seamErrorMetric": 0.0084,
  "compression": "RGBA32_Uncompressed",
  "generatedBy": "Blender 5.2.2 LTS hm_blender.py"
}
```

---

## 5. KIỂM SOÁT CHẤT LƯỢNG (QA CHECKLIST)

Trước khi đẩy lên Git worktree, mỗi bộ animation bắt buộc phải vượt qua bộ kiểm tra tự động trong `post_frames.py`:
- [x] **Kênh Alpha tinh khiết:** Không tồn tại viền đen do alpha premultiplication sai lệch (`defringe` 2 lần lấn màu mép ngoài).
- [x] **Seam Step Error < 1.5%:** Độ lệch màu trung bình giữa frame 15 và frame 0 phải bé hơn $0.015$ trên thang đo chuẩn hóa để mắt người hoàn toàn không nhận thấy vết giật khi lặp.
- [x] **Tỷ lệ nén bộ nhớ:** Mỗi frame được tối ưu hóa paletted PNG nếu không có gradient phức tạp, hoặc RGBA32 nếu có gradient chuyển sắc ánh sáng.
- [x] **Xem trước chuyển động:** Tự động xuất file GIF đối chiếu tại `_AG_VISUAL_WORK/qa/<AnimationName>.gif` để xác nhận trực quan.
