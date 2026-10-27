# WebGL runtime root cause — 2026-10-27

## Verified serving root

- URL: `http://localhost:8015/`
- Listener: `python.exe` PID `33264`, command `C:\Python314\python.exe -m http.server 8015`.
- Served build root: `D:\HITME GAME\worktrees\codex-unity-core\Builds\WebGL`.
- Evidence: the SHA-256 of the served `Build/WebGL.loader.js` is `b88172cfbc518ce2472eb9235f984dd4de4e12067f9518504cf85838b962913d`, identical to that file under the worktree.

`D:\Game Hit Me` and `D:\HITME CODEX` were not the verified served Unity project roots for this recovery.

## Findings

1. This is a development WebGL build, so the Unity footer exposes **Profile** and **Unload**. They were not a Unity startup failure.
2. The browser successfully initialized Unity/WebGL 2 and `Assets/Scenes/MainMatch.unity`; no `NullReferenceException`, `MissingReferenceException`, or browser runtime exception was observed in the recovery run.
3. A previous arena builder disabled `ArenaRoot_25D`. Because `GameObject.Find` cannot discover inactive objects, the attempted re-enable never ran. The root contains the existing full arena backdrop aligned to gameplay bounds.
4. A later full-bleed tile used a small sprite at an extremely large world scale. It covered the intended arena with a flat surface. The final builder now uses the authored FINAL floor at its real bounds and preserves the original backdrop beneath transparent FINAL layers.
5. The menu scrim had previously been nearly opaque. The active menu now keeps a light scrim and presents a real character preview, while actors remain hidden until a match starts by design.
6. `EnsureEventSystem()` was referenced by the current builder but temporarily absent during recovery. The duplicate/undefined helper state was corrected and the final WebGL build compiled successfully.

## Runtime instrumentation

`Assets/HITME/Scripts/Presentation/WebGLRuntimeDiagnostics.cs` emits:

- `BOOT_STARTED`, `UNITY_INITIALIZED`, `STARTUP_SCENE_LOADED`, `MAIN_MENU_READY`
- viewport and camera data
- visibility/bounds for `CombatField`, `ArenaBorder`, `FarMountains`
- actor renderer state and `GAMEPLAY_SCENE_ENTERED`

Observed WebGL values included `MainMatch.unity`, a `624x1350` runtime canvas, visible arena sprites, and a successful transition to `PreparingRound` from the training button.

