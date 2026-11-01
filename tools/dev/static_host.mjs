// One local HTTP origin for the hub, Online Lab, and a read-only Unity WebGL build.
import { createServer } from 'node:http';
import { connect as tcpConnect } from 'node:net';
import { readFile, stat } from 'node:fs/promises';
import { resolve, sep, extname, basename } from 'node:path';

const port = Number(process.env.HITME_HUB_PORT || 8100);
const serverPort = Number(process.env.HITME_SERVER_PORT || 8080);
const buildRoot = resolve(process.env.HITME_BUILD_DIR || '');
const localRoot = resolve(import.meta.dirname);
const buildHtml = await readFile(resolve(buildRoot, 'index.html'), 'utf8');
const buildCommit = /build\s+([0-9a-f]{7,40})/i.exec(buildHtml)?.[1] || 'UNKNOWN';
const serverCommit = process.env.HITME_SERVER_COMMIT || 'UNKNOWN';
const mime = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8', '.json': 'application/json; charset=utf-8',
  '.wasm': 'application/wasm', '.data': 'application/octet-stream', '.png': 'image/png',
  '.jpg': 'image/jpeg', '.svg': 'image/svg+xml' };

const server = createServer(async (request, response) => {
  try {
    const url = new URL(request.url, 'http://localhost');
    if (url.pathname === '/config.json') return json(response, { buildCommit, buildName: basename(buildRoot), serverCommit, protocol: 1, serverPort });
    if (url.pathname === '/api/status' || url.pathname === '/api/healthz') {
      const target = url.pathname.endsWith('status') ? 'status' : 'healthz';
      const upstream = await fetch(`http://127.0.0.1:${serverPort}/${target}`, { signal: AbortSignal.timeout(2500) });
      response.writeHead(upstream.status, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' });
      return response.end(await upstream.text());
    }
    const game = url.pathname === '/game' || url.pathname.startsWith('/game/');
    const lab = url.pathname === '/lab' || url.pathname.startsWith('/lab/');
    const root = game ? buildRoot : lab ? resolve(localRoot, 'lab') : resolve(localRoot, 'hub');
    const relative = game ? url.pathname.replace(/^\/game\/?/, '') : lab ? url.pathname.replace(/^\/lab\/?/, '') : url.pathname.slice(1);
    const name = relative || 'index.html';
    const file = resolve(root, name);
    if (file !== root && !file.startsWith(root + sep)) { response.writeHead(403); return response.end(); }
    const details = await stat(file);
    if (!details.isFile()) { response.writeHead(404); return response.end(); }
    const encoding = file.endsWith('.br') ? 'br' : file.endsWith('.gz') ? 'gzip' : null;
    const contentExt = extname(encoding ? file.slice(0, -(encoding === 'br' ? 3 : 3)) : file).toLowerCase();
    const headers = { 'Content-Type': mime[contentExt] || 'application/octet-stream',
      'Cache-Control': contentExt === '.html' || file.endsWith('.loader.js') ? 'no-store' : 'public, max-age=3600' };
    if (encoding) headers['Content-Encoding'] = encoding;
    response.writeHead(200, headers);
    response.end(await readFile(file));
  } catch (error) {
    response.writeHead(error.code === 'ENOENT' ? 404 : 502, { 'Content-Type': 'text/plain; charset=utf-8' });
    response.end(error.code === 'ENOENT' ? 'Không tìm thấy tệp.' : 'Máy chủ cục bộ chưa sẵn sàng.');
  }
});
server.listen(port, '0.0.0.0', () => process.stdout.write(`HIT ME hub http://localhost:${port}/ build ${buildCommit}\n`));
server.on('upgrade', (request, socket, head) => {
  if (request.url !== '/ws') { socket.destroy(); return; }
  const upstream = tcpConnect(serverPort, '127.0.0.1');
  upstream.on('connect', () => {
    const headers = Object.entries(request.headers).map(([key,value]) => `${key}: ${value}`).join('\r\n');
    upstream.write(`${request.method} ${request.url} HTTP/1.1\r\n${headers}\r\n\r\n`);
    if (head.length) upstream.write(head);
    socket.pipe(upstream).pipe(socket);
  });
  upstream.on('error', () => socket.destroy());
  socket.on('error', () => upstream.destroy());
  socket.on('close', () => upstream.destroy());
});
function json(response, value) {
  response.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8', 'Cache-Control': 'no-store' });
  response.end(JSON.stringify(value));
}
