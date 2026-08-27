@echo off
setlocal
pushd "%~dp0"

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start-local.ps1"
set "RBR_EXIT_CODE=%ERRORLEVEL%"

if not "%RBR_EXIT_CODE%"=="0" (
    echo.
    echo RoleBasedRecords failed to start. See the error above.
)

popd
pause
exit /b %RBR_EXIT_CODE%
