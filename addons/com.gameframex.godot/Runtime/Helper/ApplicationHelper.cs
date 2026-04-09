using System.Runtime.InteropServices;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 应用帮助类
    /// </summary>
    public static class ApplicationHelper
    {
        /// <summary>
        /// 是否是编辑器
        /// </summary>
        public static bool IsEditor => Engine.IsEditorHint();

        /// <summary>
        /// 是否是安卓
        /// </summary>
        public static bool IsAndroid => OS.HasFeature("android");

        /// <summary>
        /// 是否是 Web 平台
        /// </summary>
        public static bool IsWebGL => OS.HasFeature("web");

        /// <summary>
        /// 是否是 Web 微信小游戏平台
        /// </summary>
        public static bool IsWebGLWeChatMiniGame => IsWebGL && OS.HasFeature("wechat_minigame");

        /// <summary>
        /// 是否是 Web 抖音小游戏平台
        /// </summary>
        public static bool IsWebGLDouYinMiniGame => IsWebGL && OS.HasFeature("douyin_minigame");

        /// <summary>
        /// 是否是 Windows 平台
        /// </summary>
        public static bool IsWindows => OS.HasFeature("windows") || RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// 是否是 Linux 平台
        /// </summary>
        public static bool IsLinux => OS.HasFeature("linuxbsd") || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// 是否是 Mac 平台
        /// </summary>
        public static bool IsMacOsx => OS.HasFeature("macos") || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// 获取当前运行平台名称
        /// </summary>
        public static string PlatformName
        {
            get
            {
                if (IsAndroid) return "Android";
                if (IsIOS) return "iOS";
                if (IsMacOsx) return "MacOs";
                if (IsWindows) return "Windows";
                if (IsWebGL) return "Web";
                if (IsLinux) return "Linux";
                return string.Empty;
            }
        }

        /// <summary>
        /// 是否是 iOS 平台
        /// </summary>
        public static bool IsIOS => OS.HasFeature("ios");

        /// <summary>
        /// 退出应用
        /// </summary>
        public static void Quit()
        {
            if (Engine.GetMainLoop() is SceneTree sceneTree)
            {
                sceneTree.Quit();
            }
        }

        /// <summary>
        /// 打开 URL
        /// </summary>
        /// <param name="url">url地址</param>
        public static void OpenURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            OS.ShellOpen(url);
        }

        /// <summary>
        /// 打开设置界面（平台不支持时忽略）
        /// </summary>
        public static void OpenSetting()
        {
            // 这里保持空实现；如需平台特化可在后续接入原生桥接。
        }

        /// <summary>
        /// 请求跟踪授权（平台不支持时忽略）
        /// </summary>
        public static void OpenRequestTrackingAuthorization()
        {
            // 这里保持空实现；如需平台特化可在后续接入原生桥接。
        }
    }
}
