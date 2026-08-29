using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试自定义资源：验证 typed 资源从 .tres / PCK 链路加载。
    /// </summary>
    [GlobalClass]
    public partial class TestResource : Resource
    {
        [Export] public string TestValue { get; set; } = string.Empty;
    }
}
