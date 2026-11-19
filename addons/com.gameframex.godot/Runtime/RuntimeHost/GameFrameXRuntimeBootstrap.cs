// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the source code root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legal rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// GameFrameX 自动运行时启动入口（autoload，唯一启动路径）。
    /// </summary>
    /// <remarks>
    /// 注册为项目 autoload 后安装即生效：任意主场景无需任何改动即自动注入 GameFrameX 运行时。
    /// 没有互斥模式、没有开关、没有场景入口——启动即创建 <c>[GameFrameX]</c> 常驻 Host 并挂载全部组件。
    /// Godot 时序映射：
    /// - <c>_EnterTree</c>（早于主场景实例化 ≈ Unity BeforeSceneLoad）：重置跨场景残留静态状态、
    ///   宣告自动注入接管、应用 Options 贡献者——三者都必须早于任何模块实例化。
    /// - <c>CallDeferred(StartAutoRuntime)</c>（首个空闲帧，主场景 _Ready 全部完成后 ≈ Unity AfterSceneLoad）：
    ///   清接管标记、创建 Host 并挂载全部组件、全量实例化模块。
    /// 注意：Godot 主场景脚本的 <c>_Ready</c> 先于本引导的 Host 创建执行——
    /// 业务代码需在 <c>_Ready</c> 中经 <c>CallDeferred</c> 或首帧轮询访问框架组件（Unity Start 的等价时机）。
    /// </remarks>
    public partial class GameFrameXRuntimeBootstrap : Node
    {
        private bool s_AutoRuntimeStarted;

        /// <summary>
        /// autoload 入树：早于主场景加载，完成会话重置、接管宣告与贡献者应用。
        /// </summary>
        public override void _EnterTree()
        {
            base._EnterTree();
            if (s_AutoRuntimeStarted)
            {
                return;
            }

            s_AutoRuntimeStarted = true;

            // 会话边界对齐全新进程初始状态：清空 Host / 贡献者 / Options / 组件注册表静态残留，
            // 复用 Shutdown 清理语义兜底异常退出（正常退出时为幂等空操作）。
            GameFrameXRuntimeHost.Reset();
            GameFrameXRuntimeContributorHost.Reset();
            GameFrameXOptions.Reset();
            GameEntry.Reset();
            GameFrameworkEntry.Shutdown();

            // AssemblyLoad 钩子先退订再重订——重复初始化不会双订阅、不会产生重复描述符。
            GameFrameXRuntimeAssemblyLoadHook.Unsubscribe();
            GameFrameXRuntimeAssemblyLoadHook.EnsureSubscribed();

            // 宣告接管必须早于主场景组件 _Ready：场景里手工摆放的 GFX 组件据此自我抑制并告警；
            // 同时应用 Options 贡献者——必须早于任何模块实例化。
            GameFrameXRuntimeHost.ClaimAutoInjection();
            GameFrameXRuntimeContributorHost.EnsureApplied();

            // 延迟到首个空闲帧（主场景树构建完成后）再启动自动运行时。
            CallDeferred(nameof(StartAutoRuntime));
        }

        private void StartAutoRuntime()
        {
            GameFrameXRuntimeHost.EnsureStarted();
            GameFrameworkEntry.InstantiateAllModules();
        }
    }
}
