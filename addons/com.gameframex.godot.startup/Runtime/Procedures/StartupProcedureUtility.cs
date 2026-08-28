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
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Localization.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.SystemInfo.Runtime;
using Godot;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程工具类，提供启动相关的数据读取、结果通知和进度更新等辅助方法。
    /// </summary>
    /// <remarks>
    /// Startup procedure utility class, provides helper methods for reading startup data, notifying results and updating download progress.
    /// </remarks>
    internal static class StartupProcedureUtility
    {
        public const string GameFrameXApiKeyHeader = "GameFrameX-Tenant-Id";
        public const string GameFrameXAppIdHeader = "GameFrameX-App-Id";
        public const string GameFrameXAppSecretHeader = "GameFrameX-App-Secret";
        public const string GameFrameXTenantSecretHeader = "GameFrameX-Tenant-Secret";

        /// <summary>
        /// 从流程所有者中获取启动选项。
        /// </summary>
        /// <remarks>
        /// Gets the startup options from the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>启动选项实例 / Startup options instance</returns>
        public static StartupOptions GetOptions(IFsm<IProcedureManager> procedureOwner)
        {
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupOptions).Value as StartupOptions;
        }

        /// <summary>
        /// 从流程所有者中获取启动界面处理器。
        /// </summary>
        /// <remarks>
        /// Gets the startup UI handler from the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>启动界面处理器实例 / Startup UI handler instance</returns>
        public static IStartupUIHandler GetUIHandler(IFsm<IProcedureManager> procedureOwner)
        {
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupUIHandler).Value as IStartupUIHandler;
        }

        /// <summary>
        /// 从流程所有者中获取异步结果完成源。
        /// </summary>
        /// <remarks>
        /// Gets the async result completion source from the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>异步结果完成源 / Async result completion source</returns>
        public static TaskCompletionSource<StartupResult> GetCompletionSource(IFsm<IProcedureManager> procedureOwner)
        {
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupCompletionSource).Value as TaskCompletionSource<StartupResult>;
        }

        /// <summary>
        /// 从流程所有者中获取 HTTP 参数提供者。
        /// </summary>
        /// <remarks>
        /// Gets the HTTP params provider from the procedure owner. Returns default provider if not set.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>HTTP 参数提供者实例 / HTTP params provider instance</returns>
        public static IStartupHttpParamsProvider GetHttpParamsProvider(IFsm<IProcedureManager> procedureOwner)
        {
            var providerBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupHttpParamsProvider);
            return providerBox?.Value as IStartupHttpParamsProvider ?? new DefaultStartupHttpParamsProvider();
        }

        /// <summary>
        /// 等待下一帧完成（等价 Unity 的 UniTask.NextFrame / DelayFrame）。
        /// </summary>
        /// <remarks>
        /// 通过主循环 SceneTree 的 ProcessFrame 信号等待下一帧；非 SceneTree 主循环时立即返回。
        /// </remarks>
        public static Task WaitForNextFrameAsync()
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree == null)
            {
                return Task.CompletedTask;
            }

            return WaitForNextFrameCoreAsync(sceneTree);
        }

        private static async Task WaitForNextFrameCoreAsync(SceneTree sceneTree)
        {
            await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
        }

        /// <summary>
        /// 使用默认 HTTP 参数提供者创建 HTTP 请求参数字典。
        /// </summary>
        /// <remarks>
        /// Creates HTTP request parameters dictionary using the default provider.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>HTTP 请求参数字典 / HTTP request parameters dictionary</returns>
        public static Dictionary<string, object> CreateHttpParams(StartupOptions options)
        {
            return CreateHttpParams(options, new DefaultStartupHttpParamsProvider());
        }

        /// <summary>
        /// 使用指定的 HTTP 参数提供者创建 HTTP 请求参数字典。
        /// </summary>
        /// <remarks>
        /// Creates HTTP request parameters dictionary using the specified provider.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <param name="provider">HTTP 参数提供者 / HTTP params provider</param>
        /// <returns>HTTP 请求参数字典 / HTTP request parameters dictionary</returns>
        public static Dictionary<string, object> CreateHttpParams(StartupOptions options, IStartupHttpParamsProvider provider)
        {
            var parameters = provider.Create(options);
            ApplyRuntimeDefaults(parameters);
            return parameters.ToDictionary();
        }

        /// <summary>
        /// 创建 GameFrameX 管理后台请求头。
        /// </summary>
        /// <remarks>
        /// Creates the GameFrameX admin request headers from startup options.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>请求头字典 / Headers dictionary</returns>
        public static Dictionary<string, string> CreateGameFrameXHeaders(StartupOptions options)
        {
            var headers = new Dictionary<string, string>(4);
            if (options == null)
            {
                return headers;
            }

            AddHeaderIfNotEmpty(headers, GameFrameXApiKeyHeader, options.GameFrameXApiKey);
            AddHeaderIfNotEmpty(headers, GameFrameXAppIdHeader, options.GameFrameXAppId);
            AddHeaderIfNotEmpty(headers, GameFrameXAppSecretHeader, options.GameFrameXAppSecret);
            AddHeaderIfNotEmpty(headers, GameFrameXTenantSecretHeader, options.GameFrameXTenantSecret);
            return headers;
        }

        private static void AddHeaderIfNotEmpty(Dictionary<string, string> headers, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                headers[key] = value;
            }
        }

        /// <summary>
        /// 应用运行时默认参数到 HTTP 参数对象。
        /// </summary>
        /// <remarks>
        /// Applies runtime default values to the HTTP parameters, including language, version and device info.
        /// </remarks>
        /// <param name="parameters">HTTP 参数对象 / HTTP parameters object</param>
        private static void ApplyRuntimeDefaults(IStartupHttpParams parameters)
        {
            var startupHttpParams = parameters as StartupHttpParams;
            if (startupHttpParams == null)
            {
                return;
            }

            // 说明：Unity 的 Application.systemLanguage 返回枚举名（如 ChineseSimplified）；
            // Godot 的 OS.GetLocale() 返回 BCP-47 风格语言代码（如 zh_CN / en_US），格式与 Unity 不同，
            // 若服务端依赖 Unity 枚举名需在服务端或自定义 IStartupHttpParamsProvider 中归一化。
            startupHttpParams.Language = OS.GetLocale();
            // 说明：Godot 侧 LocalizationComponent.Language 为 string（Unity 侧为 enum），此处直接赋值。
            startupHttpParams.UserLanguage = GameEntry.GetComponent<LocalizationComponent>().Language;
            // 说明：Application.version → ProjectSettings 的 application/config/version。
            startupHttpParams.AppVersion = ProjectSettings.GetSetting("application/config/version").AsString();
            startupHttpParams.DeviceUniqueIdentifier = BlankDeviceUniqueIdentifier.DeviceUniqueIdentifier;
            startupHttpParams.Platform = ApplicationHelper.PlatformName;
        }

        /// <summary>
        /// 完成启动失败流程，通知 UI 显示错误信息并触发失败事件。
        /// </summary>
        /// <remarks>
        /// Completes the startup failure procedure, notifies UI to display error message and fires failure event.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <param name="procedureName">失败的流程名称 / Failed procedure name</param>
        /// <param name="failedUrl">失败的请求 URL / Failed request URL</param>
        /// <param name="errorMessage">错误信息 / Error message</param>
        public static void CompleteFailure(IFsm<IProcedureManager> procedureOwner, string procedureName, string failedUrl, string errorMessage)
        {
            var result = StartupResult.Fail(procedureName, failedUrl, errorMessage);
            var uiHandler = GetUIHandler(procedureOwner);
            if (uiHandler != null)
            {
                uiHandler.SetTipText(errorMessage);
            }

            GameEntry.GetComponent<EventComponent>().Fire(procedureOwner, StartupFailedEventArgs.Create(procedureName, failedUrl, errorMessage));
            var completionSource = GetCompletionSource(procedureOwner);
            if (completionSource != null)
            {
                completionSource.TrySetResult(result);
            }
        }

        /// <summary>
        /// 完成启动成功流程，触发成功事件并设置结果。
        /// </summary>
        /// <remarks>
        /// Completes the startup success procedure, fires success event and sets result.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        public static void CompleteSuccess(IFsm<IProcedureManager> procedureOwner)
        {
            GameEntry.GetComponent<EventComponent>().Fire(procedureOwner, StartupCompletedEventArgs.Create());
            var completionSource = GetCompletionSource(procedureOwner);
            if (completionSource != null)
            {
                completionSource.TrySetResult(StartupResult.Succeed());
            }
        }

        /// <summary>
        /// 设置下载进度到界面处理器。
        /// </summary>
        /// <remarks>
        /// Sets the download progress to the UI handler with formatted byte sizes.
        /// </remarks>
        /// <param name="uiHandler">界面处理器 / UI handler</param>
        /// <param name="currentBytes">当前已下载字节数 / Current downloaded bytes</param>
        /// <param name="totalBytes">总字节数 / Total bytes</param>
        public static void SetDownloadProgress(IStartupUIHandler uiHandler, long currentBytes, long totalBytes)
        {
            var progress = totalBytes <= 0 ? 0f : currentBytes / (totalBytes * 1f);
            var currentSize = Utility.File.GetBytesSize(currentBytes);
            var totalSize = Utility.File.GetBytesSize(totalBytes);
            if (uiHandler != null)
            {
                uiHandler.SetProgress(progress, currentSize + "/" + totalSize);
            }
        }
    }
}
