namespace LeanAOT.Core
{
    public class MultiDirectoryAssemblyResolver : IAssemblyResolver
    {
        private readonly List<string> _searchDirs;

        public MultiDirectoryAssemblyResolver(List<string> searchDirs)
        {
            _searchDirs = searchDirs;
        }

        public string ResolveAssembly(string assemblyName)
        {
            foreach (var dir in _searchDirs)
            {
                var candidatePath = Path.Combine(dir, assemblyName + ".dll");
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }
            throw new FileNotFoundException($"Cannot find assembly:{assemblyName} in search dirs");
        }
    }
}
