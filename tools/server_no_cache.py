import sys
import os
from http.server import ThreadingHTTPServer as HTTPServer, SimpleHTTPRequestHandler

class NoCacheHandler(SimpleHTTPRequestHandler):
    def guess_type(self, path):
        clean_path = path.split('?')[0]
        if '.wasm' in clean_path:
            return 'application/wasm'
        if '.js' in clean_path:
            return 'application/javascript'
        if '.data' in clean_path:
            return 'application/octet-stream'
        return super().guess_type(path)

    def do_GET(self):
        if self.path == '/favicon.ico' and not os.path.exists('favicon.ico'):
            self.send_response(204)
            self.end_headers()
            return
        super().do_GET()

    def end_headers(self):
        # Set Content-Encoding: br for Brotli pre-compressed WebGL release builds
        clean_path = self.path.split('?')[0]
        if clean_path.endswith('.br') or '.br?' in self.path:
            self.send_header('Content-Encoding', 'br')
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
