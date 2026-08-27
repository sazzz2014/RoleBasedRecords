@echo off
setlocal
pushd "%~dp0"

docker compose down
set "RBR_EXIT_CODE=%ERRORLEVEL%"

if not "%RBR_EXIT_CODE%"=="0" (
    echo.
    echo RoleBasedRecords could not be stopped. See the error above.
)

popd
pause
exit /b %RBR_EXIT_CODE%
