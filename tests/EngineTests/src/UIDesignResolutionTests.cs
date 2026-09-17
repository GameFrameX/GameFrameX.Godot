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
using System.Threading.Tasks;
using GameFrameX.UI.Runtime;
using Godot;
namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 组 E：UI 设计分辨率组件（Phase 3.2 迁移自 Unity com.gameframex.unity.ui 的 UIDesignResolutionComponent）。
    /// 断言：默认配置与钳制、配置到 Window content scale 的映射、分辨率变化回调触发。
    /// </summary>
    public static partial class UIDesignResolutionTests
    {
        public static void Register(List<EngineTestCase> cases)
        {
            cases.Add(new EngineTestCase("UIDesignResolution", "E1_DefaultConfigAndClamp", DefaultConfigAsync));
            cases.Add(new EngineTestCase("UIDesignResolution", "E2_ContentScaleMapping", ContentScaleMappingAsync));
            cases.Add(new EngineTestCase("UIDesignResolution", "E3_ResolutionChangedCallback", ResolutionChangedAsync));
            cases.Add(new EngineTestCase("UIDesignResolution", "E4_GroupHelperAppliesDesignResolution", GroupHelperAppliesDesignResolutionAsync));
        }
        private static async Task DefaultConfigAsync()
        {
            var window = new Window();
            EngineTestContext.Root.AddChild(window);
            var component = new UIDesignResolutionComponent();
            window.AddChild(component);
            try
            {
                EngineAssert.Equal(UIDesignResolutionComponent.UIScaleMode.ScaleWithScreenSize, component.ScaleMode, "default ScaleMode");
                EngineAssert.Equal(UIDesignResolutionComponent.UIScreenMatchMode.MatchWidthOrHeight, component.ScreenMatchMode, "default ScreenMatchMode");
                EngineAssert.Equal(1920, component.DesignWidth, "default DesignWidth");
                EngineAssert.Equal(1080, component.DesignHeight, "default DesignHeight");
                EngineAssert.Equal(0.5f, component.MatchWidthOrHeight, "default MatchWidthOrHeight");
                // _Ready 已按默认配置写入 Window content scale。
                EngineAssert.Equal(Window.ContentScaleModeEnum.CanvasItems, window.ContentScaleMode, "default applied mode");
                EngineAssert.Equal(new Vector2I(1920, 1080), window.ContentScaleSize, "default applied size");
                // 属性钳制语义（对齐 Unity Mathf.Max/Clamp01）。
                component.DesignWidth = 0;
                EngineAssert.Equal(1, component.DesignWidth, "DesignWidth clamped to 1");
                component.MatchWidthOrHeight = 2f;
                EngineAssert.Equal(1f, component.MatchWidthOrHeight, "MatchWidthOrHeight clamped to 1");
                component.MatchWidthOrHeight = -1f;
                EngineAssert.Equal(0f, component.MatchWidthOrHeight, "MatchWidthOrHeight clamped to 0");
                component.ConstantScaleFactor = 0f;
                EngineAssert.Equal(0.01f, component.ConstantScaleFactor, "ConstantScaleFactor clamped to 0.01");
            }
            finally
            {
                window.QueueFree();
            }
            await EngineTestWait.FramesAsync(1);
        }
        private static async Task ContentScaleMappingAsync()
        {
            var window = new Window();
            EngineTestContext.Root.AddChild(window);
            var component = new UIDesignResolutionComponent();
            window.AddChild(component);
            try
            {
                component.DesignWidth = 1280;
                component.DesignHeight = 720;
                component.ScreenMatchMode = UIDesignResolutionComponent.UIScreenMatchMode.MatchWidth;
                component.Apply();
                EngineAssert.Equal(Window.ContentScaleModeEnum.CanvasItems, window.ContentScaleMode, "canvas-items mode");
                EngineAssert.Equal(new Vector2I(1280, 720), window.ContentScaleSize, "design size applied");
                EngineAssert.Equal(Window.ContentScaleAspectEnum.KeepWidth, window.ContentScaleAspect, "match width aspect");
                component.ScreenMatchMode = UIDesignResolutionComponent.UIScreenMatchMode.MatchHeight;
                component.Apply();
                EngineAssert.Equal(Window.ContentScaleAspectEnum.KeepHeight, window.ContentScaleAspect, "match height aspect");
                component.ScreenMatchMode = UIDesignResolutionComponent.UIScreenMatchMode.MatchWidthOrHeight;
                component.Apply();
                EngineAssert.Equal(Window.ContentScaleAspectEnum.Expand, window.ContentScaleAspect, "match width-or-height aspect");
                component.ScaleMode = UIDesignResolutionComponent.UIScaleMode.ConstantPixelSize;
                component.ConstantScaleFactor = 2f;
                component.Apply();
                EngineAssert.Equal(Window.ContentScaleModeEnum.Disabled, window.ContentScaleMode, "constant pixel mode");
                EngineAssert.Equal(2f, window.ContentScaleFactor, "constant scale factor");
            }
            finally
            {
                window.QueueFree();
            }
            await EngineTestWait.FramesAsync(1);
        }
        private static async Task ResolutionChangedAsync()
        {
            var subViewport = new SubViewport();
            subViewport.Size = new Vector2I(100, 100);
            EngineTestContext.Root.AddChild(subViewport);
            var component = new UIDesignResolutionComponent();
            subViewport.AddChild(component);
            try
            {
                var fired = 0;
                component.ResolutionChanged += (sender, args) => fired++;
                subViewport.Size = new Vector2I(200, 150);
                await EngineTestWait.UntilDoneAsync(() => fired > 0, "resolution changed callback", 5000);
                EngineAssert.True(fired > 0, "ResolutionChanged fired on viewport size change");
            }
            finally
            {
                subViewport.QueueFree();
            }
            await EngineTestWait.FramesAsync(1);
        }

        private static async Task GroupHelperAppliesDesignResolutionAsync()
        {
            // 无 UIComponent 祖先：建组直通，不创建设计分辨率组件（对齐 Unity 空配置安全路径）。
            var orphanRoot = new Node();
            var orphanHelper = new GameFrameX.UI.GDGUI.Runtime.GDGUIUIGroupHelper().Handler(orphanRoot, "OrphanGroup", null, null, 0);
            EngineAssert.True(orphanHelper != null, "group helper created without UIComponent ancestor");
            EngineAssert.True(orphanRoot.FindChild(nameof(UIDesignResolutionComponent), true, false) == null, "no design resolution component without UIComponent ancestor");
            orphanRoot.Free();

            // 带 UIComponent 祖先：建组应触发设计分辨率组件创建并把默认设计分辨率写入 Window content scale。
            // 用静默子类抑制 UIComponent._Ready 的框架注册副作用，只验证设计分辨率链路。
            var window = new Window();
            EngineTestContext.Root.AddChild(window);
            var uiComponent = new SilentUIComponent();
            window.AddChild(uiComponent);
            var root = new Node
            {
                Name = "GDGUI"
            };
            uiComponent.AddChild(root);
            try
            {
                var helper = new GameFrameX.UI.GDGUI.Runtime.GDGUIUIGroupHelper().Handler(root, "ResolutionGroup", null, null, 0);
                EngineAssert.True(helper != null, "gdgui group helper created with UIComponent ancestor");
                EngineAssert.True(uiComponent.DesignResolution != null, "design resolution ensured by group helper");
                EngineAssert.Equal(Window.ContentScaleModeEnum.CanvasItems, window.ContentScaleMode, "content scale mode written by group helper");
                EngineAssert.Equal(new Vector2I(1920, 1080), window.ContentScaleSize, "design size written by group helper");

                // FairyGUI 组辅助器同样应用设计分辨率，并强制 Stage/GRoot 重算缩放（不抛错即通过链路）。
                var fairyGuiHelper = new GameFrameX.UI.FairyGUI.Runtime.FairyGUIUIGroupHelper().Handler(root, "FairyGroup", null, null, 0);
                EngineAssert.True(fairyGuiHelper != null, "fairygui group helper created with UIComponent ancestor");
                EngineAssert.Equal(Window.ContentScaleModeEnum.CanvasItems, window.ContentScaleMode, "content scale mode kept after fairygui group helper");
                EngineAssert.Equal(new Vector2I(1920, 1080), window.ContentScaleSize, "design size kept after fairygui group helper");
            }
            finally
            {
                window.QueueFree();
            }

            await EngineTestWait.FramesAsync(1);
        }

        /// <summary>
        /// 抑制 _Ready 的 UIComponent 测试替身：避免触发框架组件注册与初始化重试。
        /// </summary>
        private sealed partial class SilentUIComponent : UIComponent
        {
            public override void _Ready()
            {
            }
        }
    }
}
