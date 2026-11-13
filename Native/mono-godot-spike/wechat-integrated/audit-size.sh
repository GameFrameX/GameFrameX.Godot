#!/bin/bash
# 阶段5 包体核对：主包与各分包体积 vs 微信限制（主包 4MB、总计 30MB）
PKG="$1"
cd "$PKG" || exit 1
echo "== 包体核对: $PKG =="
# 主包 = 全部文件减去 game.json 声明的分包 root
SUBROOTS=$(python3 -c "import json;print(' '.join(s['root'] for s in json.load(open('game.json')).get('subpackages',[])))")
TOTAL_BYTES=$(du -sk . | awk '{print $1}')
MAIN_BYTES=$TOTAL_BYTES
for root in $SUBROOTS; do
    SUB=$(du -sk "$root" | awk '{print $1}')
    MAIN_BYTES=$((MAIN_BYTES - SUB))
    printf "%-28s %8.2f MB  (%s)\n" "分包 $root" "$(echo "$SUB/1024" | bc -l)" "$root"
done
printf "%-28s %8.2f MB\n" "主包(除分包外全部)" "$(echo "$MAIN_BYTES/1024" | bc -l)"
printf "%-28s %8.2f MB  (限 30MB)\n" "总计" "$(echo "$TOTAL_BYTES/1024" | bc -l)"
echo "-- 主包明细 top10:"
find . -type f -not -path "./engine/*" -not -path "./subpackages/*" -exec du -k {} + | sort -rn | head -10
