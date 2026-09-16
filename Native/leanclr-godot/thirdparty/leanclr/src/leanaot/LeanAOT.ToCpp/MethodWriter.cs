
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using LeanAOT.GenerationPlan;
using System;

namespace LeanAOT.ToCpp
{

    class MethodWriter : MethodWriterBase
    {

        public MethodWriter(MethodDetail method, IMethodBodyCodeFilePart methodBodyCodeFile) : base(method, methodBodyCodeFile)
        {
        }
    }
}