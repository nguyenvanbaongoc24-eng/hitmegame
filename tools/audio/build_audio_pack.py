"""Build the optional HIT ME Web MP3 pack from read-only project WAV placeholders.

README: python tools/audio/build_audio_pack.py
Use --source and --output to override paths. This command never writes to SFX.
It requires ffmpeg on PATH or the optional imageio_ffmpeg Python package.
"""
from __future__ import annotations

import argparse
import json
import shutil
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
PROJECT = REPO.parent.parent if REPO.parent.name == "worktrees" else REPO
DEFAULT_SOURCE = PROJECT / "SFX" / "HitMe_Audio_Placeholders" / "audio"
DEFAULT_OUTPUT = REPO / "web-ui" / "assets" / "audio"

# One source can serve multiple UI events; unmatched events use the WebAudio synth.
TRACKS = {
    "ui_tap": "sfx_ui_click.wav",
    "ui_error": "sfx_error.wav",
    "place_pop": "sfx_tap_place.wav",
    "ready_lock": "sfx_ready.wav",
    "timer_tick": "sfx_countdown_tick.wav",
    "timer_end": "sfx_countdown_last.wav",
    "reveal": "sfx_reveal.wav",
    "throw_dep": "sfx_throw.wav",
    "hit_dep": "sfx_hit_bop.wav",
    "ko": "sfx_eliminate.wav",
    "win": "sfx_win.wav",
    "lose": "sfx_lose.wav",
    "chat_pop": "sfx_chat_pop.wav",
    "bgm_menu": "bgm_menu_loop.wav",
    "bgm_match": "bgm_match_loop.wav",
}
HOME = {"ui_tap", "ui_error", "bgm_menu"}


def ffmpeg_executable() -> str:
    binary = shutil.which("ffmpeg")
    if binary:
        return binary
    try:
        import imageio_ffmpeg
        return imageio_ffmpeg.get_ffmpeg_exe()
    except (ImportError, RuntimeError) as error:
        raise RuntimeError("FFmpeg missing: install ffmpeg or pip install imageio-ffmpeg") from error


def build(source: Path, output: Path) -> dict:
    binary = ffmpeg_executable()
    output.mkdir(parents=True, exist_ok=True)
    tracks = {}
    for event_id, filename in TRACKS.items():
        original = source / filename
        if not original.is_file():
            raise FileNotFoundError(original)
        target = output / f"{event_id}.mp3"
        music = event_id.startswith("bgm_")
        filters = ["loudnorm=I=-18:TP=-3:LRA=11"]
        if not music:
            filters.insert(0, "silenceremove=start_periods=1:start_threshold=-55dB")
        command = [binary, "-hide_banner", "-loglevel", "error", "-y", "-i", str(original),
                   "-vn", "-af", ",".join(filters), "-ac", "2" if music else "1",
                   "-b:a", "112k" if music else "80k"]
        if not music:
            command.extend(["-t", "1.18"])
        command.append(str(target))
        subprocess.run(command, check=True, timeout=60)
        tracks[event_id] = {"file": target.name, "bytes": target.stat().st_size,
                            "preload": event_id in HOME, "source": filename,
                            "kind": "music" if music else "sfx"}
    home_bytes = sum(item["bytes"] for item in tracks.values() if item["preload"])
    if home_bytes > 1_500_000:
        raise ValueError(f"Home audio pack {home_bytes} bytes exceeds 1.5 MB")
    manifest = {"v": 1, "homeBytes": home_bytes, "tracks": tracks}
    (output / "manifest.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
                                          encoding="utf-8")
    return manifest


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()
    result = build(args.source, args.output)
    print(f"Built {len(result['tracks'])} tracks; Home preload {result['homeBytes']} bytes / 1,500,000")
