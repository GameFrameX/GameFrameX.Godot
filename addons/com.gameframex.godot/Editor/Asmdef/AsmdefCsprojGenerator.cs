#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GameFrameX.Editor.Asmdef
{
    public sealed class AsmdefGenerateResult
    {
        public string AsmdefFilePath { get; set; } = string.Empty;
        public string CsprojFilePath { get; set; } = string.Empty;
        public bool Written { get; set; }
    }

    public static class AsmdefCsprojGenerator
    {
        private static readonly Dictionary<string, string> PlatformConditionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["windows"] = "'$(OS)' == 'Windows_NT'",
            ["osx"] = "'$(OS)' != 'Windows_NT'",
            ["macos"] = "'$(OS)' != 'Windows_NT'",
            ["linux"] = "'$(OS)' != 'Windows_NT'",
            ["android"] = "'$(GodotTargetPlatform)' == 'android'",
            ["ios"] = "'$(GodotTargetPlatform)' == 'ios'",
            ["webgl"] = "'$(GodotTargetPlatform)' == 'web'"
        };

        public static List<AsmdefGenerateResult> GenerateAll(IReadOnlyList<AsmdefDocument> documents)
        {
            var results = new List<AsmdefGenerateResult>();
            var localMap = documents
                .Where(static x => x?.Model != null && !string.IsNullOrWhiteSpace(x.Model.Name))
                .GroupBy(static x => x.Model.Name.Trim(), StringComparer.Ordinal)
                .ToDictionary(static x => x.Key, static x => x.First(), StringComparer.Ordinal);

            foreach (AsmdefDocument doc in documents)
            {
                string csprojPath = AsmdefPathUtility.GetCsprojPathForAsmdef(doc.FilePath);
                string xml = BuildCsprojXml(doc, localMap);
                bool changed = AsmdefIO.WriteTextIfChanged(csprojPath, xml);
                results.Add(new AsmdefGenerateResult
                {
                    AsmdefFilePath = doc.FilePath,
                    CsprojFilePath = csprojPath,
                    Written = changed
                });
            }

            return results;
        }

        private static string BuildCsprojXml(AsmdefDocument doc, IReadOnlyDictionary<string, AsmdefDocument> localMap)
        {
            AsmdefModel model = doc.Model;
            string rootNamespace = string.IsNullOrWhiteSpace(model.RootNamespace) ? model.Name : model.RootNamespace.Trim();
            string assemblyName = model.Name.Trim();

            var project = new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"));

            var propertyGroup = new XElement("PropertyGroup",
                new XElement("TargetFramework", "net8.0"),
                new XElement("RootNamespace", rootNamespace),
                new XElement("AssemblyName", assemblyName),
                new XElement("AllowUnsafeBlocks", model.AllowUnsafeCode ? "true" : "false"),
                new XElement("Nullable", "enable"),
                new XElement("LangVersion", "latest"));

            List<string> commonDefines = CollectCommonDefines(model);
            if (commonDefines.Count > 0)
            {
                propertyGroup.Add(new XElement("DefineConstants", "$(DefineConstants);" + string.Join(";", commonDefines)));
            }

            project.Add(propertyGroup);

            AddPlatformDefineGroups(project, model);
            AddProjectReferences(project, doc, model, localMap);
            AddCompileItem(project, model);

            var document = new XDocument(new XDeclaration("1.0", "utf-8", null), project);
            return document.ToString() + Environment.NewLine;
        }

        private static List<string> CollectCommonDefines(AsmdefModel model)
        {
            var allDefines = new HashSet<string>(StringComparer.Ordinal);
            if (model.Defines != null)
            {
                foreach (string define in model.Defines)
                {
                    AddDefineIfValid(allDefines, define);
                }
            }

            if (model.VersionDefines != null)
            {
                foreach (AsmdefVersionDefine versionDefine in model.VersionDefines)
                {
                    AddDefineIfValid(allDefines, versionDefine?.Define);
                }
            }

            if (model.EditorOnly || IsEditorOnlyByIncludePlatforms(model.IncludePlatforms))
            {
                allDefines.Add("ASMDEF_EDITOR_ONLY");
            }

            return allDefines.OrderBy(static x => x, StringComparer.Ordinal).ToList();
        }

        private static void AddPlatformDefineGroups(XElement project, AsmdefModel model)
        {
            if (model.PlatformDefines == null || model.PlatformDefines.Count == 0)
            {
                return;
            }

            foreach ((string platform, List<string> defines) in model.PlatformDefines.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (!PlatformConditionMap.TryGetValue(platform, out string condition))
                {
                    continue;
                }

                var normalizedDefines = new HashSet<string>(StringComparer.Ordinal);
                foreach (string define in defines ?? Enumerable.Empty<string>())
                {
                    AddDefineIfValid(normalizedDefines, define);
                }

                if (normalizedDefines.Count == 0)
                {
                    continue;
                }

                project.Add(new XElement("PropertyGroup",
                    new XAttribute("Condition", condition),
                    new XElement("DefineConstants", "$(DefineConstants);" + string.Join(";", normalizedDefines.OrderBy(static x => x, StringComparer.Ordinal)))));
            }
        }

        private static void AddProjectReferences(XElement project, AsmdefDocument doc, AsmdefModel model, IReadOnlyDictionary<string, AsmdefDocument> localMap)
        {
            if (model.References == null || model.References.Count == 0)
            {
                return;
            }

            var projectReferencePaths = new List<string>();
            foreach (string rawRef in model.References)
            {
                string referenceName = (rawRef ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(referenceName))
                {
                    continue;
                }

                if (!localMap.TryGetValue(referenceName, out AsmdefDocument targetDoc))
                {
                    continue;
                }

                string currentDirectory = Path.GetDirectoryName(doc.FilePath) ?? string.Empty;
                string targetCsproj = AsmdefPathUtility.GetCsprojPathForAsmdef(targetDoc.FilePath);
                string relativePath = Path.GetRelativePath(currentDirectory, targetCsproj).Replace('\\', '/');
                projectReferencePaths.Add(relativePath);
            }

            if (projectReferencePaths.Count == 0)
            {
                return;
            }

            var itemGroup = new XElement("ItemGroup");
            foreach (string refPath in projectReferencePaths.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(static x => x, StringComparer.OrdinalIgnoreCase))
            {
                itemGroup.Add(new XElement("ProjectReference", new XAttribute("Include", refPath)));
            }

            project.Add(itemGroup);
        }

        private static void AddCompileItem(XElement project, AsmdefModel model)
        {
            var itemGroup = new XElement("ItemGroup",
                new XElement("Compile",
                    new XAttribute("Include", "**/*.cs"),
                    new XAttribute("Exclude", "bin/**;obj/**")));

            if (model.EditorOnly || IsEditorOnlyByIncludePlatforms(model.IncludePlatforms))
            {
                itemGroup.Add(new XElement("Compile",
                    new XAttribute("Remove", "**/Runtime/**/*.cs")));
            }

            project.Add(itemGroup);
        }

        private static bool IsEditorOnlyByIncludePlatforms(IReadOnlyCollection<string> includePlatforms)
        {
            if (includePlatforms == null || includePlatforms.Count == 0)
            {
                return false;
            }

            return includePlatforms.Count == 1 &&
                   includePlatforms.Contains("Editor", StringComparer.OrdinalIgnoreCase);
        }

        private static void AddDefineIfValid(ISet<string> set, string rawDefine)
        {
            string define = (rawDefine ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(define))
            {
                return;
            }

            set.Add(define);
        }
    }
}
#endif
