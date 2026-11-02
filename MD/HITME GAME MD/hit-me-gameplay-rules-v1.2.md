# HIT ME – LUẬT CHƠI v1.2: điều khiển pha đặt (nguồn sự thật)

> **Thay thế** mọi mô tả cũ về "chạm chỗ trống để đặt lại / giữ trên nhân vật rồi kéo để ngắm / hai cử chỉ tách riêng" (v1.1 và các prompt trước). Phần luật còn lại (3 HP, 5 giây, lộ diện đồng thời, ném đồng thời, vũ khí dừng ở người đầu tiên trúng, `maxRounds = 5`, bí mật vị trí) **giữ nguyên**.

## 1. Tóm tắt một câu
**Chạm vào đâu trong sân, nhân vật của bạn đứng ở đó; giữ ngón tay tại chỗ rồi trượt về hướng muốn ném, thước ngắm quay theo; hết 5 giây tự ném. Trong 5 giây bạn chạm chỗ khác để dời nhân vật bao nhiêu lần cũng được.**

## 2. Cử chỉ (một ngón, một cử chỉ liền mạch)

| Mã | Thao tác | Kết quả |
|---|---|---|
| G1 | **Chạm xuống** một điểm bất kỳ trong sân | Nhân vật xuất hiện/dời ngay tại điểm đó (ngay dưới ngón tay). |
| G2 | **Giữ ngón tại chỗ rồi trượt** theo hướng muốn ném | Thước ngắm quay theo hướng từ điểm chạm xuống tới ngón hiện tại; nhân vật quay mặt theo hướng đó. **Nhân vật không chạy theo ngón.** |
| G3 | **Nhả ngón** | Giữ nguyên vị trí và hướng. Thước ngắm vẫn hiện tới lúc khóa. |
| G4 | **Chạm xuống chỗ khác** (trong 5 giây) | **Làm lại hoàn toàn như chọn vị trí mới:** nhân vật dời tức thì tới điểm mới, **hướng reset về mặc định (lên)**, thước ngắm cũ bị xóa; người chơi giữ ngón rồi trượt lại từ đầu để chọn hướng. Không giới hạn số lần, tốc độ hay quãng đường. |
| G5 | Trượt **dưới ngưỡng** `minAimDrag` (≥ 4 mm vật lý) | Không đổi hướng. Đang giữ mà kéo ngược về sát điểm chạm (dưới ngưỡng) = **hủy lần ngắm này**, hướng trở về như trước lần ngắm đó (tức mặc định nếu chưa ngắm lần nào kể từ lần chạm xuống). |
| G6 | Ngón thứ hai | Bỏ qua cho tới khi ngón đầu nhả. |
| G7 | Chạm **bắt đầu ngoài sân** (HUD, nút) | Không phải cử chỉ đặt. Cử chỉ bắt đầu trong sân mà ngón trượt ra ngoài sân vẫn tính hướng (bắt con trỏ); thước chỉ vẽ trong sân. |

## 3. Khóa và mặc định
- **Hết 5 giây** (hoặc bấm SẴN SÀNG = khóa sớm): chốt trạng thái cuối `(vị trí, hướng nhìn)` và tự ném. Đang giữ ngón lúc khóa → chốt hướng hiện tại.
- **Chưa từng chạm** trong 5 giây: nhân vật đứng ở **vị trí vòng trước** (vòng 1: vị trí ngẫu nhiên hợp lệ theo `seed`).
- **Chưa từng trượt** (không ngắm): nhân vật **đứng yên và ném thẳng từ mặt nhân vật** theo hướng nhìn mặc định (**hướng lên**, ra xa người chơi) = `NoAimPolicy.ThrowFacing`.
- **Mỗi vòng mới**: hướng nhìn về mặc định; vị trí xuất phát = vị trí chốt của vòng trước (chỉ làm điểm gợi ý mờ, chưa tính là đã đặt).
- Hai nhân vật quá gần nhau khi lộ diện: Core đẩy ra theo `minSpawnDistance` (không đổi).

