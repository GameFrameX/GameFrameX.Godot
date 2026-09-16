#!/bin/bash
# build_leanclr.sh - 构建 LeanCLR GDExtension 与热更新程序集（macOS）
#
# 用法:
#   tools/build_leanclr.sh            # 全量构建 native + GodotSharpCompat + LeanCLRHotfix
#   tools/build_leanclr.sh --v2       # 额外产出 LeanCLRHotfixV2.dll 并写入热更 marker（验证热切换）
#   tools/build_leanclr.sh --native-only / --managed-only
#
# 依赖: cmake, Xcode CLT, dotnet SDK, python3（仅在 generated 绑定缺失时）
# 产物:
#   addons/com.gameframex.godot.leanclr/bin/Debug/libleanclr_godot.dylib
#   Assets/LeanCLR/Assemblies/*.dll（BCL + GodotSharpCompat + LeanCLRHotfix[+V2]）
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NATIVE="$REPO_ROOT/Native/leanclr-godot"
ASSEMBLIES="$REPO_ROOT/Assets/LeanCLR/Assemblies"
ADDON_BIN="$REPO_ROOT/addons/com.gameframex.godot.leanclr/bin"
BCL="$NATIVE/thirdparty/leanclr/src/libraries/dotnetframework4.x-linux"
BUILD_DIR="$NATIVE/build-master"
GODOT_CPP_TAG="godot-4.5-stable"

MODE="all"
V2=0
for arg in "$@"; do
  case "$arg" in
    --native-only) MODE="native" ;;
    --managed-only) MODE="managed" ;;
    --v2) V2=1 ;;
  esac
done

build_native() {
  if [ ! -f "$NATIVE/src/generated/godot_api.generated.cpp" ]; then
    echo "[leanclr] generating bindings from committed extension_api.json ..."
    (cd "$NATIVE" && python3 tools/binding_generator/generate_bindings.py --api extension_api.json)
  fi
  echo "[leanclr] configuring cmake (godot-cpp $GODOT_CPP_TAG) ..."
  cmake -S "$NATIVE" -B "$BUILD_DIR" -DCMAKE_BUILD_TYPE=Debug -DGODOT_CPP_BRANCH="$GODOT_CPP_TAG"
  echo "[leanclr] building libleanclr_godot.dylib ..."
  cmake --build "$BUILD_DIR" --target leanclr_godot -j"$(sysctl -n hw.ncpu)"
  mkdir -p "$ADDON_BIN/Debug"
  cp "$NATIVE/project/bin/Debug/libleanclr_godot.dylib" "$ADDON_BIN/Debug/"
  # 增量链接可能产出签名损坏的 dylib（macOS 会以 Code Signature Invalid 杀掉加载进程），
  # 拷贝后显式重签名兜底。
  if command -v codesign > /dev/null && [ "$(uname)" = "Darwin" ]; then
    codesign -f -s - "$ADDON_BIN/Debug/libleanclr_godot.dylib" > /dev/null 2>&1 || true
  fi
  echo "[leanclr] dylib -> $ADDON_BIN/Debug/libleanclr_godot.dylib"
}

build_managed() {
  mkdir -p "$ASSEMBLIES"
  echo "[leanclr] building GodotSharpCompat.dll ..."
  dotnet msbuild "$NATIVE/managed/GodotSharpCompat/GodotSharpCompat.csproj" \
    /p:Configuration=Debug "/p:FrameworkPathOverride=$BCL"
  # GodotSharpCompat.csproj 的 OutputPath 指向 demo 工程目录，同时供主工程使用。
  cp "$NATIVE/project/leanclr/GodotSharpCompat.dll" "$ASSEMBLIES/"

  echo "[leanclr] building LeanCLRHotfix.dll ..."
  dotnet msbuild "$REPO_ROOT/Assets/LeanCLR/LeanCLRHotfix.csproj" \
    /p:Configuration=Debug "/p:FrameworkPathOverride=$BCL" "/p:SkipBclContent=false"

  if [ "$V2" = "1" ]; then
    echo "[leanclr] building LeanCLRHotfixV2.dll (hot-update candidate) ..."
    dotnet msbuild "$REPO_ROOT/Assets/LeanCLR/LeanCLRHotfix.csproj" \
      /p:Configuration=Debug \
      /p:AssemblyName=LeanCLRHotfixV2 \
      "/p:DefineConstants=LEANCLR_HOTFIX_V2" \
      "/p:FrameworkPathOverride=$BCL"
    # msbuild 的 IncrementalClean 会移除上一次构建外的程序集：
    # 暂存 V2 -> 重编 V1 -> 回补 V2，保证两个 DLL 同时存在。
    mv "$ASSEMBLIES/LeanCLRHotfixV2.dll" "$ASSEMBLIES/LeanCLRHotfixV2.dll.staged"
    dotnet msbuild "$REPO_ROOT/Assets/LeanCLR/LeanCLRHotfix.csproj" \
      /p:Configuration=Debug "/p:FrameworkPathOverride=$BCL"
    mv "$ASSEMBLIES/LeanCLRHotfixV2.dll.staged" "$ASSEMBLIES/LeanCLRHotfixV2.dll"
    printf 'LeanCLRHotfixV2\n' > "$REPO_ROOT/Assets/LeanCLR/live_reload.txt"
    echo "[leanclr] marker -> Assets/LeanCLR/live_reload.txt (LeanCLRHotfixV2)"
  else
    rm -f "$REPO_ROOT/Assets/LeanCLR/live_reload.txt"
  fi
  ls -la "$ASSEMBLIES"
}

if [ "$MODE" = "all" ] || [ "$MODE" = "native" ]; then build_native; fi
if [ "$MODE" = "all" ] || [ "$MODE" = "managed" ]; then build_managed; fi
echo "[leanclr] done."
