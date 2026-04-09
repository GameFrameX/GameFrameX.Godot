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

using Godot;

namespace Godot.Startup.Procedure;

/// <summary>
/// 启动更新模式。
/// </summary>
public enum StartupUpdateMode
{
    /// <summary>
    /// 单机离线模式，跳过增量更新流程。
    /// </summary>
    OfflineSingle = 0,

    /// <summary>
    /// 在线可选更新，失败后允许继续启动。
    /// </summary>
    OnlineOptionalUpdate = 1,

    /// <summary>
    /// 在线强制更新，失败后阻断启动。
    /// </summary>
    OnlineForceUpdate = 2
}

/// <summary>
/// 启动更新模式上下文。
/// </summary>
public static class StartupUpdateModeContext
{
    private const string ProjectSettingKey = "gameframex/startup/update_mode";
    private const string EnvironmentVariableKey = "GFX_UPDATE_MODE";

    /// <summary>
    /// 当前启动更新模式。
    /// </summary>
    public static StartupUpdateMode CurrentMode { get; private set; } = StartupUpdateMode.OnlineOptionalUpdate;

    /// <summary>
    /// 刷新并返回当前模式。
    /// </summary>
    public static StartupUpdateMode Refresh()
    {
        CurrentMode = ResolveMode();
        return CurrentMode;
    }

    private static StartupUpdateMode ResolveMode()
    {
        var raw = global::System.Environment.GetEnvironmentVariable(EnvironmentVariableKey);
        if (string.IsNullOrWhiteSpace(raw) && ProjectSettings.HasSetting(ProjectSettingKey))
        {
            var settingValue = ProjectSettings.GetSetting(ProjectSettingKey);
            raw = settingValue.IsNull() ? string.Empty : settingValue.AsString();
        }

        if (TryParse(raw, out var mode))
        {
            return mode;
        }

        return StartupUpdateMode.OnlineOptionalUpdate;
    }

    private static bool TryParse(string raw, out StartupUpdateMode mode)
    {
        mode = StartupUpdateMode.OnlineOptionalUpdate;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var normalized = raw.Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "0":
            case "offline":
            case "offlinesingle":
                mode = StartupUpdateMode.OfflineSingle;
                return true;
            case "1":
            case "optional":
            case "onlineoptionalupdate":
                mode = StartupUpdateMode.OnlineOptionalUpdate;
                return true;
            case "2":
            case "force":
            case "onlineforceupdate":
                mode = StartupUpdateMode.OnlineForceUpdate;
                return true;
            default:
                return global::System.Enum.TryParse(raw, true, out mode);
        }
    }
}
