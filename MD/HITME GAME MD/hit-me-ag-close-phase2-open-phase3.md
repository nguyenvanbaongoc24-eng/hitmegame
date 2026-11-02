# HIT ME – LỆNH CHO ANTIGRAVITY: đối chiếu & đóng Phase 2, rồi rà soát Phase 3

> Dán toàn bộ file này cho AG. Làm đúng thứ tự, **commit sau mỗi bước** (`WIP:` nếu dang dở). Báo cáo ngắn. **Bạn không được tự chấm "100%"**: mỗi mục phải có đường dẫn bằng chứng mở ra xem được.

---

## 0. Quy tắc

- Không xóa `ASSET`, `SFX`, `Evidence`, `MD`; không sửa `LocalBot.unity`.
- Không sửa logic combat ở `Core`. Chỉ được: tham số `GameConfig`, xếp hạng/hòa khi hết vòng, lớp chuyển đổi tọa độ (đã được duyệt ở prompt trước).
- **Không tạo .md mới.** Chỉ giữ **một** báo cáo: `MD/PHASE2_REPORT.md`. Bản trùng ở `docs/PHASE2_REPORT.md` và `Evidence/phase2/PHASE2_REPORT.md`: nếu giống hệt (so băm) thì xóa hai bản thừa do chính bạn tạo, nếu khác thì gộp vào bản trong `MD/`.
- Điểm luật mơ hồ → dừng và hỏi chủ dự án.
- Tag: **chưa** dùng `v0.2-feel`. Chỉ gắn `v0.2-rc1` sau Bước 3; `v0.2-feel` do chủ dự án duyệt xong mới gắn.

## 1. Bối cảnh đã chốt (không đổi)

- 7 nhân vật chơi được: Nam Làng, Nữ Năng Động, Quậy Boy, Cô Ba, Anh Teen, Chó Vàng, Mèo Mun. **Không có thú cưng.** Một trận tối đa 6 người (lấy 6 trong 7), **không cần đổi màu (tint) để thay chỗ**.
- Chó Vàng và Mèo Mun ném bằng chân.
- Sân 9,0 × 11,4; `maxRounds = 5`; hòa HP → theo số lần trúng, vẫn bằng thì hòa; `NoAimPolicy = ThrowFacing`.
- Mọi nhân vật dùng chung một hitbox `playerRadius` (đề xuất 0,55) và cùng cỡ bóng/vòng dưới chân; thân Chó Vàng/Mèo Mun ≥ ~70% nhân vật người.
- Nhân vật cao ≈ 13% chiều cao sân, rộng ≈ 11–12% chiều rộng sân (đo từ mockup 1), sai số ±1,5%.
- Mục tiêu MVP: **2 nhân vật có đủ animation thật** (Nam Làng, Anh Nón Lá đã có). Nhân vật còn lại dùng động tác tạm bằng code, ghi rõ là placeholder.

## BƯỚC 1 – Bảng đối chiếu DoD Phase 2 (bắt buộc, chưa sửa gì)

Tạo bảng trong `MD/PHASE2_REPORT.md`, mỗi dòng: **ĐẠT / KHÔNG ĐẠT / THAY THẾ** + đường dẫn bằng chứng. Không có bằng chứng = KHÔNG ĐẠT.

