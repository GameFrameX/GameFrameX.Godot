// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//   This project is dual-licensed under the MIT License and Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any legal disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:  https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;
using Godot;

namespace Godot.Startup.Procedure;

/// <summary>
/// 获取应用版本信息流程。
/// </summary>
public sealed class ProcedureGetAppVersionInfoState : ProcedureBase
{
	private const string Unknown = "unknown";
	private const string FallbackAppVersion = "1.0.0";
	private const string BaseRequestFormFieldLanguage = "Language";
	private const string BaseRequestFormFieldAppVersion = "AppVersion";
	private const string BaseRequestFormFieldPlatform = "Platform";
	private const string BaseRequestFormFieldPackageName = "PackageName";
	private const string BaseRequestFormFieldChannel = "Channel";
	private const string BaseRequestFormFieldSubChannel = "SubChannel";
	private bool _stateChanged;
	private Task<AppVersionCheckResult> _appVersionCheckTask;

	/// <summary>
	/// 进入流程时执行。
	/// </summary>
	/// <param name="procedureOwner">流程持有者。</param>
	protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
	{
		base.OnEnter(procedureOwner);
		Log.Info("进入流程：ProcedureGetAppVersionInfoState");
		LauncherFlowProgressReporter.Report(18f, nameof(ProcedureGetAppVersionInfoState));
		_stateChanged = false;
		_appVersionCheckTask = CheckAppVersionAsync();
	}

	protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
	{
		base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
		if (_stateChanged || _appVersionCheckTask == null || _appVersionCheckTask.IsCompleted == false)
		{
			return;
		}

		_stateChanged = true;
		if (_appVersionCheckTask.IsFaulted)
		{
			var exception = _appVersionCheckTask.Exception?.GetBaseException();
			Log.Warning("[AppUpdate] app version check failed: {0}", exception?.Message ?? Unknown);
			ChangeState<ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState>(procedureOwner);
			return;
		}

		var result = _appVersionCheckTask.Result ?? AppVersionCheckResult.Continue("empty-result");
		if (!string.IsNullOrWhiteSpace(result.LogMessage))
		{
			Log.Info(result.LogMessage);
		}

		if (result.BlockStartup)
		{
			Log.Error("[AppUpdate] force upgrade required. startup halted. download={0}", result.DownloadUrl ?? string.Empty);
			return;
		}

		ChangeState<ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState>(procedureOwner);
	}

	private static async Task<AppVersionCheckResult> CheckAppVersionAsync()
	{
		var appVersion = ResolveLocalAppVersion();
		Log.Info("[AppUpdate] local appVersion={0}", appVersion);

		var globalConfig = GameEntry.GetComponent<GlobalConfigComponent>();
		if (globalConfig == null)
		{
			return AppVersionCheckResult.Continue("[AppUpdate] skip: GlobalConfigComponent not found.");
		}

		var appVersionUrl = ResolveAppVersionUrl(globalConfig);
		if (string.IsNullOrWhiteSpace(appVersionUrl))
		{
			return AppVersionCheckResult.Continue("[AppUpdate] skip: CheckAppVersionUrl is empty.");
		}

		var webComponent = GameEntry.GetComponent<WebComponent>();
		if (webComponent == null)
		{
			return AppVersionCheckResult.Continue("[AppUpdate] skip: WebComponent not found.");
		}

		try
		{
			var requestForm = BuildAppVersionRequestForm(appVersion);
			var response = await webComponent.PostToString(appVersionUrl, requestForm).ConfigureAwait(false);
			if (response == null || string.IsNullOrWhiteSpace(response.Result))
			{
				return AppVersionCheckResult.Continue("[AppUpdate] skip: empty app version response.");
			}

			if (TryParseAppVersionResponse(response.Result, out var appVersionInfo) == false)
			{
				return AppVersionCheckResult.Continue("[AppUpdate] skip: app version response parse failed.");
			}

			if (appVersionInfo.IsUpgrade == false)
			{
				return AppVersionCheckResult.Continue("[AppUpdate] app is up-to-date.");
			}

			var log = string.Format(
				"[AppUpdate] upgrade available force={0}, title={1}, announcement={2}",
				appVersionInfo.IsForce,
				appVersionInfo.UpdateTitle ?? string.Empty,
				appVersionInfo.UpdateAnnouncement ?? string.Empty);
			if (!appVersionInfo.IsForce)
			{
				return AppVersionCheckResult.Continue(log);
			}

			return AppVersionCheckResult.Block(log, appVersionInfo.AppDownloadUrl);
		}
		catch (Exception exception)
		{
			return AppVersionCheckResult.Continue($"[AppUpdate] request failed: {exception.Message}");
		}
	}

