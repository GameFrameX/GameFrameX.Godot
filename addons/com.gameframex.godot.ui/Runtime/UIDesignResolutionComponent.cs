// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using Godot;
namespace GameFrameX.UI.Runtime
{
	/// <summary>
	/// UI 设计分辨率配置组件。
	/// </summary>
	/// <remarks>
	/// 迁移自 Unity com.gameframex.unity.ui 的 UIDesignResolutionComponent。Unity 侧为纯配置组件；
	/// Godot 侧在保留全部配置语义的基础上，把配置映射到 Viewport 的 content scale 机制，
	/// 并在分辨率变化时触发 <see cref="ResolutionChanged"/> 回调。
	/// </remarks>
	public sealed partial class UIDesignResolutionComponent : Node
	{
		/// <summary>
		/// UI 缩放模式。
		/// </summary>
		public enum UIScaleMode
		{
			ConstantPixelSize,
			ScaleWithScreenSize,
			ConstantPhysicalSize
		}

		/// <summary>
		/// 屏幕适配方式。
		/// </summary>
		public enum UIScreenMatchMode
		{
			MatchWidthOrHeight,
			MatchWidth,
			MatchHeight
		}

		[Export] private UIScaleMode m_ScaleMode = UIScaleMode.ScaleWithScreenSize;
		[Export] private UIScreenMatchMode m_ScreenMatchMode = UIScreenMatchMode.MatchWidthOrHeight;
		[Export] private int m_DesignWidth = 1920;
		[Export] private int m_DesignHeight = 1080;
		[Export(PropertyHint.Range, "0,1")]
		private float m_MatchWidthOrHeight = 0.5f;
		[Export] private float m_ConstantScaleFactor = 1f;
		[Export] private float m_ReferencePixelsPerUnit = 100f;
		[Export] private float m_FallbackScreenDPI = 96f;
		[Export] private float m_DefaultSpriteDPI = 96f;
		[Export] private bool m_IgnoreOrientation = false;

		/// <summary>
		/// 分辨率（Viewport 尺寸）变化回调，随 Viewport 的 size_changed 信号触发。
		/// </summary>
		public event EventHandler ResolutionChanged;

		/// <summary>
		/// 获取或设置 UI 缩放模式。
		/// </summary>
		public UIScaleMode ScaleMode
		{
			get { return m_ScaleMode; }
			set { m_ScaleMode = value; }
		}

		/// <summary>
		/// 获取或设置屏幕适配方式。
		/// </summary>
		public UIScreenMatchMode ScreenMatchMode
		{
			get { return m_ScreenMatchMode; }
			set { m_ScreenMatchMode = value; }
		}

		/// <summary>
		/// 获取或设置设计分辨率宽度。
		/// </summary>
		public int DesignWidth
		{
			get { return Mathf.Max(1, m_DesignWidth); }
			set { m_DesignWidth = Mathf.Max(1, value); }
		}

		/// <summary>
		/// 获取或设置设计分辨率高度。
		/// </summary>
		public int DesignHeight
		{
			get { return Mathf.Max(1, m_DesignHeight); }
			set { m_DesignHeight = Mathf.Max(1, value); }
		}

		/// <summary>
		/// 获取或设置宽高混合适配权重。
		/// </summary>
		public float MatchWidthOrHeight
		{
			get { return Mathf.Clamp(m_MatchWidthOrHeight, 0f, 1f); }
			set { m_MatchWidthOrHeight = Mathf.Clamp(value, 0f, 1f); }
		}

		/// <summary>
		/// 获取或设置固定像素缩放值。
		/// </summary>
		public float ConstantScaleFactor
		{
			get { return Mathf.Max(0.01f, m_ConstantScaleFactor); }
			set { m_ConstantScaleFactor = Mathf.Max(0.01f, value); }
		}

		/// <summary>
		/// 获取或设置每单位参考像素。
		/// </summary>
		public float ReferencePixelsPerUnit
		{
			get { return Mathf.Max(1f, m_ReferencePixelsPerUnit); }
			set { m_ReferencePixelsPerUnit = Mathf.Max(1f, value); }
		}

		/// <summary>
		/// 获取或设置备用屏幕 DPI。
		/// </summary>
		public float FallbackScreenDPI
		{
			get { return Mathf.Max(1f, m_FallbackScreenDPI); }
			set { m_FallbackScreenDPI = Mathf.Max(1f, value); }
		}

		/// <summary>
		/// 获取或设置默认精灵 DPI。
		/// </summary>
		public float DefaultSpriteDPI
		{
			get { return Mathf.Max(1f, m_DefaultSpriteDPI); }
			set { m_DefaultSpriteDPI = Mathf.Max(1f, value); }
		}

		/// <summary>
		/// 获取或设置是否忽略横竖屏方向。
		/// </summary>
		public bool IgnoreOrientation
		{
			get { return m_IgnoreOrientation; }
			set { m_IgnoreOrientation = value; }
		}

		/// <summary>
		/// 组件就绪后应用一次配置并订阅分辨率变化。
		/// </summary>
		public override void _Ready()
		{
			Apply();
			var viewport = GetViewport();
			if (viewport != null)
			{
				viewport.SizeChanged += OnViewportSizeChanged;
			}
		}

		/// <summary>
		/// 把当前配置应用到所在 Viewport 的 content scale。
		/// </summary>
		public void Apply()
		{
			var window = GetViewport() as Window;
			if (window == null)
			{
				return;
			}
			switch (m_ScaleMode)
			{
				case UIScaleMode.ConstantPixelSize:
				{
					window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
					window.ContentScaleFactor = ConstantScaleFactor;
					break;
				}
				case UIScaleMode.ScaleWithScreenSize:
				{
					window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
					window.ContentScaleSize = new Vector2I(DesignWidth, DesignHeight);
					window.ContentScaleAspect = ToContentScaleAspect(m_ScreenMatchMode);
					break;
				}
				case UIScaleMode.ConstantPhysicalSize:
				{
					// ponytail: Godot 无物理尺寸（DPI）适配，ConstantPhysicalSize 退化为固定缩放系数，
					// FallbackScreenDPI/DefaultSpriteDPI 仅作为配置保留。升级路径：接入 DisplayServer 屏幕 DPI 后
					// 按 DefaultSpriteDPI/FallbackScreenDPI 换算实际系数再写入 ContentScaleFactor。
					window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
					window.ContentScaleFactor = ConstantScaleFactor;
					break;
				}
			}
		}

		/// <summary>
		/// 离开场景树时解除分辨率变化订阅。
		/// </summary>
		public override void _ExitTree()
		{
			var viewport = GetViewport();
			if (viewport != null)
			{
				viewport.SizeChanged -= OnViewportSizeChanged;
			}
			base._ExitTree();
		}

		private static Window.ContentScaleAspectEnum ToContentScaleAspect(UIScreenMatchMode matchMode)
		{
			switch (matchMode)
			{
				case UIScreenMatchMode.MatchWidth:
				{
					return Window.ContentScaleAspectEnum.KeepWidth;
				}
				case UIScreenMatchMode.MatchHeight:
				{
					return Window.ContentScaleAspectEnum.KeepHeight;
				}
				default:
				{
					return Window.ContentScaleAspectEnum.Expand;
				}
			}
		}

		private void OnViewportSizeChanged()
		{
			Apply();
			if (ResolutionChanged != null)
			{
				ResolutionChanged(this, EventArgs.Empty);
			}
		}
	}
}
