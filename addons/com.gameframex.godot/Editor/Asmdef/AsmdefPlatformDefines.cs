// 说明：纯 BCL 逻辑，不包 #if TOOLS，供单元测试触达（同 AsmdefModel.cs）。
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFrameX.Editor.Asmdef
{
    /// <summary>
    /// platformDefines 的多行文本格式（属性编辑器输入框）与模型的互转。
    /// 格式约定：一行一平台，"平台: 宏1, 宏2"，如 "windows: GF_WIN, GF_DEBUG"。
    /// </summary>
    public static class AsmdefPlatformDefines
    {
        public static string Format(IReadOnlyDictionary<string, List<string>> platformDefines)
        {
            if (platformDefines == null || platformDefines.Count == 0)
            {
                return string.Empty;
            }

            var lines = new List<string>();
            foreach (KeyValuePair<string, List<string>> pair in platformDefines.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                List<string> defines = (pair.Value ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();
                if (defines.Count == 0)
                {
                    continue;
                }

                lines.Add(pair.Key.Trim() + ": " + string.Join(", ", defines));
            }

            return string.Join("\n", lines);
        }

        public static Dictionary<string, List<string>> Parse(string text)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string rawLine in (text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                int separatorIndex = line.IndexOf(':');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string platform = line.Substring(0, separatorIndex).Trim();
                var defines = new List<string>();
                foreach (string rawDefine in line.Substring(separatorIndex + 1).Split(','))
                {
                    string define = rawDefine.Trim();
                    if (define.Length > 0 && !defines.Contains(define))
                    {
                        defines.Add(define);
                    }
                }

                if (platform.Length > 0 && defines.Count > 0)
                {
                    result[platform] = defines;
                }
            }

            return result;
        }
    }
}
