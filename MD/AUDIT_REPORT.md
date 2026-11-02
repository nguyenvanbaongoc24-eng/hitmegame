# HIT ME — AUDIT REPORT (Phase 1, Bước 0)

**Ngày rà soát:** 28/10/2026 (ICT)  
**Trạng thái:** Bước 0 đã được chủ dự án duyệt ngày 28/10/2026 — sẵn sàng bắt đầu Bước 1.

## 0. Phạm vi và sao lưu

- Tài liệu yêu cầu rà soát `D:\HITME CODEX`, nhưng đường dẫn này không có trong workspace. Dự án Unity/Git thực tế được xác định là `D:\HITME GAME\unity-client` (nhánh `main`), vì đây là thư mục có `Assets`, `Packages`, `ProjectSettings` và `.git` hợp lệ. `D:\HITME GAME\My project` là project Unity 2D mẫu, không phải Git repository và chỉ chứa `SampleScene` cùng script welcome của Unity.
- `git status --short` tại `unity-client`: sạch, nên **không có commit snapshot** cần tạo.
- Đã tạo tag sao lưu `backup-before-phase1` tại commit `3caaf5a73a756dbb64e704c1df935a2327387bab`.
- Không có file/code/scene/prefab nào bị sửa trong Bước 0. (Tệp báo cáo này là đầu ra được yêu cầu.)

## 1. Hiện trạng dự án

| Hạng mục | Kết quả |
|---|---|
| Unity | `6000.6.4f1` |
| Render pipeline | Universal Render Pipeline 17.6.0; project hiện dùng 2D Renderer/URP assets |
| Package đáng chú ý | UGUI 2.6.0, Unity Test Framework 1.8.0, Input System 1.20.0, Rider/Visual Studio integration |
| Scene build hiện tại | `Assets/Scenes/MainMatch.unity` (duy nhất trong Build Settings) |
| Scene khác | Không có `LocalBot.unity`; không có `PrototypeRules.json` trong workspace |
| Assembly definitions | Không có asmdef cho runtime/tests hiện hữu; toàn bộ code game đang vào `Assembly-CSharp` |
| Test hiện hữu | Không có script test/assembly test |

### Cây `Assets` (tối đa 3 cấp)

```text
Assets/
  Art/ (Animations, Arena, Characters, Emotes, Shaders, UI, Weapons)
  Audio/
  Editor/ (SpriteSheetAutoSlicer.cs, WeaponAutomationEditor.cs)
  Fonts/
  Prefabs/ (Projectile.prefab)
  Scenes/ (MainMatch.unity)
  ScriptableObjects/ (Characters, Weapons)
  Scripts/
    Audio/ (AudioManager.cs)
    Core/ (BotBrain.cs, CharacterData.cs, GameModels.cs, MatchStateMachine.cs, WeaponData.cs)
    Editor/ (AssetAutomationEditor.cs, SceneSetupEditor.cs, SpriteImportSetup.cs, WebBuildPipeline.cs)
    Gameplay/ (ActorView.cs, AimController.cs, CameraController25D.cs, ProjectileView.cs)
    UI/ (UIManager.cs)
  Sprites/
```

### C# hiện hữu

| Script | Dòng |
|---|---:|
| `Assets/Editor/SpriteSheetAutoSlicer.cs` | 515 |
| `Assets/Editor/WeaponAutomationEditor.cs` | 166 |
| `Assets/Scripts/Audio/AudioManager.cs` | 66 |
| `Assets/Scripts/Core/BotBrain.cs` | 50 |
| `Assets/Scripts/Core/CharacterData.cs` | 23 |
| `Assets/Scripts/Core/GameModels.cs` | 149 |
| `Assets/Scripts/Core/MatchStateMachine.cs` | 352 |
| `Assets/Scripts/Core/WeaponData.cs` | 25 |
| `Assets/Scripts/Editor/AssetAutomationEditor.cs` | 106 |
| `Assets/Scripts/Editor/SceneSetupEditor.cs` | 467 |
| `Assets/Scripts/Editor/SpriteImportSetup.cs` | 44 |
| `Assets/Scripts/Editor/WebBuildPipeline.cs` | 52 |
| `Assets/Scripts/Gameplay/ActorView.cs` | 187 |
| `Assets/Scripts/Gameplay/AimController.cs` | 125 |
| `Assets/Scripts/Gameplay/CameraController25D.cs` | 139 |
| `Assets/Scripts/Gameplay/ProjectileView.cs` | 72 |
| `Assets/Scripts/UI/UIManager.cs` | 233 |

