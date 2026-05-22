#!/usr/bin/env python3
"""Bundle a PLICY build of the game WITHOUT the online (CO-OP) feature.

The live GitHub Pages build keeps online multiplayer. For PLICY we ship the
exact same single-player game but with the online entry point removed:
  - the home "🌐 CO-OP" button is deleted (only UI route into the lobby), and
  - the relay URL is neutralised so even a ?relay=/?role= query can't connect.
Everything else (gameplay, shop, story, daily, etc.) is byte-identical.

Output: dist/hand-danmaku-plicy.zip  (index.html + og.png + bgm.mp3)
"""

import os, sys, zipfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "dist", "hand-danmaku-plicy.zip")
os.makedirs(os.path.dirname(OUT), exist_ok=True)

with open(os.path.join(ROOT, "index.html"), encoding="utf-8") as f:
    html = f.read()


def must_replace(text, old, new, label):
    if old not in text:
        sys.exit(f"ERROR: anchor not found ({label}). index.html changed — "
                 f"update scripts/make_dist_plicy.py.")
    return text.replace(old, new, 1)


# 1) Remove the only UI route into online co-op: the home "🌐 CO-OP" button.
html = must_replace(
    html,
    '<button type="button" id="coopBtn">🌐 CO-OP</button>',
    '<!-- online co-op disabled for PLICY build -->',
    "coop button",
)

# 2) Neutralise the relay URL so ?relay=/?role= URLs can't open a connection
#    (PLICY embeds with no query string, but this makes online unreachable
#    even if someone crafts a URL).
html = must_replace(
    html,
    "const RELAY_URL = new URLSearchParams(location.search).get('relay')",
    "const RELAY_URL = ''; const _RELAY_DISABLED = new URLSearchParams(location.search).get('relay')",
    "relay url",
)

# 3) Free build = ACT I demo only (stops at stage 10, teases ACT II). The paid
#    "product" build keeps PREMIUM=true and unlocks ACT II (stages 11-20).
html = must_replace(
    html,
    "const PREMIUM = true;",
    "const PREMIUM = false;  // free PLICY demo: ACT I only (ACT II = paid build)",
    "premium flag",
)

assert 'id="coopBtn"' not in html, "coopBtn still present after strip"

FILES = [
    ("og.png", "og.png"),
    ("bgm.mp3", "bgm.mp3"),
]

with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    z.writestr("index.html", html)
    for src, arc in FILES:
        z.write(os.path.join(ROOT, src), arcname=arc)

print(f"wrote {OUT}  ({os.path.getsize(OUT)} bytes)")
