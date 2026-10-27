# HIT ME VN — FINAL Asset Manifest

## Source audit

Authoritative source: `D:\HITME GAME\ASSET HITME FINAL`.

The prompt's alternate `D:\HITME CODEX\ASSET HITME FINAL` path does not exist on this machine. Source data is never edited in place.

| Asset group | Files | PNG | JSON | Source size | Classification | Import status |
|---|---:|---:|---:|---:|---|---|
| NAM LÀNG | 59 | 55 | 4 | 11.36 MB | Character pose frames plus per-state metadata | Selected for vertical slice |
| MAI HỒNG | 81 | 77 | 4 | 14.11 MB | Character pose frames plus metadata | Pending QA |
| ANH NÓN LÁ | 89 | 85 | 4 | 11.60 MB | Character pose frames plus metadata | Pending QA |
| ANH NÚI | 91 | 86 | 4 | 12.04 MB | Character pose frames plus metadata | Pending QA |
| ASSET WEAPONS | 78 | 75 | 3 | 3.28 MB | Weapon rotation/frame sequences | Dép tổ ong selected |
| ASSET BỐI CẢNH | 41 | 39 | 1 | 23.57 MB | Background/environment layers and references | Arena selection pending QA |
| ASSET EFFECT | 235 | 210 | 5 | 54.58 MB | VFX frame sequences; includes Unity import helpers | Pending QA |
| ASSET MENU | 279 | 274 | 1 | 44.51 MB | UI components, icons, panels, references | AG-owned QA |
| ASSET TARGET RULER | 9 | 8 | 1 | 2.14 MB | Aim-ruler components | Pending QA |

## Vertical-slice mapping

| Asset ID | Source | Format / alpha / layout | Unity target | Target prefab | Status |
|---|---|---|---|---|---|
| `char.nam-lang.normal` | `NAM LÀNG/BÌNH THƯỜNG/sprites/*.png` | Individual PNG frames; alpha/layout require Unity import inspection | `Assets/HITME/Art/Characters/NamLang/Normal` | Player actor visual child | Planned |
| `char.nam-lang.dep` | `NAM LÀNG/CẦM DÉP/sprites/*.png` | Individual PNG frames; alpha/layout require Unity import inspection | `Assets/HITME/Art/Characters/NamLang/Dep` | Player actor throw visual child | Planned |
| `weapon.dep-to-ong` | `ASSET WEAPONS/DÉP/sprites/*.png` | Individual PNG rotation frames; alpha/layout require Unity import inspection | `Assets/HITME/Art/Weapons/DepToOng` | `Projectile.prefab` sprite binding | Planned |
| `arena.final` | `ASSET BỐI CẢNH/BỐI CẢNH/**` | Layer/reference images; exact role requires visual QA | `Assets/HITME/Art/Environment/Arena` | `MainMatch` visual hierarchy | Planned |

`metadata.json` is reference/manifest data, not proof that a sheet is a valid Unity animation. No source image is assumed to be an approved sprite until it has been inspected in Unity.

## Imported vertical slice (Codex branch)

| Asset ID | Actual Unity import destination | Validation | Runtime binding |
|---|---|---|---|
| `char.nam-lang.normal` | `Assets/HITME/Art/Characters/NamLang/` | 55 PNG + 4 JSON copied without editing source; individual alpha PNG, `sprite-cutter/1` metadata, 1536x1024 source board, 12 FPS reference, foot pivot metadata | `sprite_001.png` bound to both actors for the smoke-test idle pose and to `NamLang.asset` |
| `char.nam-lang.dep` | `Assets/HITME/Art/Characters/NamLang/CAM DEP/` | Individual transparent pose frames | held frame used for `spriteAim` when available |
| `weapon.dep-to-ong` | `Assets/HITME/Art/Weapons/DepToOng/` | 25 transparent PNG rotation frames + metadata | `sprite_001.png` bound to `Projectile.prefab` and `DepToOng.asset` |
| `arena.festival` | `Assets/HITME/Art/Environment/FestivalArena/` | 39 PNG + one reference text file; inspected layered PNGs include A01 mountains, A02 rooftops, A03 temple roof, A07 stone floor, A08 border, A09 lotus, C01/C02 trees, C07/C08 crowd, B03 lantern | `FinalArena_2_5D` in `MainMatch` |

The imported runtime-source subset totals 110 PNG, 6 JSON, and one text file. UUID-named reference boards were intentionally excluded from the Unity project; raw FINAL source remains only at `D:\HITME GAME\ASSET HITME FINAL` and is not modified.

### Frame and animation decision

Only the idle and held-weapon frames were assigned. The supplied per-state pose/frame metadata has not yet been validated as true contiguous timing for run, throw, hit, elimination, and victory clips, so those clips were deliberately not fabricated. `ActorView` falls back to idle for unverified states. This is an explicit incomplete item, not a claim of finished animation.
