using Godot;

namespace GameFrameX.Asset.Runtime
{
    public partial class GameFrameXAssetCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(Constant);
            _ = typeof(GfAssetSystem);
            _ = typeof(AsyncOperationBase);
            _ = typeof(OperationStatus);
            _ = typeof(OperationScheduler);
            _ = typeof(EPatchStates);
            _ = typeof(AssetDownloadProgressUpdateEventArgs);
            _ = typeof(AssetFoundUpdateFilesEventArgs);
            _ = typeof(AssetPatchManifestUpdateFailedEventArgs);
            _ = typeof(AssetPatchStatesChangeEventArgs);
            _ = typeof(AssetStaticVersionUpdateFailedEventArgs);
            _ = typeof(AssetWebFileDownloadFailedEventArgs);
        }
    }
}