### Biên dịch / lỗi nghiêm trọng

Đã chạy `unity test "D:\HITME GAME\unity-client" --mode EditMode` bằng Unity CLI. Unity import và biên dịch script thành công; XML kết quả ghi `result="Passed"`, `testcasecount="0"`. Không có lỗi C# trong lần chạy này. Đây **không** phải bằng chứng test Phase 1: suite hiện có 0 test. Kết quả tạm: `%TEMP%\hitme-phase1-audit-editmode.xml`.

## 2. Prototype hiện có

`MainMatch.unity` là prototype 1 người chơi + 1 bot, có menu, HUD, đặt vị trí bằng click, ngắm theo chuột, cửa sổ 5 giây, ẩn bot trong lúc ngắm, reveal và ném. `ProjectileView` dừng sau va chạm đầu tiên và `MatchStateMachine` gom hit rồi trừ HP sau khi đạn bay xong.

Không tồn tại `LocalBot.unity`/`PrototypeRules.json` như tài liệu nêu, nên không thể kiểm kê chính hai artifact đó. Đánh giá trên `MainMatch.unity` và toàn bộ code runtime hiện hữu.

## 3. Đối chiếu từng luật Phase 1

| Luật | Trạng thái | Bằng chứng / chênh lệch |
|---|---|---|
| Tối đa 6 người (1 + 5 bot), HP 3 | **Làm sai** | Scene và `MatchStateMachine` hard-code 2 actor (`playerActor`, `botActor`). HP 3 được dùng cho hai actor, nhưng `CharacterData`/automation có giá trị khác. |
| Pha đặt 5 giây, mọi người chọn vị trí + hướng bí mật | **Một phần** | Timer 5 giây và bot bị ẩn có sẵn; chỉ người chơi có input, bot tự chọn; hướng người chơi luôn theo vị trí chuột, không phải thao tác kéo tách biệt. |
| Hết giờ/tất cả ready thì khóa; chưa chọn dùng mặc định | **Một phần** | Có auto-lock/both-locked. Chưa có chính sách mặc định được cấu hình; người chơi vẫn ném theo hướng hiện tại. |
| Reveal đồng loạt | **Một phần** | Thực hiện cho 2 actor, chưa phải 6 người. |
| Ném đồng thời thẳng đến tường | **Một phần** | Hai projectile sinh cùng lúc, đi thẳng; lifetime 1.2 s thay vì giao với tường sân theo tính toán lõi. |
| Trúng mất 1 HP, HP 0 bị loại | **Có (2 người)** | `HitRecord.damage = CombatRules.DamagePerHit`; `ActorView.TakeDamage` clamp về 0 và đổi pose. |
| Không xuyên nhiều người / `maxHitsPerThrow` | **Một phần** | Projectile bị hủy ở hit đầu, nhưng target được duyệt theo thứ tự mảng/scene, không sắp theo khoảng cách và không có cấu hình `maxHitsPerThrow`. |
| Kết thúc còn 1 người / nhánh `maxRounds` | **Làm sai** | Chỉ xử lý thắng/thua/hòa giữa 2 actor; vòng tiếp tục không giới hạn, HUD ghi `/5` cứng. |
| AFK/ngắt kết nối hook | **Chưa có** | Không có interface/hook. |
| Một file `GameConfig` cho mọi thông số | **Chưa có** | `CombatRules` chứa nhiều `const`; các giá trị khác rải trong view/state machine. |
| Core C# thuần, có asmdef | **Làm sai** | `GameModels`, `BotBrain`, `MatchStateMachine` dùng `UnityEngine`; `MatchStateMachine` còn là `MonoBehaviour` phụ thuộc view/UI/audio. Không có asmdef. |
| Máy trạng thái chuẩn | **Một phần** | Có state machine nhưng state khác yêu cầu (`Initializing`, `Aiming`, `Resolving`...), không có các pha `Locked`, `Throwing`, `RoundEnd` chuẩn và logic lẫn Unity. |
| Seeded deterministic random | **Chưa có** | `BotBrain` dùng `UnityEngine.Random`; không có seed hoặc replay. |
| 3 mức bot | **Chưa có** | Có một bot random đơn giản; không có né sát tường/memory. |
| Unit test + simulation | **Chưa có** | 0 EditMode test, không có simulator. |

