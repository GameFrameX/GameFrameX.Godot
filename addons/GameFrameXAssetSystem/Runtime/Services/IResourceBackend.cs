using UnityEngine;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal interface IResourceBackend
    {
        [UnityEngine.Scripting.Preserve]
        bool TryCreateBundleAssetLoader(object bundleResult, out IBundleAssetLoader loader, out string error);

        [UnityEngine.Scripting.Preserve]
        GameObject Instantiate(UnityEngine.Object assetObject, bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays);

        [UnityEngine.Scripting.Preserve]
        void Destroy(UnityEngine.Object target);
    }
}
