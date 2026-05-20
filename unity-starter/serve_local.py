#!/usr/bin/env python3
"""
Local test server for the WebGL build (handles Unity's gzip-compressed files).

The build uses Gzip compression with Decompression Fallback OFF, so the server
MUST send `Content-Encoding: gzip` for the .gz files — the stock
`python -m http.server` does not, and the Unity loader fails. This one does.

Usage:
    python serve_local.py            # serves ./Builds/Web on http://localhost:8080
    python serve_local.py 9000       # custom port
Then open http://localhost:8080 and allow the camera.
"""
import http.server
import os
import socketserver
import sys

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8080
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Builds", "Web")


class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=ROOT, **kwargs)

    def end_headers(self):
        if self.path.split("?")[0].endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        super().end_headers()

    def guess_type(self, path):
        base = path[:-3] if path.endswith(".gz") else path
        if base.endswith(".wasm"):
            return "application/wasm"
        if base.endswith(".js"):
            return "application/javascript"
        if base.endswith(".json"):
            return "application/json"
        if base.endswith(".data"):
            return "application/octet-stream"
        return super().guess_type(path)


if not os.path.isdir(ROOT):
    sys.exit(f"[serve_local] build not found: {ROOT}\nRun the WebGL build first.")

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print(f"[serve_local] http://localhost:{PORT}  (serving {ROOT})")
    print("[serve_local] open it, allow the camera, move your index finger. Ctrl+C to stop.")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\n[serve_local] stopped.")
