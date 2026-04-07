#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOOLS_ROOT="$(cd "${SCRIPT_DIR}/../../../GameFrameX.Tools/Config/Tools" && pwd)"
CONF="${SCRIPT_DIR}/luban.conf"
if [[ -z "${SERVER_PATH:-}" ]]; then
  echo "SERVER_PATH is not set. Please export SERVER_PATH to your server root."
  exit 1
fi
dotnet "${TOOLS_ROOT}/Luban.dll" --conf "${CONF}" --target server --dataTarget json --codeTarget cs-dotnet-json --xargs outputDataDir="${SERVER_PATH}/Server.Config/Json" --xargs outputCodeDir="${SERVER_PATH}/Server.Config/Config"
