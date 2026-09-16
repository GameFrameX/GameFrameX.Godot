using dnlib.DotNet;

namespace LeanAOT.GenerationPlan
{
    public class AssemblyPlan
    {
        public ModuleDef Module { get; set; }

        public string AssemblyName { get; set; }

        public List<ClassPlan> ClassPlans { get; }

        public List<MethodDefPlan> MethodPlans { get; }

        private readonly HashSet<IMethod> _methodSet;

        public AssemblyPlan(ModuleDef module, string assemblyName, List<ClassPlan> classPlans, List<MethodDefPlan> methodPlans)
        {
            Module = module;
            AssemblyName = assemblyName;
            ClassPlans = classPlans;
            MethodPlans = new List<MethodDefPlan>(methodPlans);
            MethodPlans.Sort((a, b) => a.MethodDef.MDToken.ToInt32().CompareTo(b.MethodDef.MDToken.ToInt32()));
            _methodSet = new HashSet<IMethod>(methodPlans.Select(mp => mp.MethodDef), MethodEqualityComparer.CompareDeclaringTypes);
        }

        public bool ContainsMethod(IMethod method)
        {
            return _methodSet.Contains(method);
        }
    }
}
