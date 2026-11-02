# C11 ΓÇô Ph├ón t├¡ch tß║úi WebGL build 8c6b328

─É├ú chß║íy `python tools/analyze_webgl_load.py` ΓåÆ **Passed! 2/2 resource traces**. Sß╗æ liß╗çu th├┤: `C11_resources.json`. Edge headless, viewport 414├ù896, mß║íng giß║ú lß║¡p 20 Mbps/40 ms. Ph├⌐p ─æo C7 ─æß╗Öc lß║¡p (3 l╞░ß╗út/build): Dev trung vß╗ï 17,672 s; Release Brotli 10,006 s.

| Build | Tß╗çp lß╗¢n nhß║Ñt | Tß╗çp thß╗⌐ hai | Phß║ún hß╗ôi t├ái nguy├¬n cuß╗æi | HOME_SHOWN | Khoß║úng sau tß║úi |
|---|---|---|---:|---:|---:|
| Dev | wasm 18,47 MB (14,01 s) | data 17,49 MB (13,74 s) | 14,78 s | 17,92 s | 3,14 s |
| Release Brotli | data.br 12,04 MB (7,05 s) | wasm.br 4,39 MB (3,73 s) | 7,29 s | 9,93 s | 2,64 s |

K├¡ch th╞░ß╗¢c to├án bß╗Ö tß╗çp build: Dev **36,31 MB**, Release **16,53 MB**. Thß╗¥i gian tß╗½ng tß╗çp ß╗ƒ bß║úng l├á `PerformanceResourceTiming.duration`, c├│ thß╗â tß║úi song song n├¬n kh├┤ng cß╗Öng lß║íi. ΓÇ£Khoß║úng sau tß║úiΓÇ¥ gß╗ôm giß║úi n├⌐n, bi├¬n dß╗ïch/khß╗ƒi tß║ío WebAssembly, khß╗ƒi tß║ío Unity, asset v├á giao diß╗çn; sß╗æ liß╗çu hiß╗çn tß║íi **kh├┤ng t├ích ri├¬ng** c├íc phß║ºn ─æ├│. Kh├┤ng suy ra thß╗¥i gian tr├¬n iPhone tß╗½ Edge headless.

─Éß╗ü xuß║Ñt cho AG, theo t├íc ─æß╗Öng dß╗▒ kiß║┐n / ─æß╗Ö kh├│:

1. **╞»u ti├¬n cao, trung b├¼nh:** t├¼m asset nß║╖ng trong Unity data, giß║úm k├¡ch th╞░ß╗¢c texture/sprite atlas v├á bß╗Å asset kh├┤ng d├╣ng khß╗Åi scene/build. `data.br` vß║½n 12,04 MB, l├á n├║t thß║»t Release.
2. **╞»u ti├¬n cao, trung b├¼nh:** kiß╗âm managed code stripping v├á c├íc package/assembly ─æ╞░ß╗úc k├⌐o v├áo WebGL; `wasm.br` 4,39 MB. ─Éß╗æi chiß║┐u chß╗⌐c n─âng tr╞░ß╗¢c/sau bß║▒ng PlayMode v├á iPhone.
3. **╞»u ti├¬n vß╗½a, trung b├¼nh:** chia asset chß╗ë d├╣ng ß╗ƒ m├án kh├íc v├á tß║úi sau khi Home hiß╗çn, nß║┐u kiß║┐n tr├║c UI cho ph├⌐p. ─Éß╗½ng tr├¼ ho├ún asset bß║»t buß╗Öc ─æß╗â bß║Ñm Home.
4. **╞»u ti├¬n vß╗½a, thß║Ñp:** ─æß║úm bß║úo hosting gß╗¡i `Content-Encoding: br`, `application/wasm` ─æ├║ng MIME v├á cache tß╗çp c├│ hash. Server ─æo ─æ├ú l├ám vß║¡y; cß║ºn x├íc minh host thß╗▒c tß║┐.
5. **╞»u ti├¬n thß║Ñp tr╞░ß╗¢c khi ─æo s├óu:** ─æo th├¬m bß║▒ng browser profiling tr├¬n iPhone/thiß║┐t bß╗ï thß║¡t ─æß╗â t├ích tß║úi mß║íng khß╗Åi khß╗ƒi tß║ío; kh├┤ng dß╗▒a v├áo khoß║úng 2,64 s ─æß╗â kß║┐t luß║¡n ri├¬ng vß╗ü giß║úi n├⌐n.
