using System;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Download.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Scene.Runtime;
using GameFrameX.UI.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Asset/Download/Scene/UI/ReferencePool/Procedure 各模块 Options 的注册-消费往返用例（纯 C#，无引擎调用）。
    /// </summary>
    public sealed class ModuleOptionsRoundTripTests : IDisposable
    {
        public void Dispose()
        {
            GameFrameXOptions.Reset(); // 清除 Options 注册，避免跨用例静态泄漏
        }

        [Fact]
        public void AssetOptions_ConfigureAndTryGet_RoundTripsDiffs()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new AssetOptions { GamePlayMode = EPlayMode.HostPlayMode });

            Assert.True(GameFrameXOptions.TryGet<AssetOptions>(out var options));
            Assert.Equal(EPlayMode.HostPlayMode, options.GamePlayMode);

            GameFrameXOptions.Reset();
            var defaults = new AssetOptions();
            Assert.Equal(EPlayMode.EditorSimulateMode, defaults.GamePlayMode);
        }

        [Fact]
        public void DownloadOptions_ConfigureAndTryGet_RoundTripsDiffs()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new DownloadOptions
            {
                AgentHelperCount = 5,
                Timeout = 60f,
                FlushSize = 2 * DownloadOptions.OneMegaBytes,
                DownloadAgentHelperTypeName = "GameFrameX.Download.Runtime.CustomHelper",
            });

            Assert.True(GameFrameXOptions.TryGet<DownloadOptions>(out var options));
            Assert.Equal(5, options.AgentHelperCount);
            Assert.Equal(60f, options.Timeout);
            Assert.Equal(2 * DownloadOptions.OneMegaBytes, options.FlushSize);
            Assert.Equal("GameFrameX.Download.Runtime.CustomHelper", options.DownloadAgentHelperTypeName);

            GameFrameXOptions.Reset();
            var defaults = new DownloadOptions();
            Assert.Equal(3, defaults.AgentHelperCount);
            Assert.Equal(30f, defaults.Timeout);
            Assert.Equal(DownloadOptions.OneMegaBytes, defaults.FlushSize);
            Assert.Equal("GameFrameX.Download.Runtime.GodotWebRequestDownloadAgentHelper", defaults.DownloadAgentHelperTypeName);
        }

        [Fact]
        public void SceneOptions_ConfigureAndTryGet_RoundTripsDiffs()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new SceneOptions { EnableLoadSceneUpdateEvent = false, EnableLoadSceneDependencyAssetEvent = false });

            Assert.True(GameFrameXOptions.TryGet<SceneOptions>(out var options));
            Assert.False(options.EnableLoadSceneUpdateEvent);
            Assert.False(options.EnableLoadSceneDependencyAssetEvent);

            GameFrameXOptions.Reset();
            var defaults = new SceneOptions();
            Assert.True(defaults.EnableLoadSceneUpdateEvent);
        }

        [Fact]
        public void UIOptions_ConfigureAndTryGet_RoundTripsDiffs()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new UIOptions
            {
                EnableOpenUIFormSuccessEvent = false,
                EnableOpenUIFormFailureEvent = false,
                EnableOpenUIFormUpdateEvent = true,
                EnableOpenUIFormDependencyAssetEvent = true,
                EnableAutoReleaseUIForm = false,
                EnableCloseUIFormCompleteEvent = false,
                IsEnableUIShowAnimation = true,
                IsEnableUIHideAnimation = true,
                InstanceAutoReleaseInterval = 30f,
                InstanceCapacity = 32,
                InstanceExpireTime = 1200f,
                InstancePriority = 1,
                UIFormHelperFactory = () => null,
                UIGroupHelperFactory = () => null,
            });

            Assert.True(GameFrameXOptions.TryGet<UIOptions>(out var options));
            Assert.False(options.EnableOpenUIFormSuccessEvent);
            Assert.False(options.EnableOpenUIFormFailureEvent);
            Assert.True(options.EnableOpenUIFormUpdateEvent);
            Assert.True(options.EnableOpenUIFormDependencyAssetEvent);
            Assert.False(options.EnableAutoReleaseUIForm);
            Assert.False(options.EnableCloseUIFormCompleteEvent);
            Assert.True(options.IsEnableUIShowAnimation);
            Assert.True(options.IsEnableUIHideAnimation);
            Assert.Equal(30f, options.InstanceAutoReleaseInterval);
            Assert.Equal(32, options.InstanceCapacity);
            Assert.Equal(1200f, options.InstanceExpireTime);
            Assert.Equal(1, options.InstancePriority);
            Assert.NotNull(options.UIFormHelperFactory);
            Assert.NotNull(options.UIGroupHelperFactory);

            GameFrameXOptions.Reset();
            var defaults = new UIOptions();
            Assert.True(defaults.EnableOpenUIFormSuccessEvent);
            Assert.True(defaults.EnableOpenUIFormFailureEvent);
            Assert.False(defaults.EnableOpenUIFormUpdateEvent);
            Assert.False(defaults.EnableOpenUIFormDependencyAssetEvent);
            Assert.True(defaults.EnableAutoReleaseUIForm);
            Assert.True(defaults.EnableCloseUIFormCompleteEvent);
            Assert.False(defaults.IsEnableUIShowAnimation);
            Assert.False(defaults.IsEnableUIHideAnimation);
            Assert.Equal(60f, defaults.InstanceAutoReleaseInterval);
            Assert.Equal(16, defaults.InstanceCapacity);
            Assert.Equal(60f, defaults.InstanceExpireTime);
            Assert.Equal(0, defaults.InstancePriority);
            Assert.Null(defaults.UIFormHelperFactory);
            Assert.Null(defaults.UIGroupHelperFactory);
        }

        [Fact]
        public void ReferencePoolOptions_ConfigureAndTryGet_RoundTripsDiffs()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new ReferencePoolOptions { EnableStrictCheck = ReferenceStrictCheckType.OnlyEnableWhenDevelopment });

            Assert.True(GameFrameXOptions.TryGet<ReferencePoolOptions>(out var options));
            Assert.Equal(ReferenceStrictCheckType.OnlyEnableWhenDevelopment, options.EnableStrictCheck);

            GameFrameXOptions.Reset();
            var defaults = new ReferencePoolOptions();
            Assert.Equal(ReferenceStrictCheckType.AlwaysEnable, defaults.EnableStrictCheck);
        }

        [Fact]
        public void ProcedureOptions_ConfigureAndTryGet_RoundTripsDiffs()
        {
            GameFrameXOptions.Reset();
            GameFrameXOptions.Configure(new ProcedureOptions
            {
                EntranceProcedureTypeName = "GameFrameX.UnitTests.ProcedureMain",
                UseStartupRunner = true,
            });

            Assert.True(GameFrameXOptions.TryGet<ProcedureOptions>(out var options));
            Assert.Equal("GameFrameX.UnitTests.ProcedureMain", options.EntranceProcedureTypeName);
            Assert.True(options.UseStartupRunner);

            GameFrameXOptions.Reset();
            var defaults = new ProcedureOptions();
            Assert.Null(defaults.EntranceProcedureTypeName);
            Assert.False(defaults.UseStartupRunner);
        }
    }
}
