using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal interface IResourceBackend
    {
        [AssetSystemPreserve]
        bool TryCreateBundleAssetLoader(object bundleResult, out IBundleAssetLoader loader, out string error);

        // Migration note (scheme 2): instantiate path is now Godot-native.
        [AssetSystemPreserve]
        Node Instantiate(object assetObject, Node parent);

        [AssetSystemPreserve]
        void Destroy(object target);
    }
}
