
using dnlib.DotNet;

namespace LeanAOT.ToCpp
{
    class EvalVariable
    {
        public readonly int varId;
        public readonly EvalDataType type;
        public readonly TypeSig typeSig;
        public readonly bool inOutVar;

        public EvalVariable(int varId, EvalDataType type, TypeSig typeSig, bool inOutVar)
        {
            this.varId = varId;
            this.type = type;
            this.typeSig = typeSig;
            this.inOutVar = inOutVar;
        }
        public override string ToString()
        {
            return $"id:{varId} type:{type} typeSig:{typeSig}";
        }
    }
}