@echo off
chcp 65001 > nul
cd /d C:\Users\taki3\source\sts2-mod-uploader\tool
ModUploader.exe upload -w "C:\Users\taki3\source\sts2-mod-uploader\SpireScaler"
echo.
echo ---- finished (exit code %ERRORLEVEL%) ----
pause
