# Touch test (WebGL LAN)
1. Run `python -m http.server 8080 -d Evidence/phase1/WebGLBuild` on the PC.
2. Find the PC IPv4 address with `ipconfig` (for example `192.168.1.20`).
3. Connect the phone to the same Wi-Fi network.
4. Open `http://<PC-IP>:8080` in the phone browser.
5. Tap an empty arena spot: BẠN must move, clamped inside the arena.
6. Hold BẠN and drag: the yellow aim ruler must follow the finger direction.
7. Drag back near BẠN: the ruler must disappear; fallback throw uses facing direction.
8. Wait five seconds without pressing SẴN SÀNG: the round must lock and throw.
9. Note browser, phone model, Wi-Fi IP, and any failure in the test record.
10. This build is uncompressed, so `http.server` works without Brotli/Gzip headers.
11. For a compressed future build, use a server that sends Content-Encoding or enable Decompression Fallback.
