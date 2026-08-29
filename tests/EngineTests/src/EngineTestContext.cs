using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试共享上下文：runner 挂树根节点，供用例创建临时节点。
    /// </summary>
    public static class EngineTestContext
    {
        public static Node Root { get; set; }
    }
}
