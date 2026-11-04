# HIT ME Web UI tokens (Home prototype)

The reference is `asset/HOME_MOCKUP_TARGET.png`, cropped to the screen at x=47, y=45, width=759, height=1745. `tools/blackbox/compare_home.py` samples the four button interiors at 12% width and 75% height of their reference rectangles. The RGB values below are actual output from `web-ui/evidence/compare_home/metrics.json`; CSS values are in `style.css`.

| Role | Reference sample RGB | Current rendered RGB | ΔE76 | CSS token |
|---|---|---|---|---|
| Quick Play interior | 254, 179, 30 | 249, 178, 37 | 3.03 | `#ffe04a → #f7a51b` |
| Create Room interior | 1, 80, 175 | 1, 99, 197 | 8.06 | `--blue: #087be2` |
| Join Room interior | 2, 107, 78 | 0, 124, 96 | 7.04 | `#22c58b → #007f61` |
| Practice interior | 39, 56, 79 | 39, 60, 84 | 2.03 | `#35516e → #182b42` |

Additional CSS palette values (`--line: #f9ce56`, `--cyan: #03dfe8`) are design approximations, not sampled reference colors.

Buttons use vertical gradients, 2–3px borders, and a 4px lower shadow. The current Home comparison passes 10/13 geometry checks and 4/4 button color checks. The three geometry differences are intentional conflicts between the new 48px touch target / 75% logo requirements and the older mockup's smaller arrows / 62.7% logo. The current arena backdrop and menu portrait are temporary substitutes; the reference has a closer village gate and a portrait holding a slipper. Do not treat the numeric layout/color checks as approval of those images or of Safari audio.
