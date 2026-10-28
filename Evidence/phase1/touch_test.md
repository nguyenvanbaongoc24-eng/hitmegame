# Touch test (WebGL LAN)
1. Run `python -m http.server 8080 -d Evidence/phase1/WebGLBuild` on the PC.
2. Put phone and PC on the same Wi-Fi; find PC IPv4 with `ipconfig`.
3. First open `http://<PC-IP>:8080/?debugPointer=1` in Safari (this only holds placement and shows telemetry).
4. Confirm overlay shows `scene=Graybox`; tap blank arena and confirm `source=touch`, touchCount and raw/world coordinates change.
5. Hold BẠN and drag: aim ruler must follow the finger; drag back near BẠN to cancel it.
6. Take a screenshot of that overlay and send it with iPhone model/iOS/Safari version.
7. Then open `http://<PC-IP>:8080` (without query): normal five-second timeout must lock and throw.
8. Verify tap moves BẠN (clamped in arena), drag aims, and Safari neither scrolls nor zooms the page.
9. Record any failure and whether the HUD, arena, six players, and ruler are visible.
10. This build is uncompressed, so `http.server` works without Brotli/Gzip headers.
11. For a compressed future build, use a server sending Content-Encoding or enable Decompression Fallback.
