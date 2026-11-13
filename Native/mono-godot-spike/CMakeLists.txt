cmake_minimum_required(VERSION 3.24)
project(mono-godot-spike LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_OSX_DEPLOYMENT_TARGET 12.0)

# 路径约定（执行时由 -DMONO_RUNTIME_DIR 传入解包后的 NuGet 运行时目录）
set(MONO_RUNTIME_DIR "" CACHE PATH "解包后的 Microsoft.NETCore.App.Runtime.Mono.osx-arm64 目录")
set(GODOT_CPP_DIR "" CACHE PATH "godot-cpp 仓库根目录")
set(MANAGED_ASM_VERSION_ONE "" CACHE FILEPATH "ManagedSpikeV1.dll 路径")
set(MANAGED_ASM_VERSION_TWO "" CACHE FILEPATH "ManagedSpikeV2.dll 路径")
set(MANAGED_MEMORY_SERIES_DIR "" CACHE PATH "ManagedSpikeR*.dll 热更内存序列目录")

if(NOT MONO_RUNTIME_DIR)
    message(FATAL_ERROR "需要 -DMONO_RUNTIME_DIR 指向解包后的 Mono 运行时包目录")
endif()
if(NOT GODOT_CPP_DIR)
    message(FATAL_ERROR "需要 -DGODOT_CPP_DIR 指向 godot-cpp 仓库根目录")
endif()

set(MONO_NATIVE_DIR ${MONO_RUNTIME_DIR}/runtimes/osx-arm64/native)
set(MONO_LIB ${MONO_NATIVE_DIR}/libcoreclr.dylib)  # 实为 Mono sgen 运行时（见 C1 notes T1）
set(MONO_INC ${MONO_NATIVE_DIR}/include/mono-2.0)

add_library(mono-spike SHARED src/mono_spike.cpp)
set_target_properties(mono-spike PROPERTIES
    SUFFIX ".dylib"
    INSTALL_RPATH "@loader_path"
    BUILD_RPATH "@loader_path")

target_include_directories(mono-spike PRIVATE
    ${GODOT_CPP_DIR}/gdextension
    ${GODOT_CPP_DIR}/gen/include
    ${GODOT_CPP_DIR}/include
    ${MONO_INC})

target_link_libraries(mono-spike
    ${GODOT_CPP_DIR}/bin/libgodot-cpp.macos.template_debug.arm64.a
    ${MONO_LIB}
    "-Wl,-rpath,@loader_path")

# 运行期依赖：Mono dylib、BCL、托管程序集复制到产物旁，Godot 进程内可加载
add_custom_command(TARGET mono-spike POST_BUILD
    # BCL（System.Runtime 等）在包内 lib/net8.0/，不在 native/ 下
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${MONO_RUNTIME_DIR}/runtimes/osx-arm64/lib/net8.0 $<TARGET_FILE_DIR:mono-spike>
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${MONO_NATIVE_DIR} $<TARGET_FILE_DIR:mono-spike>
    COMMAND ${CMAKE_COMMAND} -E copy
        ${MANAGED_ASM_VERSION_ONE} $<TARGET_FILE_DIR:mono-spike>/ManagedSpikeV1.dll
    COMMAND ${CMAKE_COMMAND} -E copy
        ${MANAGED_ASM_VERSION_TWO} $<TARGET_FILE_DIR:mono-spike>/ManagedSpikeV2.dll
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${MANAGED_MEMORY_SERIES_DIR} $<TARGET_FILE_DIR:mono-spike>/memory-series
    COMMAND install_name_tool -change
        ${MONO_LIB} "@loader_path/libcoreclr.dylib" $<TARGET_FILE:mono-spike>
    COMMENT "复制 Mono 运行时、BCL 与托管程序集到产物目录")
