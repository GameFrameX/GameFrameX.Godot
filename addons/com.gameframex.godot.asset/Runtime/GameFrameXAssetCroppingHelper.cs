using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 裁剪辅助器：通过引用类型列表防止代码裁剪误删。
    /// </summary>
    public partial class GameFrameXAssetCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(AssetManager);
            _ = typeof(Constant);
            _ = typeof(IAssetManager);
            _ = typeof(AssetComponent);
        }
    }
}
