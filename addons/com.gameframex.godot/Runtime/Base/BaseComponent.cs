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
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 基础组件。
    /// </summary>
    public sealed partial class BaseComponent : GameFrameworkComponent
    {
        private const int DefaultDpi = 96; // default windows dpi

        private float m_GameSpeedBeforePause = 1f;

        // [Export] private bool m_EditorResourceMode = true;

        [Export] private string m_TextHelperTypeName = "GameFrameX.Runtime.DefaultTextHelper";

        [Export] private string m_VersionHelperTypeName = "GameFrameX.Runtime.DefaultVersionHelper";

        [Export] private string m_LogHelperTypeName = "GameFrameX.Runtime.DefaultLogHelper";

        [Export] private string m_CompressionHelperTypeName = "GameFrameX.Runtime.DefaultCompressionHelper";

        [Export] private string m_JsonHelperTypeName = "GameFrameX.Runtime.NewtonsoftJsonHelper";

        [Export(PropertyHint.Range, "30,120,1")] private int m_FrameRate = 30;

        [Export(PropertyHint.Range, "0.25,10,0.01")] private float m_GameSpeed = 1f;

        [Export] private bool m_RunInBackground = true;

        [Export] private bool m_NeverSleep = true;


        /// <summary>
        /// 获取或设置是否使用编辑器资源模式（仅编辑器内有效）。
        /// </summary>
        // public bool EditorResourceMode
        // {
        //     get { return m_EditorResourceMode; }
        //     set { m_EditorResourceMode = value; }
        // }

        /*/// <summary>
        /// 获取或设置编辑器资源辅助器。
        /// </summary>
        public IResourceManager EditorResourceHelper { get; set; }*/

        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get { return m_FrameRate; }
            set { Engine.MaxFps = m_FrameRate = value; }
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get { return m_GameSpeed; }
            set { Engine.TimeScale = m_GameSpeed = value >= 0f ? value : 0f; }
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        public bool IsGamePaused
        {
            get { return m_GameSpeed <= 0f; }
        }

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        public bool IsNormalGameSpeed
        {
            get { return m_GameSpeed == 1f; }
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get { return m_RunInBackground; }
            set
            {
                // TODO: Godot doesn't have a direct equivalent for Application.runInBackground
                // This may need to be handled at the project level or via Display settings
                m_RunInBackground = value;
            }
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public bool NeverSleep
        {
            get { return m_NeverSleep; }
            set
            {
                // TODO: Godot doesn't have a direct equivalent for Screen.sleepTimeout
                // This may need to be handled via OS features or plugin
                m_NeverSleep = value;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            PreserveDefaultHelperTypes();
            IsAutoRegister = false;
            base._Ready();

            // TODO: DontDestroyOnLoad equivalent in Godot - use AutoLoad singletons instead
            // DontDestroyOnLoad(this);
            InitTextHelper();
            InitVersionHelper();
            InitLogHelper();
            // Log.Info("Game Framework Version: {0}", GameFramework.Version.GameFrameworkVersion);
            Log.Info("Game Version: {0}, Godot Version: {1}", Version.GameVersion, Engine.GetVersionInfo());
            InitCompressionHelper();
            InitJsonHelper();

            // TODO: Godot doesn't have a direct equivalent for Screen.dpi
            // Utility.Converter.ScreenDpi = Screen.dpi;
            // if (Utility.Converter.ScreenDpi <= 0)
            // {
            //     Utility.Converter.ScreenDpi = DefaultDpi;
            // }

            // m_EditorResourceMode &= OS.HasFeature("editor");
            // if (m_EditorResourceMode)
            // {
            //     Log.Info(
            //         "During this run, Game Framework will use editor resource files, which you should validate first.");
            // }

            Engine.MaxFps = m_FrameRate;
            Engine.TimeScale = m_GameSpeed;
            // Application.runInBackground = m_RunInBackground; // TODO: No direct equivalent
            // Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting; // TODO: No direct equivalent
        }

        /// <summary>
        /// 保活基础模块默认 Helper 类型，避免导出裁剪后反射创建失败。
        /// </summary>
        private static void PreserveDefaultHelperTypes()
        {
            _ = typeof(DefaultTextHelper);
            _ = typeof(DefaultVersionHelper);
            _ = typeof(DefaultLogHelper);
            _ = typeof(DefaultCompressionHelper);
            _ = typeof(NewtonsoftJsonHelper);
        }

        public override void _Process(double delta)
        {
            float scaledDelta = (float)delta;
            // ponytail: TimeScale=0 时 delta 也为 0，直接透传避免除零产生 NaN/Infinity 污染 Unscaled 定时器累积器（此时 Unscaled 定时器随引擎暂停）；升级路径是改用 Time.GetUnixTimeFromSystem 差值计算真实流逝时间
            float realDelta = Engine.TimeScale == 0f ? scaledDelta : (float)(delta / Engine.TimeScale);
            GameFrameworkEntry.Update(scaledDelta, realDelta);
        }

        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationWMCloseRequest)
            {
                // Equivalent runtime teardown callback
                // StopAllCoroutines();
            }
            else if (what == NotificationPredelete || what == NotificationExitTree)
            {
                // Equivalent dispose callback
                // 备案（2026-08-29 核实）：ExitTree 与 Predelete 两个通知各触发一次 Shutdown，第二次为严格幂等空转——
                // GameFrameworkEntry.Shutdown 首次调用后模块链表已 Clear（各模块 Shutdown 不会重复执行），
                // ReferencePool.ClearAll 对已清空字典为空转，FreeCachedHGlobal 有 IntPtr.Zero 守卫，SetLogHelper(null) 幂等；故不加短路标记，维持双通知兜底。
                GameFrameworkEntry.Shutdown();
            }
            else
            {
                // 迁移自 Unity com.gameframex.unity.mono 的 OnApplicationFocus / OnApplicationPause 全局事件转发；
                // Godot 侧无独立 MonoManager，由 BaseComponent 统一抛出；NotificationApplicationResumed 对应 IsPause = false。
                GameEventArgs applicationEventArgs = ResolveApplicationEventArgs(what);
                if (applicationEventArgs != null)
                {
                    FireApplicationEvent(this, applicationEventArgs);
                }
            }
        }

        /// <summary>
        /// 解析应用程序焦点/暂停通知对应的全局事件参数；非应用程序状态类通知返回 null。
        /// </summary>
        /// <param name="what">Godot 通知编号。</param>
        /// <returns>对应的事件参数；不是应用程序状态通知时返回 null。</returns>
        internal static GameEventArgs ResolveApplicationEventArgs(int what)
        {
            if (what == NotificationApplicationFocusIn)
            {
                return OnApplicationFocusChangedEventArgs.Create(true);
            }

            if (what == NotificationApplicationFocusOut)
            {
                return OnApplicationFocusChangedEventArgs.Create(false);
            }

            if (what == NotificationApplicationPaused)
            {
                return OnApplicationPauseChangedEventArgs.Create(true);
            }

            if (what == NotificationApplicationResumed)
            {
                return OnApplicationPauseChangedEventArgs.Create(false);
            }

            return null;
        }

        /// <summary>
        /// 通过事件发布契约抛出全局事件；事件组件尚未注册时静默跳过（通知可能早于组件就绪到达）。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">事件参数。</param>
        internal static void FireApplicationEvent(object sender, GameEventArgs e)
        {
            IEventPublisher eventPublisher = GameEntry.GetComponentInterface<IEventPublisher>();
            if (eventPublisher == null)
            {
                return;
            }

            eventPublisher.Fire(sender, e);
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = m_GameSpeedBeforePause;
        }

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        internal void Shutdown()
        {
            QueueFree();
        }

        private void InitTextHelper()
        {
            if (string.IsNullOrEmpty(m_TextHelperTypeName))
            {
                // 旧场景（如 gfx.scn）可能序列化空串覆盖默认值；空值兜底为默认实现，避免静默跳过注册
                m_TextHelperTypeName = typeof(DefaultTextHelper).FullName;
            }

            Type textHelperType = Utility.Assembly.GetType(m_TextHelperTypeName);
            if (textHelperType == null)
            {
                Log.Error("Can not find text helper type '{0}'.", m_TextHelperTypeName);
                return;
            }

            Utility.Text.ITextHelper textHelper = (Utility.Text.ITextHelper)Activator.CreateInstance(textHelperType);
            if (textHelper == null)
            {
                Log.Error("Can not create text helper instance '{0}'.", m_TextHelperTypeName);
                return;
            }

            Utility.Text.SetTextHelper(textHelper);
        }

        private void InitVersionHelper()
        {
            if (string.IsNullOrEmpty(m_VersionHelperTypeName))
            {
                // 旧场景（如 gfx.scn）可能序列化空串覆盖默认值；空值兜底为默认实现，避免静默跳过注册
                m_VersionHelperTypeName = typeof(DefaultVersionHelper).FullName;
            }

            Type versionHelperType = Utility.Assembly.GetType(m_VersionHelperTypeName);
            if (versionHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find version helper type '{0}'.",
                                                                     m_VersionHelperTypeName));
            }

            Version.IVersionHelper versionHelper =
                (Version.IVersionHelper)Activator.CreateInstance(versionHelperType);
            if (versionHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create version helper instance '{0}'.",
                                                                     m_VersionHelperTypeName));
            }

            Version.SetVersionHelper(versionHelper);
        }

        private void InitLogHelper()
        {
            if (string.IsNullOrEmpty(m_LogHelperTypeName))
            {
                // 旧场景（如 gfx.scn）可能序列化空串覆盖默认值；空值兜底为默认实现，避免静默跳过注册
                m_LogHelperTypeName = typeof(DefaultLogHelper).FullName;
            }

            Type logHelperType = Utility.Assembly.GetType(m_LogHelperTypeName);
            if (logHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find log helper type '{0}'.",
                                                                     m_LogHelperTypeName));
            }

            GameFrameworkLog.ILogHelper logHelper =
                (GameFrameworkLog.ILogHelper)Activator.CreateInstance(logHelperType);
            if (logHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create log helper instance '{0}'.",
                                                                     m_LogHelperTypeName));
            }

            GameFrameworkLog.SetLogHelper(logHelper);
        }

        private void InitCompressionHelper()
        {
            if (string.IsNullOrEmpty(m_CompressionHelperTypeName))
            {
                // 旧场景（如 gfx.scn）可能序列化空串覆盖默认值；空值兜底为默认实现，避免静默跳过注册
                m_CompressionHelperTypeName = typeof(DefaultCompressionHelper).FullName;
            }

            Type compressionHelperType = Utility.Assembly.GetType(m_CompressionHelperTypeName);
            if (compressionHelperType == null)
            {
                Log.Error("Can not find compression helper type '{0}'.", m_CompressionHelperTypeName);
                return;
            }

            Utility.Compression.ICompressionHelper compressionHelper =
                (Utility.Compression.ICompressionHelper)Activator.CreateInstance(compressionHelperType);
            if (compressionHelper == null)
            {
                Log.Error("Can not create compression helper instance '{0}'.", m_CompressionHelperTypeName);
                return;
            }

            Utility.Compression.SetCompressionHelper(compressionHelper);
        }

        private void InitJsonHelper()
        {
            if (string.IsNullOrEmpty(m_JsonHelperTypeName))
            {
                // 旧场景（如 gfx.scn）可能序列化空串覆盖默认值；空值兜底为默认实现，避免静默跳过注册
                m_JsonHelperTypeName = typeof(NewtonsoftJsonHelper).FullName;
            }

            Type jsonHelperType = Utility.Assembly.GetType(m_JsonHelperTypeName);
            if (jsonHelperType == null)
            {
                Log.Error("Can not find JSON helper type '{0}'.", m_JsonHelperTypeName);
                return;
            }

            Utility.Json.IJsonHelper jsonHelper = (Utility.Json.IJsonHelper)Activator.CreateInstance(jsonHelperType);
            if (jsonHelper == null)
            {
                Log.Error("Can not create JSON helper instance '{0}'.", m_JsonHelperTypeName);
                return;
            }

            Utility.Json.SetJsonHelper(jsonHelper);
        }

        private void OnLowMemory()
        {
            Log.Info("Low memory reported...");

            ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
            if (objectPoolComponent != null)
            {
                objectPoolComponent.ReleaseAllUnused();
            }

        }
    }
}
