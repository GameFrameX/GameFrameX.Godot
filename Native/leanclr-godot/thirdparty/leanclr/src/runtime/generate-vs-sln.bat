@echo off
setlocal

:: Build directory
set BUILD_DIR=%~dp0build
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"

pushd "%BUILD_DIR%"

:: Choose your generator/arch as needed
cmake -G "Visual Studio 17 2022" -A x64 ..
if errorlevel 1 (
    echo CMake generation failed.
    popd
    exit /b 1
)

echo VS solution generated in %BUILD_DIR%.
popd

endlocal
