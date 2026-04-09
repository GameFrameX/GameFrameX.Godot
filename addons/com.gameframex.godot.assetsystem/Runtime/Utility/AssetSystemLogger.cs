using System;
using System.Diagnostics;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 自定义日志处理
    /// </summary>
    [AssetSystemPreserve]
    public interface ILogger
    {
        [AssetSystemPreserve]
        void Log(string message);
        [AssetSystemPreserve]
        void Warning(string message);
        [AssetSystemPreserve]
        void Error(string message);
        [AssetSystemPreserve]
        void Exception(Exception exception);
    }

    [AssetSystemPreserve]
    public static class AssetSystemLogger
    {
        public static ILogger Logger = null;

        /// <summary>
        /// 日志
        /// </summary>
        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        public static void Log(string info)
        {
            if (Logger != null)
            {
                Logger.Log(GetTime() + info);
            }
            else
            {
            global::Godot.GD.Print(GetTime() + info);
            }
        }

        /// <summary>
        /// 警告
        /// </summary>
        [AssetSystemPreserve]
        public static void Warning(string info)
        {
            if (Logger != null)
            {
                Logger.Warning(GetTime() + info);
            }
            else
            {
            global::Godot.GD.PushWarning(GetTime() + info);
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        [AssetSystemPreserve]
        public static void Error(string info)
        {
            if (Logger != null)
            {
                Logger.Error(GetTime() + info);
            }
            else
            {
            global::Godot.GD.PushError(GetTime() + info);
            }
        }

        [AssetSystemPreserve]
        private static string GetTime()
        {
            return $"[AssetSystem]:[{DateTime.Now:HH:mm:ss.fff}]:";
        }

        /// <summary>
        /// 异常
        /// </summary>
        [AssetSystemPreserve]
        public static void Exception(Exception exception)
        {
            if (Logger != null)
            {
                Logger.Exception(exception);
            }
            else
            {
            global::Godot.GD.PushError(exception.ToString());
            }
        }
    }
}
