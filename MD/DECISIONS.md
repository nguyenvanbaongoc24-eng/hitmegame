# HIT ME – CÁC QUYẾT ĐỊNH THIẾT KẾ ĐÃ CHỐT (DECISIONS)

*Tài liệu nguồn sự thật ghi nhận các quyết định thiết kế và luật chơi đã chốt giữa Chủ dự án, Codex và Antigravity.*

---

## 1. Giới hạn số vòng tối đa (maxRounds)
- **Quyết định**: `maxRounds = 5`.
- **Luật phân định khi hết vòng**:
  - Khi chạm mốc 5 vòng mà vẫn còn $\ge 2$ người chơi sống sót: Người chơi có số máu (HP) cao nhất sẽ chiến thắng.
  - Nếu bằng máu (hòa điểm số HP cao nhất): Xét thứ hạng đồng hạng (Đồng Thắng / Tie) và chia đều phần thưởng top tương ứng.
- **Áp dụng**: Cả Server authoritative (Codex) và Client UI/Simulation (Antigravity).

---

## 2. Chính sách khi người chơi không ngắm / không chạm (No-Aim Policy)
- **Quyết định**: Áp dụng chuẩn `NoAimPolicy.ThrowFacing` theo Luật chơi v1.2.
  - **Không chạm trong 5 giây**: Nhân vật giữ nguyên vị trí của vòng trước (vòng 1: vị trí ngẫu nhiên hợp lệ sinh theo `seed`).
  - **Không trượt để ngắm**: Hướng ném mặc định là hướng thẳng mặt nhân vật (`Vec2.Up` - hướng thẳng lên, ném ra xa người chơi).
  - **Dữ liệu gửi đi**: Khi không ngắm, `aimDir = null` hoặc vector `Vec2.Up`, server tự động áp dụng `ThrowFacing`.

---

## 3. Quy tắc phòng Online
- **Sức chứa phòng**: Tối đa 6 người chơi thật. Hỗ trợ tạo và vào phòng từ 1–6 người.
- **Không có Bot trong phòng Online**: Tuyệt đối không sinh bot vào phòng thi đấu trực tuyến.
- **Quy tắc bắt đầu trận**:
  - Trận chỉ bắt đầu khi có ít nhất $\ge 2$ người chơi sẵn sàng.
  - Khi phòng chỉ có 1 người: nút Bắt Đầu bị vô hiệu hóa / thông báo rõ: *"Cần ít nhất 2 người, hoặc vào Luyện với bot"*.
  - Người chơi muốn chơi 1 mình phải chọn chế độ "Luyện với bot" ở menu chính.
- **AFK / Rớt mạng**:
  - AFK: Loại khỏi trận + cấm đấu 2 phút.
  - Rớt mạng: Không bị cấm, cho phép kết nối lại (`reconnect_token`) trong 30 giây.
