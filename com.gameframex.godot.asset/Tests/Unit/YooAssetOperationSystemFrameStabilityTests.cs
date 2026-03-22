using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace GameFrameX.Asset.Tests.Unit;

/// <summary>
/// YooAsset调度系统连续帧稳定性测试
/// </summary>
public sealed class YooAssetOperationSystemFrameStabilityTests : System.IDisposable
{
    /// <summary>
    /// 初始化调度系统
    /// </summary>
    public YooAssetOperationSystemFrameStabilityTests()
    {
        YooAsset.OperationSystem.Initialize();
    }

    /// <summary>
    /// 释放调度系统资源
    /// </summary>
    public void Dispose()
    {
        YooAsset.OperationSystem.DestroyAll();
    }

    /// <summary>
    /// 验证操作状态机在连续帧下可以稳定推进并完成
    /// </summary>
    [Fact]
    public void Update_ShouldAdvanceOperationStateMachineAcrossFrames()
    {
        var operation = new StepCompleteOperation(3);
        YooAsset.OperationSystem.StartOperation("pkg", operation);

        YooAsset.OperationSystem.Update();
        Assert.Equal(1, operation.UpdateCount);
        Assert.False(operation.IsDone);

        YooAsset.OperationSystem.Update();
        Assert.Equal(2, operation.UpdateCount);
        Assert.False(operation.IsDone);

        YooAsset.OperationSystem.Update();
        Assert.Equal(3, operation.UpdateCount);
        Assert.True(operation.IsDone);
        Assert.Equal(YooAsset.EOperationStatus.Succeed, operation.Status);
    }

    /// <summary>
    /// 验证时间片中断后下帧可继续推进后续操作，避免饥饿
    /// </summary>
    [Fact]
    public void Update_ShouldResumeFromCursorAfterTimeSliceInterrupt()
    {
        YooAsset.OperationSystem.MaxTimeSlice = 1;
        var callOrder = new List<string>();
        var heavy = new TimeSliceOperation("heavy", 2.0, callOrder);
        var light = new StepCompleteOperation(1, "light", callOrder);
        YooAsset.OperationSystem.StartOperation("pkg", heavy);
        YooAsset.OperationSystem.StartOperation("pkg", light);

        YooAsset.OperationSystem.Update();
        Assert.Equal(new[] { "heavy" }, callOrder);
        Assert.False(light.IsDone);

        YooAsset.OperationSystem.Update();
        Assert.Contains("light", callOrder);
        Assert.True(light.IsDone);
    }

    /// <summary>
    /// 验证运行中提升优先级后可在后续帧触发重排并优先执行
    /// </summary>
    [Fact]
    public void Update_ShouldReorderQueueWhenPriorityChangesAtRuntime()
    {
        YooAsset.OperationSystem.MaxTimeSlice = 1;
        var callOrder = new List<string>();
        var blocker = new TimeSliceOperation("blocker", 2.0, callOrder);
        var low = new StepCompleteOperation(1, "low", callOrder) { Priority = 1 };
        var high = new StepCompleteOperation(1, "high", callOrder) { Priority = 0 };
        YooAsset.OperationSystem.StartOperation("pkg", blocker);
        YooAsset.OperationSystem.StartOperation("pkg", low);
        YooAsset.OperationSystem.StartOperation("pkg", high);

        YooAsset.OperationSystem.Update();
        high.Priority = 999;

        YooAsset.OperationSystem.Update();
        Assert.Contains("high", callOrder);
    }

    /// <summary>
    /// 验证绝对路径可映射为res虚拟路径
    /// </summary>
    [Fact]
    public void PathConverter_ShouldMapAbsolutePathToResScheme()
    {
        var absolutePath = "/workspace/game/streaming_assets/bundles/config.bytes";
        var virtualPath = YooAsset.PathUtility.ConvertToGodotVirtualPath(
            absolutePath,
            "/workspace/game/streaming_assets",
            "/workspace/game/user_data");

        Assert.Equal("res://bundles/config.bytes", virtualPath);
    }

