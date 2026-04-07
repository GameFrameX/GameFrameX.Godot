@echo off
setlocal
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..\..
set TOOLS_ROOT=%SCRIPT_DIR%..\..\..\GameFrameX.Tools\Config\Tools
set CONF=%SCRIPT_DIR%luban.conf
if "%SERVER_PATH%"=="" (
  echo SERVER_PATH is not set. Please set SERVER_PATH env var to your server root.
  exit /b 1
)
dotnet "%TOOLS_ROOT%\Luban.dll" --conf "%CONF%" --target server --dataTarget json --codeTarget cs-dotnet-json --xargs outputDataDir="%SERVER_PATH%\\Server.Config\\Json" --xargs outputCodeDir="%SERVER_PATH%\\Server.Config\\Config"
endlocal
