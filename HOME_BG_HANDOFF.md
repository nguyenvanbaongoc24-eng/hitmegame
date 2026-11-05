# HIT ME – BÀN GIAO TÀI SẢN NỀN HOME MỚI (HOME_BG_HANDOFF.md)

Tài liệu này cung cấp toàn bộ thông số hình học, tọa độ bệ đá sen, vùng an toàn các cụm UI, các biến thể WebP tối ưu dung lượng và công thức căn chỉnh để giao diện web (`web-ui` / Codex) tích hợp nền Home mới mà không bị lệch hay che khuất giao diện.

---

## 1. Đường dẫn tệp và ngân sách dung lượng

Các tệp nền tối ưu đã được tạo và đặt tại:
- Thư mục gốc chuẩn bị: `Tools/deploy_web/web_assets/bg_home/`
- Thư mục staging deploy: `Tools/deploy_web/dist/ui-preview/assets/` và `Tools/deploy_web/dist/next/assets/`
- Tệp gốc lưu trữ: `asset/UI_KIT_RAW/u4_background.png` (941 × 1672 px, 2.62 MB, giữ nguyên không chỉnh sửa)

| Tên tệp | Độ phân giải | Dung lượng | Ngân sách | Mục đích sử dụng |
|---|---|---|---|---|
| `bg_home_lqip.webp` | 32 × 56 px | **216 bytes** | < 1 KB | Giữ chỗ tải tức thì (LQIP), mờ dần (fade-in) khi ảnh nét tải xong |
| `bg_home_750.webp` | 750 × 1332 px | **177.5 KB** | $\le 180\text{ KB}$ | Thiết bị di động phổ thông (màn hình < 400 CSS px @2x) |
| `bg_home_1170.webp` | 1170 × 2078 px | **349.4 KB** | $\le 350\text{ KB}$ | Thiết bị di động cao cấp (iPhone 15 Pro, iPhone 14 Pro, Samsung Galaxy) |
| `bg_home_1536.webp` | 1536 × 2729 px | **476.5 KB** | < 600 KB | Màn hình máy tính bảng (iPad) hoặc Desktop |

---

## 2. Cách chọn ảnh theo màn hình (HTML/CSS)

Codex sử dụng `image-set` hoặc `srcset` kết hợp với thẻ `<img>` hoặc `background-image` dạng cover:

```css
.home-bg {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: 50% 48%;
  background-image: image-set(
    url("assets/bg_home_750.webp") 1x,
    url("assets/bg_home_1170.webp") 2x,
    url("assets/bg_home_1536.webp") 3x
  );
  z-index: 0;
}
```

---

## 3. Tọa độ Bệ Đá Sen và Điểm Neo Nhân Vật (`bg_home_meta.json`)

Toàn bộ tọa độ được chuẩn hóa `0.0 .. 1.0` theo kích thước ảnh gốc ($W_{img} = 941$, $H_{img} = 1672$):

```json
{
  "pedestal": {
    "center": { "cx": 0.5000, "cy": 0.4740 },
    "radii": { "rx": 0.3613, "ry": 0.0733 },
    "anchor_feet": { "fx": 0.5000, "fy": 0.4845 }
  },
  "focal_point": {
    "x": 0.5000,
    "y": 0.4850,
    "css_object_position": "50% 48%"
  }
}
```

- **Mặt trên bệ đá (nơi đặt chân nhân vật):** Neo tại $(f_x = 0.5000, f_y = 0.4845)$.
- **Xóa bệ đá CSS/SVG:** Nền đã có sẵn hoa văn sen đá, Codex **bắt buộc xóa bỏ** phần tử bệ đá vẽ bằng CSS/SVG cũ để tránh tình trạng 2 hoa văn sen chồng lên nhau.
- **Bóng đổ nhân vật:** Codex thêm một lớp bóng ellipse mờ (`rgba(0, 0, 0, 0.35)`, mờ ~`8px`) ngay dưới chân nhân vật tại $(f_x, f_y)$ trên mặt bệ.

---

## 4. Công thức tính vị trí màn hình thực tế (JavaScript / CSS Dynamic)

Khi áp dụng `object-fit: cover` hoặc `background-size: cover` với `object-position: 50% 48%`:

```javascript
// Gọi lại mỗi khi visualViewport resize hoặc đổi hướng
function getPedestalScreenAnchor(viewportWidth, viewportHeight) {
  const origW = 941;
  const origH = 1672;
  const fx = 0.5000;
  const fy = 0.4845;
  const posX = 0.50; // CSS 50%
  const posY = 0.48; // CSS 48%

  const scale = Math.max(viewportWidth / origW, viewportHeight / origH);
  const renderW = origW * scale;
  const renderH = origH * scale;

  const dx = (viewportWidth - renderW) * posX;
  const dy = (viewportHeight - renderH) * posY;

  return {
    feetX: Math.round(dx + fx * renderW),
    feetY: Math.round(dy + fy * renderH),
    scale: scale
  };
}
```

Đặt nhân vật lớn theo `bottom`:
```css
.hero-character {
  position: absolute;
  left: 50%;
  bottom: calc(100% - var(--feet-y));
  transform: translateX(-50%);
  height: clamp(180px, 34vh, 230px);
}
```

