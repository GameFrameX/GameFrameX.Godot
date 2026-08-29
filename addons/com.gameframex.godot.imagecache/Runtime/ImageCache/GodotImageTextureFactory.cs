using Godot;

namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 基于 Godot Image API 的纹理工厂实现。
    /// </summary>
    public sealed class GodotImageTextureFactory : IImageTextureFactory
    {
        /// <summary>
        /// 将图片字节解码为纹理。
        /// ponytail: 缓存文件名固定 .png 后缀，但实际内容可能为任意网络图片格式，
        /// 依次尝试 PNG/JPG/WEBP 三种解码；Unity Texture2D.LoadImage 只自动识别 PNG/JPG。
        /// 升级路径：需要支持更多格式（BMP/TGA 等）时在此追加尝试链。
        /// </summary>
        /// <param name="buffer">图片文件字节。</param>
        /// <returns>解码成功返回 ImageTexture，失败返回 null。</returns>
        public ImageTexture Create(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }

            var image = new Image();
            if (image.LoadPngFromBuffer(buffer) == Error.Ok && !image.IsEmpty())
            {
                return ImageTexture.CreateFromImage(image);
            }

            image = new Image();
            if (image.LoadJpgFromBuffer(buffer) == Error.Ok && !image.IsEmpty())
            {
                return ImageTexture.CreateFromImage(image);
            }

            image = new Image();
            if (image.LoadWebpFromBuffer(buffer) == Error.Ok && !image.IsEmpty())
            {
                return ImageTexture.CreateFromImage(image);
            }

            return null;
        }
    }
}
