using dnlib.DotNet;

namespace LeanAOT.ToCpp
{
    class ModuleRegistrationHeaderCodeFile
    {
        private readonly string _filePath;
        private readonly ModuleDef _mod;
        private readonly CodeWriter _totalWriter;

        private readonly CodeThunkZone _headCollection;
        private readonly CodeThrunkWriter _includeWriter;

        private readonly CodeThunkZone _bodyCollection;
        private readonly CodeThrunkWriter _moduleForwardDeclarationWriter;

        public ModuleRegistrationHeaderCodeFile(string filePath, ModuleDef mod)
        {
            _filePath = filePath;
            _mod = mod;
            _totalWriter = new CodeWriter(filePath);
            _headCollection = _totalWriter.CreateThunkCollection("includes");
            _includeWriter = _headCollection.CreateThunk("includes");

            _bodyCollection = _totalWriter.CreateThunkCollection("body");
            _moduleForwardDeclarationWriter = _bodyCollection.CreateThunk("module_forward_declarations");
        }


        public void Generate()
        {
            AddInclude(ConstStrings.CodegenCommonHeaderFileName);
            _moduleForwardDeclarationWriter.AddLine();
            AddModuleForwardDeclaration(_mod);
            _moduleForwardDeclarationWriter.AddLine(ModuleGenerationUtil.GetModuleInitializeMethodDeclaration(_mod));
        }


        private void AddInclude(string include)
        {
            _includeWriter.AddLine($"#include \"{include}\"");
        }

        private void AddModuleForwardDeclaration(ModuleDef mod)
        {
            _moduleForwardDeclarationWriter.AddLine(ModuleGenerationUtil.GetModuleForwardDeclaration(mod));
            _moduleForwardDeclarationWriter.AddLine($"extern {ConstStrings.ModuleDataTypeName} {ModuleGenerationUtil.GetModuleGlobalDataVariableName(mod)};");
        }

        public void Save()
        {
            _totalWriter.Save();
        }
    }
}