    /// <summary>
    /// 验证绝对路径可映射为user虚拟路径
    /// </summary>
    [Fact]
    public void PathConverter_ShouldMapAbsolutePathToUserScheme()
    {
        var absolutePath = "/workspace/game/user_data/cache/manifest.hash";
        var virtualPath = YooAsset.PathUtility.ConvertToGodotVirtualPath(
            absolutePath,
            "/workspace/game/streaming_assets",
            "/workspace/game/user_data");

        Assert.Equal("user://cache/manifest.hash", virtualPath);
    }

    /// <summary>
    /// 验证虚拟路径可还原为绝对路径
    /// </summary>
    [Fact]
    public void PathConverter_ShouldMapVirtualPathToAbsolutePath()
    {
        var absolutePath = YooAsset.PathUtility.ConvertToAbsolutePath(
            "user://cache/manifest.hash",
            "/workspace/game/streaming_assets",
            "/workspace/game/user_data");

        Assert.Equal("/workspace/game/user_data/cache/manifest.hash", absolutePath);
    }

    [Fact]
    public void Update_ShouldCompleteE2EFlow_FromInitializeToLoad()
    {
        var trace = new List<string>();
        var context = new PipelineContext();

        var initialize = new PipelineStageOperation("initialize", () => true, () => context.Initialized = true, trace) { Priority = 500 };
        var requestVersion = new PipelineStageOperation("version", () => context.Initialized, () => context.VersionReady = true, trace) { Priority = 400 };
        var loadManifest = new PipelineStageOperation("manifest", () => context.VersionReady, () => context.ManifestReady = true, trace) { Priority = 300 };
        var downloadBundles = new PipelineStageOperation("download", () => context.ManifestReady, () => context.DownloadReady = true, trace) { Priority = 200 };
        var loadAssets = new PipelineStageOperation("load", () => context.DownloadReady, () => context.LoadReady = true, trace) { Priority = 100 };

        YooAsset.OperationSystem.StartOperation("pkg", initialize);
        YooAsset.OperationSystem.StartOperation("pkg", requestVersion);
        YooAsset.OperationSystem.StartOperation("pkg", loadManifest);
        YooAsset.OperationSystem.StartOperation("pkg", downloadBundles);
        YooAsset.OperationSystem.StartOperation("pkg", loadAssets);

        YooAsset.OperationSystem.Update();

        Assert.True(context.Initialized);
        Assert.True(context.VersionReady);
        Assert.True(context.ManifestReady);
        Assert.True(context.DownloadReady);
        Assert.True(context.LoadReady);
        Assert.Equal(new[] { "initialize", "version", "manifest", "download", "load" }, trace);
        Assert.Equal(YooAsset.EOperationStatus.Succeed, loadAssets.Status);
    }

    /// <summary>
    /// 验证网络失败时链路在版本阶段终止并阻断后续阶段
    /// </summary>
    [Fact]
    public void Update_ShouldStopFlow_WhenVersionRequestNetworkFailed()
    {
        var trace = new List<string>();
        var context = new PipelineContext();

        var initialize = new PipelineStageOperation("initialize", () => true, () => context.Initialized = true, trace) { Priority = 500 };
        var requestVersion = new FailingPipelineStageOperation("version", () => context.Initialized, "network failed", trace) { Priority = 400 };
        var loadManifest = new PipelineStageOperation("manifest", () => context.VersionReady, () => context.ManifestReady = true, trace) { Priority = 300 };
        var downloadBundles = new PipelineStageOperation("download", () => context.ManifestReady, () => context.DownloadReady = true, trace) { Priority = 200 };
        var loadAssets = new PipelineStageOperation("load", () => context.DownloadReady, () => context.LoadReady = true, trace) { Priority = 100 };

        YooAsset.OperationSystem.StartOperation("pkg", initialize);
        YooAsset.OperationSystem.StartOperation("pkg", requestVersion);
        YooAsset.OperationSystem.StartOperation("pkg", loadManifest);
        YooAsset.OperationSystem.StartOperation("pkg", downloadBundles);
        YooAsset.OperationSystem.StartOperation("pkg", loadAssets);

        YooAsset.OperationSystem.Update();

        Assert.True(context.Initialized);
        Assert.False(context.VersionReady);
        Assert.False(context.ManifestReady);
        Assert.False(context.DownloadReady);
        Assert.False(context.LoadReady);
        Assert.Equal(YooAsset.EOperationStatus.Failed, requestVersion.Status);
        Assert.Equal("network failed", requestVersion.Error);
        Assert.Equal(new[] { "initialize", "version:failed" }, trace);
    }

