using System.Collections.Generic;
using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 扫光效果管理器（对应 Unity 侧 com.gameframex.unity.fairygui.unity 的 SweepLightManager）。
    /// 提供扫光 ShaderMaterial 的统一管理和便捷的 API 接口，保持双端接口一致。
    /// </summary>
    public static class SweepLightManager
    {
        /// <summary>
        /// 扫光 Shader 资源路径。
        /// </summary>
        public const string SweepLightShaderPath = "res://addons/com.gameframex.godot.fairygui.godot/Runtime/Shaders/FairyGUI_ImageSweepLight.gdshader";

        /// <summary>
        /// 扫光材质缓存
        /// </summary>
        private static readonly Dictionary<string, ShaderMaterial> _materialCache = new Dictionary<string, ShaderMaterial>();

        /// <summary>
        /// 扫光 Shader 引用
        /// </summary>
        private static Shader _sweepLightShader;

        /// <summary>
        /// 默认扫光参数配置
        /// </summary>
        public struct SweepLightConfig
        {
            /// <summary>
            /// 扫光时间（秒）
            /// </summary>
            /// <value>扫光从开始到结束的持续时间</value>
            public float lightTime;

            /// <summary>
            /// 扫光厚度
            /// </summary>
            /// <value>扫光带的宽度，范围通常为 0-1</value>
            public float lightThick;

            /// <summary>
            /// 下次扫光间隔时间（秒）
            /// </summary>
            /// <value>两次扫光之间的等待时间</value>
            public float nextTime;

            /// <summary>
            /// 扫光角度
            /// </summary>
            /// <value>扫光的倾斜角度，范围 0-360</value>
            public float lightAngle;

            /// <summary>
            /// 扫光强度
            /// </summary>
            /// <value>扫光的亮度强度，范围 0-3</value>
            public float lightIntensity;

            /// <summary>
            /// 创建默认配置
            /// </summary>
            /// <returns>默认的扫光参数配置</returns>
            public static SweepLightConfig Default()
            {
                return new SweepLightConfig
                {
                    lightTime = 0.6f,
                    lightThick = 0.3f,
                    nextTime = 2.0f,
                    lightAngle = 45.0f,
                    lightIntensity = 1.4f
                };
            }

            /// <summary>
            /// 创建快速扫光配置
            /// </summary>
            /// <returns>快速扫光参数配置</returns>
            public static SweepLightConfig Fast()
            {
                return new SweepLightConfig
                {
                    lightTime = 0.3f,
                    lightThick = 0.2f,
                    nextTime = 1.0f,
                    lightAngle = 30.0f,
                    lightIntensity = 2.0f
                };
            }

            /// <summary>
            /// 创建慢速扫光配置
            /// </summary>
            /// <returns>慢速扫光参数配置</returns>
            public static SweepLightConfig Slow()
            {
                return new SweepLightConfig
                {
                    lightTime = 1.2f,
                    lightThick = 0.4f,
                    nextTime = 3.0f,
                    lightAngle = 60.0f,
                    lightIntensity = 1.0f
                };
            }
        }

        /// <summary>
        /// 扫光 Shader 属性
        /// </summary>
        /// <value>获取扫光 Shader，如果未加载则自动加载</value>
        public static Shader SweepLightShader
        {
            get
            {
                if (_sweepLightShader == null)
                {
                    _sweepLightShader = ResourceLoader.Load<Shader>(SweepLightShaderPath);
                    if (_sweepLightShader == null)
                    {
                        GD.PushError("SweepLightManager: 未找到 FairyGUI/ImageSweepLight Shader. path=" + SweepLightShaderPath);
                    }
                }

                return _sweepLightShader;
            }
        }

        /// <summary>
        /// 创建扫光材质
        /// </summary>
        /// <param name="config">扫光参数配置</param>
        /// <returns>配置好的扫光材质</returns>
        public static ShaderMaterial CreateSweepLightMaterial(SweepLightConfig config)
        {
            if (SweepLightShader == null)
                return null;

            var material = new ShaderMaterial
            {
                Shader = SweepLightShader,
            };
            ApplyConfigToMaterial(material, config);
            SetSweepLightEnabled(material, true);
            return material;
        }

        /// <summary>
        /// 创建默认扫光材质
        /// </summary>
        /// <returns>使用默认配置的扫光材质</returns>
        public static ShaderMaterial CreateDefaultSweepLightMaterial()
        {
            return CreateSweepLightMaterial(SweepLightConfig.Default());
        }

        /// <summary>
        /// 获取或创建缓存的扫光材质
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <param name="config">扫光参数配置</param>
        /// <returns>缓存的扫光材质</returns>
        public static ShaderMaterial GetOrCreateCachedMaterial(string configName, SweepLightConfig config)
        {
            if (_materialCache.TryGetValue(configName, out var cachedMaterial))
            {
                if (cachedMaterial != null)
                    return cachedMaterial;

                _materialCache.Remove(configName);
            }

            var newMaterial = CreateSweepLightMaterial(config);
            if (newMaterial != null)
            {
                _materialCache[configName] = newMaterial;
            }

            return newMaterial;
        }

        /// <summary>
        /// 应用配置到材质
        /// </summary>
        /// <param name="material">目标材质</param>
        /// <param name="config">扫光参数配置</param>
        public static void ApplyConfigToMaterial(ShaderMaterial material, SweepLightConfig config)
        {
            if (material == null) return;

            material.SetShaderParameter("light_time", config.lightTime);
            material.SetShaderParameter("light_thick", config.lightThick);
            material.SetShaderParameter("next_time", config.nextTime);
            material.SetShaderParameter("light_angle", config.lightAngle);
            material.SetShaderParameter("light_intensity", config.lightIntensity);
        }

        /// <summary>
        /// 为 GImage 添加扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        /// <param name="config">扫光参数配置</param>
        public static void AddSweepLightToImage(GImage image, SweepLightConfig config)
        {
            if (image == null) return;

            var sweepMaterial = CreateSweepLightMaterial(config);
            if (sweepMaterial != null)
            {
                ApplyTextureUv(sweepMaterial, image.texture);
                image.material = sweepMaterial;
            }
        }

        /// <summary>
        /// 把精灵在图集内的 UV 区域同步到材质，供 Shader 归一化采样坐标。
        /// </summary>
        /// <param name="material">目标材质</param>
        /// <param name="texture">图像当前纹理</param>
        public static void ApplyTextureUv(ShaderMaterial material, NTexture texture)
        {
            if (material == null) return;

            if (texture == null)
            {
                material.SetShaderParameter("uv_offset", new Vector2(0, 0));
                material.SetShaderParameter("uv_scale", new Vector2(1, 1));
                return;
            }

            material.SetShaderParameter("uv_offset", new Vector2(texture.uvRect.X, texture.uvRect.Y));
            material.SetShaderParameter("uv_scale", new Vector2(texture.uvRect.width, texture.uvRect.height));
        }

        /// <summary>
        /// 为 GImage 添加默认扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        public static void AddDefaultSweepLightToImage(GImage image)
        {
            AddSweepLightToImage(image, SweepLightConfig.Default());
        }

        /// <summary>
        /// 移除 GImage 的扫光效果
        /// </summary>
        /// <param name="image">目标图像组件</param>
        public static void RemoveSweepLightFromImage(GImage image)
        {
            if (image == null) return;

            // 恢复到默认材质或 null
            image.material = null;
        }

        /// <summary>
        /// 启用材质的扫光效果
        /// </summary>
        /// <param name="material">目标材质</param>
        public static void EnableSweepLight(ShaderMaterial material)
        {
            SetSweepLightEnabled(material, true);
        }

        /// <summary>
        /// 禁用材质的扫光效果
        /// </summary>
        /// <param name="material">目标材质</param>
        public static void DisableSweepLight(ShaderMaterial material)
        {
            SetSweepLightEnabled(material, false);
        }

        /// <summary>
        /// 清理材质缓存
        /// </summary>
        public static void ClearMaterialCache()
        {
            foreach (var kvp in _materialCache)
            {
                kvp.Value?.Dispose();
            }

            _materialCache.Clear();
        }

        /// <summary>
        /// 获取预设配置
        /// </summary>
        /// <param name="presetName">预设名称（"default", "fast", "slow"）</param>
        /// <returns>对应的扫光配置</returns>
        public static SweepLightConfig GetPresetConfig(string presetName)
        {
            switch (presetName.ToLower())
            {
                case "fast":
                    return SweepLightConfig.Fast();
                case "slow":
                    return SweepLightConfig.Slow();
                case "default":
                default:
                    return SweepLightConfig.Default();
            }
        }

        private static void SetSweepLightEnabled(ShaderMaterial material, bool enabled)
        {
            material?.SetShaderParameter("sweep_light_enabled", enabled);
        }
    }
}
