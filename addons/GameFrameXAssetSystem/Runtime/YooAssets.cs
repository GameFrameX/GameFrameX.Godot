using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public static partial class YooAssets
    {
        private static bool _isInitialize = false;
        private static bool _autoCreateDriver = true;
        private static int _tickFrame = 0;
        private static Node _godotDriver = null;
        private static readonly List<ResourcePackage> _packages = new List<ResourcePackage>();

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool Initialized
        {
            get { return _isInitialize; }
        }

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        /// <param name="logger">自定义日志处理</param>
        [UnityEngine.Scripting.Preserve]
        public static void Initialize(ILogger logger = null)
        {
            if (_isInitialize)
            {
                YooLogger.Warning($"{nameof(YooAssets)} is initialized !");
                return;
            }

            if (_isInitialize == false)
            {
                YooLogger.Logger = logger;

                _isInitialize = true;
                if (_autoCreateDriver)
                {
                    if (TryCreateGodotDriver() == false)
                    {
                        YooLogger.Warning("Can not create Godot driver node. Please call YooAssets.Tick() manually before Godot SceneTree is ready.");
                    }
                }
                YooLogger.Log($"{nameof(YooAssets)} initialize !");

                OperationSystem.Initialize();
            }
        }

        /// <summary>
        /// 更新资源系统
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        private static void UpdateInternal()
        {
            if (_isInitialize)
            {
                _tickFrame++;
                OperationSystem.Update();

                for (var i = 0; i < _packages.Count; i++)
                {
                    _packages[i].UpdatePackage();
                }
            }
        }

        /// <summary>
        /// 手动驱动资源系统执行一帧更新
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void Tick()
        {
            UpdateInternal();
        }

        /// <summary>
        /// 应用程序退出处理
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        internal static void OnApplicationQuit()
        {
            // 说明：在编辑器下确保播放被停止时IO类操作被终止。
            foreach (var package in _packages)
            {
                OperationSystem.ClearPackageOperation(package.PackageName);
                package.DestroyPackage();
            }

            OperationSystem.DestroyAll();
            _packages.Clear();
            _defaultPackage = null;
            _godotDriver = null;
            _tickFrame = 0;
            _isInitialize = false;
        }

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public static ResourcePackage CreatePackage(string packageName)
        {
            CheckException(packageName);
            if (ContainsPackage(packageName))
            {
                throw new Exception($"Package {packageName} already existed !");
            }

            YooLogger.Log($"Create resource package : {packageName}");
            var package = new ResourcePackage(packageName);
            _packages.Add(package);
            return package;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public static ResourcePackage GetPackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            if (package == null)
            {
                YooLogger.Error($"Can not found resource package : {packageName}");
            }

            return package;
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public static ResourcePackage TryGetPackage(string packageName)
        {
            CheckException(packageName);
            return GetPackageInternal(packageName);
        }

        /// <summary>
        /// 移除资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public static bool RemovePackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            if (package == null)
            {
                return false;
            }

            if (package.InitializeStatus != EOperationStatus.None)
            {
                YooLogger.Error($"The resource package {packageName} has not been destroyed, please call the method {nameof(ResourcePackage.DestroyAsync)} to destroy!");
                return false;
            }

            if (ReferenceEquals(package, _defaultPackage))
            {
                _defaultPackage = null;
            }

            YooLogger.Log($"Remove resource package : {packageName}");
            _packages.Remove(package);
            return true;
        }

        /// <summary>
        /// 检测资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public static bool ContainsPackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            return package != null;
        }

        /// <summary>
        /// 开启一个异步操作
        /// </summary>
        /// <param name="operation">异步操作对象</param>
        [UnityEngine.Scripting.Preserve]
        public static void StartOperation(GameAsyncOperation operation)
        {
            // 注意：游戏业务逻辑的包裹填写为空
            OperationSystem.StartOperation(string.Empty, operation);
        }


        [UnityEngine.Scripting.Preserve]
        private static ResourcePackage GetPackageInternal(string packageName)
        {
            foreach (var package in _packages)
            {
                if (package.PackageName == packageName)
                {
                    return package;
                }
            }

            return null;
        }

        /// <summary>
        /// 尝试创建Godot生命周期驱动器
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        private static bool TryCreateGodotDriver()
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree == null)
            {
                return false;
            }

            if (_godotDriver != null && _godotDriver.IsInsideTree())
            {
                return true;
            }

            var driverName = $"[{nameof(YooAssets)}]";
            _godotDriver = sceneTree.Root.FindChild(driverName, false, false) as Node;
            if (_godotDriver == null)
            {
                var driver = new YooAssetsGodotDriver();
                driver.Name = driverName;
                sceneTree.Root.AddChild(driver);
                _godotDriver = driver;
            }

            return true;
        }

        [UnityEngine.Scripting.Preserve]
        private static void CheckException(string packageName)
        {
            if (_isInitialize == false)
            {
                throw new Exception($"{nameof(YooAssets)} not initialize !");
            }

            if (string.IsNullOrEmpty(packageName))
            {
                throw new Exception("Package name is null or empty !");
            }
        }

        #region 系统参数

        /// <summary>
        /// 设置下载系统参数，自定义下载请求
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void SetDownloadSystemUnityWebRequest(UnityWebRequestDelegate createDelegate)
        {
            DownloadSystemHelper.UnityWebRequestCreater = createDelegate;
        }

        /// <summary>
        /// 设置下载系统参数，自定义 HTTP 传输层
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void SetDownloadSystemHttpTransport(IHttpTransport transport)
        {
            DownloadSystemHelper.HttpTransport = transport;
        }

        /// <summary>
        /// 设置资源后端适配器（内部使用）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        internal static void SetResourceBackend(IResourceBackend backend)
        {
            BundleAssetLoaderFactory.Backend = backend;
        }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void SetOperationSystemMaxTimeSlice(long milliseconds)
        {
            if (milliseconds < 10)
            {
                milliseconds = 10;
                YooLogger.Warning($"MaxTimeSlice minimum value is 10 milliseconds.");
            }

            OperationSystem.MaxTimeSlice = milliseconds;
        }

        /// <summary>
        /// 设置是否在初始化时自动创建运行时驱动器
        /// </summary>
        /// <param name="autoCreateDriver">是否自动创建驱动器</param>
        [UnityEngine.Scripting.Preserve]
        public static void SetAutoCreateDriver(bool autoCreateDriver)
        {
            if (_isInitialize)
            {
                YooLogger.Warning($"{nameof(SetAutoCreateDriver)} should be called before {nameof(Initialize)}.");
                return;
            }

            _autoCreateDriver = autoCreateDriver;
        }

        #endregion

        #region 调试信息

        [UnityEngine.Scripting.Preserve]
        internal static DebugReport GetDebugReport()
        {
            var report = new DebugReport();
            report.FrameCount = _tickFrame;

            foreach (var package in _packages)
            {
                var packageData = package.GetDebugPackageData();
                report.PackageDatas.Add(packageData);
            }

            return report;
        }

        [UnityEngine.Scripting.Preserve]
        public static bool TryExecuteDebugCommand(string command, string commandParam, out DebugReport report, out string message)
        {
            report = null;
            message = string.Empty;
            if (!_isInitialize)
            {
                message = $"{nameof(YooAssets)} not initialize !";
                return false;
            }

            if (string.IsNullOrEmpty(command))
            {
                message = "Debug command is null or empty !";
                return false;
            }

            var normalized = command.Trim().ToLowerInvariant();
            if (normalized == "sample_once")
            {
                var debugReport = GetDebugReport();
                if (!string.IsNullOrEmpty(commandParam))
                {
                    var packageName = commandParam.Trim();
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        var filtered = new DebugReport();
                        filtered.FrameCount = debugReport.FrameCount;
                        foreach (var packageData in debugReport.PackageDatas)
                        {
                            if (string.Equals(packageData.PackageName, packageName, StringComparison.OrdinalIgnoreCase))
                            {
                                filtered.PackageDatas.Add(packageData);
                            }
                        }

                        debugReport = filtered;
                    }
                }

                report = debugReport;
                message = "OK";
                return true;
            }

            message = $"Not support debug command : {command}";
            return false;
        }

        [UnityEngine.Scripting.Preserve]
        public static bool TryExecuteDebugCommand(byte[] commandData, out byte[] reportData, out string message)
        {
            reportData = null;
            message = string.Empty;
            if (commandData == null || commandData.Length == 0)
            {
                message = "Debug command data is null or empty !";
                return false;
            }

            RemoteCommand command;
            try
            {
                command = RemoteCommand.Deserialize(commandData);
            }
            catch (Exception ex)
            {
                message = $"Deserialize debug command failed : {ex.Message}";
                return false;
            }

            if (command == null)
            {
                message = "Debug command is null !";
                return false;
            }

            var commandName = RemoteCommand.ToCommandName(command.CommandType);
            var success = TryExecuteDebugCommand(commandName, command.CommandParam, out var report, out message);
            if (!success)
            {
                return false;
            }

            reportData = DebugReport.Serialize(report);
            return true;
        }

        #endregion
    }

    [UnityEngine.Scripting.Preserve]
    internal partial class YooAssetsGodotDriver : Node
    {
        /// <summary>
        /// Godot帧驱动更新入口
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public override void _Process(double delta)
        {
            YooAssets.Tick();
        }

        /// <summary>
        /// Godot退出树时处理资源系统收尾
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public override void _ExitTree()
        {
            YooAssets.OnApplicationQuit();
        }
    }
}