## 4. Hiển thị
- Đầu vòng chưa chạm: sân trống, chỉ có chấm mờ gợi ý vị trí cũ + dòng "Chạm để đặt nhân vật".
- Vòng sáng mỏng quanh điểm chạm (đường kính ≥ 9 mm, bằng vùng cảm ứng) để ngón che nhân vật vẫn thấy chỗ đứng.
- Thước ngắm: vệt chấm + hình nón + mũi tên từ nhân vật tới tường sân theo **hướng ném thật** (như mockup 1); luôn kéo dài ra ngoài ngón tay.
- **Hướng được tính trong tọa độ game, không phải tọa độ màn hình:** `hướng = chuẩn_hóa(ScreenToGame(ngón) − ScreenToGame(điểm chạm))` (sân có phối cảnh hình thang). Ngưỡng `minAimDrag` tính theo mm **trên màn hình**.
- Dòng hướng dẫn nhỏ (khóa chuỗi VI/EN): chưa chạm → "Chạm để đặt nhân vật"; đã đặt chưa ngắm → "Giữ và kéo để chọn hướng ném"; đã ngắm → "Chạm chỗ khác để đặt lại · Giữ và kéo để đổi hướng".
- Người chơi khác **không bao giờ** hiện trong pha đặt (bí mật vị trí, không đổi).

## 5. Dữ liệu gửi đi (không đổi giao thức)
`commit = { roundId, position, aimDir | null }`. `aimDir = null` khi chưa từng ngắm → server áp `ThrowFacing` (hướng mặc định ở **một hằng số dùng chung trong Core**). Server chỉ nhận trạng thái cuối, không thấy cử chỉ.

## 6. Bộ test bắt buộc cho máy trạng thái cử chỉ (C# thuần, không phụ thuộc Unity)
1. Chạm xuống P → vị trí = P, hướng = mặc định.
2. Giữ rồi trượt vượt ngưỡng theo d (tọa độ game) → hướng = d, vị trí vẫn P.
3. Trượt dưới ngưỡng → hướng không đổi.
4. Nhả ngón → giữ nguyên vị trí và hướng.
5. Đã ngắm ở P, chạm mới ở Q (không trượt) → vị trí = Q, hướng **reset về mặc định**, thước cũ bị xóa.
6. Chạm mới ở Q rồi trượt → vị trí = Q, hướng mới (hoàn toàn độc lập với lần trước).
7. Trượt vượt ngưỡng rồi kéo ngược dưới ngưỡng trước khi nhả → hủy lần ngắm này, hướng về mặc định (vì lần chạm xuống này chưa có hướng nào khác).
8. Ngón thứ hai bị bỏ qua khi ngón đầu còn giữ.
9. Chạm bắt đầu ngoài sân không đổi gì; bắt đầu trong sân rồi trượt ra ngoài vẫn tính hướng.
10. Khóa khi đang giữ ngón → chốt hướng hiện tại; sự kiện sau khóa bị bỏ qua.
11. Không chạm lần nào → vị trí vòng trước (vòng 1: ngẫu nhiên theo seed), hướng mặc định.
12. Sang vòng mới → hướng reset, vị trí cũ chỉ là gợi ý.
13. Điểm chạm sát biên bị kẹp vào trong sân (tính `playerRadius`).
14. Cùng chuỗi sự kiện → cùng kết quả (xác định, không phụ thuộc thời gian thực).
15. Ngưỡng 4 mm quy đổi đúng với các `dpi` khác nhau (160, 326, 460).

## 7. Các điểm đã chọn mặc định (chủ dự án xác nhận hoặc sửa)
1. Chạm mới = **dời tức thì** (hiệu ứng nhảy ≤ 0,15 s, không giới hạn tốc độ). *(Chủ dự án xác nhận 29/10.)*
2. Chạm mới = **làm lại hoàn toàn**: hướng reset về mặc định, thao tác lại từ giữ ngón đến trượt. *(Chủ dự án xác nhận 29/10; mục 2–5 dưới đây cũng đã xác nhận.)*
3. Không chạm lần nào → đứng vị trí vòng trước; vòng 1 ngẫu nhiên.
4. Chỉ nhận ngón đầu tiên; chạm bắt đầu ngoài sân bị bỏ.
5. Nút SẴN SÀNG vẫn là khóa sớm (không bắt buộc).
