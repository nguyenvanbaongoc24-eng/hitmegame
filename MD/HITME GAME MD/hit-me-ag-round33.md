# HIT ME – LỆNH CHO AG, ĐỢT 33: KHÔNG DỪNG CHỜ – LÀM LIÊN TỤC THEO HÀNG ĐỢI

> Dán cho AG, kèm `hit-me-ag-round31.md`, `hit-me-ag-round32.md`, `hit-me-ag-round30.md`, `hit-me-ag-round29-ui-skin.md`, `hit-me-gameplay-rules-v1.2.md`, `hit-me-room-size-rule-v1.3.md`. Quy tắc bằng chứng round 28 **vẫn bắt buộc** (ảnh WebGL mở bằng công cụ xem ảnh và chỉ ghi điều nhìn thấy; dòng `build <commit>`; `Passed! N/N`; audit mã thoát 0; không tích sẵn lựa chọn của chủ dự án; không tag; không sửa `tools/audit`; không sửa asset gốc; không thêm vũ khí ngoài dép; không thêm bot vào phòng online).

## Vấn đề của lượt vừa rồi
1. Bạn kết thúc bằng câu "sẵn sàng khi chủ dự án duyệt Lô 1/Lô 2 hoặc Codex hoàn thành Z4". **Đó là dừng chờ.** Chủ dự án duyệt **kết quả** của mỗi lô, không duyệt việc bắt đầu lô kế tiếp; Z4 chỉ chặn riêng bước N2. Còn việc thì phải làm tiếp.
2. Bảng bạn đưa ghi "Cổng 8094 = commit `64bc6a9` (HUD Lô 1 & Lô 2)". Nhưng round 31 giao Lô 2 là **cổng 8095, build mới**; 8094 chỉ là Lô 1. **Lô 2 chưa được giao** — hoặc bạn gắn nhãn sai. Làm rõ bằng bảng "cổng → commit → nội dung" **chính xác** và xóa nhãn không đúng.
3. Ảnh chủ dự án gửi (từ cổng 8093, build `96bbc27`, ở khung cửa sổ máy tính) cho thấy: **hàng HUD trên cùng bị cắt ở mép trên** ("VÒNG 2/5", "1.9s", "SẴN SÀNG" chạm/vượt mép), **nút SẴN SÀNG nằm ở hàng trên** thay vì vùng ngón cái phía dưới, **chữ trong thẻ HP rất nhỏ** ("HP: 2/3", "CÒN SỐNG"), sân hẹp so với khung. Một phần có thể đã sửa ở `64bc6a9`, nhưng chưa có bằng chứng: kiểm ở mục V dưới đây.

## Hàng đợi làm liên tục (theo thứ tự; xong bước nào ghi bảng rồi sang bước tiếp, không chờ)

**V. Hiển thị theo cửa sổ (làm đầu tiên, ngắn):** mẫu WebGL phải **vừa khít cửa sổ, không cắt** ở mọi kích thước: `contain` theo chiều cao (`100dvh`), không cuộn, xử lý thanh công cụ Safari iOS, thông báo xoay dọc khi ở chế độ ngang. Chụp WebGL ở **1920×1080, 1366×768, 820×1180, 1170×2532, 1080×1920** (chế độ cửa sổ giả lập) cho màn Đặt N=2 và N=4: HUD không bị cắt, nút SẴN SÀNG ở 1/3 dưới, chữ thẻ HP đạt cỡ tối thiểu theo `UiTheme` (≥ 2,2 % chiều cao màn hình), có Safe Area. Mô tả từng ảnh sau khi **mở xem**.

**1. `round31` – Lô 2 (Home, Luyện với bot, Phòng)** + sửa Lô 1 (mô tả ảnh đã mở xem, ảnh ghép cạnh asset gốc, `git diff` số cứng, không tích sẵn lựa chọn, giữ bảng cổng). **Cổng 8095** cho Lô 2; giữ 8093/8094.

**2. N1 (`round32`)** – nhập bản vá mạng của Codex vào Unity (cờ `NetConfig.Enabled=false`, `#if` cho WebGL, test với `LoopbackTransport`, WebGL vẫn chạy, `.data` trước/sau).

**3. Lô 3** – Kết quả vòng; Kết thúc trận (bảng N dòng, "HẠNG n" `[TẠM]` nếu chưa có huy chương); hộp thoại thoát; thông báo lỗi. Cổng 8096.

**4. Lô 4** – phản hồi (nhấn nút, mất máu, trúng đòn bằng `ASSET EFFECT`, đếm số), pool, 0 cấp phát, có "giảm chuyển động". Cổng 8097.

**5. Lô 5 – Âm thanh và Cài đặt:** gắn SFX có sẵn trong thư mục SFX của dự án (nhấn nút, đặt vị trí, khóa, lộ diện, ném, trúng, mất máu, đếm giây cuối, thắng/thua) qua một `AudioService` có pool; **mở khóa âm thanh ở lần chạm đầu** (yêu cầu của iOS Safari); màn **Cài đặt**: bật/tắt nhạc & hiệu ứng, thanh âm lượng, bật/tắt FPS (ba lần chạm góc vẫn hoạt động), giảm chuyển động; lưu bằng `PlayerPrefs` bọc `try/catch`. Tệp SFX nào thiếu → ghi "Thiếu", không tự tạo âm thanh.

**6. Lô 6 – Hướng dẫn lần đầu (tutorial) 3 bước theo cử chỉ v1.2:** (1) chạm để đặt nhân vật; (2) giữ rồi trượt để ngắm, thước theo ngón (dùng `HITME_AIM_KIT_V1`); (3) chạm chỗ khác để làm lại; kéo về dưới 4 mm để hủy; không ngắm = ném thẳng. Chạy ngay trong chế độ Luyện với bot ván đầu, có nút Bỏ qua; ghi "đã xem" vào `PlayerPrefs`. Không thêm luật mới.

**7. N2 (`round32`)** – chỉ khi Codex báo Z4 xong và đã lấy bản vá `unity_online_glue` của Codex (Gói B của `round28`): viết `UnityOnlineUi : IOnlineUi` gắn vào giao diện đã làm; chạy bằng `LoopbackTransport` hai người trong một tiến trình. Nếu Z4 chưa xong, **bỏ qua bước này, làm bước 8**.

**8. Việc khi hết cái làm được:** (a) viết `docs/UI_QA_CHECKLIST.md` (mỗi màn: Safe Area, chữ tối thiểu, vùng chạm ≥ 135 px, trạng thái nhấn/mờ/lỗi, tiếng Việt đủ dấu) và kiểm từng màn; (b) rà `docs/UI_ASSET_MAP.md`: mọi dòng `Thiếu`/`TẠM` gom thành một danh sách cho chủ dự án; (c) đo thời gian tải trang đầu và kích thước build, đề xuất giảm (nén, atlas) mà **không** đổi hình. Chỉ báo "hết việc" khi ba mục này có bằng chứng.

## Cách báo cáo (ngắn gọn, mỗi 2–3 bước một lần)
```
BẢNG CỔNG → COMMIT → NỘI DUNG: 8093 / 8094 / 8095 / ...
BƯỚC: V | 1 | 2 | 3 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (ảnh đã mở xem + 1 dòng nhìn thấy; Passed! N/N + tên test; .data trước/sau; audit mã thoát)
CHƯA LÀM / BỊ CHẶN (chờ ai): ...
CÂU HỎI CHO CHỦ DỰ ÁN (không tích sẵn): ...
HEAD = commit build: ...   URL (ipconfig): ...
```

## Không làm
Không viết `.jslib`/URL máy chủ thật (Codex làm); không tự đổi giao thức; không đăng nhập thật; không đổi luật; không dừng chờ khi vẫn còn bước ở hàng đợi.
