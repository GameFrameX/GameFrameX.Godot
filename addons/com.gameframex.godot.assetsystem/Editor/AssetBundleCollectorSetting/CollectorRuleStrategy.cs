using System;
using System.IO;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 收集规则的枚举策略实现(替代 Unity 版基于类名反射的规则体系)。
    /// 仅实现常用子集;未实现的规则值显式抛出 NotSupportedException,接入收集流程时再补齐。
    /// </summary>
    public static class CollectorRuleStrategy
    {
        /// <summary>
        /// 打包规则:计算资源所属 bundle 的逻辑名称(不含输出 hash 文件名,输出文件名由构建管线决定)。
        /// </summary>
        public static string GetPackBundleName(EPackRule rule, string assetPath)
        {
            switch (rule)
            {
                case EPackRule.PackSeparately:
                {
                    // 每个文件独立成包:以去扩展名的资源路径作为 bundle 名
                    return RemoveExtension(NormalizePath(assetPath));
                }
                case EPackRule.PackDirectory:
                {
                    // 目录打包:以资源所在目录路径作为 bundle 名。
                    // 注意:res:// 等虚拟路径不能交给 Path.GetDirectoryName(会把 res:// 折叠成 res:/),手工取最后斜杠前缀
                    var normalized = NormalizePath(assetPath);
                    var lastSlashIndex = normalized.LastIndexOf('/');
                    if (lastSlashIndex <= 0)
                    {
                        return normalized;
                    }

                    return normalized.Substring(0, lastSlashIndex);
                }
                case EPackRule.PackRawFile:
                {
                    // 原生文件:以去扩展名的资源路径作为 bundle 名(输出后缀 .rawfile 由构建管线追加)
                    return RemoveExtension(NormalizePath(assetPath));
                }
                case EPackRule.PackTopDirectory:
                {
                    // TODO(Phase 2.5): 需要收集目录上下文才能取顶层段,接入收集流程时实现
                    throw new NotSupportedException($"暂不支持的打包规则: {rule}");
                }
                default:
                {
                    throw new NotSupportedException($"暂不支持的打包规则: {rule}");
                }
            }
        }

        /// <summary>
        /// 寻址规则:计算资源的定位地址。
        /// </summary>
        public static string GetAssetAddress(EAddressRule rule, string assetPath, string groupName)
        {
            switch (rule)
            {
                case EAddressRule.AddressDisable:
                {
                    return string.Empty;
                }
                case EAddressRule.AddressByFileName:
                {
                    return Path.GetFileNameWithoutExtension(assetPath);
                }
                case EAddressRule.AddressByGroupAndFileName:
                {
                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    return $"{groupName}_{fileName}";
                }
                case EAddressRule.AddressByAssetPath:
                {
                    return NormalizePath(assetPath);
                }
                default:
                {
                    throw new NotSupportedException($"暂不支持的寻址规则: {rule}");
                }
            }
        }

        /// <summary>
        /// 过滤规则:判断资源文件是否参与收集。
        /// </summary>
        public static bool IsMatchFilter(EFilterRule rule, string assetPath)
        {
            switch (rule)
            {
                case EFilterRule.CollectAll:
                {
                    return true;
                }
                case EFilterRule.CollectScene:
                {
                    // 对应 Unity 的 CollectScene(.unity/.scene),Godot 场景后缀为 .tscn
                    return HasExtension(assetPath, ".tscn");
                }
                case EFilterRule.CollectTexture:
                {
                    return HasExtension(assetPath, ".png", ".jpg", ".jpeg", ".webp", ".svg", ".bmp");
                }
                case EFilterRule.CollectShader:
                {
                    // 对应 Unity 的 CollectShader(.shader),Godot 着色器后缀为 .gdshader
                    return HasExtension(assetPath, ".gdshader");
                }
                default:
                {
                    throw new NotSupportedException($"暂不支持的过滤规则: {rule}");
                }
            }
        }

        /// <summary>
        /// 忽略规则:判断资源文件是否被忽略。
        /// </summary>
        public static bool IsIgnored(EIgnoreRule rule, string assetPath)
        {
            switch (rule)
            {
                case EIgnoreRule.NormalIgnore:
                {
                    return false;
                }
                case EIgnoreRule.IgnoreAll:
                {
                    return true;
                }
                default:
                {
                    throw new NotSupportedException($"暂不支持的忽略规则: {rule}");
                }
            }
        }

        /// <summary>
        /// 激活规则:判断分组是否激活。
        /// </summary>
        public static bool IsActiveGroup(EActiveRule rule)
        {
            switch (rule)
            {
                case EActiveRule.EnableGroup:
                {
                    return true;
                }
                case EActiveRule.DisableGroup:
                {
                    return false;
                }
                default:
                {
                    throw new NotSupportedException($"暂不支持的激活规则: {rule}");
                }
            }
        }

        private static string NormalizePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return string.Empty;
            }

            return assetPath.Replace('\\', '/');
        }

        private static string RemoveExtension(string normalizedPath)
        {
            var fileName = Path.GetFileName(normalizedPath);
            var dotIndex = fileName.LastIndexOf('.');
            if (dotIndex <= 0)
            {
                return normalizedPath;
            }

            return normalizedPath.Substring(0, normalizedPath.Length - (fileName.Length - dotIndex));
        }

        private static bool HasExtension(string assetPath, params string[] extensions)
        {
            var extension = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            var lowered = extension.ToLowerInvariant();
            foreach (var candidate in extensions)
            {
                if (string.Equals(lowered, candidate, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
