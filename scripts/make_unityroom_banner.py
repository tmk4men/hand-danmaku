#!/usr/bin/env python3
"""Generate a 960x540 PNG banner sized for unityroom thumbnail."""

from PIL import Image, ImageDraw, ImageFont
import os, random, math

W, H = 960, 540
OUT = os.path.join(os.path.dirname(__file__), "..", "unityroom_banner.png")

SKY = ["#06081a", "#0c1140", "#1a2a78"]
ACCENT = "#88a8ff"
PLANET = "#3b2078"
PLANET_HI = "#5a3aa8"
SHIP_PAL = {
    '#': "#3563d6", 'B': "#1a2a78", 'C': "#9ad8ff",
    'W': "#ffffff", 'E': "#ffd066",
}
SHIP = [
    '.....##.....',
    '....####....',
    '....#WW#....',
    '...##WW##...',
    '..##BWWB##..',
    '.###BCCB###.',
    '###BCWWCB###',
    '#B#BCWWCB#B#',
    '#.BBCCCCBB.#',
    '...##EE##...',
    '....#EE#....',
    '.....EE.....',
]

def load_font(sz):
    for path in (
        "/usr/share/fonts/truetype/dejavu/DejaVuSansMono-Bold.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/mnt/c/Windows/Fonts/consolab.ttf",
    ):
        if os.path.exists(path):
            return ImageFont.truetype(path, sz)
    return ImageFont.load_default()

img = Image.new("RGB", (W, H), SKY[0])
d = ImageDraw.Draw(img)

# Sky bands
bh = H // len(SKY)
for i, c in enumerate(SKY):
    d.rectangle([0, i * bh, W, (i + 1) * bh], fill=c)

# Stars
random.seed(3)
for _ in range(200):
    x = random.randint(0, W); y = random.randint(0, H)
    s = random.random()
    if s > 0.85: size, col = 3, "#ffffff"
    elif s > 0.5: size, col = 2, "#cad0ff"
    else: size, col = 1, "#6a72a8"
    d.rectangle([x, y, x + size, y + size], fill=col)

# Planet
cx, cy, R = W * 0.82, H * 0.30, 90
for dy in range(-R, R + 1, 4):
    w = int((R * R - dy * dy) ** 0.5)
    d.rectangle([cx - w, cy + dy, cx + w, cy + dy + 4], fill=PLANET)
    if dy < 6:
        d.rectangle([cx - w, cy + dy, cx - w + int(w * 0.9), cy + dy + 4], fill=PLANET_HI)

# Bullet fan
ship_cx, ship_cy = int(W * 0.30), int(H * 0.68)
random.seed(7)
for i in range(70):
    a = math.radians(-90 + (i - 35) * 4)
    r = 70 + (i % 6) * 18
    bx = ship_cx + math.cos(a) * r
    by = ship_cy - 30 + math.sin(a) * r
    if 0 < bx < W and 0 < by < H:
        size = 5 if i % 5 == 0 else 4
        col = "#ff77c8" if i % 3 == 0 else ("#9bf2ff" if i % 3 == 1 else "#ffd27a")
        d.rectangle([bx - size, by - size, bx + size, by + size], fill=col)
        d.rectangle([bx - size/2, by - size/2, bx + size/2, by + size/2], fill="#ffffff")

# Ship sprite
cell = 10
sw = len(SHIP[0]) * cell; sh = len(SHIP) * cell
sx = ship_cx - sw // 2
sy = ship_cy - sh // 2
for ry, row in enumerate(SHIP):
    for rx, ch in enumerate(row):
        if ch == '.': continue
        d.rectangle([sx + rx * cell, sy + ry * cell, sx + (rx + 1) * cell, sy + (ry + 1) * cell],
                    fill=SHIP_PAL.get(ch, "#ffffff"))

# Title text
ftitle = load_font(70)
fsub = load_font(28)
ftag = load_font(20)

title = "HAND DANMAKU"
bbox = d.textbbox((0,0), title, font=ftitle)
tw, th = bbox[2], bbox[3]
d.text(((W - tw) // 2 + 2, 50 + 2), title, font=ftitle, fill="#000000")
d.text(((W - tw) // 2, 50), title, font=ftitle, fill="#ffffff")

sub = "手で操る弾幕シューティング"
bbox = d.textbbox((0,0), sub, font=fsub)
sw_, sh_ = bbox[2], bbox[3]
d.text(((W - sw_) // 2 + 1, 138 + 1), sub, font=fsub, fill="#000000")
d.text(((W - sw_) // 2, 138), sub, font=fsub, fill="#88e0ff")

tag = "MOVE  BOMB  GUARD  FOCUS  DASH  DRAGON"
bbox = d.textbbox((0,0), tag, font=ftag)
tw_, th_ = bbox[2], bbox[3]
d.text(((W - tw_) // 2 + 1, 188 + 1), tag, font=ftag, fill="#000000")
d.text(((W - tw_) // 2, 188), tag, font=ftag, fill="#ffd066")

img.save(OUT)
print(f"wrote {OUT}  {W}x{H}  {os.path.getsize(OUT)} bytes")
