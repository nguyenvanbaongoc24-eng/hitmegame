"""Serve a local web-dist tree with the Brotli headers used by Vercel.

Run: python tools/blackbox/serve_web_dist.py --root "D:/HITME GAME/worktrees/web-dist-mobile" --port 8093
This is for local black-box verification only. It does not modify the build.
"""
from __future__ import annotations

import argparse
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path


class Handler(SimpleHTTPRequestHandler):
    def end_headers(self) -> None:
        path = self.path.split('?', 1)[0]
        if path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
            if path.endswith('.wasm.br'):
                self.send_header('Content-Type', 'application/wasm')
            elif path.endswith('.js.br'):
                self.send_header('Content-Type', 'application/javascript')
            else:
                self.send_header('Content-Type', 'application/octet-stream')
        super().end_headers()

    def log_message(self, *_args: object) -> None:
        pass


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, required=True)
    parser.add_argument('--port', type=int, default=8093)
    args = parser.parse_args()
    with ThreadingHTTPServer(('127.0.0.1', args.port), partial(Handler, directory=str(args.root))) as server:
        print(f'Serving {args.root} at http://127.0.0.1:{args.port}/', flush=True)
        server.serve_forever()


if __name__ == '__main__':
    main()