    /// <summary>
    /// 验证清单损坏时链路在清单阶段失败并保持已下载阶段未触发
    /// </summary>
    [Fact]
    public void Update_ShouldStopFlow_WhenManifestDamaged()
    {
        var trace = new List<string>();
        var context = new PipelineContext();

        var initialize = new PipelineStageOperation("initialize", () => true, () => context.Initialized = true, trace) { Priority = 500 };
        var requestVersion = new PipelineStageOperation("version", () => context.Initialized, () => context.VersionReady = true, trace) { Priority = 400 };
        var loadManifest = new FailingPipelineStageOperation("manifest", () => context.VersionReady, "manifest damaged", trace) { Priority = 300 };
        var downloadBundles = new PipelineStageOperation("download", () => context.ManifestReady, () => context.DownloadReady = true, trace) { Priority = 200 };
        var loadAssets = new PipelineStageOperation("load", () => context.DownloadReady, () => context.LoadReady = true, trace) { Priority = 100 };

        YooAsset.OperationSystem.StartOperation("pkg", initialize);
        YooAsset.OperationSystem.StartOperation("pkg", requestVersion);
        YooAsset.OperationSystem.StartOperation("pkg", loadManifest);
        YooAsset.OperationSystem.StartOperation("pkg", downloadBundles);
        YooAsset.OperationSystem.StartOperation("pkg", loadAssets);

        YooAsset.OperationSystem.Update();

        Assert.True(context.Initialized);
        Assert.True(context.VersionReady);
        Assert.False(context.ManifestReady);
        Assert.False(context.DownloadReady);
        Assert.False(context.LoadReady);
        Assert.Equal(YooAsset.EOperationStatus.Failed, loadManifest.Status);
        Assert.Equal("manifest damaged", loadManifest.Error);
        Assert.Equal(new[] { "initialize", "version", "manifest:failed" }, trace);
    }

    /// <summary>
    /// 验证下载中断后可重试恢复并继续后续加载阶段
    /// </summary>
    [Fact]
    public void Update_ShouldRecoverFlow_WhenDownloadInterruptedThenRetried()
    {
        var trace = new List<string>();
        var context = new PipelineContext();

        var initialize = new PipelineStageOperation("initialize", () => true, () => context.Initialized = true, trace) { Priority = 500 };
        var requestVersion = new PipelineStageOperation("version", () => context.Initialized, () => context.VersionReady = true, trace) { Priority = 400 };
        var loadManifest = new PipelineStageOperation("manifest", () => context.VersionReady, () => context.ManifestReady = true, trace) { Priority = 300 };
        var downloadBundles = new RetryablePipelineStageOperation("download", () => context.ManifestReady, () => context.DownloadReady = true, trace, 2) { Priority = 200 };
        var loadAssets = new PipelineStageOperation("load", () => context.DownloadReady, () => context.LoadReady = true, trace) { Priority = 100 };

        YooAsset.OperationSystem.StartOperation("pkg", initialize);
        YooAsset.OperationSystem.StartOperation("pkg", requestVersion);
        YooAsset.OperationSystem.StartOperation("pkg", loadManifest);
        YooAsset.OperationSystem.StartOperation("pkg", downloadBundles);
        YooAsset.OperationSystem.StartOperation("pkg", loadAssets);

        YooAsset.OperationSystem.Update();
        Assert.False(context.DownloadReady);
        Assert.False(context.LoadReady);

        YooAsset.OperationSystem.Update();
        Assert.False(context.DownloadReady);
        Assert.False(context.LoadReady);

        YooAsset.OperationSystem.Update();
        Assert.True(context.DownloadReady);
        Assert.True(context.LoadReady);
        Assert.Equal(
            new[] { "initialize", "version", "manifest", "download:retry", "download:retry", "download", "load" },
            trace);
    }

