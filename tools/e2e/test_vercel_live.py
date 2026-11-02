from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(
        executable_path=r"C:\Program Files\Google\Chrome\Application\chrome.exe",
        headless=True,
        args=["--use-gl=angle", "--use-angle=swiftshader", "--no-sandbox"]
    )
    page = browser.new_page(viewport={"width": 390, "height": 844})
    print("[*] Navigating to https://hitmegame-iota.vercel.app/...")
    page.goto("https://hitmegame-iota.vercel.app/", wait_until="domcontentloaded")
    page.wait_for_timeout(5000)
    modal_style = page.evaluate("() => document.getElementById('webgl-error-modal') ? document.getElementById('webgl-error-modal').style.display : 'none'")
    print("[+] Modal display style:", modal_style)
    page.screenshot(path=r"D:\HITME GAME\unity-client\Evidence\round44\vercel_live_home.png")
    print("[+] Screenshot saved!")
    browser.close()