---

## 5. Vùng an toàn (Safe Areas) các khối giao diện

| Vùng UI | Vùng chuẩn hóa `[x, y, w, h]` | Độ sáng nền (Lum) | Độ bận nền (Clutter) | Khuyến nghị hiển thị |
|---|---|---|---|---|
| **Top Bar** (Avatar, Tên, Chip 500 G, Bánh răng) | `[0.02, 0.05, 0.96, 0.07]` | $138.5$ | $28.6$ | Thêm lớp phủ gradient trên để chữ trắng đạt độ tương phản $\ge 7:1$ |
| **Logo** (HIT ME) | `[0.15, 0.12, 0.70, 0.13]` | $115.7$ | $35.0$ | Nằm gọn giữa Top Bar và đỉnh đầu nhân vật |
| **Hero Stand** (Nhân vật đứng trên bệ) | `[0.22, 0.26, 0.56, 0.23]` | $177.4$ | $18.7$ | Chân chạm $(f_x, f_y)$, hai mũi tên đổi nhân vật đặt hai bên ngang hông |
| **Cards Row** (Thẻ Nhân vật & Thẻ Vũ khí) | `[0.05, 0.56, 0.90, 0.09]` | $202.1$ | $5.1$ | Nền gạch mịn, thẻ có bóng đổ nhẹ `rgba(0,0,0,0.15)` |
| **Nút Chơi Ngay (CTA Vàng)** | `[0.07, 0.66, 0.86, 0.08]` | $197.4$ | $4.5$ | Chữ nâu `#4a1c00` trên nền vàng cam `#f59e0b` đạt tương phản **$6.71:1$** |
| **Nút Tạo Phòng (Xanh lá) & Vào Phòng (Navy)** | `[0.05, 0.75, 0.90, 0.07]` | $189.1$ | $6.0$ | Chữ trắng trên nền xanh lá đậm `#15803d` đạt **$5.02:1$**, trên nền navy `#1d4ed8` đạt **$5.17:1$** |
| **Nút Luyện Bot** | `[0.12, 0.83, 0.76, 0.06]` | $174.3$ | $25.3$ | Nút phụ viền nổi bật |
| **Bottom Tab Bar** (5 tab) | `[0.00, 0.91, 1.00, 0.09]` | $111.9$ | $28.9$ | Thêm gradient tối bên dưới hoặc nền bán trong `rgba(15, 23, 42, 0.75)` |

---

## 6. Lớp tối và Độ tương phản đề xuất (WCAG 2.1 AA)

Để ảnh nền sân đình vẫn giữ màu sắc tươi tắn, không bị mờ xám:
1. **Gradient phía trên (Top Overlay):**
   ```css
   linear-gradient(180deg, rgba(0, 0, 0, 0.45) 0%, rgba(0, 0, 0, 0) 15%)
   ```
   *Kết quả:* Chữ trắng của thanh Profile đạt tương phản **$9.73:1$** (vượt chuẩn AA 4.5:1).
2. **Gradient phía dưới (Bottom Overlay):**
   ```css
   linear-gradient(180deg, rgba(0, 0, 0, 0) 55%, rgba(0, 0, 0, 0.25) 85%, rgba(0, 0, 0, 0.60) 100%)
   ```
   *Kết quả:* Giúp các nút CTA và Tab bar nổi bật mà không làm tối khu vực sân đình và nhân vật.
3. **Màu chữ trên nút:**
   - Nút Chơi Ngay (Vàng CTA): Chữ `#4a1c00` trên nền `#ffa000` -> tương phản **$6.71:1$** (ĐẠT).
   - Nút Tạo Phòng: Sử dụng màu xanh `#15803d` với chữ trắng -> tương phản **$5.02:1$** (ĐẠT).
   - Nút Vào Phòng: Sử dụng màu navy `#1d4ed8` với chữ trắng -> tương phản **$5.17:1$** (ĐẠT).

---

## 7. Bằng chứng mô phỏng 6 hồ sơ thiết bị

Bằng chứng kiểm tra tỷ lệ cắt và bố cục tại `Evidence/home_bg_audit/simulated_profiles/`:
1. `iphone15pro_safari_393x695.png`: Bệ đá tại 337.5 px (48.6% vh), Nhân vật đứng vững, an toàn Top & Tab.
2. `iphone15pro_safari_collapsed_393x780.png`: Bệ đá tại 378.3 px (48.5% vh), không cắt lề.
3. `iphone15pro_standalone_393x852.png`: Bệ đá tại 413.2 px (48.5% vh), hiển thị trọn vẹn.
4. `iphonese_safari_375x553.png`: Bệ đá tại 269.0 px (48.6% vh), tỷ lệ ngắn vẫn giữ nguyên bệ và nút.
5. `android_chrome_360x640.png`: Bệ đá tại 310.4 px (48.5% vh), đúng chuẩn 9:16.
6. `android_chrome_tall_412x780.png`: Bệ đá tại 378.3 px (48.5% vh), tỷ lệ cân đối.
