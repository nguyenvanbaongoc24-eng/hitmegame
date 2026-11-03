# HIT ME harness library

Run synthetic tests: `python -m unittest discover -s tools/harness_lib -p 'test_*.py' -v`. Requires Pillow and NumPy. This package reads exported JSON/screenshots; it does not open Unity. AG can copy the files with `git show prep/codex:tools/harness_lib/<file>`.

`layout_check.check(dump)` input: `{"safeArea":[x,y,w,h],"pixelsPerCssPixel":2,"minTouchCssPx":44,"maxClickableOverlapFraction":0.05,"elements":[{"name":"Ready","rect":[x,y,w,h],"clickable":true,"text":"Sẵn sàng","textRect":[x,y,w,h]}]}`. Rectangles use screenshot pixels; the minimum touch size is CSS pixels multiplied by `pixelsPerCssPixel` (default 1). Only clickable pairs are checked for overlap. Every element must fit the safe area; any text rectangle must fit its parent.

`visual_diff.compare(actual.png,reference.png,spec,overlay.png)` input spec: `{"regions":[{"name":"arena","rect":[x,y,w,h],"background":[0,0,0],"minSsim":0.8,"maxChangedFraction":0.2,"pixelThreshold":20,"maxCentroidShift":10}]}`. Images must have the same dimensions. SSIM here is global luminance SSIM per region; changed fraction and centroid shift are separate gates. The optional output is a 50% reference/actual overlay.

`head_ratio.check(anatomy)` input: `{"points":{"headTop":[x,y],"chin":[x,y],"hip":[x,y],"heel":[x,y]},"expectedHeadFraction":0.25,"tolerance":0.03}`. All y values increase downward. Result uses head height divided by full body height. `report.render({"layout":result,"visual":result})` returns a PASS/FAIL Markdown table.

Thresholds are examples, not approved visual targets. Calibrate against the complete match mockup and real device captures before treating a pass as production evidence.
