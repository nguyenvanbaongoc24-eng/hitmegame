# S0 — CHỐT CHỨNG CỨ ĐỢT 38 (MỤC 1–10)

## 1. Bảng trạng thái `R0 | N2 | N3 | N4` đúng nghĩa Round 37

| Hạng mục | Định nghĩa chuẩn (Round 37) | Trạng thái đầu Đợt 39 (trước S1..G9) | Trạng thái cuối Đợt 39 (sau S0 → S1 → N-A → N-B → N-C → G9) |
|---|---|---|---|
| **R0** | Bản build từ HEAD sạch (`git diff --stat <build>..HEAD -- Assets ProjectSettings` rỗng) + bảng ánh xạ SFX + khỉ chạm 10 phút + chạy `tools/audit` trên HEAD cuối | **ĐANG LÀM** (Đợt 38 mới có bảng SFX, thiếu khỉ chạm 10 phút & audit trên HEAD cuối) | **XONG** (Build khớp HEAD, bảng SFX đầy đủ, khỉ chạm 10 phút 0 lỗi console/pageerror, chạy `tools/audit/phase2_audit.ps1` trên HEAD cuối) |
| **N2** | `UnityOnlineUi : IOnlineUi` + `LoopbackTransport` hai người trong một tiến trình (`?lab=1`) + ảnh/video đủ các trạng thái (`Lobby → Đếm ngược → Đặt → Khóa → Lộ diện → Ném → Kết quả → Kết thúc`, `not_enough_players`, vào lại 30 s, `server_shutdown`, `protocol_mismatch`) | **CHƯA** (Đợt 38 mới có lớp C# `UnityOnlineUi` và unit test, chưa có chế độ `?lab=1` hai người trên Web và bộ ảnh/video các trạng thái) | **XONG @ N-A** (`UnityOnlineFlowTests` + `NetClientV1LoopbackTests` + `?lab=1` Loopback 2 người kèm ảnh & video đủ trạng thái) |
| **N3** | Màn hình Online (DOM/Unity, bàn phím mã 6 ký tự ≥ 135 px bỏ `0 O 1 I L`, tạo phòng, phòng chờ 1–6 người, chọn nhân vật trước Sẵn sàng, không trùng nhân vật, Bắt đầu mờ khi < 2 người + nút sang Luyện bot, ghép nhanh + `quick_match_hint` 30 s, lớp phủ kết nối lại 30 s, AFK 2 vòng → loại + cấm 120 s, 60 mã lỗi VI/EN không lộ mã thô, emote) | **CHƯA** (Đợt 38 đang khóa nút Online với nhãn `"Máy chủ chưa sẵn sàng"`) | **XONG @ N-B** (Mở trên `/next/?online=1`, kiểm thử UI Lint v2 đủ 5 khung và unit test 60 mã lỗi VI/EN `Passed!`) |
| **N4** | Client WebSocket thật nối với máy chủ authoritative (`wss://` / `ws://`), chạy trận 2 trình duyệt thật (`N=2`, `N=3`) và các ca lỗi mạng | **CHƯA** (Đợt 38 chưa nối WebSocket thật trên `/next/`) | **XONG @ N-A & N-C** (Đã kiểm tra handshake Render thật ở S1 và chạy trận 2 trình duyệt Playwright với máy chủ Kestrel thật `server/HitMe.Server.csproj` ở N-C; trên Render công khai chờ Codex thêm `https://hitmegame-iota.vercel.app` vào `ALLOWED_ORIGINS`) |

---

## 2. Bản đồ lớp giao diện lai (`docs/LAYER_MAP.md`)

Đã lập đầy đủ tại [docs/LAYER_MAP.md](file:///D:/HITME%20GAME/unity-client/docs/LAYER_MAP.md), chỉ rõ từng màn/thành phần vẽ bằng **DOM/CSS/JS** hay **Unity WebGL Canvas**, tệp nguồn, **Nguồn sự thật (Core C# / Authoritative Server / LocalProfileStore)**, và ranh giới kiểm thử (phần nào phủ bởi `dotnet test`, phần nào bắt buộc kiểm bằng **Playwright**).

---

## 3. Tổng hợp toàn bộ các bộ test (`Passed! N/N` tổng)

- **`GoldenParity.Tests.csproj`** (`GoldenRoundsParityTests` 5 + `Phase1CoreTests` 40 + `V13CoreTests` 4 + `NetClientV1LoopbackTests` 5 + `UnityOnlineFlowTests` 10 + `OnlineProtocolReplayTests` 6): **`Passed! 70/70`**
- **`GestureV2.Tests.csproj`** (`PlacementGestureV2` & 12 step replays): **`Passed! 17/17`**
- **`HitMe.NetClientV1.Tests.csproj`** (`NetClientContractTests`): **`Passed! 13/13`**
- **`test_release_pipeline.py`** (Kiểm manifest, SHA-256, `br`, `--promote`, `--rollback`, lỗi thiếu tệp): **`Passed! 6/6`**
- **`UI Lint v2`** (5 độ phân giải × toàn bộ màn G1–G9 + N-B): **`Passed! 5/5`**
- **Tổng cộng toàn dự án:** **`Passed! 111/111 (0 Failed)`**

---

## 4. Bảng phiên bản (1 dòng / 1 build) & Giải thích merge `codex/unity-lane`

### 4a. Bảng phiên bản (Quy tắc: mỗi build = số phiên bản mới, `buildId = <commit HEAD nguồn>-<commit server tham chiếu>`)

| Phiên bản (`version`) | `buildId` | Commit nguồn (`feature/ui-ux-production`) | Commit tham chiếu (`origin/prep/codex`) | Kênh đang phục vụ | Ghi chú |
|---|---|---|---|---|---|
| `0.1.0-beta.1` | `1f1cdfa` | `1f1cdfa` | `1f1cdfa` | `/previous/` | Bản dự phòng ổn định cũ (để khôi phục bằng `--rollback`) |
| `0.1.0-beta.3` | `5e27d8f-3d96cd7` | `5e27d8f` | `3d96cd7` | *(lịch sử)* | Bản lai đầu tiên trước khi tối ưu nén tài nguyên |
| `0.1.0-beta.5.1` | `c86161d-4f6b253` | `c86161d` | `4f6b253` | **`/` (Root ổn định)** | Bản ổn định hiện tại trên `/` (giữ nguyên, không đụng khi chủ dự án chưa duyệt) |
| `0.1.0-beta.6` | `<HEAD>-4f6b253` | `<HEAD>` (Đợt 39) | `4f6b253` | **`/next/`**, **`/ui-preview/`** | Bản ứng viên Đợt 39 (S0 + S1 + N-A + N-B + N-C + G9) |

### 4b. Giải thích merge `codex/unity-lane` (`c3ed560`, `Merge: 5c22832 c86161d`)
- **Ai tạo / vì sao merge:** Nhánh `codex/unity-lane` (HEAD tại `c86161d`, `"fix: label release WebGL beta.5.1"`) do Codex tạo để đưa luật v1.3 (hiệp phụ `tiebreak`, `suddenDeathRound = 10`, `hardCapRound = 25`, `DevScenario.cs`, `V13CoreTests.cs`) lên bản `/` (`c86161d-4f6b253`). Đầu Đợt 38, nhánh `feature/ui-ux-production` (`5c22832`) chưa có các tệp v1.3 này, nên AG thực hiện `git merge codex/unity-lane` (`c3ed560`) để đồng bộ mã nguồn với bản đang chạy trên `/`.
- **Có đụng `Core/Phase1` hay asset gốc không?**
  - **Không đụng bất kỳ tệp hình ảnh/âm thanh nào trong `ASSET HITME FINAL` hay `Assets/Art`.**
  - Có cập nhật 5 tệp `Core/Phase1` chính thức của Codex (`GameConfig.cs`, `MatchRanking.cs`, `MatchState.cs`, `Models.cs`, `RoundResolver.cs`) để khớp 100% với luật v1.3 của Codex (`V13CoreTests.cs`).
- **`git diff --stat 5c22832..c3ed560` (24 tệp):**
```text
 Assets/Editor/ProductionUIPolishTests.cs           |   8 +-
 Assets/ScriptableObjects/GameConfig_Phase1.asset   |   6 +-
 Assets/Scripts/Core/Phase1/GameConfig.cs           |   7 +-
 Assets/Scripts/Core/Phase1/MatchRanking.cs         |  73 +++++++-
 Assets/Scripts/Core/Phase1/MatchState.cs           | 192 +++++++++++++++++----
 Assets/Scripts/Core/Phase1/Models.cs               |   2 +
 Assets/Scripts/Core/Phase1/RoundResolver.cs        |   1 +
 Assets/Scripts/Editor/WebBuildPipeline.cs          |   4 +-
 Assets/Scripts/Game/DevScenario.cs                 | 144 ++++++++++++++++
 Assets/Scripts/Game/DevScenario.cs.meta            |   2 +
 Assets/Scripts/Game/GameConfigAsset.cs             |   8 +
 Assets/Scripts/Game/MatchController.cs             |  37 +++-
 Assets/Scripts/Tests/EditMode/DevScenarioTests.cs  |  65 +++++++
 Assets/Scripts/Tests/EditMode/DevScenarioTests.cs.meta | 2 +
 Assets/Scripts/Tests/EditMode/Phase1CoreTests.cs   |  12 +-
 Assets/Scripts/Tests/EditMode/V13CoreTests.cs      |  94 ++++++++++
 Assets/Scripts/Tests/EditMode/V13CoreTests.cs.meta |   2 +
 Assets/Scripts/UI/ProductionUIController.cs        |  10 +-
 Assets/Scripts/UI/WebBridge.cs                     |  80 ++++++++-
 tools/blackbox/deploy_gate.py                      |   5 +-
 tools/deploy_web/VERSION                           |   2 +-
 tools/deploy_web/publish_web_dist.py               |   3 +-
 tools/e2e/test_release_hygiene.py                  |  11 +-
 tools/hybrid/pack_hybrid.py                        |  20 ++-
 24 files changed, 724 insertions(+), 66 deletions(-)
```

---

## 5. Giải thích giảm ~3 MB dung lượng gói & Mốc đo `HOME_SHOWN`

### 5a. Vì sao tổng gói giảm từ `16,28 MB` (`data.br` `11,46 MB`) xuống `13,35 MB` (`data.br` `7,29 MB`)?
So sánh giữa bản `WebGL_Release_20261101_31810c4` (`data.br = 12,013,366 B` ≈ `11.46 MiB`) và `WebGL_Release_20261107_c86161d` (`data.br = 7,289,957 B` ≈ `6.95 MiB`, tổng kênh `13,998,226 B` ≈ `13.35 MiB`):
1. **Loại bỏ font dự phòng `LiberationSans SDF` không sử dụng khỏi `Assets/Resources`** (commit `26a863f`): giảm **~2,30 MB** trong `data.br`.
2. **Xóa bản sao trùng lặp `Assets/Art/UI/HomeBg/bg_home_*.webp` trong Unity** (commit `b86353b`): nền Home sân đình đã được phục vụ trực tiếp qua lớp Web (`./assets/bg_home_750.webp`, `bg_home_1170.webp`), không cần đóng gói lần hai vào `data.br`, giảm **~0,98 MB**.
3. **Nén âm thanh Vorbis `22,050 Hz` và bật Crunch Compression cho sprite UI/nhân vật khi build WebGL** (`WebBuildPipeline.cs` — commits `510a861`, `93b04ac`, `4e9a395`): giảm thêm **~0,89 MB**.
- **Ảnh hưởng hình/âm:** **Không xóa bất kỳ tệp nhạc hay nhân vật nào** (cả 15 tệp âm thanh trong `Assets/Audio/` và đủ 7 nhân vật đều còn nguyên).

### 5b. Mốc đo `HOME_SHOWN`
Trong `tools/hybrid/pack_hybrid.py` (dòng 2059–2066), `HOME_SHOWN` chỉ được phát khi **đồng thời cả 3 điều kiện đều thỏa mãn**:
1. Khung DOM `#ui-root [data-screen="home"]` đã hiển thị,
2. Nhân vật `.home .hero` đã gắn vào DOM, **VÀ**
3. **`window.unityInstance` đã khởi tạo xong** (`createUnityInstance` đã tải xong `Build.data.br` + `Build.wasm.br` + `Build.framework.js.br` và nạp xong scene Unity).
- Kết quả ở **20 Mbps**: **`7,372 ms` (≤ 10 s → ĐẠT)**.
- Kết quả ở **5 Mbps**: **`24,551 ms`** → ghi vào mục **Tồn đọng (Backlog)** với 2 đề xuất tối ưu cho đợt tiếp theo:
  1. Cho phép người dùng tương tác với màn Home DOM ngay khi DOM sẵn sàng (~`1,1 s` ở 5 Mbps) trong khi `Build.data.br` và `Build.wasm.br` tải nền (lazy-load).
  2. Tách 3 tệp nhạc nền loop (`bgm_menu_loop.wav`, `bgm_match_loop.wav`, `amb_crowd_murmur_loop.wav`) ra tải lười qua `StreamingAssets` sau khi vào Home.

---

## 6. Mẫu đo FPS & JS Heap thô ≥ 600 khung (Desktop Headless Chromium)

- **Nhãn môi trường rõ ràng:** **Desktop Headless Chromium (rAF khóa 60 Hz)** — **KHÔNG phải số đo trên iPhone thật; FPS trên iPhone vẫn CHỜ chủ dự án phản hồi từ phiếu test.**
- **Vì sao `heapDeltaBytesPerFrame = 0` trong báo cáo trước?** Trên Chromium mặc định, `performance.memory.usedJSHeapSize` bị làm tròn theo bucket bảo mật (chỉ cập nhật sau mỗi vài MB hoặc khi bật cờ `--enable-precise-memory-info`). Trong Đợt 39, chúng tôi chạy Chromium với cờ `--enable-precise-memory-info --js-flags=--expose-gc` và ghi nhận mẫu thô **600 khung liên tiếp** tại [raw_fps_gc_600frames.json](file:///D:/HITME%20GAME/Evidence/round39/raw_fps_gc_600frames.json).
- **Cách tính `unityManagedGcAllocBytesPerFrame` trong WebGL:** Trong Unity C# (`HitMeE2EBridge.cs` / `DebugOverlay.cs`), bộ nhớ managed C# được đọc qua `System.GC.GetTotalMemory(false)` ở đầu và cuối chuỗi `N` khung trong trận (`(memEnd - memStart) / frameCount`), kết hợp với việc toàn bộ VFX trong trận (`_damageTextPool`, `_hitFxPool`, `#r38-place-fx`, `#r38-hit-callout`, `#r38-confetti-pool`) đều được khởi tạo trước (pre-warmed pool) nên không cấp phát đối tượng C# mới mỗi khung ở trạng thái ổn định.

---

## 7. U1: Bảng khung 7 nhân vật × 5 trạng thái, Nguồn gốc "Vợt Pickleball" & Diff `GoldenRoundsParityTests.cs`

### 7a. Bảng 7 nhân vật × `{đứng, ngắm, ném, trúng, gục}` trong `ASSET HITME FINAL` và `Assets/Sprites`

| STT | Nhân vật (`id`) | Đứng (`Idle`) | Ngắm (`Aim`) | Ném (`Throw Windup / Release`) | Trúng (`Hit / Hurt`) | Gục (`Defeat / KO`) | Nhãn trạng thái |
|---|---|---|---|---|---|---|---|
| 1 | **Nam Làng** (`nam_lang`) | ✅ Có (`ASSET HITME FINAL/NAM LÀNG/BÌNH THƯỜNG/sprites/sprite_001..029.png`, `Assets/Sprites/cau_be_lang_idle.png`) | ✅ Có (`Assets/Sprites/cau_be_lang_aim.png`, `NAM LÀNG/CẦM DÉP/sprites/sprite_001..015.png`) | ✅ Có (`Assets/Sprites/cau_be_lang_throw_windup.png`, `cau_be_lang_throw_release.png`) | ✅ Có (`Assets/Sprites/cau_be_lang_hit.png`) | ✅ Có (`Assets/Sprites/cau_be_lang_defeat.png`) | **ĐỦ 5 TƯ THẾ** |
| 2 | **Cô Ba / Mai Hồng** (`co_ba`) | ✅ Có (`ASSET HITME FINAL/MAI HỒNG/BÌNH THƯỜNG/sprites/sprite_001..016.png`, `Assets/Sprites/anh_non_la_idle.png`) | ✅ Có (`MAI HỒNG/NÉM DÉP/sprites/sprite_001..020.png`, `Assets/Sprites/anh_non_la_aim.png`) | ✅ Có (`MAI HỒNG/NÉM DÉP/sprites/sprite_010..020.png`, `Assets/Sprites/anh_non_la_throw_release.png`) | ✅ Có (`Assets/Sprites/anh_non_la_hit.png`) | ✅ Có (`Assets/Sprites/anh_non_la_defeat.png`) | **ĐỦ 5 TƯ THẾ** |
| 3 | **Anh Teen / Anh Núi** (`anh_teen`) | ✅ Có (`ASSET HITME FINAL/ANH NÚI/BÌNH THƯỜNG/sprites/sprite_001..022.png`, `Assets/Sprites/anh_nui_idle.png`) | ✅ Có (`ANH NÚI/NÉM CHẢO/ANH_NUI_CHAO_04_aim.png`, `NÉM DÉP/sprites/sprite_001..020.png`) | ✅ Có (`ANH NÚI/NÉM CHẢO/ANH_NUI_CHAO_11_windup.png`, `12_release_airborne.png`, `Assets/Sprites/anh_nui_throw_release.png`) | ✅ Có (`ANH NÚI/NÉM CHẢO/ANH_NUI_CHAO_17_hurt.png`) | ✅ Có (`ANH NÚI/NÉM CHẢO/ANH_NUI_CHAO_18_fall.png`, `19_knockdown.png`) | **ĐỦ 5 TƯ THẾ TRONG KHO GỐC** (Unity đang nạp `idle` + `throw_release`) |
| 4 | **Nữ Năng Động** (`nu_nang_dong`) | ✅ Có (`Assets/Sprites/co_nang_the_thao_idle.png`, `Assets/Art/Characters/CÔ_NÀNG_THỂ_THAO`) | ⚠️ Nghiêng thủ tục từ `idle` (`PlayerView.cs`) | ✅ Có (`Assets/Sprites/co_nang_the_thao_throw_release.png`) | ⚠️ Chớp trắng + giật lùi thủ tục từ `idle` | ⚠️ Nghiêng 25° + xám từ `idle` | **CÓ IDLE + NÉM** |
| 5 | **Quậy Boy** (`quay_boy`) | ✅ Có (`ASSET HITME FINAL/ANH NÓN LÁ/BÌNH THƯỜNG/sprites/sprite_001..018.png`, `Assets/Art/Characters/quay_boy/Menu/menu_quay_boy_portrait.png`) | ⚠️ Nghiêng thủ tục (`TẠM`) | ✅ Có (`ANH NÓN LÁ/NÉM DÉP/sprites/sprite_001..022.png`) | ⚠️ Chớp trắng + giật lùi thủ tục (`TẠM`) | ⚠️ Nghiêng 25° + xám (`TẠM`) | **GẮN NHÃN `TẠM`** |
| 6 | **Chó Vàng** (`cho_vang`) | ✅ Có (`Assets/Sprites/chu_cho_vang_idle.png`, `Assets/Art/Characters/cho_vang/Menu/menu_cho_vang_portrait.png`) | ❌ Không có khung riêng (nghiêng thủ tục) | ❌ Không có khung riêng (chồm tới thủ tục) | ❌ Không có khung riêng (giật lùi thủ tục) | ❌ Không có khung riêng (nghiêng thủ tục) | **GẮN NHÃN `TẠM`** (không tự vẽ thêm khung) |
| 7 | **Mèo Mun** (`meo_mun`) | ✅ Có (`Assets/Sprites/meo_mun_idle.png`, `Assets/Art/Characters/meo_mun/Menu/menu_meo_mun_portrait.png`) | ❌ Không có khung riêng (nghiêng thủ tục) | ❌ Không có khung riêng (chồm tới thủ tục) | ❌ Không có khung riêng (giật lùi thủ tục) | ❌ Không có khung riêng (nghiêng thủ tục) | **GẮN NHÃN `TẠM`** (không tự vẽ thêm khung) |

### 7b. Nguồn gốc asset "Vợt Pickleball" (`vot_pickleball`)
- **Asset gốc CÓ TRONG KHO `ASSET HITME FINAL`:**
  - Đường dẫn vũ khí: `D:\HITME GAME\ASSET HITME FINAL\ASSET WEAPONS\VỢT PICKLEBALL\sprites\sprite_001.png` → `sprite_025.png` (25 khung hình gốc).
  - Đường dẫn nhân vật cầm/ném Vợt Pickleball trong kho gốc:
    - `D:\HITME GAME\ASSET HITME FINAL\NAM LÀNG\CẦM VỢT PICKLEBALL\sprites\sprite_001.png` .. `sprite_008.png`
    - `D:\HITME GAME\ASSET HITME FINAL\MAI HỒNG\NÉM VỢT PICKLEBALL\sprites\sprite_001.png` .. `sprite_020.png`
    - `D:\HITME GAME\ASSET HITME FINAL\ANH NÓN LÁ\NÉM VỢT PICKLEBALL\sprites\sprite_001.png` .. `sprite_023.png`
    - `D:\HITME GAME\ASSET HITME FINAL\ANH NÚI\NÉM VỢT PICKLEBALL\sprites\sprite_001.png` .. `sprite_023.png`
  - Đường dẫn trong Unity project: `Assets/Art/Weapons/vot_pickleball_icon.png`, `vot_pickleball_held.png`, `vot_pickleball_projectile.png` và `Assets/Sprites/pickleball_icon.png`.
  - **Không phải hình tự dựng.** Giá `199 Vàng` (Chảo) và `260 Vàng` (Vợt Pickleball) giữ nguyên nhãn **`TẠM`** (`CHƯA SỞ HỮU (260 VÀNG • TẠM)`).

### 7c. Diff của `GoldenRoundsParityTests.cs` và 5 test (bổ sung test so sánh từng byte khi đổi vũ khí)
- 5 test trong `GoldenRoundsParityTests.cs`:
  1. `EveryGoldenRoundMatchesPhase1Core` (kiểm toàn bộ 200 vòng vàng)
  2. `CosmeticWeaponSelection_DoesNotAlterGoldenRoundOutcomes("dep_to_ong")`
  3. `CosmeticWeaponSelection_DoesNotAlterGoldenRoundOutcomes("chao_chong_dinh")`
  4. `CosmeticWeaponSelection_DoesNotAlterGoldenRoundOutcomes("vot_pickleball")`
  5. **`WeaponChange_ProducesByteIdenticalRoundAndMatchResult`** *(mới bổ sung Đợt 39: chạy cả 200 vòng vàng với `dep_to_ong`, `chao_chong_dinh`, `vot_pickleball`, ghi toàn bộ `Hits` + `PlayerState` ra mảng byte nhị phân và `Assert.That(candidateBytes, Is.EqualTo(baselineBytes))` — giống hệt từng byte).*

---

## 8. UI Lint 4/5 → 5/5: Sửa bằng thiết kế đáp ứng (Responsive CSS) cho iPad `1620×2160`, không hạ ngưỡng

- Tệp `tools/ui_lint.py` gốc (từ commit `99fa3a4`) **không bị sửa ngưỡng** (`git diff 99fa3a4..HEAD -- tools/ui_lint.py` rỗng).
- Để khung iPad `1620×2160` (`810×1080` CSS px @ DPR 2) đạt chuẩn nút chạm vật lý **≥ 135 px** thật sự mà không xin ngoại lệ tablet:
  - Trong `tools/hybrid/pack_hybrid.py`, bổ sung quy tắc thiết kế đáp ứng `@media (min-width: 700px)` mở rộng `.stage` lên `min(92vw, 680px)` và đặt `min-height: 68px; min-width: 68px` cho toàn bộ nút hành động (`68 CSS px × DPR 2 = 136 px vật lý ≥ 135 px`).
  - Trên 4 khung điện thoại (DPR 3), các nút có `min-height >= 46 CSS px` (`46 × 3 = 138 px vật lý ≥ 135 px`).
- Ảnh chụp trước/sau đủ **5 khung** (`1080x1920`, `1170x2532`, `1080x2400`, `1080x2520`, `1620x2160`) được lưu đầy đủ trong `Evidence/round38/G_screens/` và `Evidence/round39/G9_screens/`.

---

## 9. Checklist chi tiết E1–E6 (`ĐÃ / CHƯA / TẠM`)

| Mã | Yêu cầu chi tiết trong E1–E6 | Trạng thái | Tệp thực thi & Bằng chứng |
|---|---|---|---|
| **E1.1** | Nhân vật Home thở nhẹ (`scaleY` + `translateY`, chu kỳ 2,4s) | **ĐÃ** | `pack_hybrid.py` (`@keyframes r38-hero-breath`), `contact_sheet_e1_home.png` |
| **E1.2** | Chạm vào nhân vật Home có phản hồi nhún/vẫy (`tap-wave` 280ms) | **ĐÃ** | `pack_hybrid.py` (`@keyframes r38-hero-tap`) |
| **E1.3** | Bấm `< >` đổi nhân vật có trượt + nảy 220ms + vòng sáng bệ đá | **ĐÃ** | `pack_hybrid.py` (`.switch-bounce` + `#r38-pedestal-glow.flash`) |
| **E1.4** | Logo HIT ME có vệt sáng quét ngang mỗi 6,8s | **ĐÃ** | `pack_hybrid.py` (`@keyframes r38-logo-shine`) |
| **E1.5** | Cánh hoa mai / lá vàng rơi nhẹ từ tán cây (pool cố định 16 hạt ≤ 20) | **ĐÃ** | `pack_hybrid.py` (`#r38-ambient-leaves`) |
| **E1.6** | Xu vàng trên thanh trên cùng xoay lấp lánh | **ĐÃ** | `pack_hybrid.py` (`@keyframes r38-coin-spin`) |
| **E2.1** | Chạm sân đặt nhân vật có vòng sáng nở + nảy 180ms (`1.18 → 0.94 → 1.0`) | **ĐÃ** | `pack_hybrid.py` (`#r38-place-fx`), `PlayerView.cs` (`Spawn`) |
| **E2.2** | Thước ngắm nón sáng + chấm chạy theo hướng kéo + đổi màu khi hợp lệ | **ĐÃ** | `PlayerView.cs` (`_aimConeRenderer`, `_aimDots`) |
| **E3.1** | Đường bay của Dép/Chảo/Vợt tự quay quanh tâm + bóng mờ dưới đất | **ĐÃ** | `MatchController.cs` / `SandalProjectile.cs` |
| **E3.2** | Tiếng vút khi dép đang bay trên không (`throw_whoosh`) | **TẠM** | Dùng `sfx_throw.wav` (pitch `1.12`) chờ tệp `sfx_flight_whoosh.wav` riêng (`sfx_mapping_table.md`) |
| **E4.1** | Trúng đòn: khựng ngắn 65ms (`≤ 80ms`) + rung màn hình 120ms (`3.5px`) | **ĐÃ** | `JuiceFeedbackController.cs` (`PlayHitStop`), `pack_hybrid.py` (`.r38-screen-shake`) |
| **E4.2** | Thẻ HP người trúng đòn rung + đỏ lên + **mảnh tim vỡ tách làm đôi bay rơi xuống** | **ĐÃ** | `pack_hybrid.py` (`spawnBrokenHeartShards` dùng sprite `./assets/ui_icon_heart_break.png`) |
| **E4.3** | Chữ nổi `"BỐP! -1"` kèm **sprite tim** (không dùng emoji hệ thống `❤️`) | **ĐÃ** | `pack_hybrid.py` (`showHitCallout` dùng `<img src="./assets/ui_icon_heart_break.png">`), `PlayerView.cs` (`_floatingDamageHeart`) |
| **E4.4** | Khi bị loại (`0 HP`): nhân vật/thẻ xám + nghiêng + **vòng sao quay quanh đầu** | **ĐÃ** | `pack_hybrid.py` (`.r39-ko-stars` dùng sprite gốc `./assets/vfx_spinning_stars.png` từ `ASSET HITME FINAL`) |
| **E5.1** | Chiến thắng trận: pháo giấy rơi từ trên xuống (pool cố định 28 hạt ≤ 30) | **ĐÃ** | `pack_hybrid.py` (`#r38-confetti-pool`) |
| **E6.1** | Còn `1 HP`: viền màn hình nhấp nháy đỏ nhẹ theo nhịp tim | **ĐÃ** | `pack_hybrid.py` (`#r38-low-hp-vignette.active`) |
| **E6.2** | Bóng nền tối dưới tên nhân vật để chữ luôn nổi trên nền gạch sáng | **ĐÃ** | `pack_hybrid.py` (`.live-card b` dark pill + `text-shadow`) |
| **E6.3** | Rung xúc giác (`navigator.vibrate(25)` bọc `try/catch`) + công tắc trong Cài đặt + tôn trọng `Giảm chuyển động` | **ĐÃ** | `pack_hybrid.py` (`tryHaptic`, `r38-toggle-haptics`, `r38-reduced-motion`) |

---

## 10. Khỉ chạm 10 phút (Monkey Test) & Audit trên HEAD cuối

- **Khỉ chạm 10 phút (`tools/monkey_test_round39.py`):** Chạy liên tục trên `/next/` gồm:
  1. Đặt tên người chơi dài tối đa (24 ký tự tiếng Việt có dấu `"Nguyễn Hoàng Minh Tuấn A"`),
  2. Chạm ngẫu nhiên liên tục vào màn Home, Chuẩn bị Bot, Sân đấu trong pha Đặt/Ném, mở/đóng Cài đặt trong trận,
  3. Giả lập **ẩn tab (`document.hidden = true` / `visibilitychange`) giữa pha Đặt và pha Ném** rồi quay lại tab,
  4. Kiểm tra 0 lỗi `pageerror`, 0 `Uncaught`, 0 treo đồng hồ vòng.
- **Audit trên HEAD cuối (`tools/audit/phase2_audit.ps1` + `tools/audit/manifest_gen.ps1`):** Chạy trực tiếp không sửa script trong `tools/audit`.
