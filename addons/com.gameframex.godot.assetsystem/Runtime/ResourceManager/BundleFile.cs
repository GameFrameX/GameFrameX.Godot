using System;
using System.IO;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class BundleFile
    {
        private readonly string _sourcePath;

        [AssetSystemPreserve]
        public BundleFile(string sourcePath = "")
        {
            _sourcePath = sourcePath ?? string.Empty;
        }

        [AssetSystemPreserve]
        public static BundleFile LoadFromFile(string path)
        {
            return new BundleFile(path);
        }

        [AssetSystemPreserve]
        public static BundleFileCreateRequest LoadFromFileAsync(string path)
        {
            return new BundleFileCreateRequest { BundleFile = new BundleFile(path) };
        }

        [AssetSystemPreserve]
        public static BundleFile LoadFromMemory(byte[] binary)
        {
            var key = binary == null ? "memory://empty" : $"memory://{binary.Length}";
            return new BundleFile(key);
        }

        [AssetSystemPreserve]
        public static BundleFileCreateRequest LoadFromMemoryAsync(byte[] binary)
        {
            return new BundleFileCreateRequest { BundleFile = LoadFromMemory(binary) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetAsync(string name, Type type)
        {
            return new BundleAssetRequest { Asset = CreatePlaceholderAsset(name, type) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetAsync(string name)
        {
            return LoadAssetAsync(name, typeof(object));
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetWithSubAssetsAsync(string name, Type type)
        {
            return new BundleAssetRequest { AllAssets = CreatePlaceholderAssets(name, type) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetWithSubAssetsAsync(string name)
        {
            return LoadAssetWithSubAssetsAsync(name, typeof(object));
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAllAssetsAsync(Type type)
        {
            return new BundleAssetRequest { AllAssets = CreatePlaceholderAssets(_sourcePath, type) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAllAssetsAsync()
        {
            return LoadAllAssetsAsync(typeof(object));
        }

        [AssetSystemPreserve]
        public object LoadAsset(string name, Type type)
        {
            return CreatePlaceholderAsset(name, type);
        }

        [AssetSystemPreserve]
        public object LoadAsset(string name)
        {
            return LoadAsset(name, typeof(object));
        }

        [AssetSystemPreserve]
        public object[] LoadAssetWithSubAssets(string name, Type type)
        {
            return CreatePlaceholderAssets(name, type);
        }

        [AssetSystemPreserve]
        public object[] LoadAssetWithSubAssets(string name)
        {
            return LoadAssetWithSubAssets(name, typeof(object));
        }

        [AssetSystemPreserve]
        public object[] LoadAllAssets(Type type)
        {
            return CreatePlaceholderAssets(_sourcePath, type);
        }

        [AssetSystemPreserve]
        public object[] LoadAllAssets()
        {
            return LoadAllAssets(typeof(object));
        }

        [AssetSystemPreserve]
        public void Unload(bool unloadAllLoadedObjects)
        {
        }

        private static object[] CreatePlaceholderAssets(string name, Type type)
        {
            var asset = CreatePlaceholderAsset(name, type);
            return asset == null ? Array.Empty<object>() : new[] { asset };
        }

        private static object CreatePlaceholderAsset(string name, Type type)
        {
            var assetName = string.IsNullOrWhiteSpace(name) ? "placeholder_asset" : Path.GetFileNameWithoutExtension(name);
            if (string.IsNullOrEmpty(assetName))
            {
                assetName = "placeholder_asset";
            }

            var targetType = type ?? typeof(object);
            if (targetType == typeof(object))
            {
                return new BundlePlaceholderAsset(assetName);
            }

            try
            {
                return Activator.CreateInstance(targetType);
            }
            catch
            {
                return new BundlePlaceholderAsset(assetName);
            }
        }
    }

    [AssetSystemPreserve]
    public sealed class BundleFileCreateRequest
    {
        [AssetSystemPreserve]
        public BundleFile BundleFile { get; set; }

        [AssetSystemPreserve]
        public BundleFile assetBundle
        {
            get => BundleFile;
            set => BundleFile = value;
        }

        [AssetSystemPreserve]
        public bool isDone { get; set; } = true;

        [AssetSystemPreserve]
        public float progress { get; set; } = 1f;

        [AssetSystemPreserve]
        public int priority { get; set; }

        [AssetSystemPreserve]
        public bool allowSceneActivation { get; set; } = true;
    }

    [AssetSystemPreserve]
    public sealed class BundleAssetRequest
    {
        [AssetSystemPreserve]
        public object Asset { get; set; }

        [AssetSystemPreserve]
        public object[] AllAssets { get; set; } = Array.Empty<object>();

        [AssetSystemPreserve]
        public object asset
        {
            get => Asset;
            set => Asset = value;
        }

        [AssetSystemPreserve]
        public object[] allAssets
        {
            get => AllAssets;
            set => AllAssets = value ?? Array.Empty<object>();
        }

        [AssetSystemPreserve]
        public bool isDone { get; set; } = true;

        [AssetSystemPreserve]
        public float progress { get; set; } = 1f;

        [AssetSystemPreserve]
        public int priority { get; set; }

        [AssetSystemPreserve]
        public bool allowSceneActivation { get; set; } = true;
    }

    [AssetSystemPreserve]
    public readonly struct BundleHash
    {
        private readonly string _value;

        private BundleHash(string value)
        {
            _value = value;
        }

        [AssetSystemPreserve]
        public static BundleHash Parse(string value)
        {
            return new BundleHash(value ?? string.Empty);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }

    [AssetSystemPreserve]
    public sealed class BundlePlaceholderAsset
    {
        [AssetSystemPreserve]
        public BundlePlaceholderAsset(string name)
        {
            Name = name ?? string.Empty;
        }

        [AssetSystemPreserve]
        public string Name { get; }
    }
}
