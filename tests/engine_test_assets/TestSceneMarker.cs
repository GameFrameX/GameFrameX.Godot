using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试场景标记脚本：加载真实 .tscn 后用于断言根节点与导出属性。
    /// </summary>
    public partial class TestSceneMarker : Node
    {
        [Export] public string SceneMarker { get; set; } = string.Empty;
    }
}