| # | Hạng mục | Bằng chứng cần có |
|---|---|---|
| 1 | Biên dịch sạch, test pass | `tests.xml` (số test), build log; **giải thích 7 warning** của WebGL build |
| 2 | Chân nhân vật hết ô vuông "nền giả" | `Evidence/phase2/feet_fix_green.png`, `feet_fix_magenta.png` (cận cảnh 6 nhân vật, trước/sau) |
| 3 | Kích thước nhân vật đúng mockup | `size_compare.png` (sân thật vs mockup 1, cùng bề rộng sân) + bảng tỉ lệ thân/sân, vòng/sân cho 6 nhân vật so với mục tiêu |
| 4 | `playerRadius`, mô phỏng | `sim_v4.txt`: vòng TB/lớn nhất, % thắng rõ, % hòa HP (nếu hòa HP > 50% thì ghi nổi bật) |
| 5 | Sân + phối cảnh | 5 ảnh `res_1080x1920 / 1179x2556 / 1080x2400 / 1080x2520 / 1620x2160.png` đủ 4 góc sân; test homography (4 góc, khứ hồi < 0,01, hướng thước khớp hướng ném) |
| 6 | HUD khớp mockup 1 | ảnh đặt cạnh mockup + bảng đo, sai số ≤ 3% |
| 7 | Chữ tiếng Việt | ảnh WebGL các chuỗi `CHỌN HƯỚNG NÉM`, `SẴN SÀNG`, `Đối thủ đang ẩn`, `Thắng`, `Thua`, `Chơi lại`, `Kết quả` đủ dấu |
| 8 | Thước ngắm | hình nón + vệt chấm + mũi tên **bám theo thước kéo ra**; ngón che vẫn nhìn rõ (vòng cảm ứng ≥ 9 mm, vô hình, tách khỏi kích thước hiển thị) |
| 9 | Hiệu ứng | video ≤ 30 giây: đặt → ngắm → lộ diện → ném (Chó Vàng/Mèo Mun ném bằng chân) → trúng → chết → kết thúc |
| 10 | Âm thanh | bảng sự kiện → file SFX hoặc "placeholder im lặng": bấm nút, đặt, tích tắc 3 giây cuối, lộ diện, ném, dép bay, trúng, chết, thắng, thua, nhạc nền |
| 11 | Hiệu năng | WebGL trên Chrome desktop: FPS TB/thấp nhất, draw call, **GC alloc mỗi khung trong ván (kỳ vọng 0)**. Số trên iPhone do chủ dự án thử |
| 12 | Đội hình 6 nhân vật | danh sách 6 nhân vật của một trận mẫu; **không dùng tint để thay chỗ** (báo cáo trước nhắc "Nam Làng Tint" — giải thích hoặc bỏ) |

## BƯỚC 2 – Kiểm tra an toàn thay đổi

Lần commit gần nhất đổi **16 file, +1168 −621**. Số dòng xóa lớn nên phải kiểm:
1. `git diff --stat` và **liệt kê mọi file bị xóa/đổi tên**; xác nhận không có gì trong `ASSET`, `SFX`, `Evidence`, `MD`, `LocalBot.unity`.
2. `git diff <v0.1-graybox> -- unity-client/Assets/Scripts/Core`: liệt kê từng thay đổi ở Core và lý do; phần không thuộc danh sách được phép thì hoàn lại.
3. Tìm `Shader.Find`, `new Material(`, `GetComponent`/`FindObjectOfType` trong `Update`, `new` trong vòng chơi (nguồn GC alloc); báo cáo và sửa nếu có.
4. **Core vẫn phải là C# thuần** (không `UnityEngine`) để server dùng lại ở Phase 4. Xác nhận asmdef `noEngineReferences = true` vẫn còn sau các thay đổi.
5. Câu "sẵn sàng kết nối `MatchStateMachine` sang Supabase Realtime" **bỏ**: server trận đấu ở Phase 4 là WebSocket trên Render, tách khỏi Supabase (Supabase chỉ lo tài khoản/vàng/kho đồ). Không thiết kế phần này bây giờ.

## BƯỚC 3 – Sửa mục KHÔNG ĐẠT, rồi gắn `v0.2-rc1`

- Sửa lần lượt các dòng KHÔNG ĐẠT ở Bước 1, mỗi lần sửa chạy lại test + AutoPlay, commit.
- Nếu ô vuông dưới chân là điểm ảnh thật trong PNG: lấy bản làm sạch do Codex tạo ở `Evidence/asset_qa/cleaned/` (nếu có), hoặc tự tạo bóng/vòng bằng code như đã hướng dẫn. Không dùng mặt nạ che.
- SFX "Dép bay": chưa có file thì để placeholder im lặng và ghi vào danh sách thiếu (chủ dự án tự lấy file). Không tự tạo âm thanh giả.
- Animation 4 nhân vật phụ: **không chặn Phase 2** (MVP chỉ cần 2). Ghi vào backlog Phase 3.
- Xong: cập nhật bảng DoD, commit, tag `v0.2-rc1`, push. **Báo chủ dự án** kèm danh sách mục còn KHÔNG ĐẠT (nếu có).

