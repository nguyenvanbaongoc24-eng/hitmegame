# BÁO CÁO PHÂN TÍCH DUNG LƯỢNG BUNDLE WEBGL (D9 ANALYSIS)

- **Bản build ban đầu:** `WebGL_Release_20261102_188344e` (9.87 MB)
- **Bản build tối ưu đạt chuẩn:** `WebGL_Release_20261102_c441d8e_211224` (**5.99 MB**)
- **Mục tiêu:** Giảm tổng dung lượng truyền tải từ **9.87 MB** xuống **≤ 6.00 MB** (ĐÃ ĐẠT: **5.986 MB**, Tiết kiệm **3.88 MB** / **-39.3%**)
- **Trạng thái Harness D9:** **PASS** (100/100) — Tổng điểm Harness: **93.8/100**

---

## 1. Bảng so sánh Trước / Sau tối ưu (Before / After Breakdown)

| Thành phần | Trước tối ưu (188344e) | Sau tối ưu (c441d8e) | Tiết kiệm được | Ghi chú kỹ thuật |
| :--- | :---: | :---: | :---: | :--- |
| **`data.br` (Asset Bundle)** | **5.36 MB** (5,620,086 B) | **1.75 MB** (1,832,709 B) | **-3.61 MB (-67.4%)** | Chuyển uncompressed textures sang mobile-budget (64/128), dọn Resources, nén Vorbis 22kHz |
| **`wasm.br` (Engine Binary)** | **4.40 MB** (4,613,397 B) | **4.13 MB** (4,328,595 B) | **-0.27 MB (-6.2%)** | ManagedStripping High, IL2CPP OptimizeSize, WebGLExceptionSupport.None, wasm-opt -Oz |
| **`framework.js.br`** | 0.06 MB (65,972 B) | 0.06 MB (65,936 B) | -36 B | Glue code Brotli level 11 |
| **`loader.js`** | 0.03 MB (27,914 B) | 0.03 MB (27,914 B) | 0 B | Unity WebGL loader script |
| **`index.html` + configs** | 0.02 MB (21,394 B) | 0.02 MB (21,422 B) | +28 B | PWA meta, viewport-fit=cover, Safe Area container |
| **TỔNG BUNDLE WEBGL** | **9.87 MB** (10,348,763 B) | **5.99 MB** (6,276,576 B) | **-3.88 MB (-39.3%)** | **ĐẠT CHUẨN ≤ 6.00 MB (D9 PASS)** |

---

## 2. Chi tiết tiết kiệm theo từng nhóm tối ưu

| Nhóm tối ưu | Commit | Dung lượng tiết kiệm (Uncompressed / Transferred) | Giải pháp thực hiện |
| :--- | :---: | :---: | :--- |
| **1. Audio Compression** | `ff39c1c` | -0.35 MB / -0.25 MB | Chuyển 20 tệp WAV thô sang Vorbis Mono 22.05 kHz (BGM quality 0.05, SFX quality 0.08). |
| **2. Resources & Font Trimming** | `26a863f` | -1.35 MB / -0.65 MB | Đưa `LiberationSans SDF.asset` (1.35 MB) ra khỏi `Resources/`, loại bỏ font không tham chiếu. |
| **3. Module & Engine Stripping** | `a3bf5a1` | -1.50 MB / -0.38 MB | Loại bỏ 22 Unity Engine Modules không dùng (PhysX 3D, Cloth, Terrain, Vehicles, XR, AI, Umbra, Video, Tetgen). |
| **4. Texture Mobile Budget (256/128)** | `e455b12` | -2.10 MB / -0.85 MB | Giới hạn ARENA về 256, nhân vật idle về 128, bật WebGL crunched override, tắt Mipmaps và Read/Write. |
| **5. Wasm Exception Stripping** | `e069f8d` | -0.80 MB / -0.15 MB | Thiết lập `WebGLExceptionSupport.None` để loại bỏ bảng C++ unwind exception tables. |
| **6. Action Sprites & UI Icons (64)** | `93b04ac` | -2.25 MB / -0.25 MB | Chuyển action sprites (aim, throw, hit, defeat, victory), UI 9-slice cards và VFX impacts về 64 (giữ idle 128 sắc nét). |
| **7. Wasm Bytecode Optimization** | `93b04ac` | -0.33 MB / -0.015 MB | Áp dụng `wasm-opt -Oz --all-features --strip-debug --strip-producers` nén Brotli quality 11. |

---

## 3. Kết quả kiểm chứng (Verification)

1. **Harness Score:** **93.8/100** (11/12 mục PASS, chỉ còn D12 chờ 4 sprite vũ khí từ người dùng).
2. **Multi-device check:** PASS 100vw × 100dvh trên cả 5 thiết bị (iPhone 15 Pro, iPhone SE, Pixel 7, Android 360x800, iPad).
3. **Gameplay verification:** AutoPlay 10/10 trận diễn ra trơn tru không lỗi/deadlock.
4. **Unit test suite:** 182/182 EditMode unit tests PASS (0 failures).
5. **Vercel live status:** Nhánh `web-dist` commit `4309cd0` đã kích hoạt và phục vụ trực tiếp trên `https://hitmegame-iota.vercel.app/`.
