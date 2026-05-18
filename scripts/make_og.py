#!/usr/bin/env python3
"""Generate a 1200x630 PNG Open Graph preview for hand-danmaku."""

from PIL import Image, ImageDraw, ImageFont
import os, sys, random

W, H = 1200, 630
OUT = os.path.join(os.path.dirname(__file__), "..", "og.png")

# --- Palette (matches NEBULA theme in the game) ---
SKY_BANDS = ["#06081a", "#0b0e2e", "#181b46"]
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

img = Image.new("RGB", (W, H), "#06081a")
d = ImageDraw.Draw(img)

# Sky bands
bh = H // len(SKY_BANDS)
for i, c in enumerate(SKY_BANDS):
    d.rectangle([0, i * bh, W, (i + 1) * bh], fill=c)

# Stars (3 tiers)
random.seed(7)
for _ in range(200):
    x = random.randint(0, W); y = random.randint(0, H)
    s = random.random()
    if s > 0.85: size = 3; color = "#ffffff"
    elif s > 0.5: size = 2; color = "#cad0ff"
    else: size = 1; color = "#6a72a8"
    d.rectangle([x, y, x + size, y + size], fill=color)

# Planet (pixel disc)
cx, cy, R = W * 0.78, H * 0.32, 110
for dy in range(-R, R + 1, 4):
    w = int((R * R - dy * dy) ** 0.5)
    d.rectangle([cx - w, cy + dy, cx + w, cy + dy + 4], fill=PLANET)
    if dy < 10:
        d.rectangle([cx - w, cy + dy, cx - w + int(w * 0.9), cy + dy + 4], fill=PLANET_HI)

# Bullet pattern fanning from the player (suggesting bullet hell)
ship_cx = int(W * 0.34)
ship_cy = int(H * 0.65)
import math
random.seed(13)
for i in range(80):
    a = math.radians(-90 + (i - 40) * 4)
    r = 80 + (i % 8) * 18
    bx = ship_cx + math.cos(a) * r
    by = ship_cy - 40 + math.sin(a) * r
    if 0 < bx < W and 0 < by < H:
        size = 6 if i % 5 == 0 else 4
        col = "#ff77c8" if i % 3 == 0 else ("#9bf2ff" if i % 3 == 1 else "#ffd27a")
        d.rectangle([bx - size, by - size, bx + size, by + size], fill=col)
        d.rectangle([bx - size/2, by - size/2, bx + size/2, by + size/2], fill="#ffffff")

# Ship sprite (chunky)
cell = 12
sw = len(SHIP[0]) * cell; sh = len(SHIP) * cell
sx = ship_cx - sw // 2
sy = ship_cy - sh // 2
for ry, row in enumerate(SHIP):
    for rx, ch in enumerate(row):
        if ch in '.':
            continue
        d.rectangle([sx + rx * cell, sy + ry * cell, sx + (rx + 1) * cell, sy + (ry + 1) * cell],
                    fill=SHIP_PAL.get(ch, "#ffffff"))

# Title text (pixel-style monospace) — fallback chain for fonts
def load_font(sz):
    for path in (
        "/usr/share/fonts/truetype/dejavu/DejaVuSansMono-Bold.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/mnt/c/Windows/Fonts/consolab.ttf",
        "/mnt/c/Windows/Fonts/arial.ttf",
    ):
        if os.path.exists(path):
            return ImageFont.truetype(path, sz)
    return ImageFont.load_default()

ftitle = load_font(96)
fsub = load_font(40)
ftag = load_font(28)

# Title
title = "HAND DANMAKU"
tw, th = d.textbbox((0,0), title, font=ftitle)[2:]
d.text(((W - tw) // 2 - 1, 70 + 1), title, font=ftitle, fill="#000000")
d.text(((W - tw) // 2, 70), title, font=ftitle, fill="#ffffff")

sub = "弾幕シューティング × 手トラッキング"
sw_, sh_ = d.textbbox((0,0), sub, font=fsub)[2:]
d.text(((W - sw_) // 2 + 1, 180 + 1), sub, font=fsub, fill="#000000")
d.text(((W - sw_) // 2, 180), sub, font=fsub, fill="#88e0ff")

tag = "MOVE / BOMB / GUARD / FOCUS / DASH / USE"
tw_, th_ = d.textbbox((0,0), tag, font=ftag)[2:]
d.text(((W - tw_) // 2 + 1, 250 + 1), tag, font=ftag, fill="#000000")
d.text(((W - tw_) // 2, 250), tag, font=ftag, fill="#ffd066")

# Footer URL
url = "tmk4men.github.io/hand-danmaku"
fu = load_font(24)
uw_, uh_ = d.textbbox((0,0), url, font=fu)[2:]
d.text((W - uw_ - 24, H - uh_ - 18), url, font=fu, fill="#cfd2f0")

img.save(OUT)
print(f"wrote {OUT}  {W}x{H}")
