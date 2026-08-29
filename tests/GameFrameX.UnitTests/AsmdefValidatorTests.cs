using System.Collections.Generic;
using System.Linq;
using GameFrameX.Editor.Asmdef;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// asmdef 依赖与配置校验器测试（G1 门禁）。
    /// 门禁要求：非法名称、缺失引用、循环依赖均可被识别并报错。
    /// 备注：纯内存构造 AsmdefDocument，无文件 IO、无 Godot native 对象。
    /// </summary>
    public sealed class AsmdefValidatorTests
    {
        private static AsmdefDocument CreateDocument(string name, params string[] references)
        {
            var model = new AsmdefModel { Name = name, RootNamespace = name };
            foreach (string reference in references)
            {
                model.References.Add(reference);
            }

            return new AsmdefDocument { FilePath = "/tmp/" + name + ".asmdef", Model = model };
        }

        [Fact]
        public void Validate_EmptyCollection_Passes()
        {
            AsmdefValidationResult result = AsmdefValidator.Validate(new List<AsmdefDocument>());

            Assert.False(result.HasError);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public void Validate_ValidDependencyGraph_Passes()
        {
            var documents = new List<AsmdefDocument>
            {
                CreateDocument("GameFrameX.Core"),
                CreateDocument("GameFrameX.Entity", "GameFrameX.Core")
            };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.False(result.HasError);
            Assert.Empty(result.Issues);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("1StartsWithDigit")]
        [InlineData("Has Space")]
        [InlineData("非法名称")]
        public void Validate_IllegalName_ReportsError(string illegalName)
        {
            var documents = new List<AsmdefDocument> { CreateDocument(illegalName) };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.True(result.HasError);
            Assert.Contains(result.Issues, x => x.Severity == AsmdefIssueSeverity.Error && x.Message.Contains(illegalName.Trim().Length > 0 ? illegalName.Trim() : "name"));
        }

        [Fact]
        public void Validate_MissingName_ReportsError()
        {
            var documents = new List<AsmdefDocument> { CreateDocument("") };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.True(result.HasError);
            Assert.Contains(result.Issues, x => x.Message.Contains("name"));
        }

        [Fact]
        public void Validate_MissingReference_DefaultReportsWarning()
        {
            var documents = new List<AsmdefDocument> { CreateDocument("GameFrameX.Entity", "GameFrameX.NotExist") };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            // 默认宽松模式：缺失引用按 Warning 报告（按外部程序集处理），不阻断生成
            Assert.False(result.HasError);
            Assert.Contains(result.Issues, x =>
                x.Severity == AsmdefIssueSeverity.Warning &&
                x.Message.Contains("GameFrameX.NotExist"));
        }

        [Fact]
        public void Validate_MissingReference_StrictModeReportsError()
        {
            var documents = new List<AsmdefDocument> { CreateDocument("GameFrameX.Entity", "GameFrameX.NotExist") };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents, strictReferences: true);

            // 严格模式（保存前强校验用）：缺失引用按 Error 报告
            Assert.True(result.HasError);
            Assert.Contains(result.Issues, x =>
                x.Severity == AsmdefIssueSeverity.Error &&
                x.Message.Contains("GameFrameX.NotExist"));
        }

        [Fact]
        public void Validate_SelfReference_ReportsError()
        {
            var documents = new List<AsmdefDocument> { CreateDocument("GameFrameX.Self", "GameFrameX.Self") };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.True(result.HasError);
            Assert.Contains(result.Issues, x => x.Message.Contains("不能引用自身"));
        }

        [Fact]
        public void Validate_DuplicateNames_ReportsError()
        {
            var documents = new List<AsmdefDocument>
            {
                CreateDocument("GameFrameX.Duplicate"),
                CreateDocument("GameFrameX.Duplicate")
            };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.True(result.HasError);
            Assert.Contains(result.Issues, x => x.Message.Contains("程序集名称重复"));
        }

        [Fact]
        public void Validate_CircularDependency_ReportsErrorWithCyclePath()
        {
            var documents = new List<AsmdefDocument>
            {
                CreateDocument("ModuleA", "ModuleC"),
                CreateDocument("ModuleB", "ModuleA"),
                CreateDocument("ModuleC", "ModuleB")
            };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.True(result.HasError);
            AsmdefValidationIssue cycleIssue = Assert.Single(result.Issues.Where(x => x.Message.Contains("循环依赖")));
            Assert.Contains("ModuleA", cycleIssue.Message);
            Assert.Contains("ModuleB", cycleIssue.Message);
            Assert.Contains("ModuleC", cycleIssue.Message);
        }

        [Fact]
        public void Validate_TwoNodeCycle_ReportsError()
        {
            var documents = new List<AsmdefDocument>
            {
                CreateDocument("Alpha", "Beta"),
                CreateDocument("Beta", "Alpha")
            };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);

            Assert.True(result.HasError);
            Assert.Contains(result.Issues, x => x.Message.Contains("循环依赖"));
        }

        [Fact]
        public void Validate_ReferenceToExistingModule_ProducesNoReferenceIssue()
        {
            var documents = new List<AsmdefDocument>
            {
                CreateDocument("GameFrameX.Core"),
                CreateDocument("GameFrameX.Entity", "GameFrameX.Core")
            };

            AsmdefValidationResult result = AsmdefValidator.Validate(documents, strictReferences: true);

            Assert.False(result.HasError);
            Assert.Empty(result.Issues);
        }
    }
}
