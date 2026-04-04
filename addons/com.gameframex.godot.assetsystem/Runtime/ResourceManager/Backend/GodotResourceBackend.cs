using UnityEngine;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal sealed class GodotResourceBackend : IResourceBackend
    {
        [UnityEngine.Scripting.Preserve]
        public bool TryCreateBundleAssetLoader(object bundleResult, out IBundleAssetLoader loader, out string error)
        {
            if (bundleResult is AssetBundle assetBundle)
            {
                loader = new UnityAssetBundleLoader(assetBundle);
                error = string.Empty;
                return true;
            }

            loader = null;
            error = "Try load raw file using bundle asset loader method !";
            return false;
        }

        [UnityEngine.Scripting.Preserve]
        public GameObject Instantiate(UnityEngine.Object assetObject, bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            var gameObject = assetObject as GameObject;
            if (gameObject == null)
            {
                return null;
            }

            if (setPositionAndRotation)
            {
                if (parent != null)
                {
                    return UnityEngine.Object.Instantiate(gameObject, position, rotation, parent);
                }

                return UnityEngine.Object.Instantiate(gameObject, position, rotation);
            }

            if (parent != null)
            {
                return UnityEngine.Object.Instantiate(gameObject, parent, worldPositionStays);
            }

            return UnityEngine.Object.Instantiate(gameObject);
        }

        [UnityEngine.Scripting.Preserve]
        public void Destroy(UnityEngine.Object target)
        {
            if (target != null)
            {
                UnityEngine.Object.Destroy(target);
            }
        }
    }
}
