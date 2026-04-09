using GameFrameX.AssetSystem.Networking;
using GameFrameX.AssetSystem;

[AssetSystemPreserve]
internal class WXFSDownloadFileOperation : DefaultDownloadFileOperation
{
    private WechatFileSystem _fileSystem;
    private ESteps _steps = ESteps.None;

    [AssetSystemPreserve]
    internal WXFSDownloadFileOperation(WechatFileSystem fileSystem, PackageBundle bundle, DownloadParam param) : base(bundle, param)
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
    private void CreateWebRequest()
    {
        _webRequest = UnityWebRequestAssetBundle.GetAssetBundle(_requestURL);
        DownloadSystemHelper.SetRequestTimeout(_webRequest, Param.Timeout);
        _webRequest.SetRequestHeader("wechatminigame-preload", "1");
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
}
