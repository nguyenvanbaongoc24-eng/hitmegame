# HIT ME – ĐỢT 9: AG sửa các lỗi Codex phát hiện, rồi mới vào Chặng D

> Dán cho AG, kèm `hit-me-round8-ag-D-codex-review.md` (mục D0, D1–D3) và `hit-me-gameplay-rules-v1.2.md`. Quy tắc giữ nguyên: commit nhỏ, không xóa `ASSET`/`SFX`/`Evidence`/`MD`/`ASSET HITME FINAL`, không sửa logic combat `Core/Phase1`, không tạo .md mới. **Mục "KHÔNG ĐẠT/CÒN THIẾU" phải điền thật; mỗi sửa phải có bằng chứng (test, ảnh, log, `grep`).**

## Kết quả kiểm tra độc lập của Codex (X1) – điều chủ dự án đã biết
Báo cáo Chặng A–C của AG ghi "100% đạt", nhưng Codex phát hiện:

| # | Phát hiện | Vị trí | Hệ quả |
|---|---|---|---|
| 1 | **Còn tint**: dùng lại nhân vật thứ 6 rồi tô màu magenta | `PlayerView.cs:222`, `:225` | Trái yêu cầu "7 nhân vật thật, không tint". AG đã báo "bỏ toàn bộ tint": báo sai. |
| 2 | **Không có pool**: tạo `PlayerView`, tạo/hủy dép mỗi vòng | `MatchController.cs:352`, `:421`, `:438` | GC alloc, giật trên điện thoại. AG báo "0 GC": chưa đo trên bản chạy thật. |
| 3 | **HUD `OnGUI` nối chuỗi mỗi khung hình** | `HudView.cs:19, 26, 45, 51` | Cấp phát rác liên tục; OnGUI chậm trên mobile. |
| 4 | **Bằng chứng không có nguồn gốc**: mọi tệp `Evidence/phase2`, `phase3` cùng dấu thời gian `17:20` | `Evidence/*` | Không biết ảnh nào chụp từ commit/scene nào. |
Điểm đạt (ghi nhận): build chỉ còn `Graybox.unity`; không `MatchStateMachine` trong scene build; `Core/Phase1` không `UnityEngine`; đường chạm mouse/touch → world → `MatchController`, hướng tính trong tọa độ world/game; pha Placing ẩn bot; không `Shader.Find`/`new Material` trong runtime.

## F1 – Hết tint, đủ 7 nhân vật thật
- Bỏ đoạn dùng lại nhân vật + tint ở `PlayerView.cs`. Đội hình mỗi trận: tối đa 6 người **khác nhau** lấy từ 7 nhân vật (Nam Làng, Nữ Năng Động, Quậy Boy, Cô Ba, Anh Teen, Chó Vàng, Mèo Mun). Người chơi chọn nhân vật của mình; bot lấy ngẫu nhiên (theo `seed`) trong số còn lại; không trùng.
- Nhân vật nào chưa có đủ animation: dùng pose tĩnh thật từ `ASSET HITME FINAL` (đưa vào bảng "nhân vật ↔ thư mục nguồn ↔ trạng thái" ở D0 mục 7); **không** tint, **không** vẽ giả.
- Bằng chứng: `grep -rn "tint\|Tint\|magenta"` trong `Scripts/Game` (kết quả rỗng hoặc chỉ là hiệu ứng trúng đòn hợp lý, giải thích từng dòng); ảnh một trận 6 người cho thấy 6 nhân vật khác nhau; test: nhiều `seed`, không bao giờ trùng nhân vật trong một trận.

## F2 – Pool cho nhân vật, dép, hiệu ứng
- Tạo sẵn `PlayerView` và dép (và hiệu ứng trúng đòn nếu có) **một lần khi vào trận**, tái dùng giữa các vòng/ván (`SetActive`/reset), không `Instantiate`/`Destroy` trong vòng chơi. Có thể một pool nhỏ dùng chung.
- Bằng chứng: `grep` cho thấy không còn `Instantiate`/`Destroy` trong đường chạy của `MatchController` ngoài bước khởi tạo; số đo GC alloc mỗi khung hình trong 1 phút trận trên **bản WebGL chạy thật** (hoặc Profiler nếu đo được), ghi rõ môi trường, trước và sau.

## F3 – HUD không cấp phát
- Bỏ `OnGUI` ở `HudView.cs` khỏi bản build; HUD hiển thị bằng uGUI/TextMeshPro (đã có `ProductionUIController`: dùng hoặc hợp nhất, **chỉ một** HUD).
- Chuỗi hiển thị (vòng, thời gian, HP, tên) chỉ cập nhật **khi giá trị đổi**; số/chuỗi lặp lại dùng bảng chuỗi dựng sẵn (cache), không nối chuỗi/`ToString()` mỗi khung hình.
- Bằng chứng: `grep -rn "OnGUI"` trong `Scripts/` (rỗng cho code build); ảnh HUD trước/sau không đổi giao diện; số đo GC ở F2.

## F4 – Nguồn gốc bằng chứng (provenance)
- Thêm `Evidence/MANIFEST.csv`: `tệp, sha256, commit, scene, bản build (id/ngày), độ phân giải, lệnh/AutoCapture tạo ra, người/công cụ`. Mọi ảnh/video/log trong `Evidence/phase2` và `phase3` phải có dòng; tệp không có nguồn rõ thì **chụp lại sau F1–F3** từ `Graybox.unity` rồi ghi vào bảng.
- Công cụ chụp tự động (`AutoCapture`) ghi sẵn `commit` và tên scene vào tên tệp hoặc tệp `.json` đi kèm; không tái sử dụng tên tệp cũ.
- Bảng DoD cập nhật thêm hai cột `commit` và `ngày giờ` lấy từ MANIFEST.

## F5 – Rồi mới làm D0 (8 mục) và D1–D3
Theo `hit-me-round8-ag-D-codex-review.md`. **Chặng D chưa mở tới khi F1–F4 có bằng chứng.** Sau D1–D3 chủ dự án thử trên máy thật; chưa gắn tag, chưa mở Phase 3.

## Mẫu báo cáo (điền thật)
```
BƯỚC: F1 | F2 | F3 | F4 | D0.x ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (grep + số đo + ảnh + test)
KHÔNG ĐẠT / CÒN THIẾU: (liệt kê thật)
CHỜ CHỦ DỰ ÁN: ...
COMMIT: <hash>
```

---

## Dành cho Codex (đã có `hit-me-codex-round7.md`)
- Tiếp tục **T1 → S2a → S2b → S2c** (replay có `expected`, giữ bí mật). Báo `Passed! N/N`.
- **X2 (sau khi AG báo F1–F4 xong và đẩy commit):** chạy lại 4 hạng mục KHÔNG ĐẠT của X1 trên commit mới (`grep` tint/Instantiate/Destroy/OnGUI + đối chiếu `Evidence/MANIFEST.csv` với `git log`), báo ĐẠT/KHÔNG ĐẠT, chỉ đọc.

## Điều cấm (nhắc lại)
Không báo "bỏ tint/0 GC" khi `grep`/số đo cho kết quả khác; không để `OnGUI`, `Instantiate`/`Destroy` trong vòng chơi; không dùng bằng chứng không rõ nguồn; không xóa `ASSET`/`SFX`/`Evidence`/`MD`/`ASSET HITME FINAL`; không sửa logic combat `Core/Phase1`.
