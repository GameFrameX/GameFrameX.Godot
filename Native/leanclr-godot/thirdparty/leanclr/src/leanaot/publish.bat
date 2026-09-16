@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..\..") do set "REPO_ROOT=%%~fI"
set "OUTPUT_DIR=%REPO_ROOT%\src\tools\leanaot"

echo Publishing LeanAOT (Release)...
echo Output: %OUTPUT_DIR%

dotnet publish "%SCRIPT_DIR%\LeanAOT\LeanAOT.csproj" -c Release -o "%OUTPUT_DIR%"
if errorlevel 1 (
	echo Publish failed.
	exit /b 1
)

echo Publish succeeded.
endlocal
