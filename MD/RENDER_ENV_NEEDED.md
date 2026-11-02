# CẤU HÌNH BIẾN MÔI TRƯỜNG TRÊN RENDER (RENDER ENVIRONMENT VARIABLES)

Tài liệu này xác định chính xác các biến môi trường cần thiết lập trên dịch vụ Render (`hitmegame`) dựa trên mã nguồn `server/Protocol/ServerRuntimeOptions.cs`, `ConnectionAdmission.cs` và `Program.cs`.

---

## 1. Biến quyết định danh sách Origin (QUAN TRỌNG NHẤT CHO VERCEL)

- **Tên biến môi trường:** `ALLOWED_ORIGINS`
- **Cơ chế hoạt động:** Phân tách bằng dấu phẩy `,` (loại bỏ khoảng trắng thừa), so sánh không phân biệt hoa thường (`OrdinalIgnoreCase`) thông qua `HashSet<string>`.
- **Mặc định khi không khai báo:** `"http://localhost,http://127.0.0.1"` *(đây là lý do Vercel `https://hitmegame-iota.vercel.app` và cổng test `:8103` bị từ chối với lỗi 403 `origin_not_allowed`)*.
- **Lưu ý định dạng:** Không thêm dấu gạch chéo `/` ở cuối domain. Bao gồm đầy đủ scheme `https://` hoặc `http://`.

### Giá trị đề xuất cấu hình trên Render:
```env
ALLOWED_ORIGINS=https://hitmegame-iota.vercel.app,http://localhost,http://127.0.0.1,http://localhost:8103,http://localhost:8085,http://localhost:3000
```
*(Nếu bạn có thêm tên miền tùy chỉnh khác cho Vercel, hãy bổ sung thêm vào chuỗi phân tách bằng dấu phẩy).*

---

## 2. Các biến môi trường khác máy chủ Render cần (Supabase & Token)

> [!IMPORTANT]
> Chỉ thiết lập trực tiếp trên giao diện Render Dashboard (**Environment** → **Add Environment Variable**), tuyệt đối không dán khóa bí mật lên chat công khai.

| Tên biến môi trường | Bắt buộc | Mô tả & Lưu ý |
|---|:---:|---|
| `ALLOWED_ORIGINS` | **Có** | Danh sách origin trình duyệt được phép kết nối WebSocket (xem mục 1). |
| `SUPABASE_URL` | Có (Phase 5b) | Địa chỉ URL project Supabase (dạng `https://xxxx.supabase.co`). |
| `SUPABASE_SECRET_KEY` | Có (Phase 5b) | Khóa service-role bí mật (chỉ máy chủ dùng, có quyền ghi hồ sơ/vàng). |
| `GUEST_TOKEN_SIGNING_KEY` | Có (Phase 5b) | Khóa ký token khách bền vững định dạng Base64 (tạo bằng lệnh `openssl rand -base64 32`). |
| `SUPABASE_SHOP_ENABLED` | Tùy chọn | Đặt `true` để bật xác thực mua hàng và trừ vàng server-side. |
| `IDLE_TIMEOUT_SECONDS` | Tùy chọn | Thời gian timeout khi phòng không hoạt động (mặc định: `90`). |
| `MAX_CONNECTIONS_PER_IP`| Tùy chọn | Số kết nối tối đa từ 1 IP (mặc định: `12`). |
| `MAX_CONNECTIONS` | Tùy chọn | Tổng kết nối đồng thời toàn server (mặc định: `500`). |

---

## 3. Các bước thao tác trên Render Dashboard

1. Truy cập [https://dashboard.render.com/](https://dashboard.render.com/) → Chọn Web Service **hitmegame**.
2. Chọn menu **Environment** ở thanh bên trái.
3. Bấm **Add Environment Variable**:
   - **Key**: `ALLOWED_ORIGINS`
   - **Value**: `https://hitmegame-iota.vercel.app,http://localhost,http://127.0.0.1,http://localhost:8103,http://localhost:8085,http://localhost:3000`
4. Bấm **Save Changes**.
5. Render sẽ tự động redeploy / restart container trong khoảng 1–2 phút.
6. Sau khi khởi động lại, kiểm tra bằng `curl -I https://hitmegame.onrender.com/healthz` (trả về HTTP 200 OK).
