using System.Collections.Generic;
using System.IO;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DatabaseRawFileProvider : ProviderOperation
    {
        private List<string> _rawFilePathCandidates;
        private string _resolvedRawFilePath;

        [AssetSystemPreserve]
        public DatabaseRawFileProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            BeginLoadTimeRecord();
            DebugBeginRecording();
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (IsDone)
            {
                return;
            }

            if (_steps == ESteps.None)
            {
                _rawFilePathCandidates = BundleAssetLoadUtility.GetPathCandidates(MainAssetInfo);
                if (_rawFilePathCandidates.Count == 0)
                {
                    var error = $"Raw file path is invalid : {MainAssetInfo.AssetPath}";
                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _resolvedRawFilePath = TryFindReadableRawFilePath(_rawFilePathCandidates);
                if (string.IsNullOrEmpty(_resolvedRawFilePath))
                {
                    var error = $"Not found raw file : {MainAssetInfo.AssetPath}";
                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.CheckBundle;

                // 注意：模拟异步加载效果提前返回
                if (IsWaitForAsyncComplete == false)
                {
                    return;
                }
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadBundleFileOp.IsDone == false)
                {
                    return;
                }

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Checking;
            }

            // 2. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                RawBundleObject = new RawBundle(null, null, _resolvedRawFilePath);
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
        }

        /// <summary>
        /// 从候选路径中查找可读取的原始文件路径
        /// </summary>
        [AssetSystemPreserve]
        private static string TryFindReadableRawFilePath(List<string> pathCandidates)
        {
            for (var i = 0; i < pathCandidates.Count; i++)
            {
                var rawFilePath = pathCandidates[i];
                if (string.IsNullOrEmpty(rawFilePath))
                {
                    continue;
                }

                if (IsRawFileExists(rawFilePath))
                {
                    return rawFilePath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 检测原始文件路径是否存在
        /// </summary>
        [AssetSystemPreserve]
        private static bool IsRawFileExists(string rawFilePath)
        {
            return File.Exists(rawFilePath);
        }
    }
}
