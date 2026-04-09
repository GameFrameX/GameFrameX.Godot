using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// Godot扩展入口：提供原生资源读取与PCK挂载桥接能力。
    /// </summary>
    public static partial class AssetSystem
    {
        private static ResourcePackage _defaultPackage;
        private static readonly HashSet<string> MountedGodotResourcePacks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 设置默认的资源包
        /// </summary>
        [AssetSystemPreserve]
        public static void SetDefaultPackage(ResourcePackage package)
        {
            if (package == null)
            {
                throw new Exception("Default package is null.");
            }

            if (ContainsPackage(package.PackageName) == false)
            {
                throw new Exception($"Default package is not registered : {package.PackageName}");
            }

            _defaultPackage = package;
        }

        #region 资源信息

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static bool IsNeedDownloadFromRemote(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.IsNeedDownloadFromRemote(location);
        }

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tag">资源标签</param>
        [AssetSystemPreserve]
        public static AssetInfo[] GetAssetInfos(string tag)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfos(tag);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        [AssetSystemPreserve]
        public static AssetInfo[] GetAssetInfos(string[] tags)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfos(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AssetInfo GetAssetInfo(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfo(location);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public static AssetInfo GetAssetInfo(string location, Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfo(location, type);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        [AssetSystemPreserve]
        public static AssetInfo GetAssetInfoByGUID(string assetGUID)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfoByGUID(assetGUID);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public static AssetInfo GetAssetInfoByGUID(string assetGUID, Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfoByGUID(assetGUID, type);
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static bool CheckLocationValid(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CheckLocationValid(location);
        }

        #endregion

        #region 原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static RawFileHandle LoadRawFileSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileSync(location);
        }


        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static RawFileHandle LoadRawFileAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileAsync(location, priority);
        }

        /// <summary>
        /// 通过原生文件句柄加载 Godot 资源。
        /// </summary>
        /// <param name="rawFileHandle">原生文件句柄</param>
        /// <param name="typeHint">资源类型提示，可为空</param>
        [AssetSystemPreserve]
        public static Resource LoadGodotResourceFromRawFile(RawFileHandle rawFileHandle, string typeHint = "")
        {
            if (rawFileHandle == null)
            {
                AssetSystemLogger.Warning("Raw file handle is null.");
                return null;
            }

            var rawFilePath = rawFileHandle.GetRawFilePath();
            if (string.IsNullOrWhiteSpace(rawFilePath))
            {
                AssetSystemLogger.Warning("Raw file path is empty.");
                return null;
            }

            return LoadGodotResourceFromPath(rawFilePath, typeHint);
        }

        /// <summary>
        /// 同步加载原生文件并转换为 Godot 资源。
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="typeHint">资源类型提示，可为空</param>
        [AssetSystemPreserve]
        public static Resource LoadGodotResourceFromRawFileSync(string location, string typeHint = "")
        {
            DebugCheckDefaultPackageValid();
            var rawFileHandle = _defaultPackage.LoadRawFileSync(location);
            try
            {
                return LoadGodotResourceFromRawFile(rawFileHandle, typeHint);
            }
            finally
            {
                rawFileHandle?.Release();
            }
        }

        /// <summary>
        /// 同步加载原生文件并转换为指定类型的 Godot 资源。
        /// </summary>
        [AssetSystemPreserve]
        public static TResource LoadGodotResourceFromRawFileSync<TResource>(string location) where TResource : Resource
        {
            var resource = LoadGodotResourceFromRawFileSync(location, typeof(TResource).Name);
            return resource as TResource;
        }

        /// <summary>
        /// 同步加载原生文件并转换为 Godot 资源。
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="typeHint">资源类型提示，可为空</param>
        [AssetSystemPreserve]
        public static Resource LoadGodotResourceFromRawFileSync(AssetInfo assetInfo, string typeHint = "")
        {
            DebugCheckDefaultPackageValid();
            var rawFileHandle = _defaultPackage.LoadRawFileSync(assetInfo);
            try
            {
                return LoadGodotResourceFromRawFile(rawFileHandle, typeHint);
            }
            finally
            {
                rawFileHandle?.Release();
            }
        }

        /// <summary>
        /// 同步加载原生文件并转换为指定类型的 Godot 资源。
        /// </summary>
        [AssetSystemPreserve]
        public static TResource LoadGodotResourceFromRawFileSync<TResource>(AssetInfo assetInfo) where TResource : Resource
        {
            var resource = LoadGodotResourceFromRawFileSync(assetInfo, typeof(TResource).Name);
            return resource as TResource;
        }

        /// <summary>
        /// 尝试用不带路径的资源名从资源包读取 Godot 资源。
        /// 优先查询指定资源包的清单；清单未命中时，会挂载并扫描永久目录下的其它 PCK。
        /// </summary>
        /// <param name="assetName">资源名。可传 "logo" 或 "logo.png"，不要传路径。</param>
        /// <param name="packageName">资源包名，默认 main。</param>
        /// <param name="assetType">可选 Godot 资源类型；不传时只按资源名匹配。</param>
        [AssetSystemPreserve]
        public static Resource TryGetPackageAsset(string assetName, string packageName = "main", Type assetType = null)
        {
            var normalizedAssetName = NormalizeLooseAssetName(assetName);
            if (string.IsNullOrEmpty(normalizedAssetName))
            {
                return null;
            }

            var package = string.IsNullOrWhiteSpace(packageName) ? null : TryGetPackage(packageName);
            if (package != null && package.TryGetAssetLocationByName(normalizedAssetName, out var location))
            {
                return TryLoadPackageRawAsset(package, location, assetType);
            }

            MountSiblingGodotResourcePacks(packageName);
            var mountedPath = TryFindMountedGodotResourcePathByName(normalizedAssetName);
            if (string.IsNullOrEmpty(mountedPath))
            {
                return null;
            }

            return TryLoadMountedGodotResource(mountedPath, assetType);
        }

        /// <summary>
        /// 尝试用不带路径的资源名从资源包读取指定 Godot 资源。
        /// </summary>
        [AssetSystemPreserve]
        public static TResource TryGetPackageAsset<TResource>(string assetName, string packageName = "main") where TResource : Resource
        {
            return TryGetPackageAsset(assetName, packageName, typeof(TResource)) as TResource;
        }

        [AssetSystemPreserve]
        private static Resource LoadGodotResourceFromPath(string rawFilePath, string typeHint)
        {
            var pathCandidates = new List<string>(2);
            if (rawFilePath.StartsWith("res://") || rawFilePath.StartsWith("user://"))
            {
                pathCandidates.Add(rawFilePath);
            }
            else
            {
                var localizedPath = ProjectSettings.LocalizePath(rawFilePath);
                if (string.IsNullOrWhiteSpace(localizedPath) == false)
                {
                    pathCandidates.Add(localizedPath);
                }
            }

            for (var i = 0; i < pathCandidates.Count; i++)
            {
                var candidate = pathCandidates[i];
                if (ResourceLoader.Exists(candidate) == false)
                {
                    continue;
                }

                var resource = ResourceLoader.Load(candidate, typeHint);
                if (resource != null)
                {
                    return resource;
                }
            }

            AssetSystemLogger.Warning($"Can not load Godot resource from raw file path : {rawFilePath}");
            return null;
        }

        [AssetSystemPreserve]
        private static Resource TryLoadPackageRawAsset(ResourcePackage package, string location, Type assetType)
        {
            var rawFileHandle = package.LoadRawFileSync(location);
            try
            {
                if (rawFileHandle == null || rawFileHandle.Status != EOperationStatus.Succeed)
                {
                    return null;
                }

                var rawFilePath = rawFileHandle.GetRawFilePath();
                if (rawFilePath.StartsWith("res://", StringComparison.Ordinal) || rawFilePath.StartsWith("user://", StringComparison.Ordinal))
                {
                    var resource = TryLoadMountedGodotResource(rawFilePath, assetType);
                    if (resource != null)
                    {
                        return resource;
                    }
                }

                if (ShouldDecodeImageAsset(location, assetType))
                {
                    return TryDecodeTexture(rawFileHandle.GetRawFileData());
                }

                return null;
            }
            finally
            {
                rawFileHandle?.Release();
            }
        }

        [AssetSystemPreserve]
        private static Resource TryLoadMountedGodotResource(string resourcePath, Type assetType)
        {
            var resource = ResourceLoader.Load(resourcePath, GetGodotTypeHint(assetType));
            if (resource == null)
            {
                return null;
            }

            return IsAssetTypeMatch(resource, assetType) ? resource : null;
        }

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

        [AssetSystemPreserve]
        private static bool ShouldDecodeImageAsset(string location, Type assetType)
        {
            if (assetType != null && typeof(Texture2D).IsAssignableFrom(assetType) == false)
            {
                return false;
            }

            var extension = Path.GetExtension(location);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase);
        }

        [AssetSystemPreserve]
        private static bool IsAssetTypeMatch(Resource resource, Type assetType)
        {
            if (assetType == null)
            {
                return true;
            }

            return assetType.IsInstanceOfType(resource);
        }

        [AssetSystemPreserve]
        private static string GetGodotTypeHint(Type assetType)
        {
            if (assetType == null || typeof(Resource).IsAssignableFrom(assetType) == false)
            {
                return string.Empty;
            }

            return assetType.Name;
        }

        [AssetSystemPreserve]
        private static string NormalizeLooseAssetName(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                return string.Empty;
            }

            var normalized = assetName.Replace('\\', '/').Trim();
            var index = normalized.LastIndexOf('/');
            if (index >= 0)
            {
                normalized = normalized.Substring(index + 1);
            }

            return normalized.ToLowerInvariant();
        }

        /// <summary>
        /// 挂载原生文件句柄对应的 Godot 资源包（PCK）。
        /// </summary>
        [AssetSystemPreserve]
        public static bool MountGodotResourcePackFromRawFile(RawFileHandle rawFileHandle, bool replaceFiles = false, int offset = 0)
        {
            if (rawFileHandle == null)
            {
                AssetSystemLogger.Warning("Raw file handle is null.");
                return false;
            }

            var rawFilePath = rawFileHandle.GetRawFilePath();
            if (string.IsNullOrWhiteSpace(rawFilePath))
            {
                AssetSystemLogger.Warning("Raw file path is empty.");
                return false;
            }

            return MountGodotResourcePack(rawFilePath, replaceFiles, offset);
        }

        /// <summary>
        /// 同步加载原生文件并挂载 Godot 资源包（PCK）。
        /// </summary>
        [AssetSystemPreserve]
        public static bool MountGodotResourcePackFromRawFileSync(string location, bool replaceFiles = false, int offset = 0)
        {
            DebugCheckDefaultPackageValid();
            var rawFileHandle = _defaultPackage.LoadRawFileSync(location);
            try
            {
                return MountGodotResourcePackFromRawFile(rawFileHandle, replaceFiles, offset);
            }
            finally
            {
                rawFileHandle?.Release();
            }
        }

        /// <summary>
        /// 同步加载原生文件并挂载 Godot 资源包（PCK）。
        /// </summary>
        [AssetSystemPreserve]
        public static bool MountGodotResourcePackFromRawFileSync(AssetInfo assetInfo, bool replaceFiles = false, int offset = 0)
        {
            DebugCheckDefaultPackageValid();
            var rawFileHandle = _defaultPackage.LoadRawFileSync(assetInfo);
            try
            {
                return MountGodotResourcePackFromRawFile(rawFileHandle, replaceFiles, offset);
            }
            finally
            {
                rawFileHandle?.Release();
            }
        }

        /// <summary>
        /// 通过路径挂载 Godot 资源包（PCK）。
        /// </summary>
        [AssetSystemPreserve]
        public static bool MountGodotResourcePackByPath(string pckPath, bool replaceFiles = false, int offset = 0)
        {
            return MountGodotResourcePack(pckPath, replaceFiles, offset);
        }

        [AssetSystemPreserve]
        private static bool MountGodotResourcePack(string rawFilePath, bool replaceFiles, int offset)
        {
            var physicalPath = ResolvePhysicalPath(rawFilePath);
            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                AssetSystemLogger.Warning($"Invalid pck path : {rawFilePath}");
                return false;
            }

            if (File.Exists(physicalPath) == false)
            {
                AssetSystemLogger.Warning($"PCK file not found : {physicalPath}");
                return false;
            }

            if (MountedGodotResourcePacks.Contains(physicalPath))
            {
                AssetSystemLogger.Log($"Godot resource pack already mounted : {physicalPath}");
                return true;
            }

            var mounted = ProjectSettings.LoadResourcePack(physicalPath, replaceFiles, offset);
            if (mounted == false)
            {
                AssetSystemLogger.Warning($"Failed to mount Godot resource pack : {physicalPath}");
                return false;
            }

            MountedGodotResourcePacks.Add(physicalPath);
            AssetSystemLogger.Log($"Mounted Godot resource pack : {physicalPath}");
            return true;
        }

        [AssetSystemPreserve]
        private static void MountSiblingGodotResourcePacks(string exceptPackageName)
        {
            var packageFolder = ProjectSettings.GlobalizePath("user://hotfix/packages");
            if (Directory.Exists(packageFolder) == false)
            {
                return;
            }

            var exceptPckFileName = string.IsNullOrWhiteSpace(exceptPackageName) ? string.Empty : $"{exceptPackageName}.pck";
            var pckFiles = Directory.GetFiles(packageFolder, "*.pck", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < pckFiles.Length; i++)
            {
                var pckPath = pckFiles[i];
                if (string.Equals(Path.GetFileName(pckPath), exceptPckFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                MountGodotResourcePack(pckPath, false, 0);
            }
        }

        [AssetSystemPreserve]
        private static string TryFindMountedGodotResourcePathByName(string normalizedAssetName)
        {
            var pending = new Stack<string>();
            pending.Push("res://");

            while (pending.Count > 0)
            {
                var directoryPath = pending.Pop();
                using var directory = DirAccess.Open(directoryPath);
                if (directory == null)
                {
                    continue;
                }

                directory.ListDirBegin();
                while (true)
                {
                    var entryName = directory.GetNext();
                    if (string.IsNullOrEmpty(entryName))
                    {
                        break;
                    }

                    if (entryName == "." || entryName == ".." || entryName.StartsWith('.'))
                    {
                        continue;
                    }

                    var entryPath = CombineGodotResourcePath(directoryPath, entryName);
                    if (directory.CurrentIsDir())
                    {
                        pending.Push(entryPath);
                        continue;
                    }

                    if (IsLooseAssetNameMatch(entryName, normalizedAssetName))
                    {
                        return entryPath;
                    }
                }
            }

            return string.Empty;
        }

        [AssetSystemPreserve]
        private static bool IsLooseAssetNameMatch(string fileName, string normalizedAssetName)
        {
            var normalizedFileName = NormalizeLooseAssetName(fileName);
            if (normalizedFileName == normalizedAssetName)
            {
                return true;
            }

            return string.Equals(Path.GetFileNameWithoutExtension(normalizedFileName), normalizedAssetName, StringComparison.Ordinal);
        }

        [AssetSystemPreserve]
        private static string CombineGodotResourcePath(string directoryPath, string entryName)
        {
            if (directoryPath.EndsWith("/", StringComparison.Ordinal))
            {
                return directoryPath + entryName;
            }

            return directoryPath + "/" + entryName;
        }

        [AssetSystemPreserve]
        private static string ResolvePhysicalPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return ProjectSettings.GlobalizePath(path);
            }

            return path;
        }

        #endregion

        #region 场景加载

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        [AssetSystemPreserve]
        public static SceneHandle LoadSceneSync(string location, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSceneSync(location, sceneMode, physicsMode);
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        [AssetSystemPreserve]
        public static SceneHandle LoadSceneSync(AssetInfo assetInfo, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSceneSync(assetInfo, sceneMode, physicsMode);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">优先级</param>
        [AssetSystemPreserve]
        public static SceneHandle LoadSceneAsync(string location, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None, bool suspendLoad = false, uint priority = 100)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSceneAsync(location, sceneMode, physicsMode, suspendLoad, priority);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">优先级</param>
        [AssetSystemPreserve]
        public static SceneHandle LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None, bool suspendLoad = false, uint priority = 100)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSceneAsync(assetInfo, sceneMode, physicsMode, suspendLoad, priority);
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetSync<TObject>(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetSync(string location, Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync(location, type);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync(location);
        }


        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync<TObject>(location, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetAsync(string location, Type type, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync(location, type, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AssetHandle LoadAssetAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync(location, priority);
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsSync<TObject>(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsSync(string location, Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync(location, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync(location);
        }


        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsAsync<TObject>(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync<TObject>(location, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsAsync(string location, Type type, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync(location, type, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static SubAssetsHandle LoadSubAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync(location, priority);
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsSync<TObject>(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsSync(string location, Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync(location, type);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync(location);
        }


        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsAsync<TObject>(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync<TObject>(location, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsAsync(string location, Type type, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync(location, type, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public static AllAssetsHandle LoadAllAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync(location, priority);
        }

        #endregion

        #region 资源下载

        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceDownloader(new string[] { tag, }, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceDownloader(tags, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(location, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(locations, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(assetInfo, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
        }

        #endregion

        #region 资源解压

        /// <summary>
        /// 创建内置资源解压器，用于解压当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceUnpacker(unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceUnpacker(tag, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceUnpacker(tags, unpackingMaxNumber, failedTryAgain);
        }

        #endregion

        #region 资源导入

        /// <summary>
        /// 创建资源导入器
        /// 注意：资源文件名称必须和资源服务器部署的文件名称一致！
        /// </summary>
        /// <param name="filePaths">资源路径列表</param>
        /// <param name="importerMaxNumber">同时导入的最大文件数</param>
        /// <param name="failedTryAgain">导入失败的重试次数</param>
        [AssetSystemPreserve]
        public static ResourceImporterOperation CreateResourceImporter(string[] filePaths, int importerMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceImporter(filePaths, importerMaxNumber, failedTryAgain);
        }

        #endregion

        #region 调试方法

        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        private static void DebugCheckDefaultPackageValid()
        {
            if (_defaultPackage == null)
            {
                throw new Exception($"Default package is null. Please use {nameof(SetDefaultPackage)} !");
            }
        }

        #endregion
    }
}