## BƯỚC 4 – Phase 3 chỉ phần RÀ SOÁT (Bước 0), không sửa code

Chủ dự án đính kèm `hit-me-phase3-prompt.md`. Chỉ làm **BƯỚC 0** của file đó, viết `MD/PHASE3_AUDIT.md`. Bổ sung/ghi đè các điểm sau:

1. **Chọn nhân vật cuộn 7 nhân vật** (không phải 5); không có thú cưng.
2. **Có 3 mockup làm tham chiếu**, tìm trong `ASSET`/thư mục mockup và đo thành bảng bố cục: **Home** (`MAIN_MENU`), **Vào phòng** (`VÀO PHÒNG`), **Biểu cảm** (`BIỂU CẢM`: bảng 10 biểu cảm Cười, Khóc, Giận, Bất ngờ, Nháy mắt, Like, Chào, Vỗ tay, Yêu thích, Chiến thắng; dòng "Chạm để gửi"; biểu cảm hiện bong bóng trên đầu nhân vật). Màn trận đấu cũng là mockup 1 (`IPHONE_MOCKUP`).
3. **Biểu cảm — quy tắc bí mật vị trí (quan trọng):** trong pha đặt, vị trí nhân vật của người khác phải ẩn. Bong bóng biểu cảm trên đầu nhân vật **sẽ lộ vị trí**. Quy định: người gửi tự thấy bong bóng trên nhân vật mình; **người khác chỉ thấy biểu cảm hiện trên thẻ người chơi ở dải trên** trong pha đặt; sau khi lộ diện mới hiện bong bóng trên nhân vật. Ghi vào kế hoạch; ở Phase 3 biểu cảm chỉ chạy cục bộ với bot, nối mạng ở Phase 4.
4. **Responsive:** dùng đúng quy tắc mục 1 của `hit-me-ag-prompt-responsive-arena.md` (Safe Area, Canvas Scaler 1080×2340, cảm ứng theo mm/dp, 5 độ phân giải kiểm thử) cho mọi màn menu.
5. Dịch vụ giả sau interface (`IProfileService`, `IRoomService`, `ISettingsService`) đúng như prompt Phase 3; chưa mạng, chưa Supabase.
6. Mọi chuỗi chữ: font TMP **atlas tĩnh 134 ký tự** hiện chỉ phủ ASCII + tiếng Việt. Ghi rủi ro: tên người chơi nhập tự do (Phase 5) cần atlas động/fallback; ở Phase 3 chỉ dùng chuỗi cố định.
7. Wireframe bằng chữ cho các màn chưa có mockup (Tạo phòng, Sảnh chờ, Chọn nhân vật/vũ khí, Hồ sơ, Cài đặt, Xếp hạng, Nhiệm vụ, Công khai); **không dựng khi chủ dự án chưa duyệt wireframe**.

Sau khi viết xong: commit, báo cáo ngắn rồi **DỪNG**.

---

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn ảnh/log/test)
KHÔNG ĐẠT / CÒN THIẾU: ...
CHỜ CHỦ DỰ ÁN: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```

## Điều cấm (nhắc lại)
Không tự chấm 100% khi thiếu bằng chứng; không sửa logic combat; không xóa `ASSET`/`SFX`/`Evidence`/`MD`; không tạo .md ngoài `MD/PHASE2_REPORT.md` và `MD/PHASE3_AUDIT.md`; không tự tạo asset/âm thanh giả; không làm menu, mạng, server, Supabase trong bước này; không tag `v0.2-feel`.
