using System.Threading;
using System.Threading.Tasks;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal class HttpTextRequestOperation : AsyncOperationBase
    {
        [UnityEngine.Scripting.Preserve]
        private enum ESteps
        {
            None,
            Request,
            Done,
        }

        private readonly string _requestURL;
        private readonly int _timeout;
        private readonly bool _appendTimeTicks;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private ESteps _steps = ESteps.None;
        private Task<HttpResponse> _requestTask;

        public string Result { get; private set; }

        [UnityEngine.Scripting.Preserve]
        internal HttpTextRequestOperation(string requestURL, int timeout, bool appendTimeTicks)
        {
            _requestURL = requestURL;
            _timeout = timeout;
            _appendTimeTicks = appendTimeTicks;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.Request;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.Request)
            {
                if (_requestTask == null)
                {
                    _requestTask = DownloadSystemHelper.RequestTextAsync(_requestURL, _timeout, _appendTimeTicks, _cancellationTokenSource.Token);
                    return;
                }

                if (_requestTask.IsCompleted == false)
                {
                    return;
                }

                _steps = ESteps.Done;
                if (_requestTask.IsFaulted)
                {
                    Status = EOperationStatus.Failed;
                    Error = _requestTask.Exception?.GetBaseException().Message ?? "request task faulted.";
                    return;
                }

                if (_requestTask.IsCanceled)
                {
                    Status = EOperationStatus.Failed;
                    Error = "request canceled.";
                    return;
                }

                var response = _requestTask.Result;
                if (response == null)
                {
                    Status = EOperationStatus.Failed;
                    Error = "response is null.";
                    return;
                }

                if (response.Success)
                {
                    Result = response.Text;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    Status = EOperationStatus.Failed;
                    Error = response.Error ?? "request failed.";
                }
            }
        }

        [UnityEngine.Scripting.Preserve]
        internal override void InternalOnAbort()
        {
            _steps = ESteps.Done;
            if (_cancellationTokenSource.IsCancellationRequested == false)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}
