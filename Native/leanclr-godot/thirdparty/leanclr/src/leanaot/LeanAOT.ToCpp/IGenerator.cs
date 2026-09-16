using LeanAOT.GenerationPlan;

namespace LeanAOT.ToCpp
{
    public interface IGenerator
    {
        void Generate(GenerationConfig conf);

        void GenerateAssembly(AssemblyPlan plan);

        void GenerateMethodDef(MethodDefPlan plan, object ctx);

        void GenerateGlobalInitializationCpp();
    }
}