## 4. Phân loại scene và script

Không di chuyển/xóa gì ở Bước 0. “Bỏ” bên dưới nghĩa là **chỉ chuyển vào `Assets/_Archive/` sau khi phần thay thế hoạt động và các tham chiếu scene cũ đã được bảo toàn**.

| Artifact | Phân loại | Lý do |
|---|---|---|
| `Scenes/MainMatch.unity` | GIỮ nguyên | Scene/prototype cũ phải được bảo toàn; Phase 1 sẽ tạo `Graybox.unity` riêng. |
| `Scripts/Core/GameModels.cs` | BỎ (sau thay thế) | Kiểu game lẫn `UnityEngine.Vector2` và `CombatRules` hard-code. |
| `Scripts/Core/MatchStateMachine.cs` | BỎ (sau thay thế) | MonoBehaviour điều phối 2 actor, UI, audio và gameplay chung một lớp. |
| `Scripts/Core/BotBrain.cs` | SỬA | Giữ ý tưởng bot nhưng chuyển thành C# thuần, seeded và ba chiến lược. |
| `Scripts/Core/CharacterData.cs`, `WeaponData.cs` | GIỮ nguyên | Dữ liệu/art prototype, không thuộc core graybox. |
| `Scripts/Gameplay/ActorView.cs`, `AimController.cs`, `ProjectileView.cs` | BỎ (sau thay thế) | Gắn chặt luật, input và va chạm Unity; không đáp ứng logic thuần/6 người. |
| `Scripts/Gameplay/CameraController25D.cs` | GIỮ nguyên | Hữu ích cho scene cũ; Graybox dùng camera riêng nếu cần. |
| `Scripts/UI/UIManager.cs` | GIỮ nguyên | Gắn với menu cũ; Graybox dùng HUD view riêng và không sửa menu. |
| `Scripts/Audio/AudioManager.cs` | GIỮ nguyên | Không chặn core; Graybox có thể không dùng âm thanh. |
| `Scripts/Editor/*.cs`, `Editor/*.cs` | GIỮ nguyên | Tooling asset/build hiện hữu, ngoài phạm vi luật Phase 1. |
| `Prefabs/Projectile.prefab` | GIỮ nguyên | Prefab cũ, không bắt buộc dùng trong graybox. |

## 5. Rủi ro và mâu thuẫn

