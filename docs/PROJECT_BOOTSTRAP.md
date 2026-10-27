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
