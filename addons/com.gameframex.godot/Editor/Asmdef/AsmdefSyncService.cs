#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GameFrameX.Editor.Asmdef
{
    public sealed class AsmdefSyncSummary
    {
        public int TotalAsmdefCount { get; set; }
        public int GeneratedCsprojCount { get; set; }
        public int UpdatedCsprojCount { get; set; }
        public List<AsmdefValidationIssue> Issues { get; } = new List<AsmdefValidationIssue>();
        public bool HasError => Issues.Any(static x => x.Severity == AsmdefIssueSeverity.Error);
    }

    public sealed class AsmdefSyncService
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan DebounceDuration = TimeSpan.FromMilliseconds(400);
        private readonly Dictionary<string, DateTime> m_FileWriteSnapshot = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> m_PendingChanges = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> m_LastGeneratedCsprojByAsmdef = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private DateTime m_LastScanAtUtc = DateTime.MinValue;
        private bool m_Initialized;
        private Action<AsmdefSyncSummary> m_OnSynced;

        public void SetCallback(Action<AsmdefSyncSummary> callback)
        {
            m_OnSynced = callback;
        }

        public void Tick()
        {
            DateTime now = DateTime.UtcNow;
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
            summary.UpdatedCsprojCount = generateResults.Count(static x => x.Written);
            DeleteStaleGeneratedCsproj(generateResults.Select(static x => x.AsmdefFilePath));
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

            m_PendingChanges[asmdefFilePath] = DateTime.UtcNow;
        }

        private void Notify(AsmdefSyncSummary summary)
        {
            m_OnSynced?.Invoke(summary);
        }

        private static List<AsmdefDocument> LoadAllDocuments(List<AsmdefValidationIssue> issues)
        {
            var documents = new List<AsmdefDocument>();
            foreach (string asmdefPath in AsmdefPathUtility.FindAllAsmdefFiles())
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
            List<string> files = AsmdefPathUtility.FindAllAsmdefFiles();
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
#endif
