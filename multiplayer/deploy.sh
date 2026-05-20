#!/usr/bin/env bash
# Hand Danmaku relay — deploy the Cloudflare Durable Objects WebSocket relay.
#
# wrangler login (OAuth) does NOT work on this WSL setup: it opens a
# localhost:8976 callback the browser can't reach. So we authenticate with an
# API token instead (no localhost needed).
#
# 1) Create the token (one time):
#      Cloudflare dashboard → My Profile → API Tokens → Create Token
#      → use the "Edit Cloudflare Workers" template → Continue → Create Token
#      → copy the token string.
#
# 2) Save it locally (it is git-ignored, never committed):
#      echo 'CLOUDFLARE_API_TOKEN=PASTE_TOKEN_HERE' > multiplayer/.cf-token
#
# 3) Deploy:
#      bash multiplayer/deploy.sh
set -euo pipefail
cd "$(dirname "$0")"

# Load the token from the git-ignored file if present.
if [ -f .cf-token ]; then
  set -a; . ./.cf-token; set +a
fi

if [ -z "${CLOUDFLARE_API_TOKEN:-}" ]; then
  echo "✗ CLOUDFLARE_API_TOKEN is not set."
  echo "  Create a token (Edit Cloudflare Workers template), then run:"
  echo "    echo 'CLOUDFLARE_API_TOKEN=YOUR_TOKEN' > multiplayer/.cf-token"
  echo "  and re-run: bash multiplayer/deploy.sh"
  exit 1
fi

echo "→ Deploying danmaku-relay to Cloudflare Workers …"
wrangler deploy
echo ""
echo "✓ Done. Your relay WebSocket URL is:  wss://danmaku-relay.<your-subdomain>.workers.dev"
echo "  Copy the https URL wrangler printed above, swap https→wss, and set RELAY_URL in index.html."
echo "  Quick test without editing index.html:  open the game with  ?relay=wss://danmaku-relay.<sub>.workers.dev"
