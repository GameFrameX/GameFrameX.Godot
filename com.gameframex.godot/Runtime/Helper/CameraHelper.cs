using System;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 相机帮助类
    /// Note: Camera screenshot functionality is different in Godot
    /// </summary>
    public static class CameraHelper
    {
        /// <summary>
        /// 获取相机快照
        /// In Godot, use Viewport texture for screenshots
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="scale">缩放比</param>
        public static Texture2D GetCaptureScreenshot(Camera3D camera, float scale = 0.5f)
        {
            // Godot uses Viewport for capturing screenshots
            // This is a placeholder implementation
            var viewport = camera.GetViewport();
            var image = viewport.GetTexture().GetData();
            if (scale != 1.0f)
            {
                image.Resize((int)(image.GetWidth() * scale), (int)(image.GetHeight() * scale));
            }
            var texture = new ImageTexture();
            texture.CreateFromImage(image);
            return texture;
        }
    }
}