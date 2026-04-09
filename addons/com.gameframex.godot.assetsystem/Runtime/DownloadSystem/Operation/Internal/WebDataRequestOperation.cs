using System;
using GameFrameX.AssetSystem.Networking;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public class WebDataRequestOperation : WebRequestOperation
    {
        private UnityWebRequestAsyncOperation _requestOperation;

        /// <summary>
        /// 请求结果
        /// </summary>
        public byte[] Result { private set; get; }


        [AssetSystemPreserve]
        public WebDataRequestOperation(string url, int timeout = 60, bool appendTimeTicks = false) : base(url, timeout, appendTimeTicks)
        {
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

            if (_steps == ESteps.CreateRequest)
            {
                _latestDownloadBytes = 0;
                _latestDownloadRealtime = AssetSystemTime.RealtimeSinceStartup;

                CreateWebRequest();
                _steps = ESteps.Download;
            }

            if (_steps == ESteps.Download)
            {
                Progress = _requestOperation.progress;
                if (_requestOperation.isDone == false)
                {
                    CheckRequestTimeout();
                    return;
                }

                if (CheckRequestResult())
                {
                    _steps = ESteps.Done;
                    Result = _webRequest.downloadHandler.data;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                }

                // 注意：最终释放请求器
                DisposeRequest();
            }
        }

        [AssetSystemPreserve]
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            DisposeRequest();
        }

        [AssetSystemPreserve]
        private void CreateWebRequest()
        {
            var requestURL = _requestURL;
            if (_appendTimeTicks)
            {
                requestURL += $"?time_ticks={DateTime.Now.Ticks}";
            }

            _webRequest = DownloadSystemHelper.CreateWebRequestGet(requestURL, (int)_timeout);
            var handler = new DownloadHandlerBuffer();
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _requestOperation = DownloadSystemHelper.SendWebRequest(_webRequest);
        }
    }
}
