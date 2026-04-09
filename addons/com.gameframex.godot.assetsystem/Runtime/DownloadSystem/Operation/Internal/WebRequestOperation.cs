using System;
using System.Collections;
using System.Collections.Concurrent;
using GameFrameX.AssetSystem.Networking;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public abstract class WebRequestOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        protected enum ESteps
        {
            None,
            CreateRequest,
            Download,
            Done,
        }

        protected UnityWebRequest _webRequest;
        protected readonly string _requestURL;
        protected ESteps _steps = ESteps.None;

        protected bool _appendTimeTicks = false;

        // 超时相关
        protected readonly float _timeout;
        protected ulong _latestDownloadBytes;
        protected float _latestDownloadRealtime;
        private bool _isAbort = false;

        public string URL
        {
            get { return _requestURL; }
        }

        [AssetSystemPreserve]
        internal WebRequestOperation(string url, int timeout, bool appendTimeTicks)
        {
            _requestURL = url;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }

        /// <summary>
        /// 释放下载器
        /// </summary>
        [AssetSystemPreserve]
        protected void DisposeRequest()
        {
            if (_webRequest != null)
            {
                _webRequest.Dispose();
                _webRequest = null;
            }
        }

        /// <summary>
        /// 检测超时
        /// </summary>
        [AssetSystemPreserve]
        protected void CheckRequestTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (_isAbort == false)
            {
                if (_latestDownloadBytes != _webRequest.downloadedBytes)
                {
                    _latestDownloadBytes = _webRequest.downloadedBytes;
                    _latestDownloadRealtime = AssetSystemTime.RealtimeSinceStartup;
                }

                var offset = AssetSystemTime.RealtimeSinceStartup - _latestDownloadRealtime;
                if (offset > _timeout)
                {
                    DownloadSystemHelper.AbortRequest(_webRequest);
                    _isAbort = true;
                }
            }
        }

        /// <summary>
        /// 检测请求结果
        /// </summary>
        [AssetSystemPreserve]
        protected bool CheckRequestResult()
        {
#if UNITY_2020_3_OR_NEWER
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                Error = DownloadSystemHelper.FormatRequestError(_webRequest, _requestURL, _isAbort);
                return false;
            }
            else
            {
                return true;
            }
#else
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
            {
                Error = DownloadSystemHelper.FormatRequestError(_webRequest, _requestURL, _isAbort);
                return false;
            }
            else
            {
                return true;
            }
#endif
        }
    }
}
