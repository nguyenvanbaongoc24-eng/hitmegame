# HIT ME VN — Codex Integration Report

## Baseline

The canonical gameplay project is present in `D:\HITME GAME\unity-client`. Existing gameplay is preserved: touch-to-aim, action lock/reveal, bot, simultaneous projectile resolving, first-hit collision, and draw logic remain in their existing scripts. No gameplay state machine was replaced during bootstrap.

Runtime, render, FINAL asset integration, and WebGL verification results are appended as implementation proceeds.

## Implemented

- FINAL Nam Lang, dep to ong, and festival arena sources imported into `Assets/HITME/Art` with Unity-generated `.meta` files.
- `Assets/HITME/Scripts/Editor/FinalFoundationBuilder.cs` creates/refreshes the visual hierarchy in the existing `Assets/Scenes/MainMatch.unity`; it never invokes `SceneSetupEditor.BuildSampleScene`, which would replace the scene.
- `FinalArena_2_5D` provides far/mid/combat/decor layers, anchored foliage transform sway, and true pivot lantern swing. `HITME_FoliageVertexWind.shader` is a Built-in compatible, subdivision-only option; four-vertex sprites use the root-fixed transform fallback rather than rubber deformation.
- `FinalCharacterVisual` foot-anchors the selected Nam Lang sprite below the existing actor roots. It also binds the final slipper projectile.
- `MatchPresentationAdapter` is read-only and polls the existing state machine, aim controller, actors, and loadout data. `MatchStateMachine` now exposes passive lock/timer accessors; no turn/reveal/throw behavior was changed.
- Main menu no longer uses a full-screen mockup image. It is composed of text, color, and a training button. A safe-area root is installed. HUD bars were reduced to avoid covering the arena.

## Files changed or added

- `Assets/Scenes/MainMatch.unity`
- `Assets/Prefabs/Projectile.prefab`
- `Assets/ScriptableObjects/Characters/NamLang.asset`
- `Assets/ScriptableObjects/Weapons/DepToOng.asset`
- `Assets/Scripts/Core/MatchStateMachine.cs`
- `Assets/HITME/Scripts/Presentation/{MatchPresentationAdapter,SafeAreaFitter,GlobalWindSettings,FoliageSway,PivotSwing}.cs`
- `Assets/HITME/Shaders/HITME_FoliageVertexWind.shader`
- `Assets/HITME/Scripts/Editor/{FinalAssetImportPostprocessor,FinalFoundationBuilder,FinalWebBuildPipeline}.cs`
- `ProjectSettings/TagManager.asset`

## Verification evidence

- Unity 6000.6.4f1 batch visual-foundation execution completed and wrote `HIT ME FINAL visual foundation built into MainMatch` with no C# compile errors.
- WebGL build from current `MainMatch` succeeded: `65,048,535` bytes, `Build Finished, Result: Success`, at `Builds/WebGL` (ignored build output). The first cold build was 190 seconds; the refresh build was 4.5 seconds.
- The WebGL player was served locally and opened in Chrome at 390x844. The modular menu and the Training button rendered and entered an actual match/result flow.
- Runtime captures: `docs/verification/webgl-main-menu-runtime.png` and `docs/verification/webgl-match-runtime.png`. The latter is intentionally retained as evidence of the unresolved arena visibility defect below.
- No iPhone device test was available. iPhone 15 evidence is limited to the 390x844 WebGL viewport and safe-area implementation.

## Known runtime issue / gate not yet passed

In the current WebGL match screen, the menu is correct and the match advances, but the final combat field and player-side visibility are not yet correct: the field appears as camera background while the upper actor/background is visible. This is reproducible in local WebGL and is **not** being presented as a completed visual arena. The cause remains under investigation; likely focus is renderer ordering/visibility in the inherited match scene. Do not accept arena presentation until this is corrected and re-captured.

Other deferred items: real per-state animation clips, SpriteAtlas asset creation/validation, TMP migration of legacy `UnityEngine.UI.Text`, full mobile touch matrix, and physical iPhone/WebGL Safari validation.
