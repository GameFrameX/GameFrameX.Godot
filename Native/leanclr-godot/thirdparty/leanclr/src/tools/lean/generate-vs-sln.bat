@echo off
setlocal

rem Directory and build folder
set "SCRIPT_DIR=%~dp0"
set "BUILD_DIR=%SCRIPT_DIR%build"

rem Arg: ARCH (x64/x86), default x64
set "ARCH=%~1"
if "%ARCH%"=="" set "ARCH=x64"
set "GENERATOR=Visual Studio 17 2022"

echo === Generate VS solution (%GENERATOR%, %ARCH%) ===
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"
if errorlevel 1 goto :error

rem Use %SCRIPT_DIR%. to avoid trailing backslash issue in quoted path
cmake -S "%SCRIPT_DIR%." -B "%BUILD_DIR%" -G "%GENERATOR%" -A %ARCH%
if errorlevel 1 goto :error

rem Try to report the generated .sln name
set "SLN="
for %%F in ("%BUILD_DIR%\*.sln") do set "SLN=%%~fF"
if defined SLN (
  echo Solution generated: "%SLN%"
) else (
  echo Warning: .sln not found under "%BUILD_DIR%" (generator may place it differently)
)

echo Done.
endlocal
exit /b 0

:error
echo Failed to generate solution. Error code %ERRORLEVEL%.
endlocal & exit /b %ERRORLEVEL%
