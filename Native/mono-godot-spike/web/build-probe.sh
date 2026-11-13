#!/usr/bin/env bash
# C1 Spike-W：Web(WASM) GDExtension 探针构建脚本
# 依赖：emcc（本机 emscripten）、/tmp/spike-deps/godot-cpp-47 的 web 静态库
set -euo -o pipefail

SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GODOT_CPP_DIRECTORY="${GODOT_CPP_DIRECTORY:-/tmp/spike-deps/godot-cpp-47}"
OUTPUT_DIRECTORY="${SCRIPT_DIRECTORY}/../build-web"

mkdir -p "${OUTPUT_DIRECTORY}"

em++ -std=gnu++17 -g -fPIC -c "${SCRIPT_DIRECTORY}/src/web_probe.cpp" \
    -I"${GODOT_CPP_DIRECTORY}/gdextension" \
    -I"${GODOT_CPP_DIRECTORY}/gen/include" \
    -I"${GODOT_CPP_DIRECTORY}/include" \
    -o "${OUTPUT_DIRECTORY}/web_probe.o"

em++ -o "${OUTPUT_DIRECTORY}/libweb-probe.wasm" \
    "${OUTPUT_DIRECTORY}/web_probe.o" \
    "${GODOT_CPP_DIRECTORY}/bin/libgodot-cpp.web.template_debug.wasm32.a" \
    -sSIDE_MODULE=1 -g

echo "== 完成：${OUTPUT_DIRECTORY}/libweb-probe.wasm =="
