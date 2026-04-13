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
//  Any legal disputes and liabilities arising from secondary development based on this project
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
using System.IO;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.UI.Runtime
{
    public partial class UIComponent
    {
        private const int DefaultOpenUIRetryFrames = 300;

        /// <summary>
        /// 通过资源路径打开界面（调用侧不需要传类型）。
        /// </summary>
        /// <param name="uiFormAssetPath">界面资源路径，例如 res://xxx/UILogin.tscn。</param>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">类型解析失败或打开失败时抛出异常。</exception>
        public async Task<IUIForm> OpenUI(string uiFormAssetPath, object userData = null, int maxRetryFrames = DefaultOpenUIRetryFrames)
        {
            if (string.IsNullOrWhiteSpace(uiFormAssetPath))
            {
                throw new GameFrameworkException("[UIComponent] OpenUI failed: uiFormAssetPath is empty.");
            }

            var uiFormType = ResolveUIFormTypeFromScene(uiFormAssetPath);

            if (uiFormType == null)
            {
                throw new GameFrameworkException($"[UIComponent] OpenUI failed: can not resolve IUIForm type by path={uiFormAssetPath}");
            }

            if (!typeof(IUIForm).IsAssignableFrom(uiFormType))
            {
                throw new GameFrameworkException($"[UIComponent] OpenUI failed: type does not implement IUIForm. type={uiFormType.FullName}, path={uiFormAssetPath}");
            }

            if (!await WaitForRuntimeInitializedAsync(maxRetryFrames))
            {
                throw new GameFrameworkException($"[UIComponent] OpenUI failed: runtime not initialized. type={uiFormType.FullName}, path={uiFormAssetPath}");
            }

            var uiFormAssetDirectory = ResolveUIAssetDirectory(uiFormAssetPath, uiFormType.Name);
            var ui = await OpenUIAsync(uiFormAssetDirectory, uiFormType, true, userData, true);
            if (ui != null)
            {
                return ui;
            }

            throw new GameFrameworkException($"[UIComponent] OpenUI failed: open ui returned null. type={uiFormType.FullName}, path={uiFormAssetPath}, directory={uiFormAssetDirectory}");
        }

        /// <summary>
        /// 通过资源名打开界面（先挂载 PCK，再按资源名匹配资源路径）。
        /// </summary>
        /// <param name="uiResourceName">界面资源名，例如 UILogin 或 UILogin.tscn。</param>
        /// <param name="packageName">可选包名；为空时按已挂载包进行匹配。</param>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">挂载失败或资源匹配失败时抛出异常。</exception>
        public async Task<IUIForm> OpenUI(string uiResourceName, string packageName, object userData = null, int maxRetryFrames = DefaultOpenUIRetryFrames)
        {
            if (string.IsNullOrWhiteSpace(uiResourceName))
            {
                throw new GameFrameworkException("[UIComponent] OpenUI(resourceName) failed: uiResourceName is empty.");
            }

            var normalizedPackageName = string.IsNullOrWhiteSpace(packageName) ? null : packageName.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedPackageName))
            {
                var pckPath = global::GameFrameX.AssetSystem.GodotAssetPath.GetHotfixPckFileVirtual(normalizedPackageName);
                var mounted = global::GameFrameX.AssetSystem.AssetSystem.MountGodotResourcePackByPath(pckPath, replaceFiles: false);
                if (!mounted)
                {
                    throw new GameFrameworkException($"[UIComponent] OpenUI(resourceName) failed: package mount failed. package={normalizedPackageName}, pck={pckPath}");
                }
            }

            var resolvedPath = ResolveUIResourcePathByName(uiResourceName, normalizedPackageName);
            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                throw new GameFrameworkException($"[UIComponent] OpenUI(resourceName) failed: resource not found after pck mount. resource={uiResourceName}, package={normalizedPackageName ?? "<all-mounted>"}");
            }

            return await OpenUI(resolvedPath, userData, maxRetryFrames);
        }

        /// <summary>
        /// 通过“资源名 + 包名”解析 UI 场景路径。
        /// 调用前应先完成目标 PCK 挂载；这里仅负责在已挂载资源中按名称候选（name/name.tscn/name.scn）匹配，
        /// 命中后返回可直接传给 OpenUI(path) 的 res:// 资源路径。
        /// </summary>
        private static string ResolveUIResourcePathByName(string uiResourceName, string packageName)
        {
            foreach (var candidate in EnumerateUIResourceNameCandidates(uiResourceName))
            {
                var scene = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackageAsset<PackedScene>(candidate, packageName);
                if (scene != null && !string.IsNullOrWhiteSpace(scene.ResourcePath))
                {
                    return scene.ResourcePath;
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> EnumerateUIResourceNameCandidates(string uiResourceName)
        {
            var normalized = uiResourceName.Trim().Replace('\\', '/');
            if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
            {
                normalized = Path.GetFileName(normalized);
            }
            else
            {
                var lastSlash = normalized.LastIndexOf('/');
                if (lastSlash >= 0)
                {
                    normalized = normalized[(lastSlash + 1)..];
                }
            }

            if (string.IsNullOrWhiteSpace(normalized))
            {
                yield break;
            }

            yield return normalized;
            if (Path.HasExtension(normalized))
            {
                yield break;
            }

            yield return normalized + ".tscn";
            yield return normalized + ".scn";
        }

        private static Type ResolveUIFormTypeFromScene(string uiFormAssetPath)
        {
            foreach (var scenePath in EnumeratePackedScenePathCandidates(uiFormAssetPath))
            {
                var scene = ResourceLoader.Load<PackedScene>(scenePath);
                if (scene == null)
                {
                    continue;
                }

                Node node = null;
                try
                {
                    node = scene.Instantiate();
                    if (node is IUIForm uiForm)
                    {
                        return uiForm.GetType();
                    }

                    GD.PushWarning($"[UIComponent] ResolveUIFormTypeFromScene root is not IUIForm. path={scenePath} rootType={node.GetType().FullName}");
                }
                catch (Exception exception)
                {
                    GD.PushWarning($"[UIComponent] ResolveUIFormTypeFromScene instantiate failed. path={scenePath} error={exception.Message}");
                }
                finally
                {
                    node?.Free();
                }
            }

            return null;
        }

        private static IEnumerable<string> EnumeratePackedScenePathCandidates(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                yield break;
            }

            var normalized = assetPath.Replace('\\', '/').Trim();
            if (Path.HasExtension(normalized))
            {
                yield return normalized;
                yield break;
            }

            yield return normalized + ".tscn";
            yield return normalized + ".scn";
        }

        private static string ResolveUIAssetDirectory(string uiFormAssetPath, string uiFormTypeName)
        {
            var normalized = uiFormAssetPath.Replace('\\', '/').Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return uiFormAssetPath;
            }

            if (Path.HasExtension(normalized))
            {
                var lastSlash = normalized.LastIndexOf('/');
                if (lastSlash > 0)
                {
                    return normalized.Substring(0, lastSlash);
                }

                return normalized;
            }

            if (!string.IsNullOrWhiteSpace(uiFormTypeName) &&
                normalized.EndsWith("/" + uiFormTypeName, StringComparison.OrdinalIgnoreCase))
            {
                var index = normalized.LastIndexOf('/');
                if (index > 0)
                {
                    return normalized.Substring(0, index);
                }
            }

            return normalized.TrimEnd('/');
        }

        /// <summary>
        /// 异步打开全屏UI。
        /// </summary>
        /// <param name="uiFormAssetPath">界面所在路径</param>
        /// <typeparam name="T">UI的具体类型。</typeparam>
        /// <param name="userData">传递给UI的用户数据。</param>
        /// <returns>返回打开的UI实例。</returns>
        public async Task<T> OpenFullScreenAsync<T>(string uiFormAssetPath, object userData = null) where T : class, IUIForm
        {
            return await OpenUIFormAsync<T>(uiFormAssetPath, true, userData, true);
        }

        /// <summary>
        /// 异步打开全屏UI。
        /// </summary>
        /// <typeparam name="T">UI的具体类型。</typeparam>
        /// <param name="userData">传递给UI的用户数据。</param>
        /// <returns>返回打开的UI实例。</returns>
        public async Task<T> OpenFullScreenAsync<T>(object userData = null) where T : class, IUIForm
        {
            var uiFormAssetName = typeof(T).Name;
            var uiFormAssetPath = Utility.Asset.Path.GetUIPath(uiFormAssetName);
            return await OpenFullScreenAsync<T>(uiFormAssetPath, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetPath">界面所在路径</param>
        /// <param name="uiFormType">界面逻辑类型。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public async Task<IUIForm> OpenUIAsync(string uiFormAssetPath, Type uiFormType, bool pauseCoveredUIForm, object userData = null, bool isFullScreen = false)
        {
            return await m_UIManager.OpenUIFormAsync(uiFormAssetPath, uiFormType, pauseCoveredUIForm, userData, isFullScreen);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetPath">界面所在路径</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        private async Task<T> OpenUIFormAsync<T>(string uiFormAssetPath, bool pauseCoveredUIForm, object userData = null, bool isFullScreen = false) where T : class, IUIForm
        {
            if (!EnsureRuntimeInitialized())
            {
                GD.PushError($"[UIComponent] OpenUIFormAsync blocked: runtime not initialized. type={typeof(T).FullName} path={uiFormAssetPath}");
                return null;
            }

            var ui = await m_UIManager.OpenUIFormAsync<T>(uiFormAssetPath, pauseCoveredUIForm, userData, isFullScreen);
            return ui as T;
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public async Task<T> OpenUIFormAsync<T>(bool pauseCoveredUIForm, object userData = null, bool isFullScreen = false) where T : class, IUIForm
        {
            var uiFormAssetName = typeof(T).Name;
            var uiFormAssetPath = Utility.Asset.Path.GetUIPath(uiFormAssetName);
            return await OpenUIFormAsync<T>(uiFormAssetPath, pauseCoveredUIForm, userData, isFullScreen);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetPath">界面所在路径</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public async Task<T> OpenAsync<T>(string uiFormAssetPath, object userData = null, bool isFullScreen = false) where T : class, IUIForm
        {
            return await OpenUIFormAsync<T>(uiFormAssetPath, false, userData, isFullScreen);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetPath">界面所在路径</param>
        /// <param name="pauseCoveredUIForm">是否暂停覆盖的UI</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public async Task<T> OpenAsync<T>(string uiFormAssetPath, bool pauseCoveredUIForm, object userData = null, bool isFullScreen = false) where T : class, IUIForm
        {
            return await OpenUIFormAsync<T>(uiFormAssetPath, pauseCoveredUIForm, userData, isFullScreen);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public async Task<T> OpenAsync<T>(object userData = null, bool isFullScreen = false) where T : class, IUIForm
        {
            var uiFormAssetName = typeof(T).Name;
            var uiFormAssetPath = Utility.Asset.Path.GetUIPath(uiFormAssetName);
            return await OpenAsync<T>(uiFormAssetPath, userData, isFullScreen);
        }

        private async Task<bool> WaitForRuntimeInitializedAsync(int maxRetryFrames)
        {
            var retryFrames = Mathf.Max(1, maxRetryFrames);
            if (EnsureRuntimeInitialized())
            {
                return true;
            }

            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree == null)
            {
                GD.PushError("[UIComponent] WaitForRuntimeInitializedAsync failed: Engine.MainLoop is not SceneTree.");
                return false;
            }

            for (var i = 0; i < retryFrames; i++)
            {
                if (EnsureRuntimeInitialized())
                {
                    return true;
                }

                if (i == 0 || i % 60 == 0)
                {
                    GD.PushWarning($"[UIComponent] waiting runtime initialization... retry={i}/{retryFrames}");
                }

                await ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
            }

            return EnsureRuntimeInitialized();
        }
    }
}
