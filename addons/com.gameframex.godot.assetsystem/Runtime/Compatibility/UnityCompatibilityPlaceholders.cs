using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; } = string.Empty;

        public static void DontDestroyOnLoad(Object target)
        {
        }

        public static T Instantiate<T>(T original) where T : Object
        {
            return original;
        }

        public static T Instantiate<T>(T original, Transform parent, bool instantiateInWorldSpace = false) where T : Object
        {
            return original;
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            return original;
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            return original;
        }

        public static void Destroy(Object obj)
        {
        }
    }

    public class Component : Object
    {
        public GameObject gameObject { get; internal set; }
        public Transform transform => gameObject?.transform;
    }

    public class Behaviour : Component
    {
    }

    public class MonoBehaviour : Behaviour
    {
    }

    public class GameObject : Object
    {
        public GameObject(string name = "")
        {
            this.name = name;
            transform = new Transform { gameObject = this };
        }

        public new string name { get; set; }
        public Transform transform { get; }

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T { gameObject = this };
            return component;
        }
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public static Vector3 zero => default;
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public static Quaternion identity => new Quaternion { w = 1f };
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new()
        {
            return new T();
        }

        public static ScriptableObject CreateInstance(Type type)
        {
            return Activator.CreateInstance(type) as ScriptableObject;
        }
    }

    public class AsyncOperation
    {
        public bool isDone { get; set; } = true;
        public float progress { get; set; } = 1f;
        public int priority { get; set; }
        public bool allowSceneActivation { get; set; } = true;
    }

    public class AssetBundle : Object
    {
        public static AssetBundle LoadFromFile(string path)
        {
            return new AssetBundle();
        }

        public static AssetBundleCreateRequest LoadFromFileAsync(string path)
        {
            return new AssetBundleCreateRequest { assetBundle = new AssetBundle() };
        }

        public static AssetBundle LoadFromMemory(byte[] binary)
        {
            return new AssetBundle();
        }

        public static AssetBundleCreateRequest LoadFromMemoryAsync(byte[] binary)
        {
            return new AssetBundleCreateRequest { assetBundle = new AssetBundle() };
        }

        public AssetBundleRequest LoadAssetAsync(string name, Type type)
        {
            return new AssetBundleRequest();
        }

        public AssetBundleRequest LoadAssetAsync(string name)
        {
            return LoadAssetAsync(name, typeof(Object));
        }

        public AssetBundleRequest LoadAssetWithSubAssetsAsync(string name, Type type)
        {
            return new AssetBundleRequest();
        }

        public AssetBundleRequest LoadAssetWithSubAssetsAsync(string name)
        {
            return LoadAssetWithSubAssetsAsync(name, typeof(Object));
        }

        public AssetBundleRequest LoadAllAssetsAsync(Type type)
        {
            return new AssetBundleRequest();
        }

        public AssetBundleRequest LoadAllAssetsAsync()
        {
            return LoadAllAssetsAsync(typeof(Object));
        }

        public Object LoadAsset(string name, Type type)
        {
            return null;
        }

        public Object LoadAsset(string name)
        {
            return LoadAsset(name, typeof(Object));
        }

        public Object[] LoadAssetWithSubAssets(string name, Type type)
        {
            return Array.Empty<Object>();
        }

        public Object[] LoadAssetWithSubAssets(string name)
        {
            return LoadAssetWithSubAssets(name, typeof(Object));
        }

        public Object[] LoadAllAssets(Type type)
        {
            return Array.Empty<Object>();
        }

        public Object[] LoadAllAssets()
        {
            return LoadAllAssets(typeof(Object));
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
        }
    }

    public class AssetBundleCreateRequest : AsyncOperation
    {
        public AssetBundle assetBundle { get; set; }
    }

    public class AssetBundleRequest : AsyncOperation
    {
        public Object asset { get; set; }
        public Object[] allAssets { get; set; } = Array.Empty<Object>();
    }

    public readonly struct Hash128
    {
        private readonly string _value;

        private Hash128(string value)
        {
            _value = value;
        }

        public static Hash128 Parse(string value)
        {
            return new Hash128(value ?? string.Empty);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DisallowMultipleComponentAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AddComponentMenuAttribute : Attribute
    {
        public AddComponentMenuAttribute(string componentMenu)
        {
            ComponentMenu = componentMenu;
        }

        public string ComponentMenu { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public sealed class SerializeField : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        public string Tooltip { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string fileName { get; set; }
        public string menuName { get; set; }
        public int order { get; set; }
    }

    public static class Time
    {
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public static int frameCount { get; set; }
        public static float realtimeSinceStartup => (float)_stopwatch.Elapsed.TotalSeconds;
        public static float unscaledDeltaTime { get; set; }
    }

    public static class Debug
    {
        public static void Log(object message)
        {
        }

        public static void LogWarning(object message)
        {
        }

        public static void LogError(object message)
        {
        }

        public static void LogException(Exception exception)
        {
        }
    }

    public static class Application
    {
        public static string dataPath => CompatibilityPathResolver.GetDataPath();
        public static string persistentDataPath => CompatibilityPathResolver.GetPersistentDataPath();
        public static string streamingAssetsPath => CompatibilityPathResolver.GetStreamingAssetsPath();
        public static string buildGUID => "godot-build-guid-placeholder";
    }

    internal static class CompatibilityPathResolver
    {
        /// <summary>
        /// 获取应用数据目录绝对路径
        /// </summary>
        public static string GetDataPath()
        {
            return NormalizePath(AppContext.BaseDirectory);
        }

        /// <summary>
        /// 获取持久化目录绝对路径
        /// </summary>
        public static string GetPersistentDataPath()
        {
            try
            {
                var path = global::Godot.ProjectSettings.GlobalizePath("user://");
                EnsureDirectoryExists(path);
                return NormalizePath(path);
            }
            catch
            {
                var fallback = Path.GetTempPath();
                EnsureDirectoryExists(fallback);
                return NormalizePath(fallback);
            }
        }

        /// <summary>
        /// 获取内置资源目录绝对路径
        /// </summary>
        public static string GetStreamingAssetsPath()
        {
            try
            {
                var path = global::Godot.ProjectSettings.GlobalizePath("res://streaming_assets");
                return NormalizePath(path);
            }
            catch
            {
                return NormalizePath(Path.Combine(AppContext.BaseDirectory, "streaming_assets"));
            }
        }

        /// <summary>
        /// 规范化路径分隔符
        /// </summary>
        private static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/');
        }

        /// <summary>
        /// 确保目录存在
        /// </summary>
        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    public static class Mathf
    {
        public static int FloorToInt(float value)
        {
            return (int)Math.Floor(value);
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }

    public static class Resources
    {
        public static T Load<T>(string path) where T : class
        {
            return null;
        }

        public static AsyncOperation UnloadUnusedAssets()
        {
            return new AsyncOperation();
        }
    }

    public static class JsonUtility
    {
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = prettyPrint
            };
            return JsonSerializer.Serialize(obj, options);
        }

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}

namespace UnityEngine.SceneManagement
{
    public enum LoadSceneMode
    {
        Single = 0,
        Additive = 1
    }

    public enum LocalPhysicsMode
    {
        None = 0
    }

    public struct LoadSceneParameters
    {
        public LoadSceneParameters(LoadSceneMode loadSceneMode) : this(loadSceneMode, LocalPhysicsMode.None)
        {
        }

        public LoadSceneParameters(LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode)
        {
            this.loadSceneMode = loadSceneMode;
            this.localPhysicsMode = localPhysicsMode;
        }

        public LoadSceneMode loadSceneMode { get; set; }
        public LocalPhysicsMode localPhysicsMode { get; set; }
    }

    public struct Scene
    {
        public string name { get; set; }
        public bool isLoaded { get; set; }

        public bool IsValid()
        {
            return true;
        }
    }

    public static class SceneManager
    {
        public static int sceneCount => 1;

        public static Scene LoadScene(string sceneName, LoadSceneParameters parameters)
        {
            return new Scene { name = sceneName, isLoaded = true };
        }

        public static UnityEngine.AsyncOperation LoadSceneAsync(string sceneName, LoadSceneParameters parameters)
        {
            return new UnityEngine.AsyncOperation();
        }

        public static UnityEngine.AsyncOperation UnloadSceneAsync(Scene scene)
        {
            return new UnityEngine.AsyncOperation();
        }

        public static bool SetActiveScene(Scene scene)
        {
            return true;
        }

        public static Scene GetSceneAt(int index)
        {
            return new Scene { name = $"Scene_{index}", isLoaded = true };
        }

        public static Scene GetActiveScene()
        {
            return new Scene { name = "ActiveScene", isLoaded = true };
        }
    }
}

namespace UnityEngine.Networking
{
    using UnityEngine;

    public class DownloadHandler : IDisposable
    {
        public virtual byte[] data => null;
        public virtual string text => null;

        public void Dispose()
        {
        }
    }

    public class DownloadHandlerScript : DownloadHandler
    {
        protected DownloadHandlerScript()
        {
        }

        protected DownloadHandlerScript(byte[] preallocatedBuffer)
        {
        }

        protected virtual bool ReceiveData(byte[] data, int dataLength)
        {
            return true;
        }

        protected virtual byte[] GetData()
        {
            return null;
        }

        protected virtual string GetText()
        {
            return null;
        }

        protected virtual float GetProgress()
        {
            return 0f;
        }
    }

    public class DownloadHandlerBuffer : DownloadHandler
    {
        private readonly byte[] _data = Array.Empty<byte>();
        public override byte[] data => _data;
        public override string text => string.Empty;
    }

    public class DownloadHandlerFile : DownloadHandler
    {
        public bool removeFileOnAbort { get; set; }

        public DownloadHandlerFile(string filePath)
        {
        }

        public DownloadHandlerFile(string filePath, bool append)
        {
        }
    }

    public class DownloadHandlerAssetBundle : DownloadHandler
    {
        public DownloadHandlerAssetBundle(string url, uint crc)
        {
        }

        public DownloadHandlerAssetBundle(string url, Hash128 hash, uint crc)
        {
        }

        public bool autoLoadAssetBundle { get; set; } = true;
        public AssetBundle assetBundle { get; set; } = new AssetBundle();

        public static AssetBundle GetContent(UnityWebRequest www)
        {
            return new AssetBundle();
        }
    }

    public static class UnityWebRequestAssetBundle
    {
        public static UnityWebRequest GetAssetBundle(string uri)
        {
            return new UnityWebRequest(uri, UnityWebRequest.kHttpVerbGET)
            {
                downloadHandler = new DownloadHandlerAssetBundle(uri, 0)
            };
        }
    }

    public class UnityWebRequestAsyncOperation : AsyncOperation
    {
        public UnityWebRequest webRequest { get; set; }
    }

    public class UnityWebRequest : IDisposable
    {
        public const string kHttpVerbGET = "GET";
        public const string kHttpVerbHEAD = "HEAD";

        public enum Result
        {
            InProgress = 0,
            Success = 1,
            ConnectionError = 2,
            ProtocolError = 3,
            DataProcessingError = 4
        }

        public UnityWebRequest(string url, string method)
        {
            this.url = url;
            this.method = method;
        }

        public string url { get; set; }
        public string method { get; set; }
        public string error { get; set; }
        public long responseCode { get; set; }
        public bool disposeDownloadHandlerOnDispose { get; set; }
        public DownloadHandler downloadHandler { get; set; }
        public float downloadProgress { get; set; } = 1f;
        public ulong downloadedBytes { get; set; }
        public bool isDone { get; set; } = true;
        public bool isNetworkError => false;
        public bool isHttpError => false;
        public int timeout { get; set; }
        public Result result { get; set; } = Result.Success;

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return new UnityWebRequestAsyncOperation { webRequest = this };
        }

        public void SetRequestHeader(string name, string value)
        {
        }

        public static UnityWebRequest Get(string uri)
        {
            return new UnityWebRequest(uri, kHttpVerbGET);
        }

        public static UnityWebRequest Head(string uri)
        {
            return new UnityWebRequest(uri, kHttpVerbHEAD);
        }

        public void Abort()
        {
        }

        public void Dispose()
        {
        }
    }
}

public static class BetterStreamingAssets
{
    public static void Initialize()
    {
    }

    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }
}

namespace UnityEngine.Networking.PlayerConnection
{
    using System;
    using System.Collections.Generic;

    public class MessageEventArgs : EventArgs
    {
        public byte[] data { get; set; } = Array.Empty<byte>();
    }

    public sealed class PlayerConnection
    {
        private static readonly PlayerConnection _instance = new PlayerConnection();
        private readonly Dictionary<Guid, Action<MessageEventArgs>> _handlers = new Dictionary<Guid, Action<MessageEventArgs>>();

        public static PlayerConnection instance => _instance;

        public void Register(Guid messageId, Action<MessageEventArgs> callback)
        {
            _handlers[messageId] = callback;
        }

        public void Unregister(Guid messageId, Action<MessageEventArgs> callback)
        {
            if (_handlers.TryGetValue(messageId, out var current) && current == callback)
            {
                _handlers.Remove(messageId);
            }
        }

        public void Send(Guid messageId, byte[] data)
        {
            if (_handlers.TryGetValue(messageId, out var callback))
            {
                callback?.Invoke(new MessageEventArgs { data = data ?? Array.Empty<byte>() });
            }
        }
    }
}
