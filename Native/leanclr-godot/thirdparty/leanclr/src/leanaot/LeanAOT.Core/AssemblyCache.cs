using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanAOT.Core
{
    public class AssemblyCache
    {
        private readonly IAssemblyResolver _assemblyPathResolver;
        private readonly ModuleContext _modCtx;
        private readonly AssemblyResolver _asmResolver;


        public ModuleContext ModCtx => _modCtx;

        public Dictionary<string, ModuleDefMD> LoadedModules { get; } = new Dictionary<string, ModuleDefMD>();

        private readonly List<ModuleDefMD> _loadedModulesIncludeNetstandard = new List<ModuleDefMD>();

        public AssemblyCache(IAssemblyResolver assemblyResolver)
        {
            _assemblyPathResolver = assemblyResolver;
            _modCtx = ModuleDef.CreateModuleContext();
            _asmResolver = (AssemblyResolver)_modCtx.AssemblyResolver;
            _asmResolver.EnableTypeDefCache = true;
            _asmResolver.UseGAC = false;
        }

        public ModuleDefMD LoadModule(string moduleName, bool loadReferenceAssemblies = true)
        {
            if (LoadedModules.TryGetValue(moduleName, out var mod))
            {
                return mod;
            }
            mod = DoLoadModule(_assemblyPathResolver.ResolveAssembly(moduleName));
            LoadedModules.Add(moduleName, mod);

            if (loadReferenceAssemblies)
            {
                foreach (var refAsm in mod.GetAssemblyRefs())
                {
                    LoadModule(refAsm.Name);
                }
            }

            return mod;
        }

        private ModuleDefMD DoLoadModule(string dllPath)
        {
            //Debug.Log($"do load module:{dllPath}");
            ModuleDefMD mod = ModuleDefMD.Load(File.ReadAllBytes(dllPath), _modCtx);
            mod.EnableTypeDefFindCache = true;
            _asmResolver.AddToCache(mod);
            _loadedModulesIncludeNetstandard.Add(mod);
            return mod;
        }
    }
}
