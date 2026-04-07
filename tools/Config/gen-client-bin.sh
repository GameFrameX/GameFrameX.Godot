#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
TOOLS_ROOT="${SCRIPT_DIR}/Tools"
CONF="${SCRIPT_DIR}/luban.conf"
DEFINES_DIR="${SCRIPT_DIR}/Defines"
EXCELS_DIR="${SCRIPT_DIR}/Excels"

if [[ ! -f "${TOOLS_ROOT}/Luban.dll" ]]; then
  echo "[ConfigGen] Missing local tool: ${TOOLS_ROOT}/Luban.dll"
  exit 1
fi
if [[ ! -f "${CONF}" ]]; then
  echo "[ConfigGen] Missing local config: ${CONF}"
  exit 1
fi
if [[ ! -d "${DEFINES_DIR}" ]]; then
  echo "[ConfigGen] Missing local schema dir: ${DEFINES_DIR}"
  exit 1
fi
if [[ ! -d "${EXCELS_DIR}" ]]; then
  echo "[ConfigGen] Missing local data dir: ${EXCELS_DIR}"
  exit 1
fi

pushd "${SCRIPT_DIR}" >/dev/null
dotnet "${TOOLS_ROOT}/Luban.dll" --conf "${CONF}" --target client --dataTarget bin --codeTarget cs-bin --validationFailAsError true --xargs outputDataDir="${PROJECT_ROOT}/Assets/Bundles/Config" --xargs outputCodeDir="${PROJECT_ROOT}/Assets/Hotfix/Config/Generate"
popd >/dev/null

