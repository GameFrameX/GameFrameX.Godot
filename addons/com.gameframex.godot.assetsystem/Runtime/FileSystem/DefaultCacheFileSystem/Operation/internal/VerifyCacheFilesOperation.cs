using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class CacheFileElement
    {
        public string PackageName { private set; get; }
        public string BundleGUID { private set; get; }
        public string FileRootPath { private set; get; }
        public string DataFilePath { private set; get; }
        public string InfoFilePath { private set; get; }

        public EFileVerifyResult Result;
        public string DataFileCRC;
        public long DataFileSize;

        [AssetSystemPreserve]
        public CacheFileElement(string packageName, string bundleGUID, string fileRootPath, string dataFilePath, string infoFilePath)
        {
            PackageName = packageName;
            BundleGUID = bundleGUID;
            FileRootPath = fileRootPath;
            DataFilePath = dataFilePath;
            InfoFilePath = infoFilePath;
        }

        [AssetSystemPreserve]
        public void DeleteFiles()
        {
            try
            {
                Directory.Delete(FileRootPath, true);
            }
            catch (Exception e)
            {
                AssetSystemLogger.Warning($"Failed to delete cache bundle folder : {e}");
            }
        }
    }

    /// <summary>
    /// 缓存文件验证（线程版）
    /// </summary>
    [AssetSystemPreserve]
    internal class VerifyCacheFilesOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        private readonly ThreadSyncContext _syncContext = new();
        private readonly DefaultCacheFileSystem _fileSystem;
        private List<CacheFileElement> _waitingList;
        private List<CacheFileElement> _verifyingList;
        private EFileVerifyLevel _verifyLevel = EFileVerifyLevel.Middle;
        private int _verifyMaxNum;
        private int _verifyTotalCount;
        private float _verifyStartTime;
        private int _succeedCount;
        private int _failedCount;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        internal VerifyCacheFilesOperation(DefaultCacheFileSystem fileSystem, List<CacheFileElement> elements)
        {
            _fileSystem = fileSystem;
            _waitingList = elements;
            _verifyLevel = _fileSystem.FileVerifyLevel;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.InitVerify;
                _verifyStartTime = AssetSystemTime.RealtimeSinceStartup;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.InitVerify)
            {
                var fileCount = _waitingList.Count;

                // 设置同时验证的最大数
                ThreadPool.GetMaxThreads(out var workerThreads, out var ioThreads);
                AssetSystemLogger.Log($"Work threads : {workerThreads}, IO threads : {ioThreads}");
                _verifyMaxNum = Math.Min(workerThreads, ioThreads);
                _verifyTotalCount = fileCount;
                if (_verifyMaxNum < 1)
                {
                    _verifyMaxNum = 1;
                }

                _verifyingList = new List<CacheFileElement>(_verifyMaxNum);
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                _syncContext.Update();

                Progress = GetProgress();
                if (_waitingList.Count == 0 && _verifyingList.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                var costTime = AssetSystemTime.RealtimeSinceStartup - _verifyStartTime;
                    AssetSystemLogger.Log($"Verify cache files elapsed time {costTime:f1} seconds");
                }

                for (var i = _waitingList.Count - 1; i >= 0; i--)
                {
                    if (OperationSystem.IsBusy)
                    {
                        break;
                    }

                    if (_verifyingList.Count >= _verifyMaxNum)
                    {
                        break;
                    }

                    var element = _waitingList[i];
                    if (BeginVerifyFileWithThread(element))
                    {
                        _waitingList.RemoveAt(i);
                        _verifyingList.Add(element);
                    }
                    else
                    {
                        AssetSystemLogger.Warning("The thread pool is failed queued.");
                        break;
                    }
                }
            }
        }

        [AssetSystemPreserve]
        private float GetProgress()
        {
            if (_verifyTotalCount == 0)
            {
                return 1f;
            }

            return (float)(_succeedCount + _failedCount) / _verifyTotalCount;
        }

        [AssetSystemPreserve]
        private bool BeginVerifyFileWithThread(CacheFileElement element)
        {
            return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), element);
        }

        [AssetSystemPreserve]
        private void VerifyInThread(object obj)
        {
            var element = (CacheFileElement)obj;
            element.Result = VerifyingCacheFile(element, _verifyLevel);
            _syncContext.Post(VerifyCallback, element);
        }

        [AssetSystemPreserve]
        private void VerifyCallback(object obj)
        {
            var element = (CacheFileElement)obj;
            _verifyingList.Remove(element);

            if (element.Result == EFileVerifyResult.Succeed)
            {
                _succeedCount++;
                var fileWrapper = new DefaultCacheFileSystem.FileWrapper(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                _fileSystem.RecordFile(element.BundleGUID, fileWrapper);
            }
            else
            {
                _failedCount++;

                AssetSystemLogger.Warning($"Failed to verify file {element.Result} and delete files : {element.FileRootPath}");
                element.DeleteFiles();
            }
        }

        /// <summary>
        /// 验证缓存文件（子线程内操作）
        /// </summary>
        [AssetSystemPreserve]
        private EFileVerifyResult VerifyingCacheFile(CacheFileElement element, EFileVerifyLevel verifyLevel)
        {
            try
            {
                if (verifyLevel == EFileVerifyLevel.Low)
                {
                    if (File.Exists(element.InfoFilePath) == false)
                    {
                        return EFileVerifyResult.InfoFileNotExisted;
                    }

                    if (File.Exists(element.DataFilePath) == false)
                    {
                        return EFileVerifyResult.DataFileNotExisted;
                    }

                    return EFileVerifyResult.Succeed;
                }
                else
                {
                    if (File.Exists(element.InfoFilePath) == false)
                    {
                        return EFileVerifyResult.InfoFileNotExisted;
                    }

                    // 解析信息文件获取验证数据
                    _fileSystem.ReadInfoFile(element.InfoFilePath, out element.DataFileCRC, out element.DataFileSize);
                }
            }
            catch (Exception)
            {
                return EFileVerifyResult.Exception;
            }

            return FileSystemHelper.FileVerify(element.DataFilePath, element.DataFileSize, element.DataFileCRC, verifyLevel);
        }
    }
}
