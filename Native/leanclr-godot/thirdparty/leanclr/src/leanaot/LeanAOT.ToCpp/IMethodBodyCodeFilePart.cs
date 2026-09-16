namespace LeanAOT.ToCpp
{
    interface IMethodBodyCodeFilePart
    {
        ForwardDeclaration ForwardDeclaration { get; }

        CodeWriter MethodWriter { get; }
    }
}
