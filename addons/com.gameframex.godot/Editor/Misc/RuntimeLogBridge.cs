#if TOOLS
using System;
using System.IO;
using System.Text;
using Godot;

namespace GameFrameX.Editor
{
    internal sealed class RuntimeLogBridge
    {
        private const double PollIntervalSeconds = 0.25d;

        private readonly string m_LogPath;
        private long m_ReadPosition;
        private double m_Elapsed;
        private bool m_Initialized;

        public RuntimeLogBridge()
        {
            m_LogPath = Path.Combine(OS.GetUserDataDir(), "logs", "godot.log");
        }

        public void Tick(double delta)
        {
            m_Elapsed += delta;
            if (m_Elapsed < PollIntervalSeconds)
            {
                return;
            }

            m_Elapsed = 0d;
            if (!File.Exists(m_LogPath))
            {
                return;
            }

            if (!m_Initialized)
            {
                m_ReadPosition = new FileInfo(m_LogPath).Length;
                m_Initialized = true;
                GD.Print($"[RuntimeLogBridge] enabled path={m_LogPath}");
                return;
            }

            using var stream = new FileStream(m_LogPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            if (stream.Length < m_ReadPosition)
            {
                m_ReadPosition = 0;
            }

            if (stream.Length == m_ReadPosition)
            {
                return;
            }

            stream.Position = m_ReadPosition;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            var deltaText = reader.ReadToEnd();
            m_ReadPosition = stream.Position;
            if (string.IsNullOrEmpty(deltaText))
            {
                return;
            }

            var lines = deltaText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (!ShouldRelay(line))
                {
                    continue;
                }

                GD.Print($"[RuntimeLogBridge] {line}");
            }
        }

        private static bool ShouldRelay(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            if (line.StartsWith("[RuntimeLogBridge]", StringComparison.Ordinal))
            {
                return false;
            }

            if (line.StartsWith("[Godot]:", StringComparison.Ordinal) ||
                line.StartsWith("[GodotGuiFlowDemo]", StringComparison.Ordinal) ||
                line.StartsWith("[UILogin]", StringComparison.Ordinal) ||
                line.StartsWith("[UIManager]", StringComparison.Ordinal) ||
                line.StartsWith("ERROR:", StringComparison.Ordinal) ||
                line.StartsWith("WARNING:", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}
#endif
