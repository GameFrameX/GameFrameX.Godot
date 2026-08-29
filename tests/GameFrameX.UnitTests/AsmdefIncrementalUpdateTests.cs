using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFrameX.Editor.Asmdef;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// asmdef 生成器增量更新策略测试（G3 门禁：同一输入重复生成不产生额外文件差异）。
    /// 增量语义：文件内容不变时不重写（mtime 不动），引用增删改后仅受影响模块的 csproj 更新。
    /// </summary>
    public sealed class AsmdefIncrementalUpdateTests : IDisposable
    {
        private readonly string m_TempDir;

        public AsmdefIncrementalUpdateTests()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "gfx_asmdef_inc_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(m_TempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(m_TempDir))
            {
                Directory.Delete(m_TempDir, true);
            }
        }

        private AsmdefDocument WriteAsmdef(string relativeDirectory, string name, params string[] references)
        {
            string directory = Path.Combine(m_TempDir, relativeDirectory);
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, name + ".asmdef");
            var model = new AsmdefModel { Name = name, RootNamespace = name };
            foreach (string reference in references)
            {
                model.References.Add(reference);
            }

            AsmdefIO.SaveDocument(filePath, model);
            return new AsmdefDocument { FilePath = filePath, Model = model };
        }

        private static string CsprojPathOf(AsmdefDocument doc)
        {
            return doc.FilePath.Replace(".asmdef", ".csproj");
        }

        [Fact]
        public void GenerateAll_TwiceOnSameInput_SecondRunWritesNothing()
        {
            AsmdefDocument a = WriteAsmdef("ModuleA", "Inc.A");
            AsmdefDocument b = WriteAsmdef("ModuleB", "Inc.B", "Inc.A");

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { a, b });
            DateTime aWriteTime = File.GetLastWriteTimeUtc(CsprojPathOf(a));
            DateTime bWriteTime = File.GetLastWriteTimeUtc(CsprojPathOf(b));
            string aContent = File.ReadAllText(CsprojPathOf(a));
            string bContent = File.ReadAllText(CsprojPathOf(b));

            List<AsmdefGenerateResult> secondRun = AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { a, b });

            Assert.All(secondRun, result => Assert.False(result.Written));
            Assert.Equal(aWriteTime, File.GetLastWriteTimeUtc(CsprojPathOf(a)));
            Assert.Equal(bWriteTime, File.GetLastWriteTimeUtc(CsprojPathOf(b)));
            Assert.Equal(aContent, File.ReadAllText(CsprojPathOf(a)));
            Assert.Equal(bContent, File.ReadAllText(CsprojPathOf(b)));
        }

        [Fact]
        public void GenerateAll_DefineOrderingIsStableAcrossRuns()
        {
            AsmdefDocument doc = WriteAsmdef("ModuleA", "Inc.Sort");
            doc.Model.Defines.Add("GF_Z");
            doc.Model.Defines.Add("GF_A");
            doc.Model.Defines.Add("GF_Z");
            doc.Model.Defines.Add("GF_M");

            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });
            string first = File.ReadAllText(CsprojPathOf(doc));
            doc.Model.Defines.Reverse();
            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { doc });
            string second = File.ReadAllText(CsprojPathOf(doc));

            // 输入顺序变化（含重复项）不影响生成结果：去重 + 稳定排序
            Assert.Equal(first, second);
            Assert.Contains("GF_A;GF_M;GF_Z", second);
        }

        [Fact]
        public void GenerateAll_ReferenceRemoved_ProjectReferenceDropped()
        {
            AsmdefDocument a = WriteAsmdef("ModuleA", "Inc.Ref");
            AsmdefDocument b = WriteAsmdef("ModuleB", "Inc.Dep", "Inc.Ref");
            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { a, b });
            Assert.Contains("../ModuleA/Inc.Ref.csproj", File.ReadAllText(CsprojPathOf(b)));

            // 增量：b 去掉对 Inc.Ref 的引用后重新生成
            b.Model.References.Clear();
            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { a, b });

            string bContent = File.ReadAllText(CsprojPathOf(b));
            Assert.DoesNotContain("Inc.Ref.csproj", bContent);
            Assert.DoesNotContain("<ProjectReference", bContent);
        }

        [Fact]
        public void GenerateAll_ReferenceSwapped_ProjectReferenceUpdated()
        {
            AsmdefDocument a = WriteAsmdef("ModuleA", "Inc.Old");
            AsmdefDocument c = WriteAsmdef("ModuleC", "Inc.New");
            AsmdefDocument b = WriteAsmdef("ModuleB", "Inc.Swap", "Inc.Old");
            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { a, b, c });

            b.Model.References.Clear();
            b.Model.References.Add("Inc.New");
            List<AsmdefGenerateResult> results = AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { a, b, c });

            // 只有 b 的 csproj 内容变化；a、c 不重写
            AsmdefGenerateResult bResult = Assert.Single(results.Where(x => x.Written));
            Assert.Equal(b.FilePath, bResult.AsmdefFilePath);
            string bContent = File.ReadAllText(CsprojPathOf(b));
            Assert.Contains("../ModuleC/Inc.New.csproj", bContent);
            Assert.DoesNotContain("Inc.Old.csproj", bContent);
        }

        [Fact]
        public void GenerateAll_UnrelatedModuleCsprojUntouched()
        {
            AsmdefDocument stable = WriteAsmdef("Stable", "Inc.Stable");
            AsmdefDocument changed = WriteAsmdef("Changed", "Inc.Changed");
            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { stable, changed });
            DateTime stableWriteTime = File.GetLastWriteTimeUtc(CsprojPathOf(stable));

            changed.Model.Defines.Add("GF_NEW_DEFINE");
            AsmdefCsprojGenerator.GenerateAll(new List<AsmdefDocument> { stable, changed });

            // 无关模块的 csproj 不产生任何文件差异（G3 门禁：避免无关 diff）
            Assert.Equal(stableWriteTime, File.GetLastWriteTimeUtc(CsprojPathOf(stable)));
        }
    }
}
