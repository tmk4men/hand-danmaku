#!/usr/bin/env python3
"""Bundle a BOOTH (download-to-own) build of the game WITHOUT online co-op.

BOOTH sells downloadable files — buyers run the game locally, not in an embedded
iframe. Two things that don't matter for the hosted (Pages/PLICY) build matter here:

  1. ASSETS must be bundled. index.html loads bgm.mp3 + 画像/背景1..10.webp at
     runtime; a hosted build gets them from the server, a download must ship them.
  2. CAMERA needs a secure context. Browsers block getUserMedia on file:// , so a
     buyer who just double-clicks index.html gets no camera. We ship a tiny local
     web-server launcher (Windows: PowerShell, no install; Mac/Linux: python3) so
     the game runs on http://localhost (a secure context → camera works).

This build keeps the full game (PREMIUM=true → ACT I + ACT II) but removes the
online CO-OP entry point. Output: dist/hand-danmaku-booth.zip
"""

import os, sys, zipfile

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "dist", "hand-danmaku-booth.zip")
os.makedirs(os.path.dirname(OUT), exist_ok=True)

with open(os.path.join(ROOT, "index.html"), encoding="utf-8") as f:
    html = f.read()


def must_replace(text, old, new, label):
    if old not in text:
        sys.exit(f"ERROR: anchor not found ({label}). index.html changed — "
                 f"update scripts/make_dist_booth.py.")
    return text.replace(old, new, 1)


# Remove the only UI route into online co-op + neutralise the relay URL.
# (PREMIUM is left = true on purpose: the BOOTH download is the full paid product.)
html = must_replace(
    html,
    '<button type="button" id="coopBtn">🌐 CO-OP</button>',
    '<!-- online co-op disabled for BOOTH build -->',
    "coop button",
)
html = must_replace(
    html,
    "const RELAY_URL = new URLSearchParams(location.search).get('relay')",
    "const RELAY_URL = ''; const _RELAY_DISABLED = new URLSearchParams(location.search).get('relay')",
    "relay url",
)
# The CO-OP button is gone, so null-guard its click listener (otherwise
# getElementById('coopBtn').addEventListener throws and halts init).
html = must_replace(
    html,
    "document.getElementById('coopBtn').addEventListener('click', () => { showCoopView(); });",
    "document.getElementById('coopBtn')?.addEventListener('click', () => { showCoopView(); });",
    "coop listener",
)
assert 'id="coopBtn"' not in html, "coopBtn still present after strip"

# ---- Launcher: a no-install local server so the camera works (see module docstring) ----
SERVER_PS1 = r"""# Minimal static web server (no install needed) so the camera works on http://localhost.
$port = 8777
$root = (Get-Location).Path
$mime = @{ '.html'='text/html; charset=utf-8'; '.js'='text/javascript'; '.css'='text/css';
          '.webp'='image/webp'; '.png'='image/png'; '.jpg'='image/jpeg'; '.jpeg'='image/jpeg';
          '.mp3'='audio/mpeg'; '.json'='application/json'; '.svg'='image/svg+xml' }
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$port/")
try { $listener.Start() } catch {
  Write-Host ""
  Write-Host "起動できませんでした（ポート $port が使用中かもしれません）。"
  Write-Host "開いている別のウィンドウを閉じてから、もう一度お試しください。"
  Read-Host "Enterで終了"; exit 1
}
Write-Host "HAND DANMAKU を起動中... ブラウザが開きます。"
Write-Host "遊び終わったら、この黒い画面を閉じてください。"
Start-Process "http://localhost:$port/index.html"
while ($listener.IsListening) {
  try {
    $ctx = $listener.GetContext()
    $rel = [System.Uri]::UnescapeDataString($ctx.Request.Url.AbsolutePath.TrimStart('/'))
    if ($rel -eq '') { $rel = 'index.html' }
    $path = Join-Path $root $rel
    if (Test-Path $path -PathType Leaf) {
      $bytes = [System.IO.File]::ReadAllBytes($path)
      $ext = [System.IO.Path]::GetExtension($path).ToLower()
      if ($mime.ContainsKey($ext)) { $ctx.Response.ContentType = $mime[$ext] }
      $ctx.Response.OutputStream.Write($bytes, 0, $bytes.Length)
    } else { $ctx.Response.StatusCode = 404 }
    $ctx.Response.Close()
  } catch { }
}
"""

LAUNCH_BAT = (
    "@echo off\r\n"
    "chcp 65001 >nul\r\n"
    "cd /d \"%~dp0\"\r\n"
    "powershell -NoProfile -ExecutionPolicy Bypass -File \"%~dp0_server.ps1\"\r\n"
    "if errorlevel 1 pause\r\n"
)

