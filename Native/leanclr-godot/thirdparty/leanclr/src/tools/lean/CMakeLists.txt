cmake_minimum_required(VERSION 3.15)
project(lean CXX)

# Bring in the runtime library
add_subdirectory(../../runtime runtime_build)

# lean executable
add_executable(lean main.cpp)

target_link_libraries(lean PRIVATE leanclr)

set_target_properties(lean PROPERTIES
    CXX_STANDARD 17
    CXX_STANDARD_REQUIRED ON
)

# Optional: place binaries under build tree for convenience
set_target_properties(lean PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)
