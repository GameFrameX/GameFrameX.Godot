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

using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程配置资产。所有项目可变项（资源模式、URL 主备列表、热更入口、HTTP 公共参数、UI 资源路径）通过此 Resource 注入。
    /// </summary>
    /// <remarks>
    /// Unity 版为 ScriptableObject，Godot 版迁移为 Resource（可在编辑器中创建 .tres 资产）。
    /// </remarks>
    [GlobalClass]
    public sealed partial class StartupOptions : Resource
    {
        /// <summary>
        /// GameFrameX 管理后台的租户ID。
        /// </summary>
        [Export] public string GameFrameXApiKey = "";

        /// <summary>
        /// GameFrameX 管理后台的租户密钥。
        /// </summary>
        [Export] public string GameFrameXTenantSecret = "";

        /// <summary>
        /// GameFrameX 管理后台的应用ID。
        /// </summary>
        [Export] public string GameFrameXAppId = "";

        /// <summary>
        /// GameFrameX 管理后台的应用密钥。
        /// </summary>
        [Export] public string GameFrameXAppSecret = "";

        /// <summary>
        /// 资源运行模式。启动流程会在加载资源前同步到 Asset 组件。
        /// </summary>
        [Export] public EPlayMode GamePlayMode = EPlayMode.EditorSimulateMode;

        /// <summary>
        /// 全局信息接口 URL 主备列表，按顺序尝试。空数组视为配置错误。
        /// </summary>
        [Export] public string[] GlobalInfoUrls = new string[0];

        /// <summary>
        /// 每个 URL 内部的总尝试次数上限（含初次）。
        /// </summary>
        [Export] public int MaxAttemptsPerUrl = 3;

        /// <summary>
        /// 重试之间的延迟毫秒数。
        /// </summary>
        [Export] public int RetryDelayMs = 3000;

        /// <summary>
        /// 是否跳过远程启动请求（全局信息、App 版本、资源包版本）。Web 单机等无后端场景启用，直接进入本地资源初始化。
        /// </summary>
        [Export] public bool SkipRemoteStartupRequests = false;

        /// <summary>
        /// Hotfix 程序集名，传给 IHotfixLauncher 使用。
        /// </summary>
        /// <remarks>Godot 侧热更工程 AssemblyName 为 Hotfix（Unity 版默认 Unity.Hotfix）。</remarks>
        [Export] public string HotfixAssemblyName = "Hotfix";

        /// <summary>
        /// Hotfix 入口类型全名。
        /// </summary>
        [Export] public string HotfixEntryTypeName = "Hotfix.HotfixLauncher";

        /// <summary>
        /// Hotfix 入口方法名。
        /// </summary>
        [Export] public string HotfixEntryMethodName = "Main";

        /// <summary>
        /// HTTP 公共参数中的应用包名（如 com.company.game）。运行时覆盖默认值。
        /// </summary>
        [Export] public string PackageName = string.Empty;

        /// <summary>
        /// HTTP 公共参数中的渠道标识。运行时覆盖默认值。
        /// </summary>
        [Export] public string Channel = string.Empty;

        /// <summary>
        /// HTTP 公共参数中的子渠道标识。运行时覆盖默认值。
        /// </summary>
        [Export] public string SubChannel = string.Empty;

        /// <summary>
        /// 启动 UI 的资源路径，传给 IStartupUIHandler。
        /// </summary>
        [Export] public string LauncherUIResName = "UI/UILauncher";
    }
}
