using System;
using System.Diagnostics;
using Godot;

namespace GameFrameX.AssetSystem
{
    public static class AssetSystemTime
    {
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        private static ulong _lastFrame = ulong.MaxValue;
        private static float _lastRealtime;
        private static float _cachedDelta;

        public static int FrameCount => (int)Engine.GetProcessFrames();

        public static float RealtimeSinceStartup => (float)Stopwatch.Elapsed.TotalSeconds;

        public static float UnscaledDeltaTime
        {
            get
            {
                var currentFrame = (ulong)Engine.GetProcessFrames();
                var currentRealtime = RealtimeSinceStartup;
                if (_lastFrame != currentFrame)
                {
                    _cachedDelta = Math.Max(0f, currentRealtime - _lastRealtime);
                    _lastRealtime = currentRealtime;
                    _lastFrame = currentFrame;
                }

                return _cachedDelta;
            }
        }
    }
}
