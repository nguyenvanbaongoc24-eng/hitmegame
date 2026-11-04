// Web audio for the Hybrid UI. All sounds are synthesized until asset licenses are verified.
const SFX_VOLUME_KEY = 'hitme.sfxVolume';
const MUSIC_VOLUME_KEY = 'hitme.musicVolume';
const clamp = value => Math.max(0, Math.min(1, Number(value) || 0));
const query = new URLSearchParams(location.search);
const log = query.get('e2e') === '1' ? (window.__hitmeAudioLog = []) : null;

const notes = {
  ui_tap:[660,0.055], ui_confirm:[880,0.13], ui_back:[420,0.09], ui_denied:[180,0.16],
  ui_toggle:[740,0.09], ui_slider_tick:[800,0.045], ui_tab:[620,0.07],
  ui_sheet_open:[540,0.13], ui_sheet_close:[420,0.12], ui_toast:[780,0.1],
  ui_error:[210,0.18], ui_select_char:[760,0.12], ui_select_weapon:[650,0.12],
  ui_copy:[920,0.09], room_join:[760,0.17], room_leave:[390,0.13],
  round_start:[660,0.2], place_pop:[430,0.11], aim_tick:[920,0.035],
  ready_lock:[720,0.15], timer_tick:[560,0.09], timer_end:[260,0.2],
  reveal:[870,0.22], throw_dep:[300,0.13], throw_chao:[280,0.15], throw_vot:[340,0.11],
  hit_dep:[170,0.12], hit_chao:[145,0.17], hit_vot:[200,0.11],
  ko:[190,0.3], win:[940,0.38], lose:[260,0.38], round_result:[710,0.17],
  emo_pop:[790,0.09], chat_pop:[680,0.08], jingle_win:[940,0.38], jingle_lose:[260,0.38]
};

class HitMeAudio {
  constructor() {
    this.context = null;
    this.sfxBus = null;
    this.musicBus = null;
    this.music = null;
    this.activeSfx = 0;
    this.lastPlayed = new Map();
    this.sfxVolume = this.readVolume(SFX_VOLUME_KEY, 0.8);
    this.musicVolume = this.readVolume(MUSIC_VOLUME_KEY, 0.6);
    this.silentAudio = null;
  }

  readVolume(key, fallback) {
    try { const raw = localStorage.getItem(key); return raw === null ? fallback : clamp(raw); }
    catch { return fallback; }
  }

