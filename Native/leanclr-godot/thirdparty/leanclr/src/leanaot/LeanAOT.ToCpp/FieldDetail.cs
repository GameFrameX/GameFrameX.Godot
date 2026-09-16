using dnlib.DotNet;
using LeanAOT.Core;

namespace LeanAOT.ToCpp
{
    public class FieldDetail
    {
        private readonly TypeDetail _parent;
        private readonly FieldDef _fieldBase;
        private readonly string _name;
        private readonly TypeSig _type;
        private readonly int _index;

        public FieldDetail(TypeDetail parent, FieldDef fieldBase, string name, TypeSig type, int index)
        {
            _parent = parent;
            _fieldBase = fieldBase;
            _name = name;
            _type = type;
            _index = index;
        }

        public TypeDetail Parent => _parent;

        public ITypeDefOrRef ParentType => _parent.Type;

        public FieldDef FieldBase => _fieldBase;

        public string Name => _name;

        public TypeSig Type => _type;

        public int Index => _index;

        public uint? FieldOffset => _fieldBase.FieldOffset;
    }
}
