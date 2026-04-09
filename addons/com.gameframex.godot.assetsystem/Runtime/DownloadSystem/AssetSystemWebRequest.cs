using System;

namespace GameFrameX.AssetSystem.Networking
{
    [AssetSystemPreserve]
    public class DownloadHandler : IDisposable
    {
        public virtual byte[] data => null;
        public virtual string text => null;

        public void Dispose()
        {
        }
    }

    [AssetSystemPreserve]
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

    [AssetSystemPreserve]
    public class DownloadHandlerBuffer : DownloadHandler
    {
        private readonly byte[] _data = Array.Empty<byte>();
        public override byte[] data => _data;
        public override string text => string.Empty;
    }

    [AssetSystemPreserve]
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

    [AssetSystemPreserve]
    public class DownloadHandlerAssetBundle : DownloadHandler
    {
        public DownloadHandlerAssetBundle(string url, uint crc)
        {
        }

        public DownloadHandlerAssetBundle(string url, BundleHash hash, uint crc)
        {
        }

        public bool autoLoadAssetBundle { get; set; } = true;
        public BundleFile BundleFile { get; set; } = new BundleFile();

        public static BundleFile GetContent(UnityWebRequest www)
        {
            return new BundleFile();
        }
    }

    [AssetSystemPreserve]
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

    [AssetSystemPreserve]
    public class UnityWebRequestAsyncOperation
    {
        public UnityWebRequest webRequest { get; set; }
        public bool isDone { get; set; } = true;
        public float progress { get; set; } = 1f;
        public int priority { get; set; }
        public bool allowSceneActivation { get; set; } = true;
    }

    [AssetSystemPreserve]
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
