# HIT ME – ĐỢT 10: build sạch có dấu vân tay + chủ dự án thử cử chỉ sớm (AG + Codex)

## Tình hình
- **Codex:** công cụ xác minh D0/D1 của AG chạy xong (`return code 0`, "All verifications finished successfully", AutoPlay 100 trận 0 lỗi). **Nhưng bản build WebGL chưa đáng tin:** `final_webgl_build.log` không có dòng "Build completed/succeeded"; các tệp trong thư mục build có thời điểm lẫn lộn (`index.html` 15:52, `.framework/.wasm` 16:44, `.data` 17:32); `UnityLockfile` còn sót. Có thể là thư mục build cũ bị ghi đè dở. Codex khuyên **chưa mở link thử trên iPhone** — đúng.
- **AG:** đang chạy `WebBuildPipeline.BuildWeb` (batchmode) cho D1. Lưu ý: AG chạy D khi **chưa** làm F1–F4 của đợt 9 (tint, pool, HUD `OnGUI`, MANIFEST). Số đo D0/D1 hiện tại **chưa phải số cuối**; sẽ phải chạy lại sau F1–F3.

## Quyết định
1. **Không kết luận M1, không gắn tag** cho tới khi F1–F4 xong và Codex chạy X2.
2. **Cho phép chủ dự án thử "cảm giác cử chỉ" sớm** (chỉ cử chỉ, không đánh giá hình/hiệu năng), nhưng **chỉ trên bản build sạch có dấu vân tay** theo mục A dưới đây. F1–F3 không đổi cử chỉ nên thử song song được.
3. AG vừa chờ chủ dự án thử vừa làm F1–F4 trên nhánh riêng; không build đè lên thư mục đang dùng để thử.

---

## PHẦN A – AG: build sạch, có dấu vân tay

1. **Chờ build hiện tại kết thúc.** Không chạy Unity batchmode thứ hai song song. Ghi kết quả thật vào `Evidence/phase2/final_webgl_build.log`: phải có dòng kết quả build (Succeeded/Failed) và thời lượng. Nếu Failed, nêu lỗi, sửa rồi build lại.
2. **Mỗi lần build ra một thư mục mới, không ghi đè:** `Builds/WebGL_<ngày>_<commit ngắn>/`. Không đụng thư mục build cũ đang được thử; xóa tệp lock thừa (`UnityLockfile`) **chỉ khi** xác nhận không còn tiến trình Unity/Bee.
3. **Dấu vân tay trên màn chơi:** góc dưới màn hình (chữ nhỏ, mờ) hiện `build <commit ngắn> · <ngày giờ> · Graybox`, lấy từ biến build (không gõ tay). Ghi vào `Evidence/MANIFEST.csv`: thư mục build, commit, sha256 của `.data/.wasm/.framework.js/.loader.js`, kích thước, log build, scene.
4. **Kiểm sau build:** mọi tệp trong thư mục build có thời gian ghi trong khoảng của lần build này (khác nhau vài phút trở lại, không lẫn từ buổi sáng); mở bằng `http` cục bộ, tải trang không lỗi console; AutoPlay 10 trận trên **bản build** (không chỉ trong Editor).
5. **Đưa chủ dự án địa chỉ để thử** + danh sách 5 điều kiểm cử chỉ (mục B). Chỉ đưa khi bước 1–4 có bằng chứng; kèm đường dẫn `final_webgl_build.log` và `MANIFEST.csv`.
6. **Song song** (nhánh `feature/ui-ux-production`, không đụng thư mục build đang thử): làm **F1 → F2 → F3 → F4** theo `hit-me-round9-ag-fixes.md`, rồi D0/D1 lại với số đo cuối, build sạch lần hai (cùng quy tắc 2–4).

### Báo cáo (điền thật)
```
BƯỚC: A1..A5 | F1..F4
ĐÃ LÀM: ...
BẰNG CHỨNG: (log dòng "Build succeeded", MANIFEST, ảnh dấu vân tay)
KHÔNG ĐẠT / CÒN THIẾU: ...
CHỜ CHỦ DỰ ÁN: ...
COMMIT: <hash>
```

## PHẦN B – CHỦ DỰ ÁN: 5 điều kiểm cử chỉ trên iPhone/Android (chỉ khi AG đưa bản có dấu vân tay)
Ghi lại **dòng dấu vân tay ở góc màn hình** để biết bản nào. Thử 3 trận.
1. Chạm một điểm trong sân → nhân vật đứng đúng chỗ ngón chạm? Che ngón vẫn thấy vòng sáng?
2. Giữ ngón rồi trượt → thước ngắm quay theo ngón; nhân vật **không** chạy theo ngón?
3. Chạm chỗ khác khi đang ngắm → nhân vật dời, hướng về mặc định (lên), thước cũ biến mất, ngắm lại được?
4. Kéo ngược sát điểm chạm → hủy ngắm? Không chạm gì / không trượt → hết 5 giây có ném thẳng hướng lên từ chỗ đứng?
5. Dép có bay **đúng hướng thước ngắm**? Cuộn/zoom trang khi kéo có bị xảy ra không (Safari)?
Báo lại: số bản (dấu vân tay), thiết bị, điều gì sai, **có quay màn hình càng tốt**. Hình dạng dép (cụt đầu), nhân vật tint, giật lag: **chưa đánh giá ở vòng này**, sẽ xử lý theo F1–F3 và Phase 6.

## PHẦN C – CODEX
1. Tiếp tục **T1 → S2a → S2b → S2c** (`hit-me-codex-round7.md`). Báo `Passed! N/N`.
2. **X3 (chỉ đọc) khi AG báo build sạch xong:** (a) `final_webgl_build.log` có dòng kết quả thành công; (b) thời gian ghi các tệp build nhất quán (chênh trong vài phút); (c) sha256 trong `MANIFEST.csv` khớp tệp thật; (d) commit ghi trong dấu vân tay tồn tại và là mã đã build; (e) không tiến trình Unity/Bee nào còn chạy, `UnityLockfile` có hợp lý không. Trả ĐẠT/KHÔNG ĐẠT từng mục.
3. **X2** như đợt 9: sau khi AG báo F1–F4 xong, chạy lại `grep` tint/`Instantiate`/`Destroy`/`OnGUI` và đối chiếu MANIFEST với `git log`.

## Điều cấm (nhắc lại)
Không ghi đè thư mục build đang được thử; không báo "đạt" khi log build thiếu dòng kết quả; không đưa bản không có dấu vân tay cho chủ dự án; không sửa logic combat `Core/Phase1`; không xóa `ASSET`/`SFX`/`Evidence`/`MD`/`ASSET HITME FINAL`.
