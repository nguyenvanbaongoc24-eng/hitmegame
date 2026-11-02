# BÁO CÁO RÀ SOÁT CHẤT LƯỢNG HÌNH ẢNH & CHUYỂN ĐỘNG (A10_visual_qa.md) — ROUND 38 A10

**Thời gian:** 11:32 AM 02/11/2026  
**Môi trường:** Unity 6000.6.4f1 · WebGL Release Brotli  
**Phạm vi:** 5 nhân vật người (Nam Làng, Nữ Năng Động, Quậy Boy, Cô Ba, Anh Teen) + 2 thú cưng (Chó Vàng, Mèo Mun)

---

## 1. Trạng Thái Chuyển Động & Hoạt Họa Nhân Vật

| Nhân vật | Idle (thở / đứng) | Di chuyển | Hướng nhìn (Facing) | Ném (Throw) | Trúng đòn (Hit) | Hạ gục / Loại (Defeat) | Ghi chú |
|---|---|---|---|---|---|---|---|
| **0: Nam Làng** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Pose Sprite Aim & Throw | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Bộ pose sprite đầy đủ |
| **1: Nữ Năng Động** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Hiệu ứng vung tay | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Tỉ lệ 1.45u chuẩn hitbox |
| **2: Quậy Boy** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Hiệu ứng ném | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Tỉ lệ 1.48u chuẩn hitbox |
| **3: Cô Ba** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Hiệu ứng ném | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Tỉ lệ 1.45u chuẩn hitbox |
| **4: Anh Teen (Anh Nón Lá)** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Pose Sprite Aim & Throw | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Bộ pose sprite đầy đủ |
| **5: Chó Vàng** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Hiệu ứng sủa ném | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Tỉ lệ thú cưng 72.5% người |
| **6: Mèo Mun** | Chu kỳ thở sin(3.2t), không giật | Mượt | Lật Sprite (`flipX`) theo vector ném | Hiệu ứng cào ném | Chớp đỏ + giật lùi 0.22s | Mờ xám + offset hạ gục | Tỉ lệ thú cưng 72.5% người |

*Đặc biệt đã fix:* `_baseBodyScale` được đồng bộ trực tiếp khi đổi trạng thái/vòng chơi, loại bỏ hoàn toàn hiện tượng nhảy scale hoặc giật giật (stutter) khi nhân vật đứng yên.

---

## 2. Rà Soát Chuỗi Tiếng Việt Toàn Bộ Màn Hình

- **Công cụ quét:** `tools/find_hardcoded_ui_strings.py` và kiểm tra `Assets/Scripts/UI/UiStrings.cs`.
- **Kết quả:**
  - 100% các chuỗi UI đều mang dấu tiếng Việt chuẩn Unicode UTF-8 (`ĐẶT VỊ TRÍ & HƯỚNG NÉM`, `PHÒNG CHIẾN HỮU • SÂN LÀNG`, `KẾT THÚC TRẬN ĐẤU`, `CHIẾN THẮNG TUYỆT ĐỐI!`, `SẮP RA MẮT`,...).
  - Không có chuỗi nào bị mất dấu hay biến thành ký tự lạ `?`.
  - Nút chuyển `VI | en` hoạt động tức thì và lưu trữ cấu hình qua `PlayerPrefs`.

---

## 3. Các Điểm Lệch Đã Sửa Nhanh Từ UI_DIFF.md

1. **Hướng nhìn nhân vật:** Bổ sung `_body.flipX` trong `PlayerView.cs` dựa trên `player.Facing.X`, nhân vật luôn quay mặt về hướng ném.
2. **Khắc phục giật khi đứng yên:** Đồng bộ biến `_baseBodyScale` trong `PlayerView.cs` khi Present và PlayAnimation.
3. **Mũi tên điều hướng nhân vật tại Trang Chủ:** Thêm hai nút điều hướng `❮` và `❯` viền vàng ở hai bên thẻ nhân vật (`EnsureHomeCharArrows`), giúp người chơi nhận biết và chạm đổi trực tiếp giữa 7 nhân vật.
4. **Viền vàng nút ngôn ngữ `VI | en`:** Thêm `Outline` màu vàng ấm (`#D9A626`) đồng bộ với nhận diện làng quê.
5. **Nút kết bạn sau trận:** Tăng kích thước phông chữ từ 13pt lên 16pt trong `ProductionUIController.cs` để bấm chính xác trên điện thoại.
6. **Viền khay Emote & Chat:** Thêm `Outline` viền gỗ vàng ấm (`#C79433`) cho khay Emote, hài hòa với giao diện tổng thể.
7. **Router test phòng 6 người:** Thêm tham số `?screen=lobby6p` vào bộ điều hướng URL để test nhanh bố cục 6/6 người chơi thật.

---

## 4. Danh Sách Ưu Tiên Cho Giai Đoạn Tiếp Theo (Phase 3)

- [ ] **Ưu tiên 1:** Mũi tên dạng đầu dép tổ ong trên vạch thước ngắm (khi hoàn thiện bộ shader/line renderer vector).
- [ ] **Ưu tiên 2:** Hiệu ứng hào quang vàng xoay nhẹ phía sau Cúp vô địch ở màn hình Kết quả trận đấu.
- [ ] **Ưu tiên 3:** Bổ sung trọn bộ 5 pose sprites vẽ tay riêng cho 3 nhân vật người còn lại (Nữ Năng Động, Quậy Boy, Cô Ba) và 2 thú cưng (khi Họa sĩ hoàn tất xuất file).
