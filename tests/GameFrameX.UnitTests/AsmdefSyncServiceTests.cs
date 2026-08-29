using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFrameX.Editor.Asmdef;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// asmdef 实时更新链路测试（G5 门禁：保存 asmdef 后 1 秒内完成 csproj 更新）。
    /// 时间预算说明：扫描间隔 500ms + 防抖 400ms = 900ms，叠加编辑器帧粒度（约 16ms）小于 1 秒。
    /// 备注：文件源与时钟均为注入实现，不触碰 Godot native API；文件 IO 落在临时目录。
    /// </summary>
    public sealed class AsmdefSyncServiceTests : IDisposable
    {
        private sealed class ManualClock
        {
            public DateTime NowUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public DateTime Now()
            {
                return NowUtc;
            }
        }

        private readonly string m_TempDir;
        private readonly ManualClock m_Clock;
        private readonly List<string> m_FileList;
        private readonly List<AsmdefSyncSummary> m_Summaries;
        private readonly AsmdefSyncService m_Service;

        public AsmdefSyncServiceTests()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "gfx_asmdef_sync_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(m_TempDir);
            m_Clock = new ManualClock();
            m_FileList = new List<string>();
            m_Summaries = new List<AsmdefSyncSummary>();
            m_Service = new AsmdefSyncService(() => m_FileList.ToList(), m_Clock.Now);
            m_Service.SetCallback(m_Summaries.Add);
        }

        public void Dispose()
        {
            if (Directory.Exists(m_TempDir))
            {
                Directory.Delete(m_TempDir, true);
            }
        }

        private string WriteAsmdefFile(string name, params string[] references)
        {
            string filePath = Path.Combine(m_TempDir, name + ".asmdef");
            var model = new AsmdefModel { Name = name, RootNamespace = name };
            foreach (string reference in references)
            {
                model.References.Add(reference);
            }

            AsmdefIO.SaveDocument(filePath, model);
            m_FileList.Add(filePath);
            return filePath;
        }

        private void RewriteAsmdef(string filePath, Action<AsmdefModel> mutate)
        {
            AsmdefDocument document = AsmdefIO.LoadDocument(filePath);
            mutate(document.Model);
            AsmdefIO.SaveDocument(filePath, document.Model);
            // 显式拉开写入时间，规避文件系统时间戳粒度导致的漏检
            File.SetLastWriteTimeUtc(filePath, m_Clock.NowUtc.AddSeconds(1));
        }

        private string CsprojPathOf(string asmdefPath)
        {
            return asmdefPath.Replace(".asmdef", ".csproj");
        }

        [Fact]
        public void Tick_Initial_RunsSyncAndGeneratesCsproj()
        {
            string a = WriteAsmdefFile("Sync.ModuleA");

            m_Service.Tick();

            Assert.True(File.Exists(CsprojPathOf(a)));
            AsmdefSyncSummary summary = Assert.Single(m_Summaries);
            Assert.Equal(1, summary.TotalAsmdefCount);
            Assert.Equal(1, summary.GeneratedCsprojCount);
            Assert.False(summary.HasError);
        }

        [Fact]
        public void Tick_WithinScanInterval_DoesNothing()
        {
            WriteAsmdefFile("Sync.Quiet");
            m_Service.Tick();
            Assert.Single(m_Summaries);

            // 距上次扫描不足 500ms：Tick 直接返回，不产生新的同步
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(200);
            m_Service.Tick();

            Assert.Single(m_Summaries);
        }

        [Fact]
        public void Tick_NoChange_NoResync()
        {
            WriteAsmdefFile("Sync.Stable");
            m_Service.Tick();

            // 足够跨过扫描间隔，但无任何文件变化：不触发同步
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(600);
            m_Service.Tick();
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(600);
            m_Service.Tick();

            Assert.Single(m_Summaries);
        }

        [Fact]
        public void Tick_SavedAsmdef_UpdatesCsprojWithinOneSecond()
        {
            string a = WriteAsmdefFile("Sync.Core");
            string b = WriteAsmdefFile("Sync.App");
            m_Service.Tick();
            Assert.True(File.Exists(CsprojPathOf(b)));

            // 模拟属性编辑器保存：改内容 + 编辑器回调 MarkDirty（T0 = 1000ms）
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(1000);
            RewriteAsmdef(b, model =>
            {
                model.References.Add("Sync.Core");
                model.Defines.Add("GF_SYNCED");
            });
            m_Service.MarkDirty(b);

            // 防抖窗口内（保存后 16ms 的编辑器帧）：跨不过扫描间隔，不更新
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(16);
            m_Service.Tick();
            string earlyContent = File.ReadAllText(CsprojPathOf(b));
            Assert.DoesNotContain("GF_SYNCED", earlyContent);

            // T0 + 520ms：跨过扫描间隔（500ms）与防抖窗口（400ms）→ 增量更新完成
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(504);
            m_Service.Tick();

            string updated = File.ReadAllText(CsprojPathOf(b));
            Assert.Contains("GF_SYNCED", updated);
            Assert.Contains("Sync.Core.csproj", updated.Replace('\\', '/'));
            // 门禁：保存（T0=1000ms）到更新完成（T=1520ms）模拟耗时 520ms < 1000ms
            AsmdefSyncSummary last = m_Summaries[m_Summaries.Count - 1];
            Assert.Equal(1, last.UpdatedCsprojCount);
        }

        [Fact]
        public void Tick_ExternalEdit_PickedUpByScanWithinOneSecond()
        {
            string a = WriteAsmdefFile("Scan.Module");
            m_Service.Tick();

            // 模拟外部编辑器改文件（无 MarkDirty）：扫描在 500ms 内发现，再过 400ms 防抖后更新
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(600);
            RewriteAsmdef(a, model =>
            {
                model.Defines.Add("GF_EXTERNAL");
            });
            m_Service.Tick();
            // 扫描已发现变更（pending），但防抖窗口内不生成
            Assert.DoesNotContain("GF_EXTERNAL", File.ReadAllText(CsprojPathOf(a)));

            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(500);
            m_Service.Tick();

            Assert.Contains("GF_EXTERNAL", File.ReadAllText(CsprojPathOf(a)));
            // 外部编辑时刻 600ms → 更新完成 1100ms：总预算 = 扫描 500ms + 防抖 400ms + 帧粒度，< 1s 达标
        }

        [Fact]
        public void Tick_ValidationError_BlocksGenerationAndReports()
        {
            string a = WriteAsmdefFile("Cyc.A", "Cyc.B");
            string b = WriteAsmdefFile("Cyc.B", "Cyc.A");

            m_Service.Tick();

            // 循环依赖：校验失败阻止生成，两个 csproj 均不落盘
            Assert.False(File.Exists(CsprojPathOf(a)));
            Assert.False(File.Exists(CsprojPathOf(b)));
            AsmdefSyncSummary summary = Assert.Single(m_Summaries);
            Assert.True(summary.HasError);
            Assert.Contains(summary.Issues, x => x.Message.Contains("循环依赖"));
        }

        [Fact]
        public void Tick_Debounce_SuppressesRapidRepeatedSaves()
        {
            string a = WriteAsmdefFile("Debounce.Module");
            m_Service.Tick();

            // 连续多次保存（间隔 < 防抖 400ms）：只在静默期后同步一次，且为最终内容
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(600);
            RewriteAsmdef(a, model =>
            {
                model.Defines.Add("GF_FIRST");
            });
            m_Service.MarkDirty(a);
            m_Service.Tick(); // T=600 扫描发现第一次保存 → pending=600，防抖窗口内
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(100);
            RewriteAsmdef(a, model =>
            {
                model.Defines.Add("GF_SECOND");
            });
            m_Service.MarkDirty(a); // T=700 第二次保存，pending 刷新

            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(500);
            m_Service.Tick(); // T=1200：扫描发现第二次变更 → pending 刷新为 1200，防抖窗口重置
            Assert.DoesNotContain("GF_SECOND", File.ReadAllText(CsprojPathOf(a)));

            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(500);
            m_Service.Tick(); // T=1700：静默超 400ms → 同步一次（最终内容）
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(600);
            m_Service.Tick(); // 无新变更：不再同步

            string content = File.ReadAllText(CsprojPathOf(a));
            Assert.Contains("GF_FIRST", content);
            Assert.Contains("GF_SECOND", content);
            int syncCountAfterInitial = m_Summaries.Count - 1;
            Assert.Equal(1, syncCountAfterInitial);
        }

        [Fact]
        public void Tick_AsmdefDeleted_RemovesStaleCsproj()
        {
            string a = WriteAsmdefFile("Gone.Module");
            m_Service.Tick();
            Assert.True(File.Exists(CsprojPathOf(a)));

            File.Delete(a);
            m_FileList.Clear();
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(600);
            m_Service.Tick(); // 扫描发现删除 → pending
            m_Clock.NowUtc = m_Clock.NowUtc.AddMilliseconds(500);
            m_Service.Tick(); // 防抖到期 → 清理陈旧 csproj

            Assert.False(File.Exists(CsprojPathOf(a)));
        }
    }
}
