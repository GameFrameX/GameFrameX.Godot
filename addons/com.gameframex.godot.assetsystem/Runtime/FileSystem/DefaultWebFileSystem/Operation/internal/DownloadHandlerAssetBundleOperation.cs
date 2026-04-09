using GameFrameX.AssetSystem.Networking;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DownloadHandlerAssetBundleOperation : DefaultDownloadFileOperation
    {
        private readonly DefaultWebFileSystem _fileSystem;
        private DownloadHandlerAssetBundle _downloadhandler;
        private ESteps _steps = ESteps.None;

        public BundleFile Result { private set; get; }


        [AssetSystemPreserve]
        internal DownloadHandlerAssetBundleOperation(DefaultWebFileSystem fileSystem, PackageBundle bundle, DownloadParam param) : base(bundle, param)
        {
            _fileSystem = fileSystem;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.CreateRequest;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            // 创建下载器
            if (_steps == ESteps.CreateRequest)
            {
                // 获取请求地址
                _requestURL = GetRequestURL();

                // 重置变量
                ResetRequestFiled();

                // 创建下载器
                CreateWebRequest();

                _steps = ESteps.CheckRequest;
            }

            // 检测下载结果
            if (_steps == ESteps.CheckRequest)
            {
                DownloadProgress = _webRequest.downloadProgress;
                DownloadedBytes = (long)_webRequest.downloadedBytes;
                Progress = DownloadProgress;
                if (_webRequest.isDone == false)
                {
                    CheckRequestTimeout();
                    return;
                }

                // 检查网络错误
                if (CheckRequestResult())
                {
                    _steps = ESteps.Done;
                    Result = _downloadhandler.BundleFile;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = IsRetryableError ? ESteps.TryAgain : ESteps.Done;
                    if (_steps == ESteps.Done)
                    {
                        Status = EOperationStatus.Failed;
                        AssetSystemLogger.Error(Error);
                    }
                }

                // 注意：最终释放请求器
                DisposeWebRequest();
            }

            // 重新尝试下载
            if (_steps == ESteps.TryAgain)
            {
                if (FailedTryAgain <= 0)
                {
                    Status = EOperationStatus.Failed;
                    _steps = ESteps.Done;
                    AssetSystemLogger.Error(Error);
                    return;
                }

                _tryAgainTimer += AssetSystemTime.UnscaledDeltaTime;
                if (_tryAgainTimer > 1f)
                {
                    FailedTryAgain--;
                    _steps = ESteps.CreateRequest;
                    AssetSystemLogger.Warning(Error);
                }
            }
        }

        [AssetSystemPreserve]
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            DisposeWebRequest();
        }

        [AssetSystemPreserve]
        private void CreateWebRequest()
        {
            _downloadhandler = CreateDownloadHandler();
            _webRequest = DownloadSystemHelper.CreateWebRequestGet(_requestURL, Param.Timeout);
            _webRequest.downloadHandler = _downloadhandler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            DownloadSystemHelper.SendWebRequest(_webRequest);
        }

        [AssetSystemPreserve]
        private void DisposeWebRequest()
        {
            if (_webRequest != null)
            {
                //注意：引擎底层会自动调用Abort方法
                _webRequest.Dispose();
                _webRequest = null;
            }
        }

        [AssetSystemPreserve]
        private DownloadHandlerAssetBundle CreateDownloadHandler()
        {
            if (_fileSystem.DisableUnityWebCache)
            {
                var downloadhandler = new DownloadHandlerAssetBundle(_requestURL, 0);
#if UNITY_2020_3_OR_NEWER
                downloadhandler.autoLoadAssetBundle = false;
#endif
                return downloadhandler;
            }
            else
            {
                // 注意：优先从浏览器缓存里获取文件
                // The file hash defining the version of the asset bundle.
                var unityCRC = Bundle.UnityCRC;
                var fileHash = BundleHash.Parse(Bundle.FileHash);
                var downloadhandler = new DownloadHandlerAssetBundle(_requestURL, fileHash, unityCRC);
#if UNITY_2020_3_OR_NEWER
                downloadhandler.autoLoadAssetBundle = false;
#endif
                return downloadhandler;
            }
        }
    }
}
