# HIT ME — DANH MỤC KIỂM TRA PHÁT HÀNH v0.2-rc1 (RELEASE CHECKLIST)
*Bảng điều kiện đóng Phase 2 nghiệm thu kỹ thuật theo yêu cầu Đợt 35*
*Ngày lập: 01/11/2026 | Nhánh: feature/ui-ux-production | HEAD: a713ee8*

---

## 1. BẢNG TIÊU CHÍ ĐÓNG PHASE 2

| # | Tiêu chí nghiệm thu | Trạng thái | Bằng chứng kiểm định / Ghi chú |
|:---:|---|:---:|---|
| **1** | **Audit 12/12 mã thoát 0** trên HEAD sạch | **ĐÃ ĐẠT** | `Evidence/audit/phase2_audit_a41b0c6.txt` (12/12 ĐẠT, exit code 0). `git diff --stat 09f48d2..HEAD -- Assets ProjectSettings` rỗng (không đổi mã nguồn từ build cuối). |
| **2** | **Manifest bao phủ Evidence** (`missing=badHash=stale=0`) | **ĐÃ ĐẠT** | `Evidence/MANIFEST.csv` ghi nhận 722 tệp, mã kiểm tra SHA-256 và commit hợp lệ. |
| **3** | **Build WebGL khớp mã nguồn** | **ĐÃ ĐẠT** | 7 build độc lập trên 7 cổng chuyên dụng (8093→8099). Build cuối `09f48d2` giữ nguyên mã nguồn tới HEAD `a713ee8`. |
| **4** | **Một file XML kiểm thử hợp nhất chuẩn** | **ĐÃ ĐẠT** | `Evidence/phase2/test-results.xml` (Tổng 127/127 tests EditMode passed, 0 fail, 0 skip). |
| **5** | **Ảnh chụp WebGL từng pha có sidecar** | **ĐÃ ĐẠT** | 25 ảnh chụp WebGL 1170×2532 trong `Evidence/` kèm file json sidecar ghi nhận tham số view & canvas. |
| **6** | **Chủ dự án thử trên iPhone (Lô 1–8)** | **CHỜ CHỦ DỰ ÁN** | Đã phát hành phiếu test nhanh `hit-me-owner-test-sheet-lo1-4.md` (mở rộng Lô 5–8 trên cổng 8098/8099). Đang chờ phản hồi thực tế từ iPhone Safari. |
| **7** | **Kiểm định chéo Codex X6** | **CHỜ CODEX** | Chờ Codex chạy đợt kiểm tra chéo X6 và cung cấp module kết nối mạng N2 (`unity_online_glue`). |
| **8** | **Đo chỉ số FPS thực tế trên điện thoại** | **CHỜ CHỦ DỰ ÁN** | Cơ chế đếm FPS đã tích hợp sẵn (chạm 3 lần góc trên-phải). Chờ số liệu FPS và nhiệt độ máy từ thiết bị di động thật. |
| **9** | **Danh sách tồn đọng (Backlog) được duyệt** | **ĐÃ LẬP** | Đã gom toàn bộ 6 tài nguyên `Thiếu` và `TẠM` tại Mục 6 của `docs/UI_ASSET_MAP.md`. |
| **10** | **Lệnh gắn tag phiên bản chuẩn bị sẵn** | **SẴN SÀNG** | Lệnh gắn tag đã soạn sẵn dưới đây, **TUYỆT ĐỐI CHƯA CHẠY** cho đến khi mục 6 và 7 hoàn tất. |

---

## 2. LỆNH GẮN TAG PHÁT HÀNH (CHUẨN BỊ SẴN — CHƯA CHẠY)

```bash
# LƯU Ý: Chỉ chạy lệnh này sau khi Chủ dự án xác nhận DUYỆT và Codex X6 hoàn tất
# git tag -a v0.2-rc1 -m "Release Candidate 1: Hit Me UI/UX Production (Lo 1-8 Complete)"
# git push origin v0.2-rc1
```
