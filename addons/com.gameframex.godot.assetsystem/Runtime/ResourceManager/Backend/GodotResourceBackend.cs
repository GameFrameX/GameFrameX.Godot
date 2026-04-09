using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class GodotResourceBackend : IResourceBackend
    {
        [AssetSystemPreserve]
        public bool TryCreateBundleAssetLoader(object bundleResult, out IBundleAssetLoader loader, out string error)
        {
            if (bundleResult is BundleFile assetBundle)
            {
                loader = new UnityAssetBundleLoader(assetBundle);
                error = string.Empty;
                return true;
            }

            loader = null;
            error = "Try load raw file using bundle asset loader method !";
            return false;
        }

        // Migration note (scheme 2): instantiate directly from Godot scene resources.
        [AssetSystemPreserve]
        public Node Instantiate(object assetObject, Node parent)
        {
            Node instance = null;
            var runtimeObject = assetObject;

            if (runtimeObject is PackedScene packedScene)
            {
                instance = packedScene.Instantiate();
            }
            else if (runtimeObject is Node node)
            {
                instance = node.Duplicate() as Node;
            }
            else
            {
                return null;
            }

            if (instance != null && parent != null)
            {
                parent.AddChild(instance);
            }

            return instance;
        }

        [AssetSystemPreserve]
        public void Destroy(object target)
        {
            if (target is Node node && GodotObject.IsInstanceValid(node))
            {
                node.QueueFree();
                return;
            }

            if (target is System.IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
