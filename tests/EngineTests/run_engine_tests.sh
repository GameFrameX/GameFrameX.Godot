#!/usr/bin/env bash
# GameFrameX Godot headless 引擎测试入口（双阶段）
# 用法: bash tests/EngineTests/run_engine_tests.sh
# 阶段1: GodotEditor=true 构建 TOOLS 程序集 + headless 编辑器模式（IsEditorHint=true）
#        跑 A 组(EditorSimulate) + B6 + C/D 组
# 阶段2: 标准构建 + headless 游戏模式
#        跑 B 组(PCK 挂载仅游戏模式可用, LoadResourcePack 编辑器不支持) + C/D 组
# 成败判定: 以各阶段日志中的 RESULT 行为准（total>0 且 failed=0），
# godot 进程退出码仅作参考——全部 PASS 后引擎 teardown 仍可能 SIGABRT/挂死
# （已备案的引擎退出期问题），RESULT 行才是事实来源。
# 退出码 0=两阶段全部通过, 1=任一阶段失败
# 可用环境变量:
#   GODOT_BIN           覆盖 Godot 可执行文件路径
#   ENGINE_TEST_TIMEOUT 单阶段超时秒数（默认 300，超时 kill -9 后按日志判定）
set -euo pipefail
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
GODOT_BIN="${GODOT_BIN:-/Applications/Godot_mono.app/Contents/MacOS/Godot}"
EDITOR_ENTRY="res://tests/EngineTests/EngineTestMain.gd"
GAME_ENTRY="res://tests/EngineTests/EngineTestRunner.tscn"
ENGINE_TEST_TIMEOUT="${ENGINE_TEST_TIMEOUT:-300}"
LOCK_DIR="/tmp/gameframex_engine_tests.lock"

# 单实例锁：两个 headless 编辑器实例并发会互抢 .godot 编辑器缓存导致双方挂死
#（2026-08-29 实测双实例互锁，单实例可正常退出）；持有进程已死则自动清陈旧锁
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  lock_pid="$(cat "${LOCK_DIR}/pid" 2>/dev/null || true)"
  if [ -n "${lock_pid}" ] && ! kill -0 "${lock_pid}" 2>/dev/null; then
    echo "[engine-tests] 清理陈旧锁（持有进程 ${lock_pid} 已退出）"
    rm -rf "$LOCK_DIR"
    mkdir "$LOCK_DIR"
  else
    echo "[engine-tests] 已有实例在运行（${LOCK_DIR}），退出。确认无实例后可删锁重试。"
    exit 1
  fi
fi
echo $$ > "${LOCK_DIR}/pid"
trap 'rm -rf "$LOCK_DIR" 2>/dev/null || true' EXIT

echo "[engine-tests] project: ${PROJECT_DIR}"
echo "[engine-tests] godot:   ${GODOT_BIN}"
"$GODOT_BIN" --version

# 跑一个阶段：后台计时器超时强杀（macOS 无 GNU timeout），结束后解析日志 RESULT 行
run_phase() {
  local log_file=$1; shift
  rm -f "$log_file"
  "$GODOT_BIN" "$@" > "$log_file" 2>&1 &
  local godot_pid=$!
  ( sleep "$ENGINE_TEST_TIMEOUT" && kill -9 "$godot_pid" 2>/dev/null ) &
  local watcher=$!
  if wait "$godot_pid" 2>/dev/null; then
    echo "[engine-tests] godot exited 0"
  else
    echo "[engine-tests] godot exited non-zero or killed（详见 ${log_file} 尾部）"
  fi
  kill "$watcher" 2>/dev/null || true
  wait "$watcher" 2>/dev/null || true
  tail -3 "$log_file"

  local verdict total failed
  verdict="$(grep -E "=== RESULT: total=[0-9]+ passed=[0-9]+ failed=[0-9]+" "$log_file" | tail -1 || true)"
  echo "[engine-tests] ${verdict:-RESULT 行缺失，判定该阶段失败}"
  [ -n "$verdict" ] || return 1
  total="$(printf '%s' "$verdict" | sed -E 's/.*total=([0-9]+).*/\1/')"
  failed="$(printf '%s' "$verdict" | sed -E 's/.*failed=([0-9]+).*/\1/')"
  [ "$total" -gt 0 ] && [ "$failed" -eq 0 ]
}

echo "[engine-tests] phase 1: tools build + editor context"
dotnet build "$PROJECT_DIR/Godot.csproj" -p:GodotEditor=true --nologo -v quiet
STATUS_EDITOR=0
run_phase "$PROJECT_DIR/tests/EngineTests/last_run_editor.log" --headless -e --path "$PROJECT_DIR" --script "$EDITOR_ENTRY" || STATUS_EDITOR=$?

echo "[engine-tests] phase 2: standard build + game context"
dotnet build "$PROJECT_DIR/Godot.csproj" --nologo -v quiet
STATUS_GAME=0
run_phase "$PROJECT_DIR/tests/EngineTests/last_run_game.log" --headless --path "$PROJECT_DIR" "$GAME_ENTRY" || STATUS_GAME=$?

echo "[engine-tests] editor verdict=${STATUS_EDITOR} game verdict=${STATUS_GAME}"
if [ "${STATUS_EDITOR}" -ne 0 ] || [ "${STATUS_GAME}" -ne 0 ]; then
  exit 1
fi
exit 0
