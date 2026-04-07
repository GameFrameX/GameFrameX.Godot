#if TOOLS
using System;
using Godot;

namespace GameFrameX.Editor.Asmdef
{
    [Tool]
    public partial class AsmdefResourceFormatSaver : ResourceFormatSaver
    {
        public override bool _Recognize(Resource resource)
        {
            return resource is AsmdefResource;
        }

        public override string[] _GetRecognizedExtensions(Resource resource)
        {
            return new[] { "asmdef" };
        }

        public override Error _Save(Resource resource, string path, uint flags)
        {
            if (resource is not AsmdefResource asmdefResource)
            {
                return Error.InvalidParameter;
            }

            try
            {
                string globalPath = ProjectSettings.GlobalizePath(path);
                AsmdefIO.SaveDocument(globalPath, asmdefResource.Model ?? new AsmdefModel());
                return Error.Ok;
            }
            catch (Exception exception)
            {
                GD.PrintErr($"保存 asmdef 资源失败: {path}, {exception.Message}");
                return Error.Failed;
            }
        }
    }
}
#endif