    /// <summary>
    /// 验证性能基线采集可生成稳定快照
    /// </summary>
    [Fact]
    public void PerformanceBaseline_ShouldCaptureStableSnapshot()
    {
        var collector = new PerformanceBaselineCollector();
        collector.RecordStartupMilliseconds(18.4);
        collector.RecordUpdateMilliseconds(2.0);
        collector.RecordUpdateMilliseconds(3.0);
        collector.RecordUpdateMilliseconds(4.0);
        collector.RecordDownloadSample(6 * 1024 * 1024L, 2.0);
        collector.RecordMemorySample(120 * 1024 * 1024L);
        collector.RecordMemorySample(160 * 1024 * 1024L);
        collector.RecordMemorySample(140 * 1024 * 1024L);

        var snapshot = collector.BuildSnapshot();

        Assert.Equal(18.4, snapshot.StartupMilliseconds, 3);
        Assert.Equal(3.0, snapshot.AverageUpdateMilliseconds, 3);
        Assert.Equal(3 * 1024 * 1024L, snapshot.DownloadThroughputBytesPerSecond);
        Assert.Equal(160 * 1024 * 1024L, snapshot.PeakMemoryBytes);
    }

    /// <summary>
    /// 验证性能预算校验可准确标记超限指标
    /// </summary>
    [Fact]
    public void PerformanceBaseline_ShouldValidateBudget()
    {
        var snapshot = new PerformanceBaselineSnapshot(
            startupMilliseconds: 22.5,
            averageUpdateMilliseconds: 4.5,
            downloadThroughputBytesPerSecond: 3 * 1024 * 1024L,
            peakMemoryBytes: 150 * 1024 * 1024L);

        var budget = new PerformanceBudget(
            maxStartupMilliseconds: 20.0,
            maxAverageUpdateMilliseconds: 5.0,
            minDownloadThroughputBytesPerSecond: 2 * 1024 * 1024L,
            maxPeakMemoryBytes: 140 * 1024 * 1024L);

        var violations = PerformanceBudgetEvaluator.Evaluate(snapshot, budget);

        Assert.Equal(2, violations.Count);
        Assert.Contains("startup", violations);
        Assert.Contains("memory", violations);
        Assert.DoesNotContain("update", violations);
        Assert.DoesNotContain("throughput", violations);
    }

    [Fact]
    public void StabilityBaseline_ShouldStayConsistent_AfterLongRunningUpdates()
    {
        var operations = new List<StepCompleteOperation>();
        var callOrder = new List<string>();
        for (var i = 0; i < 64; i++)
        {
            var operation = new StepCompleteOperation(6 + i % 3, $"long-{i}", callOrder);
            operations.Add(operation);
            YooAsset.OperationSystem.StartOperation("pkg-long", operation);
        }

        for (var frame = 0; frame < 24; frame++)
        {
            YooAsset.OperationSystem.Update();
        }

        foreach (var operation in operations)
        {
            Assert.True(operation.IsDone);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, operation.Status);
        }

        var stableCount = callOrder.Count;
        for (var frame = 0; frame < 120; frame++)
        {
            YooAsset.OperationSystem.Update();
        }

