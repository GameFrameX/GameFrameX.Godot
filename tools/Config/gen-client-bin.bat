@echo off
setlocal
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..\..
set TOOLS_ROOT=%SCRIPT_DIR%Tools
set CONF=%SCRIPT_DIR%luban.conf
set DEFINES_DIR=%SCRIPT_DIR%Defines
set EXCELS_DIR=%SCRIPT_DIR%Excels

if not exist "%TOOLS_ROOT%\Luban.dll" (
  echo [ConfigGen] Missing local tool: %TOOLS_ROOT%\Luban.dll
  exit /b 1
)
if not exist "%CONF%" (
  echo [ConfigGen] Missing local config: %CONF%
  exit /b 1
)
if not exist "%DEFINES_DIR%" (
  echo [ConfigGen] Missing local schema dir: %DEFINES_DIR%
  exit /b 1
)
if not exist "%EXCELS_DIR%" (
  echo [ConfigGen] Missing local data dir: %EXCELS_DIR%
  exit /b 1
)

pushd "%SCRIPT_DIR%"
dotnet "%TOOLS_ROOT%\Luban.dll" --conf "%CONF%" --target client --dataTarget bin --codeTarget cs-bin --validationFailAsError true --xargs outputDataDir="%PROJECT_ROOT%\Assets\Bundles\Config" --xargs outputCodeDir="%PROJECT_ROOT%\Assets\Hotfix\Config\Generate"
set EXIT_CODE=%ERRORLEVEL%
popd
exit /b %EXIT_CODE%
endlocal

