using System;
using System.IO;
using Godot;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// Godot 版 BundleFile。
    /// 构建产物 bundle 物理文件是源资源字节的原样拷贝（每文件一 bundle，扩展名 .bundle），
    /// Godot ResourceLoader 按扩展名识别资源，无法直接加载 .bundle 文件。
    /// 真加载路径：构建时源资源按 res:// 相对路径额外打入包 PCK（AssetSystemPckPathUtility.GetPckSourceInnerPath），
    /// 运行时挂载 PCK 后按 AssetPath（即源资源 res:// 路径）ResourceLoader.Load 命中；
    /// 纹理类资源在 ResourceLoader 未命中时按 bundle 文件字节解码兜底。
    /// </summary>
    [AssetSystemPreserve]
    public sealed class BundleFile
    {
        private readonly string _sourcePath;
        private byte[] _memoryData;

        [AssetSystemPreserve]
        public BundleFile(string sourcePath = "")
        {
            _sourcePath = sourcePath ?? string.Empty;
        }

        /// <summary>
        /// 从 bundle 物理文件创建。Godot 下不解析文件内容，仅记录物理路径供纹理字节兜底读取。
        /// </summary>
        [AssetSystemPreserve]
        public static BundleFile LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return new BundleFile(path);
        }

        [AssetSystemPreserve]
        public static BundleFileCreateRequest LoadFromFileAsync(string path)
        {
            return new BundleFileCreateRequest { BundleFile = LoadFromFile(path) };
        }

        /// <summary>
        /// 从内存字节创建。Godot 下仅保留字节供纹理解码兜底；res:// 资源命中仍走 ResourceLoader。
        /// </summary>
        [AssetSystemPreserve]
        public static BundleFile LoadFromMemory(byte[] binary)
        {
            return new BundleFile("memory://" + (binary == null ? 0 : binary.Length)) { _memoryData = binary };
        }

        [AssetSystemPreserve]
        public static BundleFileCreateRequest LoadFromMemoryAsync(byte[] binary)
        {
            return new BundleFileCreateRequest { BundleFile = LoadFromMemory(binary) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetAsync(string name, Type type)
        {
            return new BundleAssetRequest { Asset = TryLoadGodotResource(name, type) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetAsync(string name)
        {
            return LoadAssetAsync(name, typeof(object));
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetWithSubAssetsAsync(string name, Type type)
        {
            return new BundleAssetRequest { AllAssets = LoadAssetWithSubAssets(name, type) };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAssetWithSubAssetsAsync(string name)
        {
            return LoadAssetWithSubAssetsAsync(name, typeof(object));
        }

        // ponytail: "每文件一 bundle"策略下 bundle 内即单个源资源，AllAssets 等价于主资源；
        // 但本层拿不到 AssetPath（仅物理 hash 路径），无法反查 res:// 路径，诚实返回空数组走失败路径。
        // 升级路径：LoadBundleFileOperation 创建 BundleFile 时把 manifest 中该 bundle 的 AssetPath 列表传入。
        [AssetSystemPreserve]
        public BundleAssetRequest LoadAllAssetsAsync(Type type)
        {
            return new BundleAssetRequest { AllAssets = Array.Empty<object>() };
        }

        [AssetSystemPreserve]
        public BundleAssetRequest LoadAllAssetsAsync()
        {
            return LoadAllAssetsAsync(typeof(object));
        }

        [AssetSystemPreserve]
        public object LoadAsset(string name, Type type)
        {
            return TryLoadGodotResource(name, type);
        }

        [AssetSystemPreserve]
        public object LoadAsset(string name)
        {
            return LoadAsset(name, typeof(object));
        }

        [AssetSystemPreserve]
        public object[] LoadAssetWithSubAssets(string name, Type type)
        {
            // Godot 资源无 Unity 式 SubAsset 概念，子资源集合即主资源单元素
            var asset = TryLoadGodotResource(name, type);
            return asset == null ? Array.Empty<object>() : new object[] { asset };
        }

        [AssetSystemPreserve]
        public object[] LoadAssetWithSubAssets(string name)
        {
            return LoadAssetWithSubAssets(name, typeof(object));
        }

        [AssetSystemPreserve]
        public object[] LoadAllAssets(Type type)
        {
            return Array.Empty<object>();
        }

        [AssetSystemPreserve]
        public object[] LoadAllAssets()
        {
            return LoadAllAssets(typeof(object));
        }

        /// <summary>
        /// no-op：Godot Resource 引用计数由引擎自理，无需手动卸载。
        /// </summary>
        [AssetSystemPreserve]
        public void Unload(bool unloadAllLoadedObjects)
        {
        }

        /// <summary>
        /// 按源资源 res:// 路径真加载：ResourceLoader 命中且类型匹配才返回；
        /// 纹理类资源未命中时按 bundle 字节解码兜底；均失败返回 null（不许假成功）。
        /// </summary>
        private object TryLoadGodotResource(string name, Type type)
        {
            var resourcePath = NormalizeResourcePath(name);
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }

            if (ResourceLoader.Exists(resourcePath))
            {
                var resource = ResourceLoader.Load(resourcePath, GetGodotTypeHint(type));
                if (resource != null && BundleAssetLoadUtility.IsTypeMatch(resource, type))
                {
                    return resource;
                }
            }

            if (ShouldDecodeImage(resourcePath, type))
            {
                var texture = TryDecodeTexture(ReadBundleBytes());
                if (texture != null && BundleAssetLoadUtility.IsTypeMatch(texture, type))
                {
                    return texture;
                }
            }

            AssetSystemLogger.Warning($"Failed to load godot resource from bundle : {resourcePath} ({_sourcePath})");
            return null;
        }

        private byte[] ReadBundleBytes()
        {
            if (_memoryData != null && _memoryData.Length > 0)
            {
                return _memoryData;
            }

            if (string.IsNullOrEmpty(_sourcePath) || _sourcePath.StartsWith("memory://", StringComparison.Ordinal))
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(_sourcePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 把 AssetPath/Address 候选规范化为 Godot ResourceLoader 可识别的 res:// 路径。
        /// </summary>
        [AssetSystemPreserve]
        internal static string NormalizeResourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var normalized = path.Replace('\\', '/');
            if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            return "res://" + normalized.TrimStart('/');
        }

        /// <summary>
        /// 获取 Godot 资源类型提示。
        /// 语义与 AssetSystem.GodotExtensions/DatabaseAssetProvider 中的同名实现一致（private 无法跨类复用）。
        /// </summary>
        [AssetSystemPreserve]
        private static string GetGodotTypeHint(Type assetType)
        {
            if (assetType == null || typeof(Resource).IsAssignableFrom(assetType) == false)
            {
                return string.Empty;
            }

            return assetType.Name;
        }

        /// <summary>
        /// 纹理兜底仅对图片扩展名资源生效；语义与 AssetSystem.GodotExtensions.ShouldDecodeImageAsset 一致。
        /// </summary>
        [AssetSystemPreserve]
        private static bool ShouldDecodeImage(string resourcePath, Type assetType)
        {
            if (assetType != null && typeof(Texture2D).IsAssignableFrom(assetType) == false)
            {
                return false;
            }

            var extension = Path.GetExtension(resourcePath);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 图片字节解码兜底；语义与 AssetSystem.GodotExtensions.TryDecodeTexture 一致。
        /// </summary>
        [AssetSystemPreserve]
        private static Texture2D TryDecodeTexture(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var image = new Image();
            var error = image.LoadPngFromBuffer(data);
            if (error != Error.Ok)
            {
                error = image.LoadJpgFromBuffer(data);
            }

            if (error != Error.Ok)
            {
                error = image.LoadWebpFromBuffer(data);
            }

            return error == Error.Ok ? ImageTexture.CreateFromImage(image) : null;
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
}
