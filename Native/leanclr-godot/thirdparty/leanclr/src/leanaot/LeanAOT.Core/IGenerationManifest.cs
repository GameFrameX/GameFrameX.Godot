using dnlib.DotNet;

namespace LeanAOT.Core
{
    public interface IGenerationManifest
    {
        bool IsMethodInManifest(IMethod methodDef);
    }
}
