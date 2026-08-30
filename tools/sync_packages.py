#!/usr/bin/env python3
"""从 GameFrameX npm registry 拉取 Godot 插件包并同步到 addons/。

用法（在 Godot 主仓根执行）:
  python3 tools/sync_packages.py            # 按 godot-packages.json 同步
  python3 tools/sync_packages.py --check    # 只比对不写入（CI 门禁用）

锁文件: godot-packages.json（主仓根），版本号可写具体版本或 "latest"。
"""
import argparse
import fnmatch
import json
import os
import shutil
import subprocess
import sys
import tarfile
import tempfile
import urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
LOCK = os.path.join(ROOT, "godot-packages.json")
# tarball 内 package/ 里不应进入 addons/ 的 registry 附带物（.github CI、.releaserc），
# 以及 addons/ 本地的 SyncService / 编辑器生成物（--delete 需保护）
EXCLUDES = [".releaserc*", ".github", "*.csproj", "*.asmdef.uid", ".godot"]

# npm 发布链路会把 CRLF 归一为 LF，比较时忽略行尾差异
def _norm(path):
    with open(path, "rb") as f:
        return f.read().replace(b"\r\n", b"\n")


def _excluded(name):
    return name in EXCLUDES or any(fnmatch.fnmatch(name, e) for e in EXCLUDES)


def _walk(root):
    out = {}
    for dirpath, dirnames, filenames in os.walk(root):
        rel = os.path.relpath(dirpath, root)
        dirnames[:] = [d for d in dirnames if not _excluded(d)]
        for fn in filenames:
            if _excluded(fn):
                continue
            out[os.path.normpath(os.path.join(rel, fn))] = os.path.join(dirpath, fn)
    return out


def check_drift(src, target, name):
    """EOL 无关的内容比较，返回漂移描述列表。"""
    s, t = _walk(src), _walk(target)
    drift = []
    for k in sorted(set(s) - set(t)):
        drift.append(f"  + {k}")
    for k in sorted(set(t) - set(s)):
        drift.append(f"  - {k}")
    for k in sorted(set(s) & set(t)):
        if _norm(s[k]) != _norm(t[k]):
            drift.append(f"  ~ {k}")
    return drift


def resolve_version(registry, name, spec):
    if spec != "latest":
        return spec
    with urllib.request.urlopen(f"{registry}/{name}", timeout=20) as resp:
        return json.load(resp)["dist-tags"]["latest"]


def download(registry, name, version, dest):
    url = f"{registry}/{name}/-/{name}-{version}.tgz"
    with urllib.request.urlopen(url, timeout=60) as resp, open(dest, "wb") as f:
        shutil.copyfileobj(resp, f)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true", help="只比对，不写入")
    args = ap.parse_args()

    with open(LOCK) as f:
        lock = json.load(f)
    registry = lock["registry"].rstrip("/")

    failures = []
    for name in sorted(lock["packages"]):
        target = os.path.join(ROOT, "addons", name)
        try:
            version = resolve_version(registry, name, lock["packages"][name])
            with tempfile.TemporaryDirectory() as tmp:
                tgz = os.path.join(tmp, "pkg.tgz")
                download(registry, name, version, tgz)
                with tarfile.open(tgz) as tf:
                    tf.extractall(tmp, filter="data")
                src = os.path.join(tmp, "package")
                if not os.path.isdir(src):
                    raise RuntimeError("tarball 缺少 package/ 根")
                if args.check:
                    drift = check_drift(src, target, name)
                    if drift:
                        print(f"[{name}] 漂移:\n" + "\n".join(drift))
                        failures.append(name)
                else:
                    subprocess.run(
                        ["rsync", "-rlp", "--delete"]
                        + [f"--exclude={e}" for e in EXCLUDES]
                        + [src + "/", target + "/"], check=True)
                    print(f"[{name}] {version} 同步完成")
        except Exception as e:
            print(f"[{name}] 失败: {e}", file=sys.stderr)
            failures.append(name)

    if failures:
        print(f"\n{len(failures)} 个包未同步: {failures}", file=sys.stderr)
        sys.exit(1)
    print("\n全部同步完成" if not args.check else "\n无漂移")


if __name__ == "__main__":
    main()
