# HIT ME VN — Project Bootstrap

## Official project

- Project root: `D:\HITME GAME\unity-client`
- Unity: `6000.6.4f1`
- Target: WebGL, mobile portrait (iPhone 15 reference)
- Scene in build: `Assets/Scenes/MainMatch.unity`
- Git remote: `https://github.com/nguyenvanbaongoc24-eng/hitmegame.git`

The project was migrated in a controlled copy from `D:\AI APP\HIT ME VN GAME\UnityProject`, the only discovered HIT ME project with working gameplay (`MatchStateMachine`, `AimController`, `BotBrain`, `ProjectileView`, `UIManager`, and `MainMatch`). Only `Assets`, `Packages`, and `ProjectSettings` were copied. `Library`, `Logs`, `Temp`, `Obj`, and `UserSettings` were intentionally excluded. The source project and raw FINAL asset source remain untouched.

## Rendering baseline

The gameplay project currently uses the Built-in Render Pipeline. It has no URP package or assigned SRP asset. The separate `My project` template is URP 17.6.0 but contains no gameplay and is not the canonical project. Pipeline conversion is deferred until the existing materials and scene are validated; no material-breaking conversion has been performed.

## Git policy

Asset serialization is already Force Text (`m_SerializationMode: 2`). Unity `.meta` files are tracked. Generated Unity folders, IDE files, builds, and secrets are ignored. The largest current tracked source file is below 4 MB, so Git LFS is installed but not enabled at baseline.

## Worktree topology

The original baseline was committed on `main` before feature work. Worktrees are siblings of the Unity project, never inside `Assets`:

| Owner | Branch | Working directory |
|---|---|---|
| Integration baseline | `main` | `D:\HITME GAME\unity-client` |
| Codex | `feature/codex-unity-core` | `D:\HITME GAME\worktrees\codex-unity-core` |
| AG | `feature/ag-visual-ui` | `D:\HITME GAME\worktrees\ag-visual-ui` |

`main` baseline SHA: `3caaf5a73a756dbb64e704c1df935a2327387bab`. The remote was verified as reachable and `main` was pushed successfully.

## Foundation changes on the Codex branch

- Built-in renderer retained after audit; no URP conversion was made because the gameplay scene has existing built-in sprite materials and no URP dependency.
- `MainMatch` uses an orthographic camera at `(0,0,-10)`, size `6.15`; its legacy 2.5D camera controller is disabled rather than deleted.
- Sorting layers: `ArenaFar`, `ArenaMid`, `CombatField`, `ArenaDecor`, `Actors`, `Projectiles`, `CombatVFX`, `HUD`.
- FINAL asset imports are Sprite/Single, alpha-safe, Clamp, Bilinear, no mipmaps, HQ compression, 100 PPU for Nam Lang and 50 PPU for environment/weapons. WebGL override caps final-source textures at 1024.
- `SafeAreaFitter` supplies a safe-area root for 390x844 iPhone 15 reference and 9:16 Game View/WebGL testing.
- WebGL build entry point: `Tools/HIT ME/FINAL/Build WebGL`; it builds the existing `MainMatch` without calling the legacy scene-rebuilding tool.
