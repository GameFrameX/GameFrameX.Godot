#if TOOLS
using System;
using System.IO;
using Godot;

namespace GameFrameX.Editor.Asmdef
{
    [Tool]
    public partial class AsmdefResourceFormatLoader : ResourceFormatLoader
    {
        public override string[] _GetRecognizedExtensions()
        {
            return new[] { "asmdef" };
        }

        public override string _GetResourceType(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            // 使用内置 Resource 类型，避免 Godot 在类型识别阶段因为自定义类名不可见而拒绝打开。
            bool isAsmdef = path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase);
            if (isAsmdef)
            {
                GD.Print($"[Asmdef][Loader] _GetResourceType path={path}");
            }

            return isAsmdef ? "Resource" : string.Empty;
        }

        public override bool _HandlesType(StringName type)
        {
            string typeName = type.ToString();
            bool canHandle = string.Equals(typeName, "Resource", StringComparison.Ordinal) ||
                             string.Equals(typeName, nameof(AsmdefResource), StringComparison.Ordinal);
            if (canHandle)
            {
                GD.Print($"[Asmdef][Loader] _HandlesType type={typeName}");
            }

            return canHandle;
        }

        public override bool _Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string globalPath = ProjectSettings.GlobalizePath(path);
            bool exists = File.Exists(globalPath);
            GD.Print($"[Asmdef][Loader] _Exists path={path}, exists={exists}");
            return exists;
        }

        public override Variant _Load(string path, string originalPath = "", bool useSubThreads = false, int cacheMode = 0)
        {
            string globalPath = ProjectSettings.GlobalizePath(path);
            GD.Print($"[Asmdef][Loader] _Load path={path}, original={originalPath}, cacheMode={cacheMode}");
            try
            {
                AsmdefDocument document = AsmdefIO.LoadDocument(globalPath);
                return new AsmdefResource
                {
                    SourcePath = path,
                    Model = document.Model ?? new AsmdefModel()
                };
            }
            catch (Exception exception)
            {
                GD.PrintErr($"读取 asmdef 资源失败: {path}, {exception.Message}");
                string fallbackName = System.IO.Path.GetFileNameWithoutExtension(globalPath) ?? string.Empty;
                return new AsmdefResource
                {
                    SourcePath = path,
                    Model = new AsmdefModel
                    {
                        Name = fallbackName,
                        RootNamespace = fallbackName
                    }
                };
            }
        }
    }
}
#endif
