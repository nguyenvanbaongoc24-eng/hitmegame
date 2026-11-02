# HƯỚNG DẪN SAO LƯU & ĐẨY MÃ NGUỒN (GIT_PUSH_HELP.md)

Tài liệu ghi nhận tình trạng đồng bộ mã nguồn của nhánh `feature/ui-ux-production` lên GitHub.

---

## 1. Chẩn Đoán Vì Sao Lần Push Trước Bị Chậm / Treo

1. **Nhánh mới chưa từng đẩy lên remote**:
   - Nhánh `feature/ui-ux-production` là nhánh phát triển UI/UX mới, tách ra và chứa toàn bộ lịch sử tiến hóa từ Phase 2 đến Phase 4 (hơn 50 rounds).
2. **Kích thước diff tích lũy lớn (~343.5 MB)**:
   - Các tệp lớn nhất trong lịch sử nhánh là các ảnh chụp kiểm chứng PNG độ phân giải Retina/Mobile (`3.4 - 3.7 MB` mỗi ảnh), được lưu trong thư mục `Evidence/`.
   - Toàn bộ thư mục tạm `Builds/`, `Library/`, `Temp/`, `Obj/` đều **được `.gitignore` chặn hoàn hảo** (không có bất kỳ file tạm nào lọt vào git).
3. **Cơ chế HTTP Post Buffer của Git**:
   - Mặc định Git giới hạn buffer gửi gói nhỏ (~1MB), khiến việc đóng gói và tải lên 343 MB qua HTTPS qua mạng gia đình mất khoảng 2-4 phút và dễ bị ngắt kết nối giữa chừng nếu lệnh bị hủy quá sớm.

---

## 2. Bản Sao Lưu Cục Bộ (Local Git Bundle)

Antigravity đã tạo bản sao lưu đóng gói toàn bộ lịch sử git độc lập (đảm bảo an toàn tuyệt đối 100%, không sợ mất mã):
- **Đường dẫn**: `D:\HITME GAME\_backups\hitme_ui_ux_production_20261102.bundle`
- **Dung lượng**: **595.59 MB** (chứa toàn vẹn toàn bộ commit, blob, tree của nhánh từ ngày đầu tiên).
- **Cách khôi phục khi cần**:
  ```bash
  git clone "D:\HITME GAME\_backups\hitme_ui_ux_production_20261102.bundle" -b feature/ui-ux-production hitme_restored
  ```

---

## 3. Cách Đẩy Lên GitHub Thủ Công (Nếu cần)

Nếu tiến trình push chạy nền gặp sự cố mạng:
1. Mở PowerShell hoặc Terminal tại `D:\HITME GAME\unity-client`.
2. Tăng buffer cho Git:
   ```bash
   git config http.postBuffer 524288000
   ```
3. Chạy lệnh push:
   ```bash
   git push origin feature/ui-ux-production
   ```
4. Nếu GitHub yêu cầu đăng nhập:
   - Sử dụng GitHub Personal Access Token (PAT) hoặc GitHub CLI: `gh auth login`.