	private static string ResolveLocalAppVersion()
	{
		var version = GameFrameX.Runtime.Version.GameVersion?.Trim();
		return string.IsNullOrWhiteSpace(version) ? FallbackAppVersion : version;
	}

	private static string ResolveAppVersionUrl(GlobalConfigComponent globalConfig)
	{
		if (globalConfig == null)
		{
			return string.Empty;
		}

		if (!string.IsNullOrWhiteSpace(globalConfig.CheckAppVersionUrl))
		{
			return globalConfig.CheckAppVersionUrl.Trim();
		}

		var globalInfo = globalConfig.GlobalInfo;
		if (globalInfo != null && !string.IsNullOrWhiteSpace(globalInfo.CheckAppVersionUrl))
		{
			return globalInfo.CheckAppVersionUrl.Trim();
		}

		return string.Empty;
	}

	private static Dictionary<string, object> BuildAppVersionRequestForm(string appVersion)
	{
		return new Dictionary<string, object>(StringComparer.Ordinal)
		{
			[BaseRequestFormFieldLanguage] = TranslationServer.GetLocale(),
			[BaseRequestFormFieldAppVersion] = appVersion,
			[BaseRequestFormFieldPlatform] = OS.GetName(),
			[BaseRequestFormFieldPackageName] = ResolveProjectName(),
			[BaseRequestFormFieldChannel] = string.Empty,
			[BaseRequestFormFieldSubChannel] = string.Empty
		};
	}

	private static string ResolveProjectName()
	{
		if (!ProjectSettings.HasSetting("application/config/name"))
		{
			return Unknown;
		}

		var settingValue = ProjectSettings.GetSetting("application/config/name");
		var projectName = settingValue.IsNull() ? string.Empty : settingValue.AsString();
		return string.IsNullOrWhiteSpace(projectName) ? Unknown : projectName.Trim();
	}

	private static bool TryParseAppVersionResponse(string responseText, out ResponseGameAppVersion response)
	{
		response = null;
		if (string.IsNullOrWhiteSpace(responseText))
		{
			return false;
		}

		try
		{
			var wrappedData = responseText.ToHttpJsonResultData<ResponseGameAppVersion>();
			if (wrappedData != null && wrappedData.IsSuccess)
			{
				response = wrappedData.Data ?? new ResponseGameAppVersion();
				return true;
			}
		}
		catch (Exception)
		{
			// Ignore wrapper parse failure and fall back to direct parse.
		}

		try
		{
			var directData = Utility.Json.ToObject<ResponseGameAppVersion>(responseText);
			if (directData == null)
			{
				return false;
			}

			response = directData;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private sealed class AppVersionCheckResult
	{
		public bool BlockStartup { get; private set; }
		public string LogMessage { get; private set; } = string.Empty;
		public string DownloadUrl { get; private set; } = string.Empty;

		public static AppVersionCheckResult Continue(string logMessage)
		{
			return new AppVersionCheckResult
			{
				BlockStartup = false,
				LogMessage = logMessage ?? string.Empty
			};
		}

		public static AppVersionCheckResult Block(string logMessage, string downloadUrl)
		{
			return new AppVersionCheckResult
			{
				BlockStartup = true,
				LogMessage = logMessage ?? string.Empty,
				DownloadUrl = downloadUrl ?? string.Empty
			};
		}
	}
}
