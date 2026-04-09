using Godot;

namespace GameFrameX.AssetSystem
{
    internal static class GodotBuildIdentity
    {
        public static string Current
        {
            get
            {
                var projectName = ProjectSettings.GetSetting("application/config/name", "gameframex-godot").ToString();
                var version = ProjectSettings.GetSetting("application/config/version", string.Empty).ToString();
                if (string.IsNullOrWhiteSpace(version))
                {
                    version = Engine.IsEditorHint() ? "editor" : "runtime";
                }

                return $"{projectName}:{version}";
            }
        }
    }
}
