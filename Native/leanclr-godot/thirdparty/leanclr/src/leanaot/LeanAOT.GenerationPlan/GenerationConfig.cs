using LeanAOT.Core;

namespace LeanAOT.GenerationPlan
{
    public class GenerationConfig
    {
        public string outputCodeDir;

        public int maxCppFileSize = 10 * 1024 * 1024;

        public Manifest manifest;

        public AssemblyCache assemblyCache;
    }

    public class GenerationConfigBuilder
    {
        public string outputCodeDir;

        public List<string> dllSearchPaths = new List<string>();

        public Manifest manifest;

        public AssemblyCache assemblyCache;

        private string ResolveAssemblyPath(string assemblyName)
        {
            foreach (var dir in dllSearchPaths)
            {
                var candidatePath = Path.Combine(dir, assemblyName + ".dll");
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }
            throw new FileNotFoundException($"Assembly {assemblyName} not found in search paths.");
        }

        public GenerationConfig Build()
        {
            var conf = new GenerationConfig();
            conf.outputCodeDir = outputCodeDir;
            conf.manifest = manifest;
            conf.assemblyCache = assemblyCache;
            return conf;
        }
    }
}
