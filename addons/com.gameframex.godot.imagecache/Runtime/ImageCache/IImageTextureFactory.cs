using Godot;

namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 纹理工厂接口：把图片字节解码为 ImageTexture。
    /// 迁移备注（Unity → Godot）：Unity 的 Texture2D.LoadImage 是 native 实例方法，
    /// Godot 的 Image.LoadPngFromBuffer 等同样是 native 调用，在 xunit 宿主会段错误，
    /// 因此隔离为接口，运行时用 GodotImageTextureFactory，单元测试注入 stub。
    /// </summary>
    public interface IImageTextureFactory
    {
        /// <summary>
        /// 将图片字节解码为纹理。
        /// </summary>
        /// <param name="buffer">图片文件字节。</param>
        /// <returns>解码成功返回 ImageTexture，失败返回 null。</returns>
        ImageTexture Create(byte[] buffer);
    }
}
