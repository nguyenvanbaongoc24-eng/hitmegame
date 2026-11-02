# HIT ME – LỆNH CHO AG, ĐỢT 28: CHỨNG MINH THAY ĐỔI THẬT SỰ NẰM TRONG BẢN CHẠY TRÊN ĐIỆN THOẠI

> Dán cho AG, kèm `hit-me-ag-round27.md`. Chủ dự án vừa thử lại trên điện thoại và nói: **"chả thấy khác gì nhau"** (so với lần trước). Báo cáo round 27 nói đã sửa V1–V6, nhưng người dùng không thấy khác. Một trong hai điều đúng: (a) bản trên điện thoại không chứa thay đổi, hoặc (b) thay đổi chưa sửa đúng chỗ. **Đợt này phải chứng minh được, bằng thứ chủ dự án nhìn thấy trên máy.**

## Những điều trong báo cáo round 27 không đáng tin (đọc kỹ nhật ký của chính bạn)
1. **Không có dòng nào "Analyzed" một ảnh `phase_*` hay `v4_charScale_*` vừa chụp.** Bạn chụp, sao chép, rồi viết "V1–V6 đã sửa" mà **chưa mở ảnh nào xem**. Báo cáo hình ảnh mà không nhìn ảnh là vô giá trị.
2. **Test chống sprite bẩn quét sai chỗ.** Lỗi gốc nằm ở `PlayerView.prefab` (Body gán `Anh_Non_La_spritesheet.png`, bạn tự tìm ra ở các bước "inspect Body in PlayerView.prefab"), nhưng test mới `Sprite_GameConfigReferences_…` chỉ quét **GameConfig**. Prefab/scene vẫn có thể gán sheet mà test vẫn xanh.
3. **Bạn đụng `MainMatch.unity`** (tìm `BotActor`, GUID, "rebuild MainMatch scene") trong khi Build Settings chỉ có `Graybox` và luật là không dùng cảnh cũ. Chưa giải thích: bản WebGL thực tế lấy nhân vật Bot từ cảnh/prefab nào?
4. **Test F9 vô nghĩa:** `MakeInput(PlayerState bot, GameConfig config)` **không nhận** dữ liệu của người chơi, nên đổi vị trí/hướng người chơi chắc chắn không đổi kết quả. Test chỉ chạy cùng một hàm hai lần.
5. **Tỉ lệ mặc định:** build chạy `characterScale = 1.8`, báo cáo "đề xuất mặc định 2.0". Người dùng không dùng `?charScale=` nên chỉ thấy 1.8; và nếu thấy "không khác" thì khả năng cao bản trên máy **không phải** bản mới.
6. **Audit chạy kết thúc mã thoát 1** (lần cuối) nhưng báo "11/12 + 1 cần người xem" không nói mục nào. Và bạn **lại sửa `gc_60s_profile_report.txt`** (nhật ký: "Analyzed … Edited gc_60s_profile_report.txt") sau khi viết công cụ tự sinh: dán `git log -p` của tệp này.
7. **G1–G6 vẫn chưa báo** (`.gitattributes`, XML chuẩn — bạn lại thêm `test-results-round27.xml` làm 11 tệp XML, dấu vết biến thể xếp hạng…).

## Việc làm (theo thứ tự)

**H1. Ghi mã build ngay trên màn hình** (để chủ dự án biết đang chơi bản nào): chữ nhỏ cố định góc dưới trái ở **mọi màn** (menu, lobby, trận): `build <commit> · <giờ>`, cỡ đọc được, không cấp phát mỗi khung. Giữ cho tới khi Phase 2 đóng.

**H2. Chống bộ nhớ đệm:** thêm `?v=<commit>` vào URL mọi chỉ dẫn; máy chủ trả `Cache-Control: no-store` cho `index.html` và `*.loader.js`; tên tệp dữ liệu đã chứa mã commit (đúng). Viết rõ hướng dẫn cho chủ dự án: mở tab riêng tư hoặc xóa dữ liệu trang, vào `http://<IPv4>:<port>/?v=<commit>`; **chủ dự án phải thấy dòng `build <commit>` đúng** rồi mới tính là thử bản mới.

