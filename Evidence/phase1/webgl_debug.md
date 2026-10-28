# WebGL debug — Step 1 (2026-10-28)

## Reproduction

- Opened the exact published artifact at `http://127.0.0.1:8080/` in desktop Chrome.
- Reproduced the iPhone symptom: only the default skybox and one cyan player circle appear; arena, HUD, aim ruler, and the remaining players are absent.
- Chrome DevTools Device Toolbar (`Responsive`, 400 x 725 CSS px) reproduces the same output.
- Conclusion: this is a **build/runtime display failure**, not an iPhone-only layout failure. Continue with Step 2 before treating input as a separate mobile defect.

## Browser console evidence

- `NullReferenceException: Object reference not set to an instance of an object.`
- `Can't add component 'LineRenderer' to player because a component is already added to the game object.`
- Informational warning: `The AudioContext was not allowed to start`; unrelated to rendering/input.

## Initial source evidence (no code change in this step)

- `Assets/Scenes/Graybox.unity` is the configured scene and contains Main Camera, Arena, HUD, and Match Controller.
- The camera is orthographic but has `m_ClearFlags: 1` (Skybox), explaining the default sky visible when runtime setup fails.
- `PlayerView.Initialise` adds two `LineRenderer` components to the same GameObject. Unity permits only one; the failed second add produces the console error and subsequent null reference, interrupting match initialization before HUD/state setup.
- Runtime `Shader.Find("Sprites/Default")` / `new Material(...)` also exist in `PlayerView`; these remain to be addressed in Step 2.

## Step 2 result — presentation repair

- Commit `73588fd` moves the aim ruler and touch ring to separate child GameObjects (Unity allows one `LineRenderer` per GameObject), reuses the already-included `SpriteRenderer.sharedMaterial`, and changes Graybox camera clear flags to Solid Color.
- WebGL was rebuilt from the explicit `Assets/Scenes/Graybox.unity` build list. The Chrome Device Toolbar capture `webgl_chrome_device_debug.png` now shows the HUD, player, touch ring, and game canvas; Console has no `NullReferenceException` or `LineRenderer` error.
- The remaining browser message is the expected AudioContext user-gesture warning; it does not block rendering or pointer handling.

## Step 3 result — mobile input diagnostics

- Added the custom `PROJECT:Phase1Mobile` template. It sets a locked viewport, `touch-action: none`, `overscroll-behavior: none`, disables selection/callout, and cancels canvas touch/gesture defaults for Safari.
- `?debugPointer=1` shows scene, FPS, touch count, raw screen position, converted world position, pointer phase/source, and gesture state. It also holds only the diagnostic placement phase, so the ordinary five-second timer is unchanged without that query flag.
- Chrome Device Toolbar receives the automation's **mouse** event (`touchCount=0`); a blank-canvas click moved BẠN, demonstrating the canvas-to-world path for mouse. This automation cannot synthesize a trusted physical iPhone touch, so it is not evidence that `Input.touches` has been received on iPhone.
- Legacy Input stays in place: it already maps both mouse and `Input.touches` through `PointerSample → PlacementGestureState`. There is no observed Legacy-touch failure after the rendering fix that justifies adding Input System yet.
- Physical iPhone verification remains required: open `http://<PC-IP>:8080/?debugPointer=1`, touch/drag, and provide a screenshot of the overlay.
