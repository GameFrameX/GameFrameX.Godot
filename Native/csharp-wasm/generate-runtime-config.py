#!/usr/bin/env python3
"""C1 Spike-W2：生成 dotnet.js 所需的启动 config JSON。

构造 10.x dotnet.js 期望的 boot config：
- resources.assembly: [{name, behavior:assembly}] 数组
- resources.runtime: 唯一 dotnetwasm 资产（dotnet.native.wasm）
- 顶层 assembly 数组 + assemblyRootPath 指向 dll 目录
"""
import json
import os
import sys
from pathlib import Path

def main() -> None:
    managed_directory = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("www")
    core_names = ["System.Private.CoreLib.dll", "System.Runtime.InteropServices.JavaScript.dll"]
    # 可选白名单：SPIKE_ASSEMBLY_ALLOWLIST 指向每行一个 dll 名的文件，用于裁剪包体
    allowlist_path = os.environ.get("SPIKE_ASSEMBLY_ALLOWLIST")
    allowlist = None
    if allowlist_path:
        allowlist = {line.strip() for line in Path(allowlist_path).read_text().splitlines() if line.strip()}
    assembly_names = [
        dll_path.name for dll_path in sorted(managed_directory.glob("*.dll"))
        if dll_path.name not in core_names and (allowlist is None or dll_path.name in allowlist)
    ]
    config = {
        "mainAssemblyName": "WasmLogic",
        "assembly": assembly_names,
        "resources": {
            "coreAssembly": [
                {"name": name, "behavior": "assembly"} for name in core_names
            ],
            "assembly": [
                {"name": name, "behavior": "assembly"} for name in assembly_names
            ],
            "wasmNative": [{"name": "dotnet.native.wasm"}],
            "jsModuleNative": [{"name": "dotnet.native.js"}],
            "jsModuleRuntime": [{"name": "dotnet.runtime.js"}],
        },
    }
    output_path = managed_directory / "runtime-config.json"
    output_path.write_text(json.dumps(config, indent=1))
    print(f"wrote {output_path} with {len(core_names)} core + {len(assembly_names)} assemblies")


if __name__ == "__main__":
    main()
