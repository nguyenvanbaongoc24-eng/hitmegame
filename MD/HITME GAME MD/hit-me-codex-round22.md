# HIT ME – LỆNH CHO CODEX, ĐỢT 22

> Dán cho Codex, kèm `hit-me-codex-round21.md` (H2, A3, P4, A2, Z1, Z2, S5, Z3…), `hit-me-ag-remaining-work-v2.md` (để biết AG sẽ làm gì). **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá Y4 (`0922b66`)
**Đạt:** kiểm thử phủ định `Passed! 7/7 + 7/7` trên **14 ca** (scene thừa, `UnityEngine` trong Core, chuỗi cấm, XML fail, manifest/SHA/commit sai, build thiếu `.wasm`, lockfile, UI thiếu ảnh, GC > 0, xóa asset, thiếu docs); công cụ nay ghi **`CẦN NGƯỜI XEM`** cho số cứng/chuỗi cần đánh giá bằng mắt thay vì tự kết luận ĐẠT; đã chạy audit chỉ đọc trên worktree tạm ở commit AG `fe2bb22`. Đây đúng là kiểu bằng chứng cần có.

**Kết quả audit trên `fe2bb22` (danh sách đầu tiên cho AG):** Core thiếu asmdef `noEngineReferences`; MANIFEST `missing=234`, `badHash=42`; số cứng cần xem ở `ArenaView.cs`/`GameConfigAsset.cs`; báo cáo GC không có giá trị `ManagedAllocPerFrame` parse được; thiếu `docs/PHASE2_REPORT.md` và `docs/01_GAMEPLAY_RULES.md`.

## Hai điều cần điều tra ngay (ngắn)
### Y5 – "Core thiếu asmdef `noEngineReferences`" mâu thuẫn với báo cáo trước
Ở X2 (build `942ee08`) Codex ghi `Core/Phase1` "giữ `noEngineReferences`"; nay audit báo thiếu. Xác định:
1. Tệp `.asmdef` nào đang nói đến (đường dẫn), nội dung hiện tại có `"noEngineReferences": true` không; hay audit đang tìm sai tệp/sai thư mục (ví dụ `Core/Phase1` có asmdef riêng, còn `Core/` thì không)?
2. Nếu thật sự mất: `git log -p -- <đường dẫn .asmdef>` cho biết commit nào làm mất (AG hay bản vá `core_cleanup`/`gesture_v2_sync`).
3. Nếu audit sai: sửa công cụ và thêm ca kiểm thử phủ định cho đúng trường hợp đó.
Trả lời ngắn trong khung chat: "audit sai" hoặc "AG làm mất ở commit X"; AG sẽ khôi phục nếu mất.

### Y6 – Bộ sinh `MANIFEST.csv` tự động (mới, tránh lỗi nhập tay)
`missing=234` và `badHash=42` cho thấy MANIFEST viết tay/viết một lần rồi tệp bị ghi đè. Viết `tools/audit/manifest_gen.ps1`:
1. Duyệt `Evidence/` (và thư mục build nếu có `-BuildDir`), cho **mỗi tệp** ghi một dòng: `tệp, sha256, kích thước, giờ ghi, commit` (commit lần cuối chạm tệp bằng `git log -1 --format=%h -- <tệp>`; nếu chưa commit thì ghi `UNCOMMITTED`), và các cột `scene`, `bản build`, `độ phân giải`, `lệnh tạo`, `công cụ` **đọc từ tệp provenance `.json` đi kèm** nếu có (ảnh `x.png` ↔ `x.json`); không có `.json` thì để trống và đánh dấu `NO_PROVENANCE`.
2. Giữ nguyên các cột/thứ tự của `MANIFEST.csv` hiện có (để `phase2_audit.ps1` đọc được); ghi sang tệp mới `MANIFEST.generated.csv`, **không** ghi đè bản hiện có trừ khi có `-Overwrite`.
3. Chế độ `-Check`: chỉ so sánh bản hiện có với thực tế và in số `missing`, `badHash`, `stale` (dòng có tệp không còn), mã thoát khác 0 nếu có lệch.
4. Test (phủ định + khẳng định): thư mục mẫu đủ → `-Check` đạt; thêm tệp mới → `missing=1`; sửa 1 byte → `badHash=1`; xóa tệp → `stale=1`; `.json` thiếu → `NO_PROVENANCE`.
5. Chú thích đầu tệp: cách AG chạy trên thư mục dự án thật (3 dòng). `Passed! N/N`.
Đẩy lên `prep/codex` để AG dùng ở bước S8.

## Rồi tiếp tục (không chờ AG)
**H2 (gateway bắt buộc `join.token`) → A3 → P4 → A2 → Z1 → Z2 → S5 → Z3** → C2 → R3 → P5 → P6 → P7. **X5** khi AG báo xong. **Khi AG báo S1 (xếp hạng về `Core/Phase1`)**: đối chiếu `server/ServerMatch` với hàm xếp hạng mới của Core trên vector vàng (cùng thứ hạng, đồng hạng, người bị loại).

## Báo cáo
```
VIỆC: Y5 | Y6 | H2 | A3 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" / kết luận)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không sửa thư mục của AG hay `Builds`; không ghi đè `MANIFEST.csv` của AG; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không đưa bí mật/khóa thật vào mã; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả test.
