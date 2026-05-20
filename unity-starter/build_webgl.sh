#!/usr/bin/env bash
# ===========================================================================
#  HAND DANMAKU - one-shot WebGL build for unityroom (macOS / Linux)
#  Usage: ./build_webgl.sh [path/to/Unity]
#  Output: Builds/Web/
# ===========================================================================
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")" && pwd)"
VER="$(awk -F': ' '/^m_EditorVersion:/{print $2}' "$PROJECT/ProjectSettings/ProjectVersion.txt" | tr -d '\r')"

UNITY="${1:-}"
if [ -z "$UNITY" ]; then
  case "$(uname -s)" in
    Darwin) UNITY="/Applications/Unity/Hub/Editor/$VER/Unity.app/Contents/MacOS/Unity" ;;
    *)      UNITY="$HOME/Unity/Hub/Editor/$VER/Editor/Unity" ;;
  esac
fi

if [ ! -x "$UNITY" ]; then
  echo "[ERROR] Unity not found at: $UNITY"
  echo "Install Unity $VER + WebGL Build Support, or pass the path as arg 1."
  exit 1
fi

echo "Building WebGL with $UNITY ..."
"$UNITY" -quit -batchmode -projectPath "$PROJECT" \
  -buildTarget WebGL -executeMethod BuildScript.BuildWebGL -logFile -

echo
echo "[OK] Build done       -> $PROJECT/Builds/Web"
echo "[OK] Upload-ready zip -> $PROJECT/Builds/HandDanmaku_unityroom.zip"
echo "Next: upload that zip to unityroom (no manual zipping needed)."
