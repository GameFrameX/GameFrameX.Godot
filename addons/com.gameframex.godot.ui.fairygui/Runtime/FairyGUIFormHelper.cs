using System;
using System.Reflection;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace GameFrameX.UI.FairyGUI.Runtime
{
    /// <summary>
    /// FairyGUI 界面辅助器。
    /// </summary>
    public partial class FairyGUIFormHelper : UIFormHelperBase
    {
        private UIComponent m_UIComponent;

        /// <summary>
        /// 初始化辅助器依赖。
        /// </summary>
        public override void _Ready()
        {
            m_UIComponent = GameEntry.GetComponent<UIComponent>();
            if (m_UIComponent == null)
            {
                Log.Warning("UI component is invalid.");
            }
        }

        /// <summary>
        /// 实例化界面资源。
        /// </summary>
        /// <param name="uiFormAsset">界面资源对象。</param>
        /// <returns>界面实例对象。</returns>
        public override object InstantiateUIForm(object uiFormAsset)
        {
            if (uiFormAsset is PackedScene packedScene)
            {
                Log.Info("[FairyGUIFormHelper] Instantiate begin scene={0}", packedScene.ResourcePath);
                var node = packedScene.Instantiate();
                Log.Info("[FairyGUIFormHelper] Instantiate done nodeType={0}", node?.GetType().FullName ?? "<null>");
                return node;
            }

            Log.Error("UI form asset is not a PackedScene.");
            return null;
        }

        /// <summary>
        /// 创建界面逻辑对象。
        /// </summary>
        /// <param name="uiFormInstance">界面实例对象。</param>
        /// <param name="uiFormType">界面类型。</param>
        /// <param name="userData">用户数据。</param>
        /// <returns>界面逻辑实例。</returns>
        public override IUIForm CreateUIForm(object uiFormInstance, Type uiFormType, object userData)
        {
            if (!(uiFormInstance is Node node))
            {
                Log.Error("UI form instance is not a Node.");
                return null;
            }

            Log.Info("[FairyGUIFormHelper] CreateUIForm begin type={0} node={1}", uiFormType?.FullName ?? "<null>", node.Name);

            var uiForm = node as IUIForm;
            if (uiForm == null)
            {
                uiForm = CreateUIFormByTypeFallback(node, uiFormType);
                if (uiForm == null)
                {
                    Log.Error("UI form instance is not of type IUIForm.");
                    return null;
                }
            }

            BindUIGroup(uiForm, uiFormType);
            BindAnimationOption(uiForm, uiFormType);

            if (!(uiForm.UIGroup?.Helper is Node helperNode))
            {
                Log.Error("UI group helper is invalid.");
                return null;
            }

            if (uiForm is not Node uiFormNode)
            {
                Log.Error("UI form is not a Node.");
                return null;
            }

            helperNode.AddChild(uiFormNode);
            if (uiFormNode is Control control)
            {
                SetFullScreen(control);
            }

            Log.Info("[FairyGUIFormHelper] CreateUIForm done type={0} group={1}", uiFormType?.FullName ?? "<null>", uiForm.UIGroup?.Name ?? "<null>");

            return uiForm;
        }

        /// <summary>
        /// 释放界面实例。
        /// </summary>
        /// <param name="uiFormAsset">界面资源对象。</param>
        /// <param name="uiFormInstance">界面实例对象。</param>
        /// <param name="assetHandle">资源句柄。</param>
        /// <param name="uiFormAssetPath">界面资源路径。</param>
        /// <param name="uiFormAssetName">界面资源名。</param>
        public override void ReleaseUIForm(object uiFormAsset, object uiFormInstance, object assetHandle, string uiFormAssetPath, string uiFormAssetName)
        {
            if (uiFormInstance is Node node && node.IsInsideTree())
            {
                node.QueueFree();
            }
        }

        /// <summary>
        /// 绑定界面所属分组。
        /// </summary>
        /// <param name="uiForm">界面实例。</param>
        /// <param name="uiFormType">界面类型。</param>
        private void BindUIGroup(IUIForm uiForm, Type uiFormType)
        {
            if (uiForm.UIGroup != null || m_UIComponent == null)
            {
                return;
            }

            var attribute = uiFormType?.GetCustomAttribute(typeof(OptionUIGroupAttribute)) as OptionUIGroupAttribute;
            if (attribute == null)
            {
                return;
            }

            uiForm.UIGroup = m_UIComponent.GetUIGroup(attribute.GroupName);
        }

        /// <summary>
        /// 绑定界面动画配置。
        /// </summary>
        /// <param name="uiForm">界面实例。</param>
        /// <param name="uiFormType">界面类型。</param>
        private void BindAnimationOption(IUIForm uiForm, Type uiFormType)
        {
            if (m_UIComponent == null)
            {
                return;
            }

            var showAttribute = uiFormType?.GetCustomAttribute(typeof(OptionUIShowAnimationAttribute)) as OptionUIShowAnimationAttribute;
            if (showAttribute != null)
            {
                uiForm.EnableShowAnimation = showAttribute.Enable;
                uiForm.ShowAnimationName = showAttribute.AnimationName;
            }
            else
            {
                uiForm.EnableShowAnimation = m_UIComponent.IsEnableUIShowAnimation;
            }

            var hideAttribute = uiFormType?.GetCustomAttribute(typeof(OptionUIHideAnimationAttribute)) as OptionUIHideAnimationAttribute;
            if (hideAttribute != null)
            {
                uiForm.EnableHideAnimation = hideAttribute.Enable;
                uiForm.HideAnimationName = hideAttribute.AnimationName;
            }
            else
            {
                uiForm.EnableHideAnimation = m_UIComponent.IsEnableUIHideAnimation;
            }
        }

        private static void SetFullScreen(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.LayoutMode = 3;
            control.AnchorsPreset = 15;
            control.AnchorRight = 1.0f;
            control.AnchorBottom = 1.0f;
            control.GrowHorizontal = Control.GrowDirection.Both;
            control.GrowVertical = Control.GrowDirection.Both;
            control.Position = Vector2.Zero;
        }

        private static IUIForm CreateUIFormByTypeFallback(Node sourceNode, Type uiFormType)
        {
            if (sourceNode == null || uiFormType == null || !typeof(IUIForm).IsAssignableFrom(uiFormType))
            {
                return null;
            }

            object created;
            try
            {
                created = Activator.CreateInstance(uiFormType);
            }
            catch (Exception exception)
            {
                Log.Warning("Create UI form fallback failed: can not create type '{0}', exception '{1}'.", uiFormType.FullName, exception.Message);
                return null;
            }

            if (created is not Node targetNode || created is not IUIForm uiForm)
            {
                return null;
            }

            if (sourceNode is Control sourceControl && targetNode is Control targetControl)
            {
                targetControl.LayoutMode = sourceControl.LayoutMode;
                targetControl.AnchorsPreset = sourceControl.AnchorsPreset;
                targetControl.AnchorLeft = sourceControl.AnchorLeft;
                targetControl.AnchorTop = sourceControl.AnchorTop;
                targetControl.AnchorRight = sourceControl.AnchorRight;
                targetControl.AnchorBottom = sourceControl.AnchorBottom;
                targetControl.OffsetLeft = sourceControl.OffsetLeft;
                targetControl.OffsetTop = sourceControl.OffsetTop;
                targetControl.OffsetRight = sourceControl.OffsetRight;
                targetControl.OffsetBottom = sourceControl.OffsetBottom;
                targetControl.GrowHorizontal = sourceControl.GrowHorizontal;
                targetControl.GrowVertical = sourceControl.GrowVertical;
            }

            targetNode.Name = sourceNode.Name;
            sourceNode.Name = "ViewRoot";
            targetNode.AddChild(sourceNode);
            if (sourceNode is Control sourceRootControl)
            {
                SetFullScreen(sourceRootControl);
            }

            Log.Warning("UI form fallback created by type '{0}' because scene root script instance is missing IUIForm. attachMode=ViewRootChild", uiFormType.FullName);
            return uiForm;
        }
    }
}