1. **Sai target dự án trong tài liệu:** `D:\HITME CODEX`, `LocalBot.unity` và `PrototypeRules.json` đều không có. Kế hoạch này chỉ an toàn nếu chủ dự án xác nhận `unity-client` là project đích.
2. **Không thể tách asmdef trực tiếp tại chỗ:** hiện `HitMe.Core` chứa `MonoBehaviour`, `UnityEngine` và các kiểu view. Cần đặt core mới ở thư mục/asmdef riêng trước, rồi adapter Unity mới tham chiếu vào.
3. **Thứ tự va chạm không xác định:** prototype duyệt targets theo mảng, không theo điểm cắt gần nhất, trái quy tắc hit đầu tiên.
4. **Quy tắc lẫn UI:** HP, round cap, thời gian, kích thước sân và tốc độ nằm ở constants/view/scene khác nhau; sẽ dẫn tới game-client và server sau này lệch nhau.
5. **Đầu vào mobile chưa đúng yêu cầu:** click là di chuyển, không có cơ chế drag-from-character để aim; không kiểm tra `minSpawnDistance`.
6. **Artifact không cần thiết cho Phase 1:** source có nhiều art, menu, Weapon/Character ScriptableObject. Không được dùng để mở rộng Phase 1; chỉ giữ nguyên.
7. **Build tool cũ sai đường dẫn:** `WebBuildPipeline.cs` trỏ `D:/HIT ME VN GAME/Builds/WebLocal`, không nằm trong workspace hiện tại.

## 6. Kế hoạch Bước 1–4 (chỉ thực hiện sau duyệt)

1. **Bước 1 — cấu trúc:** tạo `HitMe.Core`, `HitMe.Game`, `HitMe.Tests` asmdef; xây core C# thuần (`Vec2`, config data, state, inputs, resolver, hit calculator, seeded RNG, bot); không sửa `MainMatch.unity`.
2. **Bước 2 — luật/test:** viết NUnit EditMode cho toàn bộ ca bắt buộc, dùng `GameConfig` duy nhất và simulation 1.000 trận deterministic; lưu bằng chứng test/sim vào `Evidence/phase1/`.
3. **Bước 3 — graybox:** tạo `Graybox.unity` độc lập với sân 10×16, 6 vòng tròn/player views, HUD và input chuột/cảm ứng. Unity layer chỉ trình bày kết quả của core.
4. **Bước 4 — xác minh:** chạy toàn test, simulator và 10 trận thủ công; chụp bốn pha; thử Android/WebGL nếu module có sẵn; tạo `PHASE1_REPORT.md`, commit và tag `v0.1-graybox` khi toàn bộ DoD đạt.

## 7. Câu hỏi cần chủ dự án xác nhận trước Bước 1

1. Xác nhận **project đích là `D:\HITME GAME\unity-client`**, không phải đường dẫn/document artifact bị thiếu `D:\HITME CODEX`, `LocalBot.unity`, `PrototypeRules.json`.
2. **A — đã chốt:** vũ khí dừng ở người đầu tiên trúng (`maxHitsPerThrow = 1`).
3. **B — đã chốt:** trận kết thúc khi chỉ còn một người sống.
4. **C — đã chốt:** người chết cùng bị loại và vòng tiếp tục nếu vẫn còn người sống; nếu hai người cuối cùng cùng chết thì cả hai đồng thắng.
5. **D — đã chốt:** hết 5 giây khi chưa đặt thì nhân vật giữ nguyên vị trí, tự ném theo hướng thẳng mặt gần nhất (hướng đã khóa/hướng mặc định); không đứng yên không ném.
6. **E — đã chốt:** không tự trúng, không có đồng đội ở Phase 1.

---

```text
BƯỚC: 0
ĐÃ LÀM: rà soát chỉ-đọc project Unity thực tế, đối chiếu luật, kiểm tra biên dịch/EditMode, tạo tag backup.
FILE THAY ĐỔI: MD/AUDIT_REPORT.md; Git metadata: tag backup-before-phase1.
BẰNG CHỨNG: git tag -> 3caaf5a73a756dbb64e704c1df935a2327387bab; %TEMP%/hitme-phase1-audit-editmode.xml (Passed, 0 test).
VẤN ĐỀ/CÂU HỎI: project/path artifact trong tài liệu không khớp; luật A–E đã được chủ dự án xác nhận ngày 28/10/2026.
SẴN SÀNG SANG BƯỚC: 1? có.
```
