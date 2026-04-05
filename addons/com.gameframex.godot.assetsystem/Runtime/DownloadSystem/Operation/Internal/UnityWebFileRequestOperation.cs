using System;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal class UnityWebFileRequestOperation : UnityWebRequestOperation
    {
        private UnityWebRequestAsyncOperation _requestOperation;
        private HttpDataRequestOperation _httpDataRequestOp;
        private readonly string _fileSavePath;

        [UnityEngine.Scripting.Preserve]
        internal UnityWebFileRequestOperation(string url, string fileSavePath, int timeout = 60, bool appendTimeTicks = false) : base(url, timeout, appendTimeTicks)
        {
            _fileSavePath = fileSavePath;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.CreateRequest;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.CreateRequest)
            {
                _latestDownloadBytes = 0;
                _latestDownloadRealtime = Time.realtimeSinceStartup;

                CreateRequest();
                _steps = ESteps.Download;
            }

            if (_steps == ESteps.Download)
            {
                if (_httpDataRequestOp != null)
                {
                    Progress = _httpDataRequestOp.Progress;
                    if (_httpDataRequestOp.IsDone == false)
                    {
                        return;
                    }

                    if (_httpDataRequestOp.Status == EOperationStatus.Succeed)
                    {
                        if (WriteDownloadedFile(_httpDataRequestOp.Result))
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Succeed;
                        }
                        else
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"Write downloaded file failed : {_fileSavePath}";
                        }
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = _httpDataRequestOp.Error;
                    }
                }
                else
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
        }

        [UnityEngine.Scripting.Preserve]
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            if (_httpDataRequestOp != null)
            {
                _httpDataRequestOp.SetAbort();
                _httpDataRequestOp = null;
            }

            DisposeRequest();
        }

        [UnityEngine.Scripting.Preserve]
        private void CreateRequest()
        {
            var requestURL = _requestURL;
            if (_appendTimeTicks)
            {
                requestURL += $"?time_ticks={DateTime.Now.Ticks}";
            }

            if (DownloadSystemHelper.HttpTransport != null)
            {
                _httpDataRequestOp = new HttpDataRequestOperation(requestURL, (int)_timeout, _appendTimeTicks);
                OperationSystem.StartOperation(null, _httpDataRequestOp);
            }
            else
            {
                _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(requestURL, (int)_timeout);
                var handler = new DownloadHandlerFile(_fileSavePath);
                handler.removeFileOnAbort = true;
                _webRequest.downloadHandler = handler;
                _webRequest.disposeDownloadHandlerOnDispose = true;
                _requestOperation = DownloadSystemHelper.SendRequest(_webRequest);
            }
        }

        [UnityEngine.Scripting.Preserve]
        private bool WriteDownloadedFile(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return false;
            }

            FileUtility.CreateFileDirectory(_fileSavePath);
            File.WriteAllBytes(_fileSavePath, data);
            return true;
        }
    }
}
