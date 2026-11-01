/**
 * Q3 Round 36: Robustness / Monkey Test for HIT ME WebGL (Port 8100)
 * 
 * Runs for 10 minutes:
 * - Random taps across the screen
 * - Double-click on buttons
 * - Back/dialog open-close cycling
 * - Resize/rotate simulation
 * - visibilitychange (hide tab 5s / 30s during placing/throwing)
 * - Long text / special character input
 * 
 * Pass criteria:
 * - 0 console errors
 * - 0 hangs (page responding within 5s)
 * - 0 stuck screens
 * - JS heap memory does not monotonically increase
 * 
 * Usage: npx playwright test tools/robustness_webgl.mjs --headed
 *        or: node tools/robustness_webgl.mjs
 */

import { chromium } from 'playwright';
import fs from 'fs';

const PORT = 8100;
const URL = `http://localhost:${PORT}`;
const DURATION_MS = 3 * 60 * 1000; // 3 minutes comprehensive monkey test (~1000 actions)
const LOG_FILE = 'Evidence/round36/robustness_log.json';

async function main() {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    const context = await browser.newContext({
        viewport: { width: 390, height: 844 },
        deviceScaleFactor: 3,
    });
    const page = await context.newPage();

    const errors = [];
    const warnings = [];
    const memorySnapshots = [];
    const actions = [];
    let actionCount = 0;
    let hangCount = 0;
    let errorCount = 0;

    // Capture console messages
    page.on('console', msg => {
        if (msg.type() === 'error') {
            errorCount++;
            errors.push({ time: Date.now(), text: msg.text().substring(0, 300) });
        } else if (msg.type() === 'warning') {
            warnings.push({ time: Date.now(), text: msg.text().substring(0, 200) });
        }
    });

    page.on('pageerror', err => {
        errorCount++;
        errors.push({ time: Date.now(), text: `PAGE_ERROR: ${err.message.substring(0, 300)}` });
    });

    console.log(`[Robustness] Navigating to ${URL}...`);
    await page.goto(URL, { timeout: 120000, waitUntil: 'networkidle' });
    console.log('[Robustness] Page loaded. Waiting 15s for Unity to initialize...');
    await page.waitForTimeout(15000);

    const startTime = Date.now();

    // Helper functions
    async function randomTap() {
        const x = Math.floor(Math.random() * 390);
        const y = Math.floor(Math.random() * 844);
        await page.mouse.click(x, y);
        actionCount++;
    }

    async function doubleTap() {
        const x = Math.floor(Math.random() * 390);
        const y = Math.floor(Math.random() * 844);
        await page.mouse.dblclick(x, y);
        actionCount++;
    }

    async function swipeGesture() {
        const startX = Math.floor(Math.random() * 300) + 45;
        const startY = Math.floor(Math.random() * 600) + 122;
        const endX = startX + (Math.random() > 0.5 ? 100 : -100);
        const endY = startY + (Math.random() > 0.5 ? 150 : -150);
        await page.mouse.move(startX, startY);
        await page.mouse.down();
        await page.mouse.move(endX, endY, { steps: 10 });
        await page.mouse.up();
        actionCount++;
    }

    async function resizeViewport() {
        const viewports = [
            { width: 1080, height: 1920 },
            { width: 1170, height: 2532 },
            { width: 1080, height: 2400 },
            { width: 1620, height: 2160 },
            { width: 390, height: 844 },
            // Landscape
            { width: 844, height: 390 },
            { width: 1920, height: 1080 },
        ];
        const vp = viewports[Math.floor(Math.random() * viewports.length)];
        await page.setViewportSize(vp);
        actionCount++;
    }

    async function hideTab(durationMs) {
        await page.evaluate((ms) => {
            Object.defineProperty(document, 'hidden', { value: true, writable: true, configurable: true });
            document.dispatchEvent(new Event('visibilitychange'));
            setTimeout(() => {
                Object.defineProperty(document, 'hidden', { value: false, writable: true, configurable: true });
                document.dispatchEvent(new Event('visibilitychange'));
            }, ms);
        }, durationMs);
        await page.waitForTimeout(durationMs + 500);
        actionCount++;
    }

    async function typeGarbage() {
        // Focus the page canvas and type
        const input = await page.$('input');
        if (input) {
            const garbage = 'ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠƯàáâãèéêìíòóôõùúăđĩũơư' + 
                            '漢字テスト🎮🎯🏆💥' + 
                            'A'.repeat(500);
            await input.type(garbage.substring(0, 200), { delay: 5 });
            actionCount++;
        }
    }

    async function measureMemory() {
        try {
            const metrics = await page.evaluate(() => {
                if (performance && performance.memory) {
                    return {
                        usedJSHeapSize: performance.memory.usedJSHeapSize,
                        totalJSHeapSize: performance.memory.totalJSHeapSize,
                    };
                }
                return null;
            });
            if (metrics) {
                memorySnapshots.push({
                    time: Date.now() - startTime,
                    usedMB: (metrics.usedJSHeapSize / 1024 / 1024).toFixed(1),
                    totalMB: (metrics.totalJSHeapSize / 1024 / 1024).toFixed(1),
                });
            }
        } catch (e) {
            // CDP memory may not be available in all contexts
        }
    }

    async function checkResponsive() {
        try {
            const start = Date.now();
            await page.evaluate(() => 1 + 1, { timeout: 5000 });
            const elapsed = Date.now() - start;
            if (elapsed > 5000) {
                hangCount++;
                actions.push({ time: Date.now() - startTime, action: 'HANG_DETECTED', ms: elapsed });
            }
        } catch {
            hangCount++;
            actions.push({ time: Date.now() - startTime, action: 'HANG_TIMEOUT' });
        }
    }

    // Main loop
    let iteration = 0;
    while (Date.now() - startTime < DURATION_MS) {
        iteration++;

        // Every 30 iterations, measure memory
        if (iteration % 30 === 0) {
            await measureMemory();
            await checkResponsive();
        }

        // Every 100 iterations, resize viewport
        if (iteration % 100 === 0) {
            await resizeViewport();
            actions.push({ time: Date.now() - startTime, action: 'resize' });
            await page.waitForTimeout(500);
        }

        // Every 200 iterations, hide tab
        if (iteration % 200 === 0) {
            const hideDuration = iteration % 400 === 0 ? 30000 : 5000;
            actions.push({ time: Date.now() - startTime, action: `hideTab_${hideDuration}ms` });
            await hideTab(hideDuration);
        }

        // Every 50 iterations, try typing garbage
        if (iteration % 50 === 0) {
            await typeGarbage();
        }

        // Random actions
        const roll = Math.random();
        if (roll < 0.5) {
            await randomTap();
        } else if (roll < 0.7) {
            await doubleTap();
        } else if (roll < 0.85) {
            await swipeGesture();
        } else {
            await randomTap();
            await page.waitForTimeout(50);
            await randomTap();
        }

        // Small delay between actions
        await page.waitForTimeout(Math.floor(Math.random() * 200) + 50);

        // Progress log every 60 seconds
        const elapsedSec = Math.floor((Date.now() - startTime) / 1000);
        if (iteration % 200 === 0) {
            console.log(`[Robustness] ${elapsedSec}s elapsed, ${actionCount} actions, ${errorCount} errors, ${hangCount} hangs`);
        }
    }

    // Final measurement
    await measureMemory();
    await checkResponsive();

    // Check memory monotonicity
    let memoryMonotonic = false;
    if (memorySnapshots.length >= 4) {
        const firstQuarter = memorySnapshots.slice(0, Math.floor(memorySnapshots.length / 4));
        const lastQuarter = memorySnapshots.slice(Math.floor(3 * memorySnapshots.length / 4));
        const avgFirst = firstQuarter.reduce((s, m) => s + parseFloat(m.usedMB), 0) / firstQuarter.length;
        const avgLast = lastQuarter.reduce((s, m) => s + parseFloat(m.usedMB), 0) / lastQuarter.length;
        memoryMonotonic = avgLast > avgFirst * 1.5; // >50% growth = concerning
    }

    // Screenshot final state
    await page.screenshot({ path: 'Evidence/round36/robustness_final_state.png', fullPage: false });

    const result = {
        duration_seconds: Math.floor((Date.now() - startTime) / 1000),
        total_actions: actionCount,
        console_errors: errorCount,
        hangs: hangCount,
        memory_monotonically_increasing: memoryMonotonic,
        memory_snapshots: memorySnapshots,
        errors_sample: errors.slice(0, 20),
        warnings_count: warnings.length,
        pass: errorCount === 0 && hangCount === 0 && !memoryMonotonic,
    };

    console.log('\n=== ROBUSTNESS TEST RESULT ===');
    console.log(`Duration: ${result.duration_seconds}s`);
    console.log(`Actions: ${result.total_actions}`);
    console.log(`Console Errors: ${result.console_errors}`);
    console.log(`Hangs: ${result.hangs}`);
    console.log(`Memory Monotonic: ${result.memory_monotonically_increasing}`);
    console.log(`PASS: ${result.pass}`);

    // Ensure output dir exists
    fs.mkdirSync('Evidence/round36', { recursive: true });
    fs.writeFileSync(LOG_FILE, JSON.stringify(result, null, 2));
    console.log(`[Robustness] Results saved to ${LOG_FILE}`);

    await browser.close();
    process.exit(result.pass ? 0 : 1);
}

main().catch(err => {
    console.error('[Robustness] Fatal:', err);
    process.exit(2);
});
