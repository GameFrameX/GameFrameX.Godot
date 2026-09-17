namespace FairyGUI
{
    /// <summary>
    /// GImage 扩展方法（对应 Unity 侧 com.gameframex.unity.fairygui.unity 的 GImageExtensions）。
    /// 为 GImage 组件提供便捷的扫光效果操作方法，保持双端接口一致。
    /// </summary>
    public static class GImageExtensions
    {
        /// <summary>
        /// 为图像添加扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <param name="lightTime">扫光时间（秒）</param>
        /// <param name="lightThick">扫光厚度</param>
        /// <param name="nextTime">下次扫光间隔时间（秒）</param>
        /// <param name="lightAngle">扫光角度（0-360 度）</param>
        /// <param name="lightIntensity">扫光强度（0-3）</param>
        /// <returns>返回图像组件本身，支持链式调用</returns>
        public static GImage AddSweepLight(this GImage image, float lightTime = 0.6f, float lightThick = 0.3f, float nextTime = 2.0f, float lightAngle = 45.0f, float lightIntensity = 1.4f)
        {
            if (image == null)
            {
                return image;
            }

            var config = new SweepLightManager.SweepLightConfig
            {
                lightTime = lightTime,
                lightThick = lightThick,
                nextTime = nextTime,
                lightAngle = lightAngle,
                lightIntensity = lightIntensity
            };

            SweepLightManager.AddSweepLightToImage(image, config);
            return image;
        }

        /// <summary>
        /// 为图像添加默认扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <returns>返回图像组件本身，支持链式调用</returns>
        public static GImage AddDefaultSweepLight(this GImage image)
        {
            if (image == null)
            {
                return image;
            }

            SweepLightManager.AddDefaultSweepLightToImage(image);
            return image;
        }

        /// <summary>
        /// 为图像添加快速扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <returns>返回图像组件本身，支持链式调用</returns>
        public static GImage AddFastSweepLight(this GImage image)
        {
            if (image == null)
            {
                return image;
            }

            SweepLightManager.AddSweepLightToImage(image, SweepLightManager.SweepLightConfig.Fast());
            return image;
        }

        /// <summary>
        /// 为图像添加慢速扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <returns>返回图像组件本身，支持链式调用</returns>
        public static GImage AddSlowSweepLight(this GImage image)
        {
            if (image == null)
            {
                return image;
            }

            SweepLightManager.AddSweepLightToImage(image, SweepLightManager.SweepLightConfig.Slow());
            return image;
        }

        /// <summary>
        /// 为图像添加预设扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <param name="presetName">预设名称（"default", "fast", "slow"）</param>
        /// <returns>返回图像组件本身，支持链式调用</returns>
        public static GImage AddSweepLightPreset(this GImage image, string presetName)
        {
            if (image == null)
            {
                return image;
            }

            var config = SweepLightManager.GetPresetConfig(presetName);
            SweepLightManager.AddSweepLightToImage(image, config);
            return image;
        }

        /// <summary>
        /// 移除图像的扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <returns>返回图像组件本身，支持链式调用</returns>
        public static GImage RemoveSweepLight(this GImage image)
        {
            if (image == null)
            {
                return image;
            }

            SweepLightManager.RemoveSweepLightFromImage(image);
            return image;
        }
    }
}
