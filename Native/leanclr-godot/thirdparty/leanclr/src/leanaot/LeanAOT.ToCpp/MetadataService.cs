using dnlib.DotNet;

namespace LeanAOT.ToCpp
{

    public class MetadataService
    {
        private readonly Dictionary<IMethod, MethodDetail> _methodDetails = new Dictionary<IMethod, MethodDetail>(MethodEqualityComparer.CompareDeclaringTypes);
        private readonly Dictionary<ITypeDefOrRef, TypeDetail> _typeDetails = new Dictionary<ITypeDefOrRef, TypeDetail>(TypeEqualityComparer.Instance);

        public MethodDetail GetMethodDetail(IMethod method)
        {
            if (_methodDetails.TryGetValue(method, out MethodDetail detail))
            {
                return detail;
            }
            detail = CreateDetail(method);
            _methodDetails.Add(method, detail);
            return detail;
        }

        private MethodDetail CreateDetail(IMethod method)
        {
            return new MethodDetail(method);
        }

        public TypeDetail GetTypeDetail(ITypeDefOrRef type)
        {
            if (_typeDetails.TryGetValue(type, out TypeDetail detail))
            {
                return detail;
            }
            detail = CreateDetail(type);
            _typeDetails.Add(type, detail);
            return detail;
        }

        private TypeDetail CreateDetail(ITypeDefOrRef type)
        {
            return new TypeDetail(type, this.GetTypeDetail);
        }

        public FieldDetail GetFieldDetail(IField field)
        {
            var type = field.DeclaringType;
            TypeDetail typeDetail = GetTypeDetail(field.DeclaringType);
            return typeDetail.GetFieldDetail(field);
        }
    }
}