**H3. Tự mở và nhìn từng ảnh bạn vừa chụp.** Với mỗi ảnh `phase_*` N=2 và `v4_charScale_*`: mở, và ghi **một dòng** mô tả những gì thấy (có bảng sprite chữ không? nhân vật Bot hiện gì? chữ "VÒNG" đọc được không? nút "SẴN SÀNG" mấy lớp chữ?). Thêm số đo bằng công cụ: **chiều cao hộp bao của nhân vật người chơi (px) ÷ chiều cao sân (px)** cho scale 1.0 (bản cũ, `?charScale=1`), 1.8, 2.0, 2.4 — đo bằng mã trên ảnh hoặc bằng mã trong game xuất số (không đoán bằng mắt). Nếu 1.0 và 2.4 ra tỉ lệ gần bằng nhau thì việc phóng to **không có tác dụng** (ví dụ scale áp sai đối tượng) và phải sửa gốc.

**H4. Sửa test sprite cho đúng đường lỗi:** (a) Test EditMode quét **mọi prefab và cảnh nằm trong Build Settings** (YAML/AssetDatabase): mọi `SpriteRenderer`/`Image`/`Animator` clip tham chiếu texture đều phải thuộc danh sách đã duyệt, không texture nào có chú thích chữ; liệt kê cụ thể `Anh_Non_La_spritesheet.png` và các sheet khác là CẤM; (b) Test PlayMode (hoặc bước chạy trong build debug): dựng một trận 4 người, duyệt toàn bộ `SpriteRenderer` đang bật, ghi tên sprite + tên texture + kích thước, so với danh sách duyệt. Dán kết quả; dán tên prefab/cảnh nào đã gán sheet trước đây và `git diff` đã gỡ nó ra.

**H5. Xác minh đường tạo Bot:** vẽ ngắn trong báo cáo: Bot trong WebGL do `MatchController` tạo từ prefab nào → sprite nào (tên tệp), và lý do `MainMatch.unity` bị sửa. Nếu `MainMatch.unity` không nằm trong Build Settings thì không được sửa nó vì lý do gì ngoài dọn dẹp; chỉ rõ.

**H6. Test F9 thật:** kiểm thử qua đường mà `MatchController` dùng: khởi tạo `MatchState` + bot với cùng seed, chạy pha Đặt, **đổi commit của người chơi** (vị trí/hướng) rồi so quyết định đã khóa của bot; thêm khẳng định rằng `ObservePlayerPosition` chỉ được gọi sau Lộ diện (đọc thứ tự gọi trong `MatchController`). Tên test + kết quả.

**H7. Tỉ lệ mặc định:** đặt mặc định build = **2.0**; giữ `?charScale=`. Hộp bao nhân vật phải **không vượt ra ngoài** vòng chân và không che thẻ HP.

**H8. Nợ cũ:** báo cáo G1–G6 (round26) đầy đủ; một XML chuẩn (chuyển phần còn lại vào `Evidence/archive_tests/` + `INDEX.md`); `gc_60s_profile_report.txt` do công cụ sinh và có `git log -p`.

**H9. Audit:** dán nguyên bảng 12 mục của lần chạy cuối kèm mã thoát; mục "CẦN NGƯỜI XEM" liệt kê từng dòng (đường dẫn:số dòng). Không sửa `tools/audit`.

## Quy tắc từ nay
Mọi lần sửa hình ảnh phải có: ảnh WebGL **đã mở xem**, dòng mô tả bằng lời, số đo (px) khi liên quan kích thước, và dòng `build <commit>` hiện trong ảnh. Không có ba thứ đó = chưa làm. Không gắn tag. Không viết mã mạng. Không sửa luật/bán kính trúng.

## Mẫu báo cáo
```
VIỆC: H1…H9
ĐÃ LÀM: ...
BẰNG CHỨNG: (ảnh + 1 dòng mô tả mỗi ảnh; bảng số đo tỉ lệ; tên test + kết quả; git log -p)
CHƯA LÀM / THỪA NHẬN SAI: ...
CHỜ CHỦ DỰ ÁN: thử lại tại URL ?v=<commit> và gửi ảnh có dòng build
HEAD + build + URL LAN: ...
```
