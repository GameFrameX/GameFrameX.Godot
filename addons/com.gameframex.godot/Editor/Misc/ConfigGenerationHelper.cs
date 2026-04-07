#if TOOLS
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 配置生成执行帮助类。
    /// </summary>
    internal static class ConfigGenerationHelper
    {
        private const string ClientBatchPath = "res://tools/Config/gen-client-json.bat";
        private const string ClientShellPath = "res://tools/Config/gen-client-json.sh";
        private const string OutputConfigDir = "res://Assets/Bundles/Config";
        private const string OutputCodeDir = "res://Assets/Hotfix/Config/Generate";

        /// <summary>
        /// 生成客户端配置，并验证导出目录。
        /// </summary>
        /// <param name="summary">执行摘要。</param>
        /// <returns>是否执行成功。</returns>
        public static bool GenerateClientJson(out string summary)
        {
            string projectRoot = NormalizeDirectory(ProjectSettings.GlobalizePath("res://"));
            string outputConfigDir = NormalizeDirectory(ProjectSettings.GlobalizePath(OutputConfigDir));
            string outputCodeDir = NormalizeDirectory(ProjectSettings.GlobalizePath(OutputCodeDir));
            Directory.CreateDirectory(outputConfigDir);
            Directory.CreateDirectory(outputCodeDir);

            try
            {
                ProcessStartInfo startInfo = BuildStartInfo(projectRoot);
                using Process process = Process.Start(startInfo);
                if (process == null)
                {
                    summary = "配置生成失败：无法启动生成进程。";
                    return false;
                }

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    GD.Print(stdout.TrimEnd());
                }

                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    GD.PrintErr(stderr.TrimEnd());
                }

                if (process.ExitCode != 0)
                {
                    summary = $"配置生成失败：进程退出码 {process.ExitCode}。";
                    return false;
                }

                int configFileCount = Directory.Exists(outputConfigDir)
                    ? Directory.EnumerateFiles(outputConfigDir, "*", SearchOption.AllDirectories).Count()
                    : 0;
                int codeFileCount = Directory.Exists(outputCodeDir)
                    ? Directory.EnumerateFiles(outputCodeDir, "*.cs", SearchOption.AllDirectories).Count()
                    : 0;

                if (codeFileCount <= 0)
                {
                    summary =
                        $"配置生成完成但代码产物校验失败：{outputCodeDir} .cs数={codeFileCount}。";
                    return false;
                }

                summary =
                    $"配置生成成功：{outputConfigDir} 文件数={configFileCount}, {outputCodeDir} .cs数={codeFileCount}。";
                return true;
            }
            catch (Exception exception)
            {
                summary = $"配置生成异常：{exception.Message}";
                return false;
            }
        }

        private static ProcessStartInfo BuildStartInfo(string projectRoot)
        {
            bool isWindows = OperatingSystem.IsWindows();
            string scriptPath = ProjectSettings.GlobalizePath(isWindows ? ClientBatchPath : ClientShellPath);
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"找不到生成脚本: {scriptPath}");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = projectRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (isWindows)
            {
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/c \"{scriptPath}\"";
            }
            else
            {
                startInfo.FileName = "/bin/bash";
                startInfo.Arguments = $"\"{scriptPath}\"";
            }

            return startInfo;
        }

        private static string NormalizeDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
#endif
