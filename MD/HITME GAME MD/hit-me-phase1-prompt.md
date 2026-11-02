# HIT ME – PHASE 1: Gameplay graybox offline (có bước rà soát bắt buộc)

> Dán toàn bộ file này cho Codex / Antigravity. Làm **đúng thứ tự các bước**. Không bỏ bước, không làm trước phạm vi của phase sau.

---

## 0. Vai trò & nguyên tắc

Bạn là kỹ sư Unity C# làm game mobile "Hit Me". Mục tiêu của Phase 1 là **một bản chơi được, đúng luật, chạy offline với 5 bot, hiển thị bằng hình khối đơn giản (graybox)**. Chưa làm đẹp.

Nguyên tắc cứng:
1. **Không xóa, không di chuyển** thư mục `ASSET`, `SFX`, `SpriteSources`, `Evidence`, `MD`. Không xóa scene/prefab hiện có; nếu cần bỏ thì chuyển vào `client/Assets/_Archive/` và ghi vào báo cáo.
2. **Không tạo file .md mới** ngoài các file được yêu cầu bên dưới (`AUDIT_REPORT.md`, `PHASE1_REPORT.md`).
3. **Không làm** trong Phase 1: art thật, menu/Home, mạng, server, Supabase, đăng nhập, cửa hàng, chat. Nếu thấy cần, ghi vào báo cáo mục "Đề xuất cho phase sau", đừng làm.
4. Logic game phải **tách khỏi Unity** (thuần C#) để Phase 4 dùng lại trên server.
5. Mỗi bước kết thúc bằng **bằng chứng** (log, kết quả test, screenshot lưu vào `Evidence/phase1/`). Không báo "xong" nếu chưa có bằng chứng.
6. Gặp điểm luật mơ hồ → **dừng và hỏi**, không tự đoán (xem mục 2).

---

## BƯỚC 0 – RÀ SOÁT TỪ ĐẦU (chỉ đọc, chưa sửa code)

Mục tiêu: hiểu hiện trạng thật để không đi sai hướng.

0.1. Sao lưu: chạy `git status`, nếu có thay đổi chưa commit thì commit `chore: snapshot before phase1`, rồi tạo tag `backup-before-phase1`.

0.2. Kiểm kê dự án `D:\HITME CODEX`:
- Phiên bản Unity, render pipeline, các package đang dùng.
- Cây thư mục `client/Assets` (tối đa 3 cấp), danh sách scene, danh sách script C# kèm số dòng.
- Project có **lỗi biên dịch / warning nghiêm trọng** không? Mở và ghi lại.
- Prototype `LocalBot.unity` + `PrototypeRules.json` hiện làm được gì, thiếu gì so với luật ở mục 2.

0.2b. Đối chiếu luật: liệt kê **từng điều luật ở mục 2** đã có / một phần / chưa có / làm sai trong code hiện tại.

0.3. Phân loại từng script và scene: **GIỮ nguyên / SỬA / BỎ (chuyển _Archive)**, kèm lý do 1 dòng.

0.4. Rủi ro & mâu thuẫn: liệt kê chỗ code hiện tại mâu thuẫn với luật, chỗ trộn logic với giao diện, chỗ phụ thuộc vào asset chưa có.

0.5. Viết `MD/AUDIT_REPORT.md` gồm: hiện trạng, bảng đối chiếu luật, bảng GIỮ/SỬA/BỎ, rủi ro, **kế hoạch cụ thể cho Bước 1–4**, và danh sách **câu hỏi cần chủ dự án xác nhận** (mục 2.3).

> ⛔ **DỪNG tại đây.** Gửi báo cáo cho chủ dự án và **chờ xác nhận** trước khi sang Bước 1.

---

## 2. LUẬT GAME (nguồn sự thật cho Phase 1)

### 2.1 Luật đã chốt
- Tối đa **6 người** (1 người chơi + 5 bot trong Phase 1), mỗi người **HP = 3**.
- Mỗi vòng có **pha đặt (5 giây)**: mọi người **đồng thời** (a) chọn vị trí đặt nhân vật tự do trong sân, và (b) kéo để chọn hướng ném. Trong pha này **không ai thấy vị trí hay hướng của người khác**.
- Hết 5 giây **hoặc** tất cả đã bấm SẴN SÀNG → **khóa**. Ai chưa chọn thì dùng giá trị mặc định (xem 2.3).
- **Lộ diện đồng loạt:** tất cả nhân vật xuất hiện cùng lúc ở vị trí đã chọn.
- **Ném đồng thời:** mỗi người ném vũ khí (dép) theo đường thẳng từ vị trí của mình theo hướng đã chọn tới **tường sân**.
- Trúng = người bị trúng **mất 1 HP**. HP về 0 = bị loại.
- Vũ khí **không xuyên nhiều người** (xem câu hỏi 2.3-A).
- Trận kết thúc khi **chỉ còn 1 người** (xem câu hỏi 2.3-B).
- AFK/ngắt kết nối: **chưa làm** ở Phase 1 (chỉ để chỗ hook).

### 2.2 Thông số (đặt trong 1 file cấu hình, không hard-code)
Tạo `GameConfig` (ScriptableObject hoặc JSON) gồm:

| Tham số | Mặc định | Ghi chú |
|---|---|---|
| `maxPlayers` | 6 | |
| `startHp` | 3 | |
| `placeSeconds` | 5.0 | thời gian pha đặt |
| `maxRounds` | 5 | chỉ dùng nếu chọn luật 2.3-B(2) |
| `arenaWidth`, `arenaHeight` | 10 × 16 (đơn vị) | sân chữ nhật dọc, mobile |
| `playerRadius` | 0.45 | bán kính trúng |
| `weaponSpeed` | 14 | đơn vị/giây (chỉ để hoạt cảnh) |
| `damagePerHit` | 1 | |
| `maxHitsPerThrow` | 1 | xem 2.3-A |
| `minSpawnDistance` | 0.9 | tối thiểu giữa 2 nhân vật khi đặt |
| `seed` | cố định hoặc ngẫu nhiên | để test lặp lại được |

### 2.3 CÂU HỎI CẦN CHỦ DỰ ÁN XÁC NHẬN (agent đưa vào AUDIT_REPORT, chờ trả lời)
- **A. "Không xuyên nhiều người, chỉ đi qua một người"** có nghĩa là:
  1. vũ khí trúng người đầu tiên trên đường đi thì **dừng lại** (mặc định của Phase 1), hay
  2. vũ khí đi xuyên qua đúng 1 người rồi tiếp tục và có thể trúng người thứ hai?
  → Code theo tham số `maxHitsPerThrow`; mặc định 1 (dừng ở người đầu tiên).
- **B. Điều kiện kết thúc:** HUD đang hiện "Vòng 1/5". Trận kết thúc khi (1) còn 1 người sống, hoặc (2) hết 5 vòng thì ai nhiều HP nhất thắng? Mặc định: **kết thúc khi còn ≤ 1 người sống; nếu hết `maxRounds` mà còn nhiều người thì xếp hạng theo HP**.
- **C. Nhiều người cùng chết trong 1 vòng** và không còn ai sống: hòa, hay người HP cao nhất trước vòng đó thắng? Mặc định: **hòa giữa những người chết cùng vòng**.
- **D. Người chưa đặt kịp khi hết giờ:** mặc định đứng ở **vị trí vòng trước** (vòng 1: vị trí ngẫu nhiên hợp lệ) và **không ném** (hoặc ném theo hướng cũ). Chọn 1.
- **E. Có tự trúng mình / đồng đội không?** Mặc định: không có tự trúng, không có đồng đội.

---

## BƯỚC 1 – Cấu trúc code (sau khi được duyệt)

```
client/Assets/Scripts/
  Core/          ← C# thuần, KHÔNG using UnityEngine (asmdef: HitMe.Core)
    GameConfig, Vec2 (struct riêng), PlayerState, RoundInput, Phase enum,
    MatchState, RoundResolver, HitCalculator, BotBrain, SeededRandom
  Game/          ← MonoBehaviour, hiển thị & nhập liệu (asmdef: HitMe.Game, tham chiếu Core)
    MatchController, ArenaView, PlayerView, AimInput, PlacementInput, HudView, WeaponFlightView
  Tests/EditMode/ ← NUnit (asmdef: HitMe.Tests)
```

Yêu cầu:
- Core không phụ thuộc Unity → chạy được trong EditMode test.
- Vòng chơi là **máy trạng thái** rõ ràng: `Placing → Locked → Reveal → Throwing → Resolve → (RoundEnd | MatchEnd)`.
- Mọi ngẫu nhiên đi qua `SeededRandom` để tái hiện được.

## BƯỚC 2 – Logic lõi + test

2.1. `HitCalculator`: với mỗi người ném, đoạn thẳng từ vị trí → tới tường theo hướng; tìm các người sống khác có đường tròn (bán kính `playerRadius`) bị đoạn cắt; sắp theo khoảng cách; áp `maxHitsPerThrow`.
2.2. `RoundResolver`: tính mọi lượt ném trên **cùng một ảnh chụp vị trí**; cộng dồn sát thương; trừ HP **sau khi** tất cả đã tính (không phụ thuộc thứ tự); đánh dấu người bị loại.
2.3. `MatchState`: quản lý vòng, người sống, điều kiện thắng theo xác nhận ở 2.3-B/C.
2.4. `BotBrain` (3 mức đơn giản): ngẫu nhiên hoàn toàn / né sát tường & ném về phía trung tâm / nhớ vị trí các vòng trước để đoán.

**Unit test bắt buộc (EditMode, tất cả phải pass):**
- Ném thẳng trúng 1 người → -1 HP. Ném lệch xa → không trúng.
- Hai người cùng nằm trên 1 đường ném → kết quả đúng theo `maxHitsPerThrow` (test cả giá trị 1 và 2).
- Hai người ném trúng nhau cùng lúc → cả hai đều -1 (đồng thời, không phụ thuộc thứ tự).
- Nhiều người cùng ném vào 1 người → nhận nhiều sát thương, HP không âm dưới 0 theo luật hiển thị.
- Người đã bị loại không ném và không bị trúng.
- Hai nhân vật đặt quá gần (< `minSpawnDistance`) → bị đẩy ra hợp lệ.
- Đặt ngoài sân → bị kẹp vào trong sân.
- Điều kiện kết thúc: còn 1 người; không còn ai; hết `maxRounds`.
- Cùng `seed` + cùng input → cùng kết quả (determinism), chạy 1000 trận mô phỏng bot vs bot không lỗi, không treo.

## BƯỚC 3 – Màn graybox trong Unity

Tạo scene mới `Graybox.unity` (không sửa `LocalBot.unity`). Hiển thị:
- Sân chữ nhật (ô màu xám), tường là viền; nhân vật = vòng tròn màu khác nhau, có nhãn tên + số HP; vũ khí = chấm/đoạn thẳng bay.
- HUD tối thiểu (chữ thường, chưa cần art): `Vòng x/5`, đồng hồ đếm ngược, danh sách 6 người kèm HP, nút **SẴN SÀNG**, dòng hướng dẫn "Chạm để đặt · Kéo để chọn hướng".
- Nhập liệu cảm ứng/chuột:
  1. Chạm/click vào sân để đặt vị trí; kéo nhân vật để dời.
  2. Kéo từ nhân vật ra để chọn hướng, hiện mũi tên/đường ngắm (giống mockup "Kéo để chọn hướng").
  3. Bấm SẴN SÀNG để khóa sớm.
- Pha **Lộ diện**: bot hiện ra đồng loạt (chấm hiệu ứng đơn giản) → **Ném**: các vũ khí bay cùng lúc → **Kết quả**: trúng thì nháy đỏ, HP giảm, người chết mờ đi.
- Cuối trận: bảng kết quả + nút "Chơi lại".
- Khung hình tham chiếu: tỷ lệ màn dọc 9:19.5, an toàn với tai thỏ; sân chiếm phần lớn màn hình, chỉ chừa dải nhỏ phía trên cho thẻ người chơi.

## BƯỚC 4 – Kiểm thử & bằng chứng

- Chạy toàn bộ unit test, lưu kết quả `Evidence/phase1/tests.txt`.
- Chạy mô phỏng 1000 trận bot vs bot, lưu thống kê (số vòng trung bình, tỉ lệ thắng theo vị trí bot) `Evidence/phase1/sim.txt`.
- Chơi tay 10 trận liên tiếp trong Editor, ghi log; chụp screenshot cho từng pha (Placing, Reveal, Throwing, Result) vào `Evidence/phase1/`.
- Build thử 1 bản Android hoặc WebGL (nếu môi trường cho phép) và ghi kết quả; nếu không build được, ghi rõ lý do.

---

## TIÊU CHÍ XONG (Definition of Done)

- [ ] `AUDIT_REPORT.md` đã được chủ dự án duyệt trước khi sang Bước 1.
- [ ] Project biên dịch sạch, không lỗi đỏ.
- [ ] Core là C# thuần, có asmdef riêng, không tham chiếu UnityEngine.
- [ ] Toàn bộ unit test ở Bước 2 pass; mô phỏng 1000 trận không lỗi/treo.
- [ ] Chơi tay 10 trận liên tiếp trên `Graybox.unity` đúng luật, không lỗi.
- [ ] Mọi thông số nằm trong `GameConfig`, không hard-code.
- [ ] Không xóa `ASSET`, `SFX`, scene/prefab cũ.
- [ ] Có `MD/PHASE1_REPORT.md`: đã làm gì, bằng chứng ở đâu, luật đã chốt (kết quả 2.3), vấn đề còn tồn đọng, đề xuất cho Phase 2.
- [ ] Commit sạch, tag `v0.1-graybox`.

## ĐIỀU CẤM (nhắc lại)
Không làm art/menu/mạng; không viết thêm .md ngoài danh sách; không sửa luật tự ý; không báo xong khi thiếu bằng chứng; không xóa thư mục `ASSET`/`SFX`.

## Mẫu báo cáo cuối mỗi bước
```
BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn log/screenshot/test)
VẤN ĐỀ/CÂU HỎI: ...
SẴN SÀNG SANG BƯỚC: x+1? (có/không)
```
