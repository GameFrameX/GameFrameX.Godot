#if TOOLS
using Godot;

namespace GameFrameX.Editor.Asmdef
{
    [Tool]
    [GlobalClass]
    public partial class AsmdefResource : Resource
    {
        public string SourcePath { get; set; } = string.Empty;
        public AsmdefModel Model { get; set; } = new AsmdefModel();
    }
}
#endif
