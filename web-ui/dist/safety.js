/* Web-only shell. Never writes exception messages, URLs or credentials to diagnostics. */
const records = [];
const record = kind => { records.push({kind, at: Date.now()}); if (records.length > 50) records.shift(); };
window.__hitmeDiagnostics = {records};

const css = document.createElement('style');
css.textContent = `
  .hitme-browser-tip{position:fixed;z-index:110;left:8px;right:8px;bottom:max(12px,env(safe-area-inset-bottom));max-width:430px;margin:auto;padding:12px 42px 12px 12px;background:#13273df5;border:2px solid #f6ce58;border-radius:14px;color:#fff;font:700 13px/1.4 sans-serif;box-shadow:0 6px 18px #0009}
  .hitme-browser-tip button{margin-top:8px;padding:8px 12px;background:#ffcf43;color:#1c2731;border:0;border-radius:8px;font-weight:800}
  .hitme-browser-tip .hitme-tip-close{position:absolute;right:6px;top:2px;background:transparent;color:#fff;font-size:24px}
  .hitme-interruption{position:fixed;z-index:300;inset:0;display:grid;place-items:center;padding:20px;background:#09121cdd}
  .hitme-interruption>div{max-width:360px;width:100%;padding:22px;background:#13283b;border:2px solid #f6ce58;border-radius:18px;color:#fff;text-align:center;font:700 17px/1.4 sans-serif}
  .hitme-interruption button{min-height:48px;margin:8px 4px 0;padding:8px 15px;border-radius:10px;border:1px solid #f6ce58;background:#ffcf43;color:#192434;font-weight:800}
  .hitme-interruption button:last-child{background:#233c50;color:#fff}
`;
document.head.append(css);

function inAppBrowser() {
  if (matchMedia('(display-mode: standalone)').matches || navigator.standalone) return false;
  return /zalo|FBAN|FBAV|FB_IAB|Messenger|Instagram|\bLine\b|TikTok/i.test(navigator.userAgent);
}
function showBrowserTip() {
  document.querySelector('.hitme-browser-tip')?.remove();
  if (!inAppBrowser()) return;
  const tip = document.createElement('aside');
  tip.className = 'hitme-browser-tip';
  tip.innerHTML = '<span>Bạn đang mở trong trình duyệt của ứng dụng. Để cài vào Màn hình chính và chơi toàn màn hình, hãy mở link bằng Safari (ba chấm → Mở bằng trình duyệt).</span><br><button type="button" data-copy>Sao chép đường dẫn</button><button type="button" class="hitme-tip-close" aria-label="Đóng">×</button>';
  tip.querySelector('[data-copy]').onclick = async () => {
    try { await navigator.clipboard.writeText(location.href); tip.querySelector('[data-copy]').textContent = 'Đã sao chép'; }
    catch { tip.querySelector('[data-copy]').textContent = 'Không thể sao chép'; }
  };
  tip.querySelector('.hitme-tip-close').onclick = () => tip.remove();
  document.body.append(tip);
}
if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', showBrowserTip, {once:true});
else showBrowserTip();
window.addEventListener('appinstalled', showBrowserTip);

window.__hitmeFatal = () => {
  record('unity_fatal');
  if (document.querySelector('.hitme-interruption')) return;
  const panel = document.createElement('div');
  panel.className = 'hitme-interruption';
  panel.setAttribute('role', 'alertdialog');
  panel.innerHTML = '<div>Game bị gián đoạn. Bấm để tải lại.<br><button type="button" data-reload>Tải lại</button><button type="button" data-later>Chơi lại sau</button></div>';
  panel.querySelector('[data-reload]').onclick = () => location.reload();
  panel.querySelector('[data-later]').onclick = () => panel.remove();
  document.body.append(panel);
};
window.addEventListener('error', event => {
  if (event.message === 'Script error.' && !event.filename && !event.lineno && !event.error) {
    record('opaque_cross_origin');
    event.stopImmediatePropagation();
    event.preventDefault();
    return;
  }
  record('script_error');
}, true);
window.addEventListener('hitme-bridge-error', () => record('bridge_error'));
document.addEventListener('webglcontextlost', event => {
  event.preventDefault();
  window.__hitmeFatal();
}, true);
if (window.__hitmePendingFatal) window.__hitmeFatal();
