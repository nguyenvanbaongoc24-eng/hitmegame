"""Scan tracked/staged Git blobs for credential shapes without printing values.

Run: python tools/supabase/scan_secrets.py
     python tools/supabase/scan_secrets.py --staged
The history scan visits each reachable blob on prep/codex once. Findings print
only the Git path, short blob/commit IDs and pattern class; never the match.
"""

from __future__ import annotations

import argparse
import pathlib
import re
import subprocess
import sys


PATTERNS = {
    "database-url-with-password": re.compile(rb"postgres(?:ql)?://[^\s\"']+:[^@\s\"']+@", re.I),
    "supabase-secret-key": re.compile(rb"sb_" + rb"secret_[A-Za-z0-9_-]{12,}"),
    "jwt-shaped-key": re.compile(rb"eyJ[A-Za-z0-9_-]{18,}\.eyJ[A-Za-z0-9_-]{18,}\.[A-Za-z0-9_-]{16,}"),
}
IGNORED_SUFFIXES = (".png", ".jpg", ".jpeg", ".webp", ".wav", ".mp3", ".ogg", ".pdf", ".wasm", ".data")


def git(*args: str) -> bytes:
    return subprocess.run(["git", *args], stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                          check=True).stdout


def scan(staged: bool) -> int:
    if staged:
        rows = git("ls-files", "--stage", "-z").split(b"\0")
        entries = []
        for row in rows:
            if not row:
                continue
            metadata, path = row.split(b"\t", 1)
            entries.append((metadata.split()[1].decode(), path.decode("utf-8", "replace")))
    else:
        rows = git("rev-list", "--objects", "prep/codex").splitlines()
        entries = []
        for row in rows:
            parts = row.split(b" ", 1)
            if len(parts) == 2:
                entries.append((parts[0].decode(), parts[1].decode("utf-8", "replace")))

    checked = set()
    findings = []
    batch = subprocess.Popen(["git", "cat-file", "--batch"], stdin=subprocess.PIPE,
                             stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
    assert batch.stdin is not None and batch.stdout is not None
    for object_id, path in entries:
        if staged and (pathlib.PurePosixPath(path).name == ".env" or pathlib.PurePosixPath(path).name.startswith(".env.")):
            findings.append((path, object_id[:12], "staged", "env-file"))
        if object_id in checked or path.lower().endswith(IGNORED_SUFFIXES):
            continue
        checked.add(object_id)
        batch.stdin.write((object_id + "\n").encode())
        batch.stdin.flush()
        header = batch.stdout.readline().split()
        if len(header) < 3:
            raise RuntimeError("Git cat-file did not return an object header")
        kind, size = header[1], int(header[2])
        if kind != b"blob":
            batch.stdout.read(size + 1)
            continue
        if size > 2_000_000:
            remaining = size + 1
            while remaining:
                remaining -= len(batch.stdout.read(min(65536, remaining)))
            continue
        data = batch.stdout.read(size)
        batch.stdout.read(1)
        if b"\0" in data[:4096]:
            continue
        for label, pattern in PATTERNS.items():
            if pattern.search(data):
                commit = "staged" if staged else git("log", "prep/codex", "--find-object=" + object_id,
                    "--format=%h", "-1").decode().strip() or "unknown"
                findings.append((path, object_id[:12], commit, label))

    batch.stdin.close()
    batch.stdout.close()
    batch.wait()
    for path, blob, commit, label in sorted(findings):
        print(f"{label}: path={path} blob={blob} commit={commit}")
    print(f"Secret scan: blobs={len(checked)} findings={len(findings)} mode={'staged' if staged else 'history'}")
    return 1 if findings else 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--staged", action="store_true")
    sys.exit(scan(parser.parse_args().staged))
