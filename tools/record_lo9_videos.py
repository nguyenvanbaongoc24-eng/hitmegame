"""Record WebGL gameplay videos for Lo 9 (N=2 and N=4) using Playwright."""
import os, time, shutil
from playwright.sync_api import sync_playwright

video_tmp_dir = r"D:\HITME GAME\unity-client\Evidence\round35\video_tmp"
out_dir = r"D:\HITME GAME\unity-client\Evidence\round35"
os.makedirs(video_tmp_dir, exist_ok=True)
os.makedirs(out_dir, exist_ok=True)

with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)

    # 1. Record N=2 match (1 player + 1 bot)
    print("Recording N=2 match on port 8100...")
    context_n2 = browser.new_context(
        viewport={"width": 414, "height": 896},
        record_video_dir=video_tmp_dir,
        record_video_size={"width": 414, "height": 896}
    )
    page_n2 = context_n2.new_page()
    page_n2.goto("http://127.0.0.1:8100/?practiceBots=1&autoPlayMatches=1", wait_until="networkidle", timeout=60000)
    time.sleep(3) # Wait for Unity engine to boot

    # Wait for gameplay loop (Placing -> Lock -> Reveal -> Throw -> Resolve)
    print("Playing N=2 round...")
    time.sleep(12)

    # Capture in-game screenshot
    page_n2.screenshot(path=os.path.join(out_dir, "lo9_01_gameplay_n2_live_1170x2532.png"))

    page_n2.close()
    context_n2.close()

    # Move video
    files = [f for f in os.listdir(video_tmp_dir) if f.endswith(".webm")]
    if files:
        src = os.path.join(video_tmp_dir, files[0])
        dst = os.path.join(out_dir, "match_n2_gameplay.webm")
        shutil.move(src, dst)
        print(f"Saved N=2 video: {dst} ({os.path.getsize(dst):,} bytes)")

    # 2. Record N=4 match (1 player + 3 bots)
    print("Recording N=4 match on port 8100...")
    context_n4 = browser.new_context(
        viewport={"width": 414, "height": 896},
        record_video_dir=video_tmp_dir,
        record_video_size={"width": 414, "height": 896}
    )
    page_n4 = context_n4.new_page()
    page_n4.goto("http://127.0.0.1:8100/?practiceBots=3&autoPlayMatches=1", wait_until="networkidle", timeout=60000)
    time.sleep(3)
    print("Playing N=4 round...")
    time.sleep(12)

    # Capture in-game screenshot
    page_n4.screenshot(path=os.path.join(out_dir, "lo9_02_gameplay_n4_live_1170x2532.png"))

    page_n4.close()
    context_n4.close()

    files = [f for f in os.listdir(video_tmp_dir) if f.endswith(".webm")]
    if files:
        src = os.path.join(video_tmp_dir, files[0])
        dst = os.path.join(out_dir, "match_n4_gameplay.webm")
        shutil.move(src, dst)
        print(f"Saved N=4 video: {dst} ({os.path.getsize(dst):,} bytes)")

    browser.close()

# Clean up tmp dir
if os.path.exists(video_tmp_dir):
    shutil.rmtree(video_tmp_dir, ignore_errors=True)

print("Lo 9 videos and screenshots recorded successfully!")
