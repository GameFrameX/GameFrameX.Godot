#!/bin/bash
# build.sh - Build script for leanclr runtime on Linux and macOS
# Usage: ./build.sh [Debug|Release]

set -e

BUILD_TYPE=${1:-Release}
BUILD_DIR="build/$BUILD_TYPE"

# Create build directory
mkdir -p "$BUILD_DIR"

# Run CMake to configure the project
cmake -S . -B "$BUILD_DIR" -DCMAKE_BUILD_TYPE=$BUILD_TYPE

# Build the project
cmake --build "$BUILD_DIR" -- -j$(nproc || sysctl -n hw.ncpu)

echo "Build finished in $BUILD_DIR"
