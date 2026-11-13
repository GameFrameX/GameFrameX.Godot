#!/usr/bin/env bash
# C1 mono-godot-spike 一键可复现构建脚本
# 用法：bash Native/mono-godot-spike/bootstrap.sh
# 依赖：cmake、dotnet SDK 8+、scons、Xcode 命令行工具
# 产物：build/libmono-spike.dylib（含 Mono 运行时、BCL、V1/V2 托管程序集）
set -euo -o pipefail

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPS_DIRECTORY="${SCRIPT_DIRECTORY}/deps"
BUILD_DIRECTORY="${SCRIPT_DIRECTORY}/build"
MANAGED_DIRECTORY="${SCRIPT_DIRECTORY}/managed"

# GitHub 直连慢时走镜像；置空则直连
GITHUB_MIRROR="${GITHUB_MIRROR:-https://ghfast.top/}"
MONO_RUNTIME_PACKAGE_VERSION="8.0.31"
GODOT_CPP_API_VERSION="4.7"
MACOS_DEPLOYMENT_TARGET="12.0"

download() {
    local url="$1" output="$2"
    if [ -n "${GITHUB_MIRROR}" ]; then
        curl -sL --retry 3 -C - -o "${output}" "${GITHUB_MIRROR}${url}"
    else
        curl -sL --retry 3 -C - -o "${output}" "${url}"
    fi
}

echo "== [1/5] 拉取 godot-cpp（master，api_version=${GODOT_CPP_API_VERSION}）=="
mkdir -p "${DEPS_DIRECTORY}"
if [ ! -f "${DEPS_DIRECTORY}/godot-cpp/SConstruct" ]; then
    # codeload 域名不走镜像（ghfast 等镜像仅支持 github.com 资产）
    curl -sL --retry 3 -C - -o "${DEPS_DIRECTORY}/godot-cpp.tar.gz" \
        "https://codeload.github.com/godotengine/godot-cpp/tar.gz/refs/heads/master"
    rm -rf "${DEPS_DIRECTORY}/godot-cpp"
    tar xzf "${DEPS_DIRECTORY}/godot-cpp.tar.gz" -C "${DEPS_DIRECTORY}"
    mv "${DEPS_DIRECTORY}/godot-cpp-master" "${DEPS_DIRECTORY}/godot-cpp"
fi

echo "== [2/5] 编译 godot-cpp 静态库 =="
command -v scons > /dev/null || { echo "缺少 scons：brew install scons" >&2; exit 1; }
scons -C "${DEPS_DIRECTORY}/godot-cpp" \
    platform=macos arch=arm64 target=template_debug \
    api_version="${GODOT_CPP_API_VERSION}" \
    macos_deployment_target="${MACOS_DEPLOYMENT_TARGET}" -j"$(sysctl -n hw.ncpu)"

echo "== [3/5] 拉取 Mono 运行时包 ${MONO_RUNTIME_PACKAGE_VERSION} =="
if [ ! -f "${DEPS_DIRECTORY}/mono-runtime/runtimes/osx-arm64/native/libcoreclr.dylib" ]; then
    # nuget 包直连稳定，不走镜像
    curl -sL --retry 3 -o "${DEPS_DIRECTORY}/mono-runtime-pack.zip" \
        "https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Runtime.Mono.osx-arm64/${MONO_RUNTIME_PACKAGE_VERSION}"
    rm -rf "${DEPS_DIRECTORY}/mono-runtime"
    mkdir -p "${DEPS_DIRECTORY}/mono-runtime"
    unzip -qo "${DEPS_DIRECTORY}/mono-runtime-pack.zip" -d "${DEPS_DIRECTORY}/mono-runtime"
fi

echo "== [4/5] 构建 V1/V2 托管程序集 =="
dotnet build "${MANAGED_DIRECTORY}" -c Release \
    -p:AssemblyName=ManagedSpikeV1 -p:DefineConstants=SPIKE_VERSION_ONE \
    -o "${MANAGED_DIRECTORY}/bin/spike-version-one"
dotnet build "${MANAGED_DIRECTORY}" -c Release \
    -p:AssemblyName=ManagedSpikeV2 -p:DefineConstants=SPIKE_VERSION_TWO \
    -o "${MANAGED_DIRECTORY}/bin/spike-version-two"

echo "== [4b/5] 构建热更内存序列副本 R01-R05 =="
for series_number in 01 02 03 04 05; do
    dotnet build "${MANAGED_DIRECTORY}" -c Release \
        -p:AssemblyName="ManagedSpikeR${series_number}" -p:DefineConstants=SPIKE_VERSION_ONE \
        -o "${MANAGED_DIRECTORY}/bin/spike-memory-series/R${series_number}"
done

echo "== [5/5] cmake 配置并构建 libmono-spike.dylib =="
cmake -S "${SCRIPT_DIRECTORY}" -B "${BUILD_DIRECTORY}" \
    -DCMAKE_BUILD_TYPE=Debug \
    -DMONO_RUNTIME_DIR="${DEPS_DIRECTORY}/mono-runtime" \
    -DGODOT_CPP_DIR="${DEPS_DIRECTORY}/godot-cpp" \
    -DMANAGED_ASM_VERSION_ONE="${MANAGED_DIRECTORY}/bin/spike-version-one/ManagedSpikeV1.dll" \
    -DMANAGED_ASM_VERSION_TWO="${MANAGED_DIRECTORY}/bin/spike-version-two/ManagedSpikeV2.dll" \
    -DMANAGED_MEMORY_SERIES_DIR="${MANAGED_DIRECTORY}/bin/spike-memory-series"
cmake --build "${BUILD_DIRECTORY}"

echo "== 完成：${BUILD_DIRECTORY}/libmono-spike.dylib =="
echo "验证：/path/to/Godot --path ${SCRIPT_DIRECTORY}/godot-test --headless --quit-after 4"
