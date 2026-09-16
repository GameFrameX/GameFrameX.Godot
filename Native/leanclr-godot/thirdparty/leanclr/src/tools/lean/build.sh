#!/bin/bash
# build.sh for lean tool (Linux/macOS)
set -e
BUILD_DIR="build"
BUILD_TYPE=${1:-Debug}

cmake -S . -B "$BUILD_DIR" -DCMAKE_BUILD_TYPE=$BUILD_TYPE
cmake --build "$BUILD_DIR" --target lean -- -j$(nproc || sysctl -n hw.ncpu)
EXE="$BUILD_DIR/bin/lean"
if [ -f "$EXE" ]; then
  echo "Built: $EXE"
else
  echo "Warning: expected exe not found at $EXE"
fi
echo "Done."
