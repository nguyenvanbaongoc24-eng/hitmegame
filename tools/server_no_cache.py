import sys
import os
from http.server import HTTPServer, SimpleHTTPRequestHandler

class NoCacheHandler(SimpleHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/favicon.ico' and not os.path.exists('favicon.ico'):
            self.send_response(204)
            self.end_headers()
            return
        super().do_GET()

    def end_headers(self):
        # Set Content-Encoding: br for Brotli pre-compressed WebGL release builds
        if self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
            if '.wasm' in self.path:
                self.send_header('Content-Type', 'application/wasm')
            elif '.data' in self.path:
                self.send_header('Content-Type', 'application/octet-stream')
            elif '.js' in self.path:
                self.send_header('Content-Type', 'application/javascript')
        # Set anti-cache headers for all responses to prevent mobile browser caching
        self.send_header('Cache-Control', 'no-store, no-cache, must-revalidate, max-age=0')
        self.send_header('Pragma', 'no-cache')
        self.send_header('Expires', '0')
        self.send_header('Access-Control-Allow-Origin', '*')
        super().end_headers()

if __name__ == '__main__':
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8093
    directory = sys.argv[2] if len(sys.argv) > 2 else '.'
    os.chdir(directory)
    print(f"Serving {directory} on 0.0.0.0:{port} with Cache-Control: no-store...")
    server = HTTPServer(('0.0.0.0', port), NoCacheHandler)
    server.serve_forever()
