param(
    [string]$GodotExe = "D:\Program Files\Godot_v4.6.1-stable_mono_win64\Godot_v4.6.1-stable_mono_win64.exe",
    [string]$ProjectPath = "E:\Project_godot\gfx\GameFrameX.Godot",
    [string]$RemoteDebug = "localhost:6007"
)

if (-not (Test-Path -LiteralPath $GodotExe))
{
    Write-Error "Godot executable not found: $GodotExe"
    exit 1
}

if (-not (Test-Path -LiteralPath $ProjectPath))
{
    Write-Error "Project path not found: $ProjectPath"
    exit 1
}

Write-Host "Launching with remote debugger: $RemoteDebug"
Write-Host "Godot: $GodotExe"
Write-Host "Project: $ProjectPath"

& $GodotExe --path $ProjectPath --remote-debug $RemoteDebug
