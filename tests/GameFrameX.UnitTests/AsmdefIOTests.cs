using System;
using System.IO;
using GameFrameX.Editor.Asmdef;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// asmdef 模型与读取器测试（G0 骨架门禁）。
    /// 备注：
    /// 1. 合法样例取自实施计划 4.2 节示例；非法样例覆盖坏 JSON 与空文件回退。
    /// 2. 纯 BCL 逻辑（System.Text.Json），不构造任何 Godot native 对象。
    /// 3. 文件 IO 全部落在临时目录（Path.GetTempPath + Guid），Dispose 时清理。
    /// </summary>
    public sealed class AsmdefIOTests : IDisposable
    {
        private readonly string m_TempDir;

        public AsmdefIOTests()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "gfx_asmdef_io_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(m_TempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(m_TempDir))
            {
                Directory.Delete(m_TempDir, true);
            }
        }

        private string TempFile(string fileName)
        {
            return Path.Combine(m_TempDir, fileName);
        }

        [Fact]
        public void LoadDocument_ValidSample_MapsAllFields()
        {
            string filePath = TempFile("GameFrameX.Entity.asmdef");
            File.WriteAllText(filePath, "{\n  \"name\": \"GameFrameX.Entity\",\n  \"rootNamespace\": \"GameFrameX.Entity\",\n  \"references\": [\"GameFrameX.Core\"],\n  \"defines\": [\"GF_ENTITY\", \"GF_DEBUG_VIEW\"],\n  \"platformDefines\": {\n    \"windows\": [\"GF_WIN\"],\n    \"android\": [\"GF_ANDROID\"]\n  },\n  \"editorOnly\": false\n}\n");

            AsmdefDocument document = AsmdefIO.LoadDocument(filePath);

            Assert.Equal(filePath, document.FilePath);
            Assert.Equal("GameFrameX.Entity", document.Model.Name);
            Assert.Equal("GameFrameX.Entity", document.Model.RootNamespace);
            Assert.Equal(new[] { "GameFrameX.Core" }, document.Model.References);
            Assert.Equal(new[] { "GF_ENTITY", "GF_DEBUG_VIEW" }, document.Model.Defines);
            Assert.Equal(new[] { "GF_WIN" }, document.Model.PlatformDefines["windows"]);
            Assert.Equal(new[] { "GF_ANDROID" }, document.Model.PlatformDefines["android"]);
            Assert.False(document.Model.EditorOnly);
        }

        [Fact]
        public void LoadDocument_UnityCompatSample_MapsCompatibilityFields()
        {
            // Hotfix.asmdef 的存量形态：Unity 兼容字段 + 扩展字段并存
            string filePath = TempFile("Hotfix.asmdef");
            File.WriteAllText(filePath, "{\n  \"name\": \"Hotfix\",\n  \"rootNamespace\": \"Hotfix\",\n  \"references\": [],\n  \"includePlatforms\": [],\n  \"excludePlatforms\": [],\n  \"allowUnsafeCode\": false,\n  \"overrideReferences\": false,\n  \"precompiledReferences\": [],\n  \"autoReferenced\": true,\n  \"defineConstraints\": [],\n  \"versionDefines\": [],\n  \"noEngineReferences\": false,\n  \"defines\": [],\n  \"platformDefines\": {},\n  \"editorOnly\": false\n}\n");

            AsmdefDocument document = AsmdefIO.LoadDocument(filePath);

            Assert.Equal("Hotfix", document.Model.Name);
            Assert.True(document.Model.AutoReferenced);
            Assert.False(document.Model.AllowUnsafeCode);
            Assert.False(document.Model.NoEngineReferences);
            Assert.Empty(document.Model.References);
            Assert.Empty(document.Model.PlatformDefines);
        }

        [Fact]
        public void LoadDocument_EmptyFile_FallsBackToFileNameAndWritesBack()
        {
            string filePath = TempFile("Empty.Module.asmdef");
            File.WriteAllText(filePath, string.Empty);

            AsmdefDocument document = AsmdefIO.LoadDocument(filePath);

            Assert.Equal("Empty.Module", document.Model.Name);
            Assert.Equal("Empty.Module", document.Model.RootNamespace);
            // 空文件首次加载会自动写回完整 JSON，后续不再被当作无效文件
            Assert.True(File.Exists(filePath));
            string rewritten = File.ReadAllText(filePath);
            Assert.Contains("\"Empty.Module\"", rewritten);
        }

        [Fact]
        public void LoadDocument_MalformedJson_Throws()
        {
            string filePath = TempFile("Broken.asmdef");
            File.WriteAllText(filePath, "{ this is not json ]");

            Assert.ThrowsAny<Exception>(() => AsmdefIO.LoadDocument(filePath));
        }

        [Fact]
        public void SaveAndReload_RoundTripsExtensionFields()
        {
            string filePath = TempFile("RoundTrip.asmdef");
            var model = new AsmdefModel
            {
                Name = "RoundTrip.Module",
                RootNamespace = "RoundTrip",
                EditorOnly = true
            };
            model.References.Add("GameFrameX.Core");
            model.Defines.Add("GF_RT");
            model.PlatformDefines["windows"] = new System.Collections.Generic.List<string> { "GF_WIN_RT" };

            AsmdefIO.SaveDocument(filePath, model);
            AsmdefDocument reloaded = AsmdefIO.LoadDocument(filePath);

            Assert.Equal("RoundTrip.Module", reloaded.Model.Name);
            Assert.Equal("RoundTrip", reloaded.Model.RootNamespace);
            Assert.True(reloaded.Model.EditorOnly);
            Assert.Equal(new[] { "GameFrameX.Core" }, reloaded.Model.References);
            Assert.Equal(new[] { "GF_RT" }, reloaded.Model.Defines);
            Assert.Equal(new[] { "GF_WIN_RT" }, reloaded.Model.PlatformDefines["windows"]);
        }

        [Fact]
        public void SaveDocument_EndsWithNewLine()
        {
            string filePath = TempFile("NewLine.asmdef");
            AsmdefIO.SaveDocument(filePath, new AsmdefModel { Name = "NewLine" });

            string content = File.ReadAllText(filePath);
            Assert.EndsWith("\n", content, StringComparison.Ordinal);
        }

        [Fact]
        public void WriteTextIfChanged_SameContent_ReturnsFalseWithoutWrite()
        {
            string filePath = TempFile("Idempotent.txt");
            bool firstWrite = AsmdefIO.WriteTextIfChanged(filePath, "stable content");
            DateTime firstWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);

            bool secondWrite = AsmdefIO.WriteTextIfChanged(filePath, "stable content");

            Assert.True(firstWrite);
            Assert.False(secondWrite);
            Assert.Equal(firstWriteTimeUtc, File.GetLastWriteTimeUtc(filePath));
        }

        [Fact]
        public void WriteTextIfChanged_DifferentContent_ReturnsTrue()
        {
            string filePath = TempFile("Changed.txt");
            AsmdefIO.WriteTextIfChanged(filePath, "old");

            bool changed = AsmdefIO.WriteTextIfChanged(filePath, "new");

            Assert.True(changed);
            Assert.Equal("new", File.ReadAllText(filePath));
        }

        [Fact]
        public void LoadDocument_EmptyPath_Throws()
        {
            Assert.Throws<ArgumentException>(() => AsmdefIO.LoadDocument(string.Empty));
        }
    }
}