        Assert.Equal(stableCount, callOrder.Count);
    }

    [Fact]
    public void StabilityBaseline_ShouldKeepPassing_WhenFlowUpdatesRepeatedly()
    {
        var trace = new List<string>();
        for (var round = 0; round < 30; round++)
        {
            var context = new PipelineContext();
            var initialize = new PipelineStageOperation($"initialize-{round}", () => true, () => context.Initialized = true, trace) { Priority = 500 };
            var requestVersion = new PipelineStageOperation($"version-{round}", () => context.Initialized, () => context.VersionReady = true, trace) { Priority = 400 };
            var loadManifest = new PipelineStageOperation($"manifest-{round}", () => context.VersionReady, () => context.ManifestReady = true, trace) { Priority = 300 };
            var downloadBundles = new PipelineStageOperation($"download-{round}", () => context.ManifestReady, () => context.DownloadReady = true, trace) { Priority = 200 };
            var loadAssets = new PipelineStageOperation($"load-{round}", () => context.DownloadReady, () => context.LoadReady = true, trace) { Priority = 100 };

            YooAsset.OperationSystem.StartOperation("pkg-repeat", initialize);
            YooAsset.OperationSystem.StartOperation("pkg-repeat", requestVersion);
            YooAsset.OperationSystem.StartOperation("pkg-repeat", loadManifest);
            YooAsset.OperationSystem.StartOperation("pkg-repeat", downloadBundles);
            YooAsset.OperationSystem.StartOperation("pkg-repeat", loadAssets);
            YooAsset.OperationSystem.Update();

            Assert.True(context.LoadReady);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, loadAssets.Status);
        }

        Assert.Equal(30 * 5, trace.Count);
    }

    [Fact]
    public void StabilityBaseline_ShouldSwitchPackagesRepeatedly_WithoutDanglingOperations()
    {
        var slowOperations = new List<AbortAwareOperation>();
        string? previousPackageName = null;

        for (var round = 0; round < 12; round++)
        {
            var packageName = round % 2 == 0 ? "pkg-a" : "pkg-b";
            var slow = new AbortAwareOperation();
            slowOperations.Add(slow);
            YooAsset.OperationSystem.StartOperation(packageName, slow);

            if (previousPackageName != null)
            {
                YooAsset.OperationSystem.ClearPackageOperation(previousPackageName);
            }

            var quick = new StepCompleteOperation(1);
            YooAsset.OperationSystem.StartOperation(packageName, quick);
            YooAsset.OperationSystem.Update();

            Assert.True(quick.IsDone);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, quick.Status);
            previousPackageName = packageName;
        }

        Assert.NotNull(previousPackageName);
        YooAsset.OperationSystem.ClearPackageOperation(previousPackageName);
        YooAsset.OperationSystem.Update();

        foreach (var slow in slowOperations)
        {
            Assert.Equal(YooAsset.EOperationStatus.Failed, slow.Status);
            Assert.Equal("user abort", slow.Error);
            Assert.Equal(1, slow.AbortCount);
        }
    }

    private sealed class StepCompleteOperation : YooAsset.AsyncOperationBase
    {
        private readonly int _completeAfterCount;
        private readonly string? _name;
        private readonly List<string>? _callOrder;
        private int _updateCount;

        /// <summary>
        /// 构造按次数完成的测试操作
        /// </summary>
        public StepCompleteOperation(int completeAfterCount, string? name = null, List<string>? callOrder = null)
        {
            _completeAfterCount = completeAfterCount;
            _name = name;
            _callOrder = callOrder;
        }

        /// <summary>
        /// 获取当前更新次数
        /// </summary>
        public int UpdateCount => _updateCount;

        /// <summary>
        /// 启动操作
        /// </summary>
        public override void InternalOnStart()
        {
        }

        /// <summary>
        /// 推进操作状态
        /// </summary>
        public override void InternalOnUpdate()
        {
            _updateCount++;
            if (_name != null)
            {
                _callOrder?.Add(_name);
            }

            if (_updateCount >= _completeAfterCount)
            {
                Status = YooAsset.EOperationStatus.Succeed;
            }
        }
    }

    private sealed class TimeSliceOperation : YooAsset.AsyncOperationBase
    {
        private readonly string _name;
        private readonly double _busyMilliseconds;
        private readonly List<string> _callOrder;

        /// <summary>
        /// 构造时间片占用操作
        /// </summary>
        public TimeSliceOperation(string name, double busyMilliseconds, List<string> callOrder)
        {
            _name = name;
            _busyMilliseconds = busyMilliseconds;
            _callOrder = callOrder;
        }

        /// <summary>
        /// 启动操作
        /// </summary>
        public override void InternalOnStart()
        {
        }

        /// <summary>
        /// 执行一次耗时更新
        /// </summary>
        public override void InternalOnUpdate()
        {
            _callOrder.Add(_name);
            var watch = Stopwatch.StartNew();
            while (watch.Elapsed.TotalMilliseconds < _busyMilliseconds)
            {
            }
        }
    }

    private sealed class PipelineContext
    {
        public bool Initialized;
        public bool VersionReady;
        public bool ManifestReady;
        public bool DownloadReady;
        public bool LoadReady;
    }

    private sealed class PipelineStageOperation : YooAsset.AsyncOperationBase
    {
        private readonly string _name;
        private readonly System.Func<bool> _canRun;
        private readonly System.Action _onComplete;
        private readonly List<string> _trace;

        public PipelineStageOperation(string name, System.Func<bool> canRun, System.Action onComplete, List<string> trace)
        {
            _name = name;
            _canRun = canRun;
            _onComplete = onComplete;
            _trace = trace;
        }

        public override void InternalOnStart()
        {
        }

        public override void InternalOnUpdate()
        {
            if (_canRun() == false)
            {
                return;
            }

            _trace.Add(_name);
            _onComplete.Invoke();
            Status = YooAsset.EOperationStatus.Succeed;
        }
    }

    /// <summary>
    /// 表示执行即失败的链路阶段
    /// </summary>
    private sealed class FailingPipelineStageOperation : YooAsset.AsyncOperationBase
    {
        private readonly string _name;
        private readonly System.Func<bool> _canRun;
        private readonly string _error;
        private readonly List<string> _trace;

        /// <summary>
        /// 构造失败阶段操作
        /// </summary>
        public FailingPipelineStageOperation(string name, System.Func<bool> canRun, string error, List<string> trace)
        {
            _name = name;
            _canRun = canRun;
            _error = error;
            _trace = trace;
        }

        /// <summary>
        /// 启动失败阶段
        /// </summary>
        public override void InternalOnStart()
        {
        }

        /// <summary>
        /// 满足前置条件后立即失败
        /// </summary>
        public override void InternalOnUpdate()
        {
            if (_canRun() == false)
            {
                return;
            }

            _trace.Add($"{_name}:failed");
            Error = _error;
            Status = YooAsset.EOperationStatus.Failed;
        }
    }

    /// <summary>
    /// 表示可重试后成功的链路阶段
    /// </summary>
    private sealed class RetryablePipelineStageOperation : YooAsset.AsyncOperationBase
    {
        private readonly string _name;
        private readonly System.Func<bool> _canRun;
        private readonly System.Action _onComplete;
        private readonly List<string> _trace;
        private int _remainingRetryCount;

        /// <summary>
        /// 构造重试阶段操作
        /// </summary>
        public RetryablePipelineStageOperation(string name, System.Func<bool> canRun, System.Action onComplete, List<string> trace, int retryCountBeforeSuccess)
        {
            _name = name;
            _canRun = canRun;
            _onComplete = onComplete;
            _trace = trace;
            _remainingRetryCount = retryCountBeforeSuccess;
        }

        /// <summary>
        /// 启动重试阶段
        /// </summary>
        public override void InternalOnStart()
        {
        }

        /// <summary>
        /// 先模拟中断重试，耗尽后进入成功状态
        /// </summary>
        public override void InternalOnUpdate()
        {
            if (_canRun() == false)
            {
                return;
            }

            if (_remainingRetryCount > 0)
            {
                _trace.Add($"{_name}:retry");
                _remainingRetryCount--;
                return;
            }

            _trace.Add(_name);
            _onComplete.Invoke();
            Status = YooAsset.EOperationStatus.Succeed;
        }
    }

    /// <summary>
    /// 表示性能基线采集器
    /// </summary>
    private sealed class PerformanceBaselineCollector
    {
        private double _startupMilliseconds;
        private double _updateMillisecondsTotal;
        private int _updateSamples;
        private long _downloadBytesTotal;
        private double _downloadSecondsTotal;
        private long _peakMemoryBytes;

        /// <summary>
        /// 记录启动耗时
        /// </summary>
        public void RecordStartupMilliseconds(double milliseconds)
        {
            _startupMilliseconds = milliseconds;
        }

        /// <summary>
        /// 记录单帧更新耗时
        /// </summary>
        public void RecordUpdateMilliseconds(double milliseconds)
        {
            _updateMillisecondsTotal += milliseconds;
            _updateSamples++;
        }

        /// <summary>
        /// 记录下载样本用于吞吐计算
        /// </summary>
        public void RecordDownloadSample(long downloadedBytes, double elapsedSeconds)
        {
            _downloadBytesTotal += downloadedBytes;
            _downloadSecondsTotal += elapsedSeconds;
        }

        /// <summary>
        /// 记录内存样本并更新峰值
        /// </summary>
        public void RecordMemorySample(long currentBytes)
        {
            if (currentBytes > _peakMemoryBytes)
            {
                _peakMemoryBytes = currentBytes;
            }
        }

        /// <summary>
        /// 构建基线快照
        /// </summary>
        public PerformanceBaselineSnapshot BuildSnapshot()
        {
            var averageUpdateMilliseconds = _updateSamples > 0 ? _updateMillisecondsTotal / _updateSamples : 0d;
            var throughput = _downloadSecondsTotal > 0d ? (long)(_downloadBytesTotal / _downloadSecondsTotal) : 0L;
            return new PerformanceBaselineSnapshot(_startupMilliseconds, averageUpdateMilliseconds, throughput, _peakMemoryBytes);
        }
    }

    /// <summary>
    /// 表示性能基线快照
    /// </summary>
    private sealed class PerformanceBaselineSnapshot
    {
        /// <summary>
        /// 构造性能基线快照
        /// </summary>
        public PerformanceBaselineSnapshot(double startupMilliseconds, double averageUpdateMilliseconds, long downloadThroughputBytesPerSecond, long peakMemoryBytes)
        {
            StartupMilliseconds = startupMilliseconds;
            AverageUpdateMilliseconds = averageUpdateMilliseconds;
            DownloadThroughputBytesPerSecond = downloadThroughputBytesPerSecond;
            PeakMemoryBytes = peakMemoryBytes;
        }

        public double StartupMilliseconds { get; }
        public double AverageUpdateMilliseconds { get; }
        public long DownloadThroughputBytesPerSecond { get; }
        public long PeakMemoryBytes { get; }
    }

    /// <summary>
    /// 表示性能预算阈值
    /// </summary>
    private sealed class PerformanceBudget
    {
        /// <summary>
        /// 构造性能预算阈值
        /// </summary>
        public PerformanceBudget(double maxStartupMilliseconds, double maxAverageUpdateMilliseconds, long minDownloadThroughputBytesPerSecond, long maxPeakMemoryBytes)
        {
            MaxStartupMilliseconds = maxStartupMilliseconds;
            MaxAverageUpdateMilliseconds = maxAverageUpdateMilliseconds;
            MinDownloadThroughputBytesPerSecond = minDownloadThroughputBytesPerSecond;
            MaxPeakMemoryBytes = maxPeakMemoryBytes;
        }

        public double MaxStartupMilliseconds { get; }
        public double MaxAverageUpdateMilliseconds { get; }
        public long MinDownloadThroughputBytesPerSecond { get; }
        public long MaxPeakMemoryBytes { get; }
    }

    /// <summary>
    /// 表示性能预算评估器
    /// </summary>
    private static class PerformanceBudgetEvaluator
    {
        /// <summary>
        /// 评估快照是否满足预算
        /// </summary>
        public static List<string> Evaluate(PerformanceBaselineSnapshot snapshot, PerformanceBudget budget)
        {
            var violations = new List<string>();
            if (snapshot.StartupMilliseconds > budget.MaxStartupMilliseconds)
            {
                violations.Add("startup");
            }

            if (snapshot.AverageUpdateMilliseconds > budget.MaxAverageUpdateMilliseconds)
            {
                violations.Add("update");
            }

            if (snapshot.DownloadThroughputBytesPerSecond < budget.MinDownloadThroughputBytesPerSecond)
            {
                violations.Add("throughput");
            }

            if (snapshot.PeakMemoryBytes > budget.MaxPeakMemoryBytes)
            {
                violations.Add("memory");
            }

            return violations;
        }
    }

    private sealed class AbortAwareOperation : YooAsset.AsyncOperationBase
    {
        public int AbortCount { get; private set; }

        public override void InternalOnStart()
        {
        }

        public override void InternalOnUpdate()
        {
        }

        internal override void InternalOnAbort()
        {
            AbortCount++;
        }
    }
}
