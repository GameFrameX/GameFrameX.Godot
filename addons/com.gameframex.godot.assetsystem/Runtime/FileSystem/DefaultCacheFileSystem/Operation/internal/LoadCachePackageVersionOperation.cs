using System.IO;
using UnityEngine;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal class LoadCachePackageVersionOperation : AsyncOperationBase
    {
        [UnityEngine.Scripting.Preserve]
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private UnityWebTextRequestOperation _webTextRequestOp;
        private HttpTextRequestOperation _httpTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        [UnityEngine.Scripting.Preserve]
        internal LoadCachePackageVersionOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.RequestPackageVersion)
            {
                var fileName = YooAssetSettingsData.GetPackageVersionFileName(_fileSystem.PackageName);
                var filePath = PathUtility.Combine(_fileSystem.FileRoot, fileName);
                if (File.Exists(filePath))
                {
                    if (_webTextRequestOp == null && _httpTextRequestOp == null)
                    {
                        var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                        if (DownloadSystemHelper.HttpTransport != null)
                        {
                            _httpTextRequestOp = new HttpTextRequestOperation(url, timeout: 10, appendTimeTicks: false);
                            OperationSystem.StartOperation(_fileSystem.PackageName, _httpTextRequestOp);
                        }
                        else
                        {
                            _webTextRequestOp = new UnityWebTextRequestOperation(url);
                            OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
                        }
                    }

                    var currentOperation = (AsyncOperationBase)_httpTextRequestOp ?? _webTextRequestOp;
                    if (currentOperation == null)
                    {
                        return;
                    }

                    Progress = currentOperation.Progress;
                    if (currentOperation.IsDone == false)
                    {
                        return;
                    }

                    if (currentOperation.Status == EOperationStatus.Succeed)
                    {
                        var rawVersion = _httpTextRequestOp != null ? _httpTextRequestOp.Result : _webTextRequestOp.Result;
                        PackageVersion = NormalizeText(rawVersion);
                        Debug.Log($"LoadCachePackageVersionOperation 加载本地沙盒版本成功：{PackageVersion}");
                        if (string.IsNullOrEmpty(PackageVersion))
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"cache package version file content is empty !";
                        }
                        else
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Succeed;
                        }
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = currentOperation.Error;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found cache version file : {filePath}";
                }
            }
        }

        private static string NormalizeText(string value)
        {
            return (value ?? string.Empty).Replace("\uFEFF", string.Empty).Trim();
        }
    }
}
