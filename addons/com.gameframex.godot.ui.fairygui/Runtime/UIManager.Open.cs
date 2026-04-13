using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace GameFrameX.UI.FairyGUI.Runtime
{
    /// <summary>
    /// FairyGUI 界面管理器打开逻辑。
    /// </summary>
    internal sealed partial class UIManager
    {
        private readonly List<UIFormLoadingObject> m_LoadingUIForms = new List<UIFormLoadingObject>(64);
        private readonly List<UIFormLoadingObject> m_UIFormsRemoveList = new List<UIFormLoadingObject>(64);

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        /// <param name="uiFormAssetPath">界面资源目录。</param>
        /// <param name="uiFormType">界面类型。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖界面。</param>
        /// <param name="userData">用户数据。</param>
        /// <param name="isFullScreen">是否全屏。</param>
        /// <returns>界面实例。</returns>
        protected override async Task<IUIForm> InnerOpenUIFormAsync(string uiFormAssetPath, Type uiFormType, bool pauseCoveredUIForm, object userData, bool isFullScreen = false)
        {
            GameFrameworkGuard.NotNull(m_UIFormHelper, nameof(m_UIFormHelper));
            GameFrameworkGuard.NotNull(uiFormType, nameof(uiFormType));

            var uiFormAssetName = uiFormType.Name;
            var assetPath = PathHelper.Combine(uiFormAssetPath, uiFormAssetName);
            var uiFormInstanceObject = m_InstancePool.Spawn(assetPath);
            if (uiFormInstanceObject != null)
            {
                return InternalOpenUIForm(-1, uiFormAssetName, uiFormType, uiFormInstanceObject.Target, pauseCoveredUIForm, false, 0f, userData, isFullScreen);
            }

            var uiFormTask = InnerLoadUIFormAsync(uiFormAssetPath, uiFormType, pauseCoveredUIForm, userData, isFullScreen, uiFormAssetName, assetPath);
            var loadingObject = UIFormLoadingObject.Create(uiFormAssetPath, uiFormAssetName, uiFormType, uiFormTask);
            m_LoadingUIForms.Add(loadingObject);

            var result = await uiFormTask;

            foreach (var value in m_LoadingUIForms)
            {
                if (value.UIFormAssetPath == uiFormAssetPath && value.UIFormAssetName == uiFormAssetName && value.UIFormType == uiFormType)
                {
                    m_UIFormsRemoveList.Add(value);
                }
            }

            foreach (var value in m_UIFormsRemoveList)
            {
                m_LoadingUIForms.Remove(value);
                ReferencePool.Release(value);
            }

            m_UIFormsRemoveList.Clear();
            return result;
        }

        /// <summary>
        /// 异步加载界面资源并创建实例。
        /// </summary>
        /// <param name="uiFormAssetPath">界面资源目录。</param>
        /// <param name="uiFormType">界面类型。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖界面。</param>
        /// <param name="userData">用户数据。</param>
        /// <param name="isFullScreen">是否全屏。</param>
        /// <param name="uiFormAssetName">界面资源名。</param>
        /// <param name="assetPath">资源完整路径。</param>
        /// <returns>界面实例。</returns>
        private Task<IUIForm> InnerLoadUIFormAsync(string uiFormAssetPath, Type uiFormType, bool pauseCoveredUIForm, object userData, bool isFullScreen, string uiFormAssetName, string assetPath)
        {
            var serialId = ++m_Serial;
            m_UIFormsBeingLoaded.Add(serialId, uiFormAssetName);
            var openUIFormInfo = OpenUIFormInfo.Create(serialId, assetPath, uiFormAssetName, uiFormType, pauseCoveredUIForm, userData, isFullScreen);

            // 优先使用 Godot 的 PackedScene 直接加载，兼容 res:// 与绝对路径。
            var packedScene = LoadPackedScene(assetPath);
            if (packedScene != null)
            {
                return Task.FromResult(LoadAssetSuccessCallback(assetPath, packedScene, 1f, openUIFormInfo));
            }

            return Task.FromResult(LoadAssetFailureCallback(assetPath, $"PackedScene load failed for path: {assetPath}", openUIFormInfo));
        }

        /// <summary>
        /// 执行界面实例创建和打开流程。
        /// </summary>
        /// <param name="serialId">序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名。</param>
        /// <param name="uiFormType">界面类型。</param>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖界面。</param>
        /// <param name="isNewInstance">是否新实例。</param>
        /// <param name="duration">加载耗时。</param>
        /// <param name="userData">用户数据。</param>
        /// <param name="isFullScreen">是否全屏。</param>
        /// <returns>界面实例。</returns>
        private IUIForm InternalOpenUIForm(int serialId, string uiFormAssetName, Type uiFormType, object uiFormInstance, bool pauseCoveredUIForm, bool isNewInstance, float duration, object userData, bool isFullScreen)
        {
            try
            {
                var uiForm = m_UIFormHelper.CreateUIForm(uiFormInstance, uiFormType, userData);
                if (uiForm == null)
                {
                    throw new GameFrameworkException("Can not create UI form in UI form helper.");
                }

                var uiGroup = uiForm.UIGroup;
                if (uiGroup == null)
                {
                    throw new GameFrameworkException("UI group is invalid.");
                }

                uiForm.Init(serialId, uiFormAssetName, uiGroup, null, pauseCoveredUIForm, isNewInstance, userData, RecycleInterval, isFullScreen);

                if (!uiGroup.InternalHasInstanceUIForm(uiFormAssetName, uiForm))
                {
                    uiGroup.AddUIForm(uiForm);
                }

                uiForm.OnOpen(userData);
                uiForm.BindEvent();
                uiForm.LoadData();
                uiForm.UpdateLocalization();
                if (uiForm.EnableShowAnimation)
                {
                    uiForm.Show(m_UIFormShowHandler, null);
                }

                uiGroup.Refresh();
                if (m_OpenUIFormSuccessEventHandler != null)
                {
                    var successArgs = OpenUIFormSuccessEventArgs.Create(uiForm, duration, userData);
                    m_OpenUIFormSuccessEventHandler(this, successArgs);
                }

                return uiForm;
            }
            catch (Exception exception)
            {
                if (m_OpenUIFormFailureEventHandler != null)
                {
                    var failureArgs = OpenUIFormFailureEventArgs.Create(serialId, uiFormAssetName, pauseCoveredUIForm, exception.ToString(), userData);
                    m_OpenUIFormFailureEventHandler(this, failureArgs);
                    return GetUIForm(failureArgs.SerialId);
                }

                throw;
            }
        }

        /// <summary>
        /// 处理资源加载成功。
        /// </summary>
        /// <param name="uiFormAssetPath">资源路径。</param>
        /// <param name="uiFormAsset">资源对象。</param>
        /// <param name="duration">加载耗时。</param>
        /// <param name="userData">用户数据。</param>
        /// <returns>界面实例。</returns>
        private IUIForm LoadAssetSuccessCallback(string uiFormAssetPath, object uiFormAsset, float duration, object userData)
        {
            var openUIFormInfo = userData as OpenUIFormInfo;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                var form = GetUIForm(openUIFormInfo.SerialId);
                m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                m_UIFormHelper.ReleaseUIForm(uiFormAsset, null, openUIFormInfo.AssetHandle, uiFormAssetPath, openUIFormInfo.AssetName);
                ReferencePool.Release(openUIFormInfo);
                return form;
            }

            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            var uiFormInstanceObject = UIFormInstanceObject.Create(
                uiFormAssetPath,
                openUIFormInfo.AssetName,
                uiFormAsset,
                m_UIFormHelper.InstantiateUIForm(uiFormAsset),
                m_UIFormHelper,
                openUIFormInfo.AssetHandle);
            m_InstancePool.Register(uiFormInstanceObject, true);

            var uiForm = InternalOpenUIForm(
                openUIFormInfo.SerialId,
                openUIFormInfo.AssetName,
                openUIFormInfo.FormType,
                uiFormInstanceObject.Target,
                openUIFormInfo.PauseCoveredUIForm,
                true,
                duration,
                openUIFormInfo.UserData,
                openUIFormInfo.IsFullScreen);
            ReferencePool.Release(openUIFormInfo);
            return uiForm;
        }

        /// <summary>
        /// 处理资源加载失败。
        /// </summary>
        /// <param name="uiFormAssetName">资源名。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户数据。</param>
        /// <returns>界面实例。</returns>
        private IUIForm LoadAssetFailureCallback(string uiFormAssetName, string errorMessage, object userData)
        {
            var openUIFormInfo = userData as OpenUIFormInfo;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                var uiForm = GetUIForm(openUIFormInfo.SerialId);
                m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                ReferencePool.Release(openUIFormInfo);
                return uiForm;
            }

            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            var appendErrorMessage = Utility.Text.Format("Load UI form failure, asset name '{0}', error message '{1}'.", uiFormAssetName, errorMessage);
            if (m_OpenUIFormFailureEventHandler != null)
            {
                var failureArgs = OpenUIFormFailureEventArgs.Create(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.PauseCoveredUIForm, appendErrorMessage, openUIFormInfo.UserData);
                m_OpenUIFormFailureEventHandler(this, failureArgs);
                return GetUIForm(openUIFormInfo.SerialId);
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        /// <summary>
        /// 解析并加载 PackedScene 资源。
        /// </summary>
        /// <param name="assetPath">界面资源路径。</param>
        /// <returns>PackedScene 实例。</returns>
        private PackedScene LoadPackedScene(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            foreach (var path in EnumeratePackedScenePathCandidates(assetPath))
            {
                var normalizedPath = NormalizeToResourcePath(path);
                if (normalizedPath == null || !ResourceLoader.Exists(normalizedPath))
                {
                    continue;
                }

                var scene = AssetSystemResources.Load<PackedScene>(normalizedPath);
                if (scene != null)
                {
                    return scene;
                }
            }

            // Fallback: match by resource name from mounted PCK/package manifest.
            // This keeps runtime package loading resilient when path mapping changes.
            var sceneName = Path.GetFileName(assetPath.Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            var sceneFromPackage = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackageAsset<PackedScene>(sceneName);
            if (sceneFromPackage != null)
            {
                return sceneFromPackage;
            }

            return null;
        }

        private static IEnumerable<string> EnumeratePackedScenePathCandidates(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                yield break;
            }

            if (Path.HasExtension(assetPath))
            {
                yield return assetPath;
                yield break;
            }

            yield return assetPath + ".tscn";
            yield return assetPath + ".scn";
        }

        /// <summary>
        /// 将路径规范化为 Godot 可加载的资源路径。
        /// </summary>
        /// <param name="path">输入路径。</param>
        /// <returns>规范化后路径。</returns>
        private string NormalizeToResourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var normalized = path.Replace('\\', '/');
            if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || normalized.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            const string marker = "/Godot/";
            var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                normalized = "res://" + normalized.Substring(index + marker.Length);
            }

            return normalized;
        }
    }
}

