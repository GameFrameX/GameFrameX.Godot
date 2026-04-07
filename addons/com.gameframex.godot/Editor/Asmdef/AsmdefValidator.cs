#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameFrameX.Editor.Asmdef
{
    public enum AsmdefIssueSeverity
    {
        Warning = 0,
        Error = 1
    }

    public sealed class AsmdefValidationIssue
    {
        public AsmdefIssueSeverity Severity { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class AsmdefValidationResult
    {
        public List<AsmdefValidationIssue> Issues { get; } = new List<AsmdefValidationIssue>();
        public bool HasError => Issues.Any(static x => x.Severity == AsmdefIssueSeverity.Error);
    }

    public static class AsmdefValidator
    {
        private static readonly Regex AssemblyNameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_.-]*$", RegexOptions.Compiled);

        public static AsmdefValidationResult Validate(IReadOnlyList<AsmdefDocument> documents)
        {
            var result = new AsmdefValidationResult();
            if (documents == null || documents.Count == 0)
            {
                return result;
            }

            var localMap = new Dictionary<string, AsmdefDocument>(StringComparer.Ordinal);
            foreach (AsmdefDocument doc in documents)
            {
                ValidateSingleDocument(doc, result);
                if (doc?.Model == null || string.IsNullOrWhiteSpace(doc.Model.Name))
                {
                    continue;
                }

                string name = doc.Model.Name.Trim();
                if (localMap.TryGetValue(name, out AsmdefDocument existing))
                {
                    result.Issues.Add(new AsmdefValidationIssue
                    {
                        Severity = AsmdefIssueSeverity.Error,
                        FilePath = doc.FilePath,
                        Message = $"程序集名称重复：'{name}'（已存在：{existing.FilePath}）"
                    });
                }
                else
                {
                    localMap[name] = doc;
                }
            }

            ValidateReferences(documents, localMap, result);
            ValidateCycles(localMap, result);
            return result;
        }

        private static void ValidateSingleDocument(AsmdefDocument doc, AsmdefValidationResult result)
        {
            if (doc == null)
            {
                return;
            }

            AsmdefModel model = doc.Model ?? new AsmdefModel();
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                result.Issues.Add(new AsmdefValidationIssue
                {
                    Severity = AsmdefIssueSeverity.Error,
                    FilePath = doc.FilePath,
                    Message = "缺少必填字段：name。"
                });
                return;
            }

            string name = model.Name.Trim();
            if (!AssemblyNameRegex.IsMatch(name))
            {
                result.Issues.Add(new AsmdefValidationIssue
                {
                    Severity = AsmdefIssueSeverity.Error,
                    FilePath = doc.FilePath,
                    Message = $"程序集名称不合法：'{name}'。"
                });
            }
        }

        private static void ValidateReferences(IReadOnlyList<AsmdefDocument> documents, IReadOnlyDictionary<string, AsmdefDocument> localMap, AsmdefValidationResult result)
        {
            foreach (AsmdefDocument doc in documents)
            {
                AsmdefModel model = doc?.Model;
                if (model == null || model.References == null)
                {
                    continue;
                }

                foreach (string rawRef in model.References)
                {
                    string referenceName = (rawRef ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(referenceName))
                    {
                        continue;
                    }

                    if (string.Equals(referenceName, model.Name, StringComparison.Ordinal))
                    {
                        result.Issues.Add(new AsmdefValidationIssue
                        {
                            Severity = AsmdefIssueSeverity.Error,
                            FilePath = doc.FilePath,
                            Message = $"程序集 '{model.Name}' 不能引用自身。"
                        });
                        continue;
                    }

                    if (!localMap.ContainsKey(referenceName))
                    {
                        result.Issues.Add(new AsmdefValidationIssue
                        {
                            Severity = AsmdefIssueSeverity.Warning,
                            FilePath = doc.FilePath,
                            Message = $"引用 '{referenceName}' 未在本地 asmdef 集合中找到，将按外部程序集处理。"
                        });
                    }
                }
            }
        }

        private static void ValidateCycles(IReadOnlyDictionary<string, AsmdefDocument> localMap, AsmdefValidationResult result)
        {
            var visitState = new Dictionary<string, int>(StringComparer.Ordinal);
            var stack = new Stack<string>();
            foreach (string name in localMap.Keys)
            {
                if (!visitState.ContainsKey(name))
                {
                    Dfs(name, localMap, visitState, stack, result);
                }
            }
        }

        private static void Dfs(string current, IReadOnlyDictionary<string, AsmdefDocument> localMap, Dictionary<string, int> state, Stack<string> stack, AsmdefValidationResult result)
        {
            state[current] = 1;
            stack.Push(current);
            AsmdefDocument doc = localMap[current];
            foreach (string rawRef in doc.Model.References)
            {
                string reference = (rawRef ?? string.Empty).Trim();
                if (!localMap.ContainsKey(reference))
                {
                    continue;
                }

                if (!state.TryGetValue(reference, out int refState))
                {
                    Dfs(reference, localMap, state, stack, result);
                    continue;
                }

                if (refState == 1)
                {
                    string cycle = BuildCycleMessage(reference, stack);
                    result.Issues.Add(new AsmdefValidationIssue
                    {
                        Severity = AsmdefIssueSeverity.Error,
                        FilePath = doc.FilePath,
                        Message = $"检测到循环依赖：{cycle}"
                    });
                }
            }

            stack.Pop();
            state[current] = 2;
        }

        private static string BuildCycleMessage(string target, IEnumerable<string> stack)
        {
            var names = stack.Reverse().ToList();
            int startIndex = names.FindIndex(x => string.Equals(x, target, StringComparison.Ordinal));
            if (startIndex < 0)
            {
                names.Add(target);
                return string.Join(" -> ", names);
            }

            var cycleNodes = names.Skip(startIndex).ToList();
            cycleNodes.Add(target);
            return string.Join(" -> ", cycleNodes);
        }
    }
}
#endif
