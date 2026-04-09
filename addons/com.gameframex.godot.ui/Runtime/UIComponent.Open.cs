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
using System.Reflection;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.UI.Runtime
{
    public partial class UIComponent
    {
        private const int DefaultOpenRequiredRetryFrames = 300;

        /// <summary>
        /// 打开并确保成功显示全屏界面。
        /// </summary>
        /// <typeparam name="T">UI 的具体类型。</typeparam>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">打开失败时抛出异常。</exception>
        public async Task<T> OpenRequiredFullScreenAsync<T>(object userData = null, int maxRetryFrames = DefaultOpenRequiredRetryFrames) where T : class, IUIForm
        {
            if (!await WaitForRuntimeInitializedAsync(maxRetryFrames))
            {
                throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: runtime not initialized. type={typeof(T).FullName}");
            }

            var ui = await OpenAsync<T>(userData, true);
            if (ui != null)
            {
                return ui;
            }

            throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: open ui returned null. type={typeof(T).FullName}");
        }

        /// <summary>
        /// 打开并确保成功显示全屏界面（简写别名）。
        /// </summary>
        /// <typeparam name="T">UI 的具体类型。</typeparam>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">打开失败时抛出异常。</exception>
        public Task<T> OpenRequiredAsync<T>(object userData = null, int maxRetryFrames = DefaultOpenRequiredRetryFrames) where T : class, IUIForm
        {
            return OpenRequiredFullScreenAsync<T>(userData, maxRetryFrames);
        }

        /// <summary>
        /// 打开并确保成功显示全屏界面。
        /// </summary>
        /// <typeparam name="T">UI 的具体类型。</typeparam>
        /// <param name="uiFormAssetPath">界面所在路径。</param>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">打开失败时抛出异常。</exception>
        public async Task<T> OpenRequiredFullScreenAsync<T>(string uiFormAssetPath, object userData = null, int maxRetryFrames = DefaultOpenRequiredRetryFrames) where T : class, IUIForm
        {
            if (!await WaitForRuntimeInitializedAsync(maxRetryFrames))
            {
                throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: runtime not initialized. type={typeof(T).FullName}, path={uiFormAssetPath}");
            }

            var ui = await OpenFullScreenAsync<T>(uiFormAssetPath, userData);
            if (ui != null)
            {
                return ui;
            }

            throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: open ui returned null. type={typeof(T).FullName}, path={uiFormAssetPath}");
        }

        /// <summary>
        /// 打开并确保成功显示全屏界面（简写别名）。
        /// </summary>
        /// <typeparam name="T">UI 的具体类型。</typeparam>
        /// <param name="uiFormAssetPath">界面所在路径。</param>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">打开失败时抛出异常。</exception>
        public Task<T> OpenRequiredAsync<T>(string uiFormAssetPath, object userData = null, int maxRetryFrames = DefaultOpenRequiredRetryFrames) where T : class, IUIForm
        {
            return OpenRequiredFullScreenAsync<T>(uiFormAssetPath, userData, maxRetryFrames);
        }

        /// <summary>
        /// 按类型打开并确保成功显示全屏界面（支持运行时反射类型）。
        /// </summary>
        /// <param name="uiFormType">界面逻辑类型。</param>
        /// <param name="uiFormAssetPath">界面所在路径，空时自动使用类型名推导。</param>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        /// <exception cref="GameFrameworkException">参数非法或打开失败时抛出异常。</exception>
        public async Task<IUIForm> OpenRequiredFullScreenAsync(Type uiFormType, string uiFormAssetPath = null, object userData = null, int maxRetryFrames = DefaultOpenRequiredRetryFrames)
        {
            if (uiFormType == null)
            {
                throw new GameFrameworkException("[UIComponent] OpenRequiredFullScreenAsync failed: uiFormType is null.");
            }

            if (!typeof(IUIForm).IsAssignableFrom(uiFormType))
            {
                throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: type does not implement IUIForm. type={uiFormType.FullName}");
            }

            uiFormAssetPath = ResolveUIFormAssetPath(uiFormType, uiFormAssetPath);

            if (!await WaitForRuntimeInitializedAsync(maxRetryFrames))
            {
                throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: runtime not initialized. type={uiFormType.FullName}, path={uiFormAssetPath}");
            }

            var ui = await OpenUIAsync(uiFormAssetPath, uiFormType, true, userData, true);
            if (ui != null)
            {
                return ui;
            }

            throw new GameFrameworkException($"[UIComponent] OpenRequiredFullScreenAsync failed: open ui returned null. type={uiFormType.FullName}, path={uiFormAssetPath}");
        }

        /// <summary>
        /// 按类型打开并确保成功显示全屏界面（简写别名）。
        /// </summary>
        /// <param name="uiFormType">界面逻辑类型。</param>
        /// <param name="uiFormAssetPath">界面所在路径，空时自动使用类型名推导。</param>
        /// <param name="userData">传递给 UI 的用户数据。</param>
        /// <param name="maxRetryFrames">等待运行时初始化的最大帧数。</param>
        /// <returns>返回打开的 UI 实例。</returns>
        public Task<IUIForm> OpenRequiredAsync(Type uiFormType, string uiFormAssetPath = null, object userData = null, int maxRetryFrames = DefaultOpenRequiredRetryFrames)
        {
            return OpenRequiredFullScreenAsync(uiFormType, uiFormAssetPath, userData, maxRetryFrames);
        }

        private static string ResolveUIFormAssetPath(Type uiFormType, string uiFormAssetPath)
        {
            if (!uiFormAssetPath.IsNullOrWhiteSpace())
            {
                return uiFormAssetPath;
            }

            var resolvedPath = Utility.Asset.Path.GetUIPath(uiFormType.Name);
            var attribute = uiFormType.GetCustomAttribute(typeof(OptionUIConfigAttribute));
            if (attribute is not OptionUIConfigAttribute optionUIConfig)
            {
                return resolvedPath;
            }

            if (optionUIConfig.IsResource)
            {
                return "UI";
            }

            if (!optionUIConfig.Path.IsNullOrWhiteSpace())
            {
                return optionUIConfig.Path;
            }

            if (!optionUIConfig.PackageName.IsNullOrWhiteSpace())
            {
                return Utility.Asset.Path.GetUIPath(optionUIConfig.PackageName);
            }

            return resolvedPath;
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
            var attribute = typeof(T).GetCustomAttribute(typeof(OptionUIConfigAttribute));
            if (attribute is OptionUIConfigAttribute optionUIConfig)
            {
                if (optionUIConfig.Path.IsNullOrWhiteSpace())
                {
                    uiFormAssetPath = Utility.Asset.Path.GetUIPath(optionUIConfig.PackageName);
                }
                else
                {
                    uiFormAssetPath = optionUIConfig.Path;
                }

                if (optionUIConfig.IsResource)
                {
                    uiFormAssetPath = "UI";
                }
            }

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
