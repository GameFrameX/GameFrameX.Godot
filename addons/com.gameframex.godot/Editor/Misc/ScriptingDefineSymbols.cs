#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 脚本宏定义帮助类。
    /// </summary>
    public static class ScriptingDefineSymbols
    {
        private const string CsprojFileName = "Godot.csproj";
        private const string CompileTriggerStampFilePath = "res://addons/com.gameframex.godot/Editor/Misc/CompileTriggerStamp.cs";

        /// <summary>
        /// 检查是否存在指定的脚本宏定义。
        /// </summary>
        /// <param name="scriptingDefineSymbol">要检查的脚本宏定义。</param>
        /// <returns>是否存在指定宏定义。</returns>
        public static bool HasScriptingDefineSymbol(string scriptingDefineSymbol)
        {
            if (string.IsNullOrEmpty(scriptingDefineSymbol))
            {
                return false;
            }

            string[] scriptingDefineSymbols = GetScriptingDefineSymbols();
            foreach (string i in scriptingDefineSymbols)
            {
                if (string.Equals(i, scriptingDefineSymbol, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 增加指定的脚本宏定义。
        /// </summary>
        /// <param name="scriptingDefineSymbol">要增加的脚本宏定义。</param>
        public static void AddScriptingDefineSymbol(string scriptingDefineSymbol)
        {
            if (string.IsNullOrEmpty(scriptingDefineSymbol))
            {
                return;
            }

            if (HasScriptingDefineSymbol(scriptingDefineSymbol))
            {
                return;
            }

            List<string> scriptingDefineSymbols = new List<string>(GetScriptingDefineSymbols())
            {
                scriptingDefineSymbol
            };
            SetScriptingDefineSymbols(scriptingDefineSymbols.ToArray());
        }

        /// <summary>
        /// 移除指定的脚本宏定义。
        /// </summary>
        /// <param name="scriptingDefineSymbol">要移除的脚本宏定义。</param>
        public static void RemoveScriptingDefineSymbol(string scriptingDefineSymbol)
        {
            if (string.IsNullOrEmpty(scriptingDefineSymbol))
            {
                return;
            }

            if (!HasScriptingDefineSymbol(scriptingDefineSymbol))
            {
                return;
            }

            List<string> scriptingDefineSymbols = new List<string>(GetScriptingDefineSymbols());
            while (scriptingDefineSymbols.Contains(scriptingDefineSymbol))
            {
                scriptingDefineSymbols.Remove(scriptingDefineSymbol);
            }

            SetScriptingDefineSymbols(scriptingDefineSymbols.ToArray());
        }

        /// <summary>
        /// 获取当前工程的脚本宏定义。
        /// </summary>
        /// <returns>脚本宏定义数组。</returns>
        public static string[] GetScriptingDefineSymbols()
        {
            string defineValue = LoadDefineConstantsValue();
            return defineValue
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// 设置当前工程的脚本宏定义。
        /// </summary>
        /// <param name="scriptingDefineSymbols">要设置的脚本宏定义。</param>
        public static void SetScriptingDefineSymbols(string[] scriptingDefineSymbols)
        {
            string[] symbols = scriptingDefineSymbols ?? Array.Empty<string>();
            string defineValue = string.Join(";",
                symbols
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.Ordinal));
            bool changed = SaveDefineConstantsValue(defineValue);
            if (changed)
            {
                TriggerSafeRecompile();
            }
        }

        /// <summary>
        /// 读取工程文件中的 DefineConstants 字符串。
        /// </summary>
        /// <returns>DefineConstants 原始字符串。</returns>
        private static string LoadDefineConstantsValue()
        {
            string csprojPath = GetCsprojPath();
            if (string.IsNullOrEmpty(csprojPath) || !File.Exists(csprojPath))
            {
                return string.Empty;
            }

            XDocument doc = XDocument.Load(csprojPath);
            return doc.Descendants("DefineConstants").FirstOrDefault()?.Value ?? string.Empty;
        }

        /// <summary>
        /// 将 DefineConstants 字符串写回工程文件。
        /// </summary>
        /// <param name="defineConstantsValue">要写入的 DefineConstants 内容。</param>
        /// <returns>是否发生了实际变更。</returns>
        private static bool SaveDefineConstantsValue(string defineConstantsValue)
        {
            string csprojPath = GetCsprojPath();
            if (string.IsNullOrEmpty(csprojPath))
            {
                return false;
            }

            XDocument doc;
            if (File.Exists(csprojPath))
            {
                doc = XDocument.Load(csprojPath);
            }
            else
            {
                doc = new XDocument(new XElement("Project", new XElement("PropertyGroup")));
            }

            XElement project = doc.Root;
            if (project == null)
            {
                return false;
            }

            XElement propertyGroup = project.Elements("PropertyGroup").FirstOrDefault();
            if (propertyGroup == null)
            {
                propertyGroup = new XElement("PropertyGroup");
                project.AddFirst(propertyGroup);
            }

            XElement defineNode = propertyGroup.Element("DefineConstants");
            if (defineNode == null)
            {
                defineNode = new XElement("DefineConstants");
                propertyGroup.Add(defineNode);
            }

            string targetValue = defineConstantsValue ?? string.Empty;
            if (string.Equals(defineNode.Value, targetValue, StringComparison.Ordinal))
            {
                return false;
            }

            defineNode.Value = targetValue;
            doc.Save(csprojPath);
            return true;
        }

        /// <summary>
        /// 通过更新 C# 标记文件安全触发 Godot 自动重编译。
        /// </summary>
        private static void TriggerSafeRecompile()
        {
            try
            {
                string stampFilePath = ProjectSettings.GlobalizePath(CompileTriggerStampFilePath);
                string stampValue = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                string content =
                    "// Auto-generated by ScriptingDefineSymbols. Do not edit.\n" +
                    "namespace GameFrameX.Editor\n" +
                    "{\n" +
                    "    /// <summary>\n" +
                    "    /// 编译触发标记文件。\n" +
                    "    /// </summary>\n" +
                    "    internal static class CompileTriggerStamp\n" +
                    "    {\n" +
                    $"        internal const string Value = \"{stampValue}\";\n" +
                    "    }\n" +
                    "}\n";
                File.WriteAllText(stampFilePath, content);
            }
            catch (Exception exception)
            {
                GD.PushWarning($"触发自动重编译失败: {exception.Message}");
            }
        }

        /// <summary>
        /// 获取 Godot.csproj 的绝对路径。
        /// </summary>
        /// <returns>工程文件绝对路径。</returns>
        private static string GetCsprojPath()
        {
            return ProjectSettings.GlobalizePath($"res://{CsprojFileName}");
        }
    }
}
#endif
