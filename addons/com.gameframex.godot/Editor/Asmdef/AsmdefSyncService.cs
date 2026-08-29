// 说明：纯 BCL 逻辑，不包 #if TOOLS，供单元测试触达（同 AsmdefModel.cs）。
// asmdef 文件发现与时间源通过构造函数注入：编辑器侧注入 AsmdefPathUtility.FindAllAsmdefFiles，
// 单元测试注入 lambda 与可控时钟，避免依赖 Godot native API。
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameFrameX.Editor.Asmdef
{
    public sealed class AsmdefSyncSummary
    {
        public int TotalAsmdefCount { get; set; }
        public int GeneratedCsprojCount { get; set; }
        public int UpdatedCsprojCount { get; set; }
        public List<AsmdefValidationIssue> Issues { get; } = new List<AsmdefValidationIssue>();
        public bool HasError => Issues.Any(x => x.Severity == AsmdefIssueSeverity.Error);
    }

    public sealed class AsmdefSyncService
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan DebounceDuration = TimeSpan.FromMilliseconds(400);
        private readonly Func<List<string>> m_FileFinder;
        private readonly Func<DateTime> m_Clock;
        private readonly Dictionary<string, DateTime> m_FileWriteSnapshot = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> m_PendingChanges = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> m_LastGeneratedCsprojByAsmdef = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private DateTime m_LastScanAtUtc = DateTime.MinValue;
        private bool m_Initialized;
        private Action<AsmdefSyncSummary> m_OnSynced;

        /// <param name="fileFinder">asmdef 文件发现器（编辑器侧传 AsmdefPathUtility.FindAllAsmdefFiles）。</param>
        /// <param name="clock">时间源（默认 DateTime.UtcNow；测试注入可控时钟）。</param>
        public AsmdefSyncService(Func<List<string>> fileFinder, Func<DateTime> clock = null)
        {
            m_FileFinder = fileFinder ?? throw new ArgumentNullException(nameof(fileFinder));
            m_Clock = clock ?? (Func<DateTime>)(() => DateTime.UtcNow);
        }

        public void SetCallback(Action<AsmdefSyncSummary> callback)
        {
            m_OnSynced = callback;
        }

        public void Tick()
        {
            DateTime now = m_Clock();
            if (now - m_LastScanAtUtc < ScanInterval)
            {
                return;
            }

            m_LastScanAtUtc = now;
            ScanForChanges(now);
            if (!m_Initialized)
            {
                m_Initialized = true;
                RunSync();
                // 初始化扫描把存量文件记入了 pending；全量同步已覆盖它们，清掉避免
                // 下一次跨防抖窗口的 Tick 触发一次多余的空同步
                m_PendingChanges.Clear();
                return;
            }

            if (m_PendingChanges.Count == 0)
            {
                return;
            }

            DateTime oldestChange = m_PendingChanges.Values.Min();
            if (now - oldestChange >= DebounceDuration)
            {
                RunSync();
                m_PendingChanges.Clear();
            }
        }

        public AsmdefSyncSummary RunSync()
        {
            var summary = new AsmdefSyncSummary();
            List<AsmdefDocument> documents = LoadAllDocuments(summary.Issues);
            summary.TotalAsmdefCount = documents.Count;

            AsmdefValidationResult validation = AsmdefValidator.Validate(documents);
            summary.Issues.AddRange(validation.Issues);
            if (validation.HasError)
            {
                Notify(summary);
                return summary;
            }

            List<AsmdefGenerateResult> generateResults = AsmdefCsprojGenerator.GenerateAll(documents);
            summary.GeneratedCsprojCount = generateResults.Count;
            summary.UpdatedCsprojCount = generateResults.Count(x => x.Written);
            DeleteStaleGeneratedCsproj(generateResults.Select(x => x.AsmdefFilePath));
            UpdateGeneratedMap(generateResults);
            Notify(summary);
            return summary;
        }

        public void MarkDirty(string asmdefFilePath)
        {
            if (string.IsNullOrWhiteSpace(asmdefFilePath))
            {
                return;
            }

            m_PendingChanges[asmdefFilePath] = m_Clock();
        }

        private void Notify(AsmdefSyncSummary summary)
        {
            m_OnSynced?.Invoke(summary);
        }

        private List<AsmdefDocument> LoadAllDocuments(List<AsmdefValidationIssue> issues)
        {
            var documents = new List<AsmdefDocument>();
            foreach (string asmdefPath in m_FileFinder())
            {
                try
                {
                    documents.Add(AsmdefIO.LoadDocument(asmdefPath));
                }
                catch (Exception exception)
                {
                    issues.Add(new AsmdefValidationIssue
                    {
                        Severity = AsmdefIssueSeverity.Error,
                        FilePath = asmdefPath,
                        Message = $"读取 asmdef 失败：{exception.Message}"
                    });
                }
            }

            return documents;
        }

        private void ScanForChanges(DateTime now)
        {
            List<string> files = m_FileFinder();
            var currentSet = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);

            foreach (string file in files)
            {
                DateTime lastWrite = GetLastWriteTimeUtc(file);
                if (!m_FileWriteSnapshot.TryGetValue(file, out DateTime oldWrite))
                {
                    m_FileWriteSnapshot[file] = lastWrite;
                    m_PendingChanges[file] = now;
                    continue;
                }

                if (lastWrite != oldWrite)
                {
                    m_FileWriteSnapshot[file] = lastWrite;
                    m_PendingChanges[file] = now;
                }
            }

            foreach (string oldFile in m_FileWriteSnapshot.Keys.ToList())
            {
                if (currentSet.Contains(oldFile))
                {
                    continue;
                }

                m_FileWriteSnapshot.Remove(oldFile);
                m_PendingChanges[oldFile] = now;
            }
        }

        private void DeleteStaleGeneratedCsproj(IEnumerable<string> currentAsmdefs)
        {
            var currentSet = new HashSet<string>(currentAsmdefs ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            foreach (string asmdefPath in m_LastGeneratedCsprojByAsmdef.Keys.ToList())
            {
                if (currentSet.Contains(asmdefPath))
                {
                    continue;
                }

                if (m_LastGeneratedCsprojByAsmdef.TryGetValue(asmdefPath, out string csprojPath) && File.Exists(csprojPath))
                {
                    try
                    {
                        File.Delete(csprojPath);
                    }
                    catch
                    {
                        // 文件删除失败时保持静默，避免影响主流程
                    }
                }

                m_LastGeneratedCsprojByAsmdef.Remove(asmdefPath);
            }
        }

        private void UpdateGeneratedMap(IEnumerable<AsmdefGenerateResult> results)
        {
            foreach (AsmdefGenerateResult result in results)
            {
                m_LastGeneratedCsprojByAsmdef[result.AsmdefFilePath] = result.CsprojFilePath;
            }
        }

        private static DateTime GetLastWriteTimeUtc(string filePath)
        {
            try
            {
                return File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
