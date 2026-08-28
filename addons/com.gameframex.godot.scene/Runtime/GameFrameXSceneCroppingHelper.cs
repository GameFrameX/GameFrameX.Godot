using Godot;

namespace GameFrameX.Scene.Runtime
{
    public partial class GameFrameXSceneCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(ActiveSceneChangedEventArgs);
            _ = typeof(GameSceneManager);
            _ = typeof(IGameSceneManager);
            _ = typeof(LoadSceneFailureEventArgs);
            _ = typeof(LoadSceneSuccessEventArgs);
            _ = typeof(LoadSceneUpdateEventArgs);
            _ = typeof(SceneComponent);
            _ = typeof(UnloadSceneFailureEventArgs);
            _ = typeof(UnloadSceneSuccessEventArgs);
        }
    }
}
