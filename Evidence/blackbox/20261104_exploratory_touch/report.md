# Exploratory touch on deployed WebKit build

URL: `https://hitmegame-iota.vercel.app/?e2e=1`; Playwright WebKit iPhone 15 Pro emulation, CSS viewport 393×852, DPR 3. These manually chosen coordinates came from the visible screenshot because the read-only bridge is absent. They are exploratory evidence, not T2/T3/T5 automated passes.

1. Tap CSS `(196,735)` on `LUYỆN VỚI BOT`: selection screen opened. See `after_training_tap.png`.
2. Tap CSS `(152,268)` at the center of `Nữ Năng Động`: the highlight and description still showed `Nam Làng` after 1.5 seconds. See `after_character_tap.png`. It is byte-for-byte identical to `after_training_tap.png` (SHA-256 `A7E00E38EBD445AD2B49BB5C7D6F3C12D7E06388808E4ECB009E149871F417A8`). This reproduces the selection touch problem in WebKit emulation; root cause is not determined.
3. Tap CSS `(196,705)` on `BẮT ĐẦU (VÀO SÂN)`: match tutorial opened. Tap `(120,525)` on `BỎ QUA`: match screen appeared. See `after_start_tap.png` and `match_after_tutorial_skip.png`.

Visible issues in the screenshots: the selection screen has uncovered landscape strips at the top and bottom; the Quậy Boy card shows the orange-haired girl; there is no weapon selection row. The match screenshot shows a dark smudge over the central flower, landscape strips above/below the arena, `CHA T` clipped at bottom right, Quậy Boy and Chó Vàng used as bots, and tiny hearts on the HUD. The arena art itself spans the screen width in this frame; that does not establish the playable-rectangle or placement bounds without the bridge.

This capture does not test a physical iPhone, aim dragging, hit reaction, KO, or sound. Antigravity A0–A2 remain needed. Do not promote these images to goldens.
