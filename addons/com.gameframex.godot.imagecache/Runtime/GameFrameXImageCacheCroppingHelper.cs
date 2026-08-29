using Godot;

namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 图片缓存裁剪辅助器，引用程序集中所有公开类型以防止代码裁剪。
    /// 迁移备注（Unity → Godot）：沿用其它包（timer/download 等）的 CroppingHelper 惯例，
    /// MonoBehaviour.Start → Node._Ready；Unity 的 [Preserve] 特性在 Godot 侧无对应（Godot.NET.Sdk 不做 IL 裁剪）。
    /// </summary>
    public partial class GameFrameXImageCacheCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(ImageCacheComponent);
            _ = typeof(IImageCacheManager);
            _ = typeof(ImageCacheManager);
            _ = typeof(ImageCacheConfig);
        }
    }
}
