#!/usr/bin/env python3
"""Bundle the game into hand-danmaku.zip for PLICY / itch.io upload."""

import os, zipfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "dist", "hand-danmaku.zip")
os.makedirs(os.path.dirname(OUT), exist_ok=True)

FILES = [
    ("index.html", "index.html"),
    ("og.png",     "og.png"),
    ("bgm.mp3",    "bgm.mp3"),
]

with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    for src, arc in FILES:
        z.write(os.path.join(ROOT, src), arcname=arc)

print(f"wrote {OUT}  ({os.path.getsize(OUT)} bytes)")
