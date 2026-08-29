using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GameFrameX.Editor.Asmdef;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// asmdef -&gt; csproj 生成器测试（G2 门禁：内容映射正确性）。
    /// 备注：
    /// 1. 生成物落在临时目录（Path.GetTempPath + Guid），Dispose 时清理，不触碰项目树。
    /// 2. "生成 csproj 可被 dotnet build 正常解析"由阶段门禁命令对同一批生成物另行验证（见阶段报告）。
    /// </summary>
    public sealed class AsmdefCsprojGeneratorTests : IDisposable
    {
        private readonly string m_TempDir;

        public AsmdefCsprojGeneratorTests()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "gfx_asmdef_gen_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(m_TempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(m_TempDir))
            {
                Directory.Delete(m_TempDir, true);
            }
        }

        private AsmdefDocument WriteAsmdef(string relativeDirectory, string name, Action<AsmdefModel> customize = null)
        {
            string directory = Path.Combine(m_TempDir, relativeDirectory);
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, name + ".asmdef");
            var model = new AsmdefModel { Name = name, RootNamespace = name };
            customize?.Invoke(model);
            AsmdefIO.SaveDocument(filePath, model);
            return new AsmdefDocument { FilePath = filePath, Model = model };
        }

        private static XDocument LoadGenerated(string csprojPath)
        {
            return XDocument.Parse(File.ReadAllText(csprojPath));
        }

        private static XElement FirstProperty(XDocument document, string name)
        {
            return document.Root.Elements("PropertyGroup").Elements(name).First();
        }

        [Fact]
        public void GenerateAll_CreatesCsprojNextToAsmdef()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "ModuleA");

            List<AsmdefGenerateResult> results = AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            AsmdefGenerateResult result = Assert.Single(results);
            Assert.True(result.Written);
            Assert.Equal(Path.Combine(m_TempDir, "ModuleA", "ModuleA.csproj"), result.CsprojFilePath);
            Assert.True(File.Exists(result.CsprojFilePath));
        }

        [Fact]
        public void GenerateAll_CorePropertiesAreMapped()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "GameFrameX.Entity", model =>
            {
                model.RootNamespace = "GameFrameX.Entity.Nested";
                model.AllowUnsafeCode = true;
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            Assert.Equal("Microsoft.NET.Sdk", csproj.Root.Attribute("Sdk")?.Value);
            Assert.Equal("net8.0", FirstProperty(csproj, "TargetFramework").Value);
            Assert.Equal("GameFrameX.Entity.Nested", FirstProperty(csproj, "RootNamespace").Value);
            Assert.Equal("GameFrameX.Entity", FirstProperty(csproj, "AssemblyName").Value);
            Assert.Equal("true", FirstProperty(csproj, "AllowUnsafeBlocks").Value);
            // 显式 Compile 列表必须关闭默认 glob，否则 dotnet build 报 NETSDK1022
            Assert.Equal("false", FirstProperty(csproj, "EnableDefaultCompileItems").Value);
        }

        [Fact]
        public void GenerateAll_EmptyRootNamespace_FallsBackToName()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Fallback.Name", model =>
            {
                model.RootNamespace = " ";
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            Assert.Equal("Fallback.Name", FirstProperty(csproj, "RootNamespace").Value);
        }

        [Fact]
        public void GenerateAll_DefinesAreDedupedAndStablySorted()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Defines.Module", model =>
            {
                model.Defines.Add("GF_ZZZ");
                model.Defines.Add("GF_AAA");
                model.Defines.Add("GF_AAA");
                model.Defines.Add("  GF_MMM  ");
                model.Defines.Add(" ");
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            string defineConstants = FirstProperty(csproj, "DefineConstants").Value;
            Assert.Equal("$(DefineConstants);GF_AAA;GF_MMM;GF_ZZZ", defineConstants);
        }

        [Fact]
        public void GenerateAll_EditorOnly_AddsEditorOnlyDefine()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Editor.Only", model =>
            {
                model.EditorOnly = true;
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            Assert.Contains("ASMDEF_EDITOR_ONLY", FirstProperty(csproj, "DefineConstants").Value);
        }

        [Fact]
        public void GenerateAll_IncludePlatformsEditorOnly_AddsEditorOnlyDefine()
        {
            // Unity 兼容路径：includePlatforms 仅含 Editor 时按 EditorOnly 处理
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Platform.EditorOnly", model =>
            {
                model.IncludePlatforms.Add("Editor");
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            Assert.Contains("ASMDEF_EDITOR_ONLY", FirstProperty(csproj, "DefineConstants").Value);
        }

        [Fact]
        public void GenerateAll_ProjectReferencesUseSortedRelativePaths()
        {
            AsmdefDocument core = WriteAsmdef("Core", "GameFrameX.Core");
            AsmdefDocument entity = WriteAsmdef("Entity", "GameFrameX.Entity", model =>
            {
                model.References.Add("GameFrameX.Zzz");
                model.References.Add("GameFrameX.Core");
            });
            AsmdefDocument zzz = WriteAsmdef("Zzz", "GameFrameX.Zzz");

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { core, entity, zzz });

            XDocument csproj = LoadGenerated(entity.FilePath.Replace(".asmdef", ".csproj"));
            List<string> includes = csproj.Root.Elements("ItemGroup")
                .Elements("ProjectReference")
                .Select(x => x.Attribute("Include")?.Value)
                .ToList();
            Assert.Equal(new List<string> { "../Core/GameFrameX.Core.csproj", "../Zzz/GameFrameX.Zzz.csproj" }, includes);
        }

        [Fact]
        public void GenerateAll_MissingReference_IsSkipped()
        {
            AsmdefDocument entity = WriteAsmdef("Entity", "GameFrameX.Entity", model =>
            {
                model.References.Add("GameFrameX.NotExist");
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { entity });

            XDocument csproj = LoadGenerated(entity.FilePath.Replace(".asmdef", ".csproj"));
            Assert.DoesNotContain(csproj.Root.Elements(), x => x.Name == "ProjectReference" || x.Elements("ProjectReference").Any());
        }

        [Fact]
        public void GenerateAll_PlatformDefines_EmitConditionedPropertyGroups()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Platform.Module", model =>
            {
                model.PlatformDefines["android"] = new List<string> { "GF_ANDROID" };
                model.PlatformDefines["windows"] = new List<string> { "GF_WIN" };
                model.PlatformDefines["unknown-platform"] = new List<string> { "GF_UNKNOWN" };
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            List<XElement> conditioned = csproj.Root.Elements("PropertyGroup")
                .Where(x => x.Attribute("Condition") != null)
                .ToList();
            // 未知平台被忽略，只生成已知平台（android / windows）两组，且按平台键稳定排序（android < windows）
            Assert.Equal(2, conditioned.Count);
            Assert.Equal("'$(GodotTargetPlatform)' == 'android'", conditioned[0].Attribute("Condition")?.Value);
            Assert.Contains("GF_ANDROID", conditioned[0].Element("DefineConstants").Value);
            Assert.Equal("'$(OS)' == 'Windows_NT'", conditioned[1].Attribute("Condition")?.Value);
            Assert.Contains("GF_WIN", conditioned[1].Element("DefineConstants").Value);
        }

        [Fact]
        public void GenerateAll_VersionDefines_MergeIntoCommonDefines()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Version.Module", model =>
            {
                model.VersionDefines.Add(new AsmdefVersionDefine { Name = "x", Expression = "1", Define = "GF_VERSIONED" });
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            Assert.Contains("GF_VERSIONED", FirstProperty(csproj, "DefineConstants").Value);
        }

        [Fact]
        public void GenerateAll_NoDefines_OmitsDefineConstants()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Plain.Module");

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });

            XDocument csproj = LoadGenerated(doc.FilePath.Replace(".asmdef", ".csproj"));
            Assert.DoesNotContain(csproj.Root.Elements("PropertyGroup").Elements(), x => x.Name == "DefineConstants");
        }

        [Fact]
        public void GenerateAll_Gate_GeneratedCsprojBuildsUnderDotnet()
        {
            // G2 门禁：生成物可被 dotnet build 正常解析。
            // 双模块验证 ProjectReference 链路：Gate.App 代码直接调用 Gate.Core 类型，编译失败即门禁失败。
            AsmdefDocument core = WriteAsmdef("GateCore", "Gate.Core");
            AsmdefDocument app = WriteAsmdef("GateApp", "Gate.App", model =>
            {
                model.References.Add("Gate.Core");
                model.Defines.Add("GF_GATE");
            });

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { core, app });

            string coreDir = Path.GetDirectoryName(core.FilePath);
            File.WriteAllText(Path.Combine(coreDir, "GateCoreMarker.cs"),
                "namespace Gate.Core { public static class GateCoreMarker { public const string Value = \"core\"; } }\n");
            string appDir = Path.GetDirectoryName(app.FilePath);
            File.WriteAllText(Path.Combine(appDir, "GateAppMarker.cs"),
                "namespace Gate.App { public static class GateAppMarker { public static string Probe() { return Gate.Core.GateCoreMarker.Value; } } }\n");

            string csprojPath = app.FilePath.Replace(".asmdef", ".csproj");
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build \"" + csprojPath + "\" --nologo -v quiet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                process.WaitForExit(120000);
                Assert.True(process.ExitCode == 0, "生成 csproj dotnet build 失败：" + output);
            }
        }
    }
}
