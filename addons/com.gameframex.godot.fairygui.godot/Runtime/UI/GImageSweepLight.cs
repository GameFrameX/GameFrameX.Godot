using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 带扫光效果的图像组件（对应 Unity 侧 com.gameframex.unity.fairygui.unity 的 GImageSweepLight）。
    /// 继承自 GImage，添加扫光效果的参数控制功能，保持双端接口一致。
    /// </summary>
    public class GImageSweepLight : GImage
    {
        /// <summary>
        /// 扫光材质
        /// </summary>
        private ShaderMaterial _sweepLightMaterial;

        /// <summary>
        /// 扫光时间（秒）
        /// </summary>
        /// <value>扫光从开始到结束的持续时间</value>
        public float lightTime
        {
            get { return GetMaterialFloat("light_time", 0.6f); }
            set { SetMaterialFloat("light_time", value); }
        }

        /// <summary>
        /// 扫光厚度
        /// </summary>
        /// <value>扫光带的宽度，范围通常为 0-1</value>
        public float lightThick
        {
            get { return GetMaterialFloat("light_thick", 0.3f); }
            set { SetMaterialFloat("light_thick", value); }
        }

        /// <summary>
        /// 纹理。重写以在替换纹理时同步图集 UV 区域到扫光材质
        /// （Godot 移植的网格使用图集子区域 UV，Shader 需要该区域做归一化）。
        /// </summary>
        public new NTexture texture
        {
            get { return base.texture; }
            set
            {
                base.texture = value;
                if (_sweepLightMaterial != null)
                {
                    SweepLightManager.ApplyTextureUv(_sweepLightMaterial, value);
                }
            }
        }


        /// <summary>
        /// 下次扫光间隔时间（秒）
        /// </summary>
        /// <value>两次扫光之间的等待时间</value>
        public float nextTime
        {
            get { return GetMaterialFloat("next_time", 2.0f); }
            set { SetMaterialFloat("next_time", value); }
        }

        /// <summary>
        /// 扫光角度
        /// </summary>
        /// <value>扫光的倾斜角度，范围 0-360</value>
        public float lightAngle
        {
            get { return GetMaterialFloat("light_angle", 45.0f); }
            set { SetMaterialFloat("light_angle", Mathf.Clamp(value, 0f, 360f)); }
        }

        /// <summary>
        /// 扫光强度
        /// </summary>
        /// <value>扫光的亮度强度，范围 0-3</value>
        public float lightIntensity
        {
            get { return GetMaterialFloat("light_intensity", 1.4f); }
            set { SetMaterialFloat("light_intensity", Mathf.Clamp(value, 0f, 3f)); }
        }

        /// <summary>
        /// 是否启用扫光效果
        /// </summary>
        /// <value>true 表示启用扫光效果，false 表示禁用</value>
        public bool sweepLightEnabled
        {
            get { return _sweepLightMaterial != null && IsSweepLightEnabled(_sweepLightMaterial); }
            set
            {
                EnsureSweepLightMaterial();
                if (_sweepLightMaterial == null)
                {
                    return;
                }

                if (value)
                {
                    SweepLightManager.EnableSweepLight(_sweepLightMaterial);
                }
                else
                {
                    SweepLightManager.DisableSweepLight(_sweepLightMaterial);
                }
            }
        }


        /// <summary>
        /// 创建显示对象
        /// </summary>
        override protected void CreateDisplayObject()
        {
            base.CreateDisplayObject();
            InitializeSweepLightMaterial();
        }

        /// <summary>
        /// 初始化扫光材质
        /// </summary>
        private void InitializeSweepLightMaterial()
        {
            if (SweepLightManager.SweepLightShader == null)
            {
                GD.PushWarning("GSweepLightImage: 未找到 FairyGUI/ImageSweepLight Shader");
                return;
            }

            _sweepLightMaterial = SweepLightManager.CreateDefaultSweepLightMaterial();
            material = _sweepLightMaterial;
        }

        /// <summary>
        /// 确保扫光材质已创建
        /// </summary>
        private void EnsureSweepLightMaterial()
        {
            if (_sweepLightMaterial == null)
            {
                InitializeSweepLightMaterial();
            }
        }

        /// <summary>
        /// 获取材质的浮点参数值
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>参数值</returns>
        private float GetMaterialFloat(string parameterName, float defaultValue)
        {
            if (_sweepLightMaterial != null)
            {
                var value = _sweepLightMaterial.GetShaderParameter(parameterName);
                if (value.VariantType == Variant.Type.Float)
                {
                    return value.AsSingle();
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置材质的浮点参数值
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="value">参数值</param>
        private void SetMaterialFloat(string parameterName, float value)
        {
            EnsureSweepLightMaterial();
            _sweepLightMaterial?.SetShaderParameter(parameterName, value);
        }

        private static bool IsSweepLightEnabled(ShaderMaterial material)
        {
            var value = material.GetShaderParameter("sweep_light_enabled");
            return value.VariantType == Variant.Type.Bool && value.AsBool();
        }

        /// <summary>
        /// 开始扫光效果
        /// 重置时间参数以立即开始新的扫光循环
        /// </summary>
        public void StartSweepLight()
        {
            sweepLightEnabled = true;
        }

        /// <summary>
        /// 停止扫光效果
        /// </summary>
        public void StopSweepLight()
        {
            sweepLightEnabled = false;
        }

        /// <summary>
        /// 设置扫光参数
        /// </summary>
        /// <param name="sweepLightTime">扫光时间</param>
        /// <param name="sweepLightThick">扫光厚度</param>
        /// <param name="sweepNextTime">下次扫光间隔</param>
        /// <param name="sweepLightAngle">扫光角度</param>
        /// <param name="sweepLightIntensity">扫光强度</param>
        public void SetSweepLightParameters(float sweepLightTime, float sweepLightThick, float sweepNextTime, float sweepLightAngle, float sweepLightIntensity)
        {
            this.lightTime = sweepLightTime;
            this.lightThick = sweepLightThick;
            this.nextTime = sweepNextTime;
            this.lightAngle = sweepLightAngle;
            this.lightIntensity = sweepLightIntensity;
        }

        /// <summary>
        /// 销毁时清理资源
        /// </summary>
        public override void Dispose()
        {
            if (_sweepLightMaterial != null)
            {
                _sweepLightMaterial.Dispose();
                _sweepLightMaterial = null;
            }

            base.Dispose();
        }
    }
}
