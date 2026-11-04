// Contract: MD/HITME GAME MD/hit-me-strict-reset.md section 4.
// Unity emits strings or objects. Keep the web layer inert until it receives ready.
const listeners = new Set();
export const onUnity = callback => { listeners.add(callback); return () => listeners.delete(callback); };
export function send(method, payload = {}) {
  const allowed = new Set(['StartPractice','QuickPlay','CreateRoom','JoinRoom','LeaveRoom','Ready','SetCharacter','SetWeapon','SetSettings','Resume','ExitMatch','SendEmote']);
  if (!allowed.has(method)) throw new Error(`Unknown bridge method: ${method}`);
  if (!window.unityInstance?.SendMessage) return false;
  window.unityInstance.SendMessage('WebBridge', method, JSON.stringify(payload));
  return true;
}
window.hitmeUi = {
  onUnity(event) {
    let value;
    try { value = typeof event === 'string' ? JSON.parse(event) : event; }
    catch { return; }
    if (!value || typeof value.type !== 'string') return;
    listeners.forEach(listener => listener(value));
  }
};
