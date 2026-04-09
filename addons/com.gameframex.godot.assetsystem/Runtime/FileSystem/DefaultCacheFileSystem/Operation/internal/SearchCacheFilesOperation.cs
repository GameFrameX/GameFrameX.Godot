using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class SearchCacheFilesOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            Prepare,
            SearchFiles,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private IEnumerator<DirectoryInfo> _filesEnumerator = null;
        private float _verifyStartTime;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 需要验证的元素
        /// </summary>
        public readonly List<CacheFileElement> Result = new(5000);


        [AssetSystemPreserve]
        internal SearchCacheFilesOperation(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.Prepare;
                _verifyStartTime = AssetSystemTime.RealtimeSinceStartup;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.Prepare)
            {
                var rootDirectory = new DirectoryInfo(_fileSystem.GetCacheFilesRoot());
                if (rootDirectory.Exists)
                {
                    var directorieInfos = rootDirectory.EnumerateDirectories();
                    _filesEnumerator = directorieInfos.GetEnumerator();
                }

                _steps = ESteps.SearchFiles;
            }

            if (_steps == ESteps.SearchFiles)
            {
                if (SearchFiles())
                {
                    return;
                }

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
                var costTime = AssetSystemTime.RealtimeSinceStartup - _verifyStartTime;
                AssetSystemLogger.Log($"Search cache files elapsed time {costTime:f1} seconds");
            }
        }

        [AssetSystemPreserve]
        private bool SearchFiles()
        {
            if (_filesEnumerator == null)
            {
                return false;
            }

            bool isFindItem;
            while (true)
            {
                isFindItem = _filesEnumerator.MoveNext();
                if (isFindItem == false)
                {
                    break;
                }

                var rootFoder = _filesEnumerator.Current;
                var childDirectories = rootFoder.GetDirectories();
                foreach (var chidDirectory in childDirectories)
                {
                    var bundleGUID = chidDirectory.Name;
                    if (_fileSystem.IsRecordFile(bundleGUID))
                    {
                        continue;
                    }

                    // 创建验证元素类
                    var fileRootPath = chidDirectory.FullName;
                    var dataFilePath = $"{fileRootPath}/{DefaultCacheFileSystemDefine.SaveBundleDataFileName}";
                    var infoFilePath = $"{fileRootPath}/{DefaultCacheFileSystemDefine.SaveBundleInfoFileName}";

                    // 存储的数据文件追加文件格式
                    if (_fileSystem.AppendFileExtension)
                    {
                        var dataFileExtension = FindDataFileExtension(chidDirectory);
                        if (string.IsNullOrEmpty(dataFileExtension) == false)
                        {
                            dataFilePath += dataFileExtension;
                        }
                    }

                    var element = new CacheFileElement(_fileSystem.PackageName, bundleGUID, fileRootPath, dataFilePath, infoFilePath);
                    Result.Add(element);
                }

                if (OperationSystem.IsBusy)
                {
                    break;
                }
            }

            return isFindItem;
        }

        [AssetSystemPreserve]
        private string FindDataFileExtension(DirectoryInfo directoryInfo)
        {
            var dataFileExtension = string.Empty;
            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Name.StartsWith(DefaultCacheFileSystemDefine.SaveBundleDataFileName))
                {
                    dataFileExtension = fileInfo.Extension;
                    break;
                }
            }

            return dataFileExtension;
        }
    }
}
