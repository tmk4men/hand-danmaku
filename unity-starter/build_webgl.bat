@echo off
REM ===========================================================================
REM  HAND DANMAKU - one-click WebGL build for unityroom (Windows)
REM  Double-click this file, or run it from a terminal. Output: Builds\Web\
REM  Pass a custom editor path as the first argument if auto-detect fails.
REM ===========================================================================
setlocal

set "PROJECT=%~dp0"

REM --- read editor version from ProjectVersion.txt ----------------------------
set "VER="
for /f "tokens=2 delims= " %%v in ('findstr /b "m_EditorVersion:" "%PROJECT%ProjectSettings\ProjectVersion.txt"') do set "VER=%%v"

REM --- locate Unity.exe -------------------------------------------------------
set "UNITY=%~1"
if "%UNITY%"=="" set "UNITY=C:\Program Files\Unity\Hub\Editor\%VER%\Editor\Unity.exe"

if not exist "%UNITY%" (
  echo [ERROR] Unity.exe not found at:
  echo         %UNITY%
  echo.
  echo Install Unity %VER% + WebGL Build Support via Unity Hub, or pass the path:
  echo         build_webgl.bat "C:\Program Files\Unity\Hub\Editor\&lt;ver&gt;\Editor\Unity.exe"
  exit /b 1
)

echo Building WebGL with %UNITY% ...
"%UNITY%" -quit -batchmode -projectPath "%PROJECT:~0,-1%" -buildTarget WebGL -executeMethod BuildScript.BuildWebGL -logFile -
set "RC=%ERRORLEVEL%"

if "%RC%"=="0" (
  echo.
  echo [OK] Build done -^> %PROJECT%Builds\Web
  echo Next: zip the contents of Builds\Web and upload to unityroom.
) else (
  echo.
  echo [FAILED] exit code %RC% - scroll up for the Unity error.
)
exit /b %RC%