LAUNCH_COMMAND = (
    "#!/bin/bash\n"
    "cd \"$(dirname \"$0\")\"\n"
    "echo 'HAND DANMAKU を起動中... ブラウザが開きます。終わったらこのウィンドウを閉じてください。'\n"
    "( sleep 1; (open 'http://localhost:8777/index.html' 2>/dev/null || xdg-open 'http://localhost:8777/index.html' 2>/dev/null) ) &\n"
    "python3 -m http.server 8777 2>/dev/null || python -m http.server 8777\n"
)

README = """HAND DANMAKU （ダウンロード版）
================================

手のジェスチャだけで操作する弾幕シューティングです。Webカメラが必要です。

──────────────────────────────
■ 遊び方
──────────────────────────────
● Windows
   「起動_Windows.bat」をダブルクリック → ブラウザが自動で開きます
   → カメラの使用を「許可」 → ▶ PLAY NOW

● Mac / Linux
   「起動_Mac.command」をダブルクリック（初回は右クリック→「開く」）
   ※ Python3 が必要です（macOSは「xcode-select --install」で入ります）

──────────────────────────────
■ なぜランチャー（黒い画面）が必要？
──────────────────────────────
ブラウザはセキュリティ上、カメラを「http://localhost」か「https://」でしか
使えません。index.html を直接ダブルクリックすると（file:// になり）カメラが
動きません。付属ランチャーが一時的にローカルサーバーを立てて解決します。
遊び終わったら黒い画面（コマンド窓）を閉じればサーバーも止まります。

──────────────────────────────
■ 動作環境・注意
──────────────────────────────
・最新の Google Chrome / Microsoft Edge を推奨
・Webカメラ必須。明るい場所で手を映すと認識が安定します
・初回起動時はネット接続が必要です（手の認識エンジンを読み込みます。以降は軽くなります）
・Windowsで「WindowsによってPCが保護されました」と出たら「詳細情報」→「実行」
・うまく開かない時は、表示されたURL（http://localhost:8777/index.html）を
  ブラウザに手で貼り付けてください

──────────────────────────────
■ 操作（すべて手のジェスチャー）
──────────────────────────────
・移動 ：人差し指で機体を動かす
・メイン射撃 ：パー（手を開く）
・サブ射撃 ：ピース✌
・ボム ：ピンチ（親指と人差し指でつまむ）
・ガード ：グー（握る）
・アイテム使用 ：親指を内側に折る
・ドラゴンブレイク ：アイテム取得後にサムズアップ👍

──────────────────────────────
■ 収録内容
──────────────────────────────
・ACT I（ステージ1〜10）＋ ACT II「深層」（ステージ11〜20／新ボス・新敵・新ギミック）
・ステージ10クリアで解放される新機体「OBLIVION」
・武器/機体/サブ武器のロードアウト、ショップ、デイリーチャレンジ ほか
※ オンライン協力(CO-OP)は本ダウンロード版では無効です。

オンライン無料版（デモ）: https://tmk4men.github.io/hand-danmaku/

たのしんでください！
"""

# Root-level assets the game loads at runtime + the OGP card.
ROOT_FILES = ["index.html(generated)", "bgm.mp3", "og.png"]

with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    z.writestr("index.html", html)
    z.write(os.path.join(ROOT, "bgm.mp3"), "bgm.mp3")
    z.write(os.path.join(ROOT, "og.png"), "og.png")
    # Per-theme background tiles (画像/背景1..10.webp). Optional (procedural fallback
    # exists) but bundled so the paid build looks complete offline.
    for i in range(1, 11):
        rel = f"画像/背景{i}.webp"
        src = os.path.join(ROOT, rel)
        if os.path.exists(src):
            z.write(src, rel)
        else:
            print(f"WARN: missing {rel} (skipped)")
    # Launchers + readme
    z.writestr("_server.ps1", SERVER_PS1)
    z.writestr("起動_Windows.bat", LAUNCH_BAT)
    # store the .command with an exec bit so Mac can run it after a chmod/right-click
    info = zipfile.ZipInfo("起動_Mac.command")
    info.external_attr = (0o755 & 0xFFFF) << 16
    info.compress_type = zipfile.ZIP_DEFLATED
    z.writestr(info, LAUNCH_COMMAND)
    z.writestr("はじめに.txt", README)

print(f"wrote {OUT}  ({os.path.getsize(OUT)} bytes)")
