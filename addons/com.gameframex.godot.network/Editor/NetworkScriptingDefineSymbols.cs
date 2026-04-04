#if TOOLS
using GameFrameX.Editor;

namespace GameFrameX.Network.Editor
{
    /// <summary>
    /// 网络模块脚本宏定义帮助类。
    /// </summary>
    public static class NetworkScriptingDefineSymbols
    {
        public const string EnableNetworkReceiveLogScriptingDefineSymbol = "ENABLE_GAMEFRAMEX_NETWORK_RECEIVE_LOG";
        public const string EnableNetworkSendLogScriptingDefineSymbol = "ENABLE_GAMEFRAMEX_NETWORK_SEND_LOG";
        public const string ForceEnableNetworkSendLogScriptingDefineSymbol = "FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET";

        /// <summary>
        /// 功能：关闭强制使用 WebSocket 网络宏定义。
        /// </summary>
        public static void DisableForceWebSocketNetwork()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(ForceEnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 功能：开启强制使用 WebSocket 网络宏定义。
        /// </summary>
        public static void EnableForceWebSocketNetwork()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(ForceEnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 功能：关闭网络接收日志宏定义。
        /// </summary>
        public static void DisableNetworkReceiveLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableNetworkReceiveLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 功能：开启网络接收日志宏定义。
        /// </summary>
        public static void EnableNetworkReceiveLogs()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableNetworkReceiveLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 功能：关闭网络发送日志宏定义。
        /// </summary>
        public static void DisableNetworkSendLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 功能：开启网络发送日志宏定义。
        /// </summary>
        public static void EnableNetworkSendLogs()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableNetworkSendLogScriptingDefineSymbol);
        }
    }
}
#endif
