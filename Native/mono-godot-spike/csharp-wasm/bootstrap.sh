#!/usr/bin/env bash
# C1 Spike-W2：C# WASM 逻辑层一键重建脚本。
# 重建三个运行时组装目录（均为可重建产物，不入 git）：
#   csharp-wasm/www                      浏览器 PoC 站点（dotnet.js 全家 + BCL + WasmLogic + config）
#   wechat-integrated/dotnetsdk          微信合体工程 C# 分包（CJS 化运行时 + .bin 程序集）
#   wechat-csharp-probe/dotnet           微信探针工程运行时目录（同上 + V2）
# 依赖：dotnet SDK 10、node/npm（esbuild）、curl。
# 用法：bash Native/mono-godot-spike/csharp-wasm/bootstrap.sh
set -euo -o pipefail

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SPIKE_DIRECTORY="${SCRIPT_DIRECTORY}/.."
DEPS_DIRECTORY="${SPIKE_DIRECTORY}/deps-csharp"
RUNTIME_PACKAGE_VERSION="10.0.14"
# GitHub 直连慢时走镜像；置空则直连（本脚本仅用 nuget，直连稳定，预留变量备用）
GITHUB_MIRROR="${GITHUB_MIRROR:-}"

echo "== [1/5] 拉取 browser-wasm 运行时包 ${RUNTIME_PACKAGE_VERSION} =="
mkdir -p "${DEPS_DIRECTORY}"
if [ ! -f "${DEPS_DIRECTORY}/browser-wasm/runtimes/browser-wasm/native/dotnet.native.wasm" ]; then
    curl -sL --retry 3 -o "${DEPS_DIRECTORY}/browser-wasm.zip" \
        "https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Runtime.Mono.browser-wasm/${RUNTIME_PACKAGE_VERSION}"
    rm -rf "${DEPS_DIRECTORY}/browser-wasm"
    mkdir -p "${DEPS_DIRECTORY}/browser-wasm"
    unzip -qo "${DEPS_DIRECTORY}/browser-wasm.zip" -d "${DEPS_DIRECTORY}/browser-wasm"
fi
RUNTIME_NATIVE="${DEPS_DIRECTORY}/browser-wasm/runtimes/browser-wasm/native"
RUNTIME_MANAGED="${DEPS_DIRECTORY}/browser-wasm/runtimes/browser-wasm/lib/net10.0"

echo "== [2/5] esbuild 转换 dotnet 运行时 JS 为 CJS（微信 require 可加载）=="
command -v npx > /dev/null || { echo "缺少 node/npm（npx）" >&2; exit 1; }
STAGING_DIRECTORY="${DEPS_DIRECTORY}/cjs"
mkdir -p "${STAGING_DIRECTORY}"
for module_name in dotnet dotnet.native dotnet.runtime; do
    npx esbuild "${RUNTIME_NATIVE}/${module_name}.js" --format=cjs --platform=neutral \
        --define:import.meta.url="\"https://usr/dotnet/${module_name}.js\"" \
        --outfile="${STAGING_DIRECTORY}/${module_name}.cjs.js" --log-level=error
done

echo "== [3/5] 构建 WasmLogic / WasmLogicV2 =="
for project_name in WasmLogic WasmLogicV2; do
    dotnet build "${SCRIPT_DIRECTORY}/${project_name}" -c Release -o "${SCRIPT_DIRECTORY}/${project_name}/bin/out"
done

echo "== [4/5] 组装浏览器 PoC 站点 www/ =="
WEB_DIRECTORY="${SCRIPT_DIRECTORY}/www"
rm -rf "${WEB_DIRECTORY}"
mkdir -p "${WEB_DIRECTORY}"
cp "${RUNTIME_NATIVE}/dotnet.js" "${RUNTIME_NATIVE}/dotnet.native.js" \
   "${RUNTIME_NATIVE}/dotnet.runtime.js" "${RUNTIME_NATIVE}/dotnet.native.wasm" "${WEB_DIRECTORY}/"
cp "${RUNTIME_NATIVE}/System.Private.CoreLib.dll" "${WEB_DIRECTORY}/"
cp "${RUNTIME_MANAGED}"/*.dll "${WEB_DIRECTORY}/"
cp "${SCRIPT_DIRECTORY}/WasmLogic/bin/out/WasmLogic.dll" "${WEB_DIRECTORY}/"
(cd "${SCRIPT_DIRECTORY}" && python3 generate-runtime-config.py)

echo "== [5/5] 组装微信工程运行时目录（.bin = .dll 改名，小游戏包不收 .dll）=="
assemble_wechat_runtime() {
    local target_directory="$1"
    local include_version_two="$2"
    rm -rf "${target_directory}"
    mkdir -p "${target_directory}"
    cp "${STAGING_DIRECTORY}"/*.cjs.js "${RUNTIME_NATIVE}/dotnet.native.wasm" "${target_directory}/"
    cp "${RUNTIME_NATIVE}/System.Private.CoreLib.dll" "${target_directory}/System.Private.CoreLib.bin"
    cp "${RUNTIME_MANAGED}/System.Runtime.InteropServices.JavaScript.dll" \
       "${target_directory}/System.Runtime.InteropServices.JavaScript.bin"
    cp "${RUNTIME_MANAGED}/System.Runtime.dll" "${target_directory}/System.Runtime.bin"
    cp "${SCRIPT_DIRECTORY}/WasmLogic/bin/out/WasmLogic.dll" "${target_directory}/WasmLogic.bin"
    if [ "${include_version_two}" = "yes" ]; then
        cp "${SCRIPT_DIRECTORY}/WasmLogicV2/bin/out/WasmLogicV2.dll" "${target_directory}/WasmLogicV2.bin"
    fi
}
assemble_wechat_runtime "${SPIKE_DIRECTORY}/wechat-integrated/dotnetsdk" yes
# 微信分包根必须有自己的 game.js（已入 git，重建时保留）
[ -f "${SPIKE_DIRECTORY}/wechat-integrated/dotnetsdk/game.js" ] || \
    echo '/* dotnetsdk 分包入口（微信要求每个分包根有 game.js）*/' > "${SPIKE_DIRECTORY}/wechat-integrated/dotnetsdk/game.js"
assemble_wechat_runtime "${SPIKE_DIRECTORY}/wechat-csharp-probe/dotnet" yes

echo "== 完成：www/、wechat-integrated/dotnetsdk/、wechat-csharp-probe/dotnet/ 均已重建 =="