  unlock() {
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) return false;
    if (!this.context) {
      this.context = new AudioContextClass();
      this.sfxBus = this.context.createGain();
      this.musicBus = this.context.createGain();
      this.sfxBus.gain.value = this.sfxVolume;
      this.musicBus.gain.value = this.musicVolume;
      this.sfxBus.connect(this.context.destination);
      this.musicBus.connect(this.context.destination);
    }
    try {
      if (navigator.audioSession) navigator.audioSession.type = 'playback';
      else this.ensureSilentAudio();
    } catch { this.ensureSilentAudio(); }
    void this.context.resume();
    return true;
  }

  ensureSilentAudio() {
    if (this.silentAudio) { void this.silentAudio.play().catch(() => {}); return; }
    const bytes = new Uint8Array(46);
    const view = new DataView(bytes.buffer);
    const write = (offset, value) => [...value].forEach((c,i) => { bytes[offset+i] = c.charCodeAt(0); });
    write(0,'RIFF'); view.setUint32(4,38,true); write(8,'WAVEfmt ');
    view.setUint32(16,16,true); view.setUint16(20,1,true); view.setUint16(22,1,true);
    view.setUint32(24,8000,true); view.setUint32(28,16000,true);
    view.setUint16(32,2,true); view.setUint16(34,16,true);
    write(36,'data'); view.setUint32(40,2,true);
    this.silentAudio = new Audio(URL.createObjectURL(new Blob([bytes], {type:'audio/wav'})));
    this.silentAudio.loop = true;
    this.silentAudio.volume = 0;
    void this.silentAudio.play().catch(() => {});
  }

  setVolumes({music = this.musicVolume, sfx = this.sfxVolume} = {}) {
    this.musicVolume = clamp(music);
    this.sfxVolume = clamp(sfx);
    try { localStorage.setItem(MUSIC_VOLUME_KEY, String(this.musicVolume));
      localStorage.setItem(SFX_VOLUME_KEY, String(this.sfxVolume)); } catch { /* storage unavailable */ }
    if (this.musicBus) this.musicBus.gain.setTargetAtTime(this.musicVolume, this.context.currentTime, 0.02);
    if (this.sfxBus) this.sfxBus.gain.setTargetAtTime(this.sfxVolume, this.context.currentTime, 0.02);
  }

  playSfx(id, volume = 1, pan = 0) {
    if (!Object.hasOwn(notes,id) || !this.unlock() || this.activeSfx >= 8) return false;
    const nowMs = performance.now();
    if (nowMs - (this.lastPlayed.get(id) ?? -Infinity) < 50) return false;
    this.lastPlayed.set(id, nowMs);
    const [baseFrequency,duration] = notes[id];
    const ctx = this.context;
    const oscillator = ctx.createOscillator();
    const envelope = ctx.createGain();
    const panner = typeof ctx.createStereoPanner === 'function' ? ctx.createStereoPanner() : null;
    oscillator.type = id.includes('denied') || id.includes('error') || id.startsWith('hit_') ? 'triangle' : 'sine';
    oscillator.frequency.value = baseFrequency * (0.95 + Math.random() * 0.1);
    const start = ctx.currentTime;
    envelope.gain.setValueAtTime(0.0001,start);
    envelope.gain.exponentialRampToValueAtTime(Math.max(0.0001,clamp(volume) * 0.23),start + 0.008);
    envelope.gain.exponentialRampToValueAtTime(0.0001,start + duration);
    oscillator.connect(envelope);
    if (panner) { panner.pan.value = Math.max(-1,Math.min(1,Number(pan)||0)); envelope.connect(panner); panner.connect(this.sfxBus); }
    else envelope.connect(this.sfxBus);
    this.activeSfx++;
    oscillator.onended = () => { this.activeSfx--; oscillator.disconnect(); envelope.disconnect(); panner?.disconnect(); };
    oscillator.start(start);
    oscillator.stop(start + duration + 0.01);
    if (log) log.push({t:nowMs,id});
    if (['win','lose','ko'].includes(id)) this.duckMusic();
    return true;
  }

  duckMusic() {
    if (!this.musicBus || !this.context) return;
    const gain = this.musicBus.gain;
    const now = this.context.currentTime;
    gain.cancelScheduledValues(now);
    gain.setValueAtTime(gain.value,now);
    gain.linearRampToValueAtTime(this.musicVolume * 0.398,now + 0.04);
    gain.linearRampToValueAtTime(this.musicVolume,now + 0.44);
  }

  playMusic(id, action = 'play') {
    if (action === 'stop') { this.stopMusic(); return; }
    if (!['bgm_menu','bgm_match'].includes(id) || !this.unlock() || this.music?.id === id) return;
    const ctx = this.context;
    const old = this.music;
    const gain = ctx.createGain();
    gain.gain.setValueAtTime(0.0001,ctx.currentTime);
    gain.gain.linearRampToValueAtTime(0.15,ctx.currentTime + 0.6);
    gain.connect(this.musicBus);
    const roots = id === 'bgm_menu' ? [220,277.18,329.63] : [196,246.94,293.66];
    const voices = roots.map(frequency => {
      const oscillator = ctx.createOscillator(); oscillator.type = 'sine';
      oscillator.frequency.value = frequency;
      oscillator.connect(gain); oscillator.start(); return oscillator;
    });
    this.music = {id,gain,voices};
    if (old) this.fadeOut(old);
    if (log) log.push({t:performance.now(),id});
  }

  fadeOut(track) {
    const now = this.context.currentTime;
    track.gain.gain.cancelScheduledValues(now);
    track.gain.gain.setValueAtTime(track.gain.gain.value,now);
    track.gain.gain.linearRampToValueAtTime(0.0001,now + 0.6);
    track.voices.forEach(voice => voice.stop(now + 0.61));
  }

  stopMusic() { if (this.music) this.fadeOut(this.music); this.music = null; }
}

export const audio = new HitMeAudio();
document.addEventListener('pointerdown', () => audio.unlock(), {capture:true});
