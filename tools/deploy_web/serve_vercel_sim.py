#!/usr/bin/env python3
"""
serve_vercel_sim.py — Local HTTP server accurately simulating Vercel's headers for Unity WebGL Brotli.
Serves Tools/deploy_web/dist/ on specified port (default 8104).
"""

import sys
import os
import mimetypes
from pathlib import Path
from http.server import HTTPServer, SimpleHTTPRequestHandler

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8104
DIST_DIR = Path(__file__).resolve().parent / "dist"

if not DIST_DIR.exists():
    # If not packaged yet, serve directly from parent with fallback
    DIST_DIR = Path(__file__).resolve().parent

class VercelSimHandler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(DIST_DIR), **kwargs)

    def end_headers(self):
        # Security headers matching vercel.json
        self.send_header("X-Content-Type-Options", "nosniff")
        self.send_header("X-Frame-Options", "SAMEORIGIN")
        self.send_header("Access-Control-Allow-Origin", "*")
        super().end_headers()

    def send_head(self):
        path = self.translate_path(self.path)
        if os.path.isdir(path):
            parts = self.path.split('?', 1)[0].split('#', 1)[0]
            if not parts.endswith('/'):
                self.send_response(301)
                self.send_header("Location", parts + "/")
                self.end_headers()
                return None
            for index in "index.html", "index.htm":
                index_path = os.path.join(path, index)
                if os.path.exists(index_path):
                    path = index_path
                    break

        try:
            f = open(path, 'rb')
        except OSError:
            self.send_error(404, "File not found")
            return None

        fs = os.fstat(f.fileno())
        self.send_response(200)

        # Match Vercel Header Rules
        req_path = self.path.split('?')[0]

        if req_path.endswith(".br"):
            self.send_header("Content-Encoding", "br")

        if req_path.endswith(".wasm") or req_path.endswith(".wasm.br"):
            self.send_header("Content-Type", "application/wasm")
        elif req_path.endswith(".data") or req_path.endswith(".data.br"):
            self.send_header("Content-Type", "application/octet-stream")
        elif req_path.endswith(".js") or req_path.endswith(".js.br"):
            self.send_header("Content-Type", "application/javascript; charset=utf-8")
        elif req_path.endswith(".json"):
            self.send_header("Content-Type", "application/json; charset=utf-8")
        elif req_path.endswith(".html") or req_path == "/" or req_path == "":
            self.send_header("Content-Type", "text/html; charset=utf-8")
            self.send_header("Cache-Control", "no-cache, no-store, must-revalidate")
        else:
            ctype = self.guess_type(path)
            self.send_header("Content-Type", ctype)

        if "/Build/" in req_path:
            self.send_header("Cache-Control", "public, max-age=31536000, immutable")

        self.send_header("Content-Length", str(fs[6]))
        self.send_header("Last-Modified", self.date_time_string(fs.st_mtime))
        self.end_headers()
        return f

def run():
    print(f"[*] Starting Vercel simulation server at http://127.0.0.1:{PORT}")
    print(f"[*] Serving directory: {DIST_DIR}")
    server = HTTPServer(("0.0.0.0", PORT), VercelSimHandler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n[*] Server stopped.")

if __name__ == "__main__":
    run()
