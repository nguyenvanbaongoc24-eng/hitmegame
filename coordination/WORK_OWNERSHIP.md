# HIT ME VN - Work ownership

| Scope | Owner | Branch / worktree | Rule |
|---|---|---|---|
| Match state, input, aim, projectiles, bot, build, scene integration | Codex | `feature/codex-unity-core` / `D:\HITME GAME\worktrees\codex-unity-core` | Codex is the sole projectile-spawn authority. |
| Visual UI, menu/HUD presentation prefabs, UI art integration | AG | `feature/ag-visual-ui` / `D:\HITME GAME\worktrees\ag-visual-ui` | Do not create gameplay state or edit core-owned files. |
| Integration baseline | Shared only through reviewed commits | `main` / `D:\HITME GAME\unity-client` | Do not work directly in this tree while feature work is active. |

AG paths: `Assets/HITME/UI`, `Assets/HITME/Art/UI`, `Assets/HITME/Prefabs/Presentation`.

Codex paths: `Assets/Scripts/Core`, `Assets/Scripts/Gameplay`, `Assets/HITME/Scripts/Presentation`, `Assets/HITME/Scripts/Editor`, `Assets/HITME/Shaders`, `Assets/Scenes/MainMatch.unity`, build/project configuration.

If a change needs both owners, update the contract first and hand over a commit SHA; do not edit the other worktree's uncommitted files.
