using System;
using GameFrameX.Runtime;
using GameFrameX.Sound.Runtime;
using Godot;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 声音模块纯逻辑单元测试（迁移自 Unity com.gameframex.unity.sound Tests/UnitTests.cs，NUnit → xunit，18 用例全平移）。
    /// 覆盖 PlaySoundErrorCode / PlaySoundParams / SoundPlayContext / 4 个 EventArgs。
    /// Godot 运行时依赖的部分（SoundComponent、DefaultSoundAgentHelper 等节点辅助器）归引擎测试，
    /// 不在本文件覆盖；SoundManager/SoundGroup 代理选择算法见 SoundGroupTests.cs。
    /// </summary>
    public sealed class SoundPlayParamsTests
    {
        // ──────────────── PlaySoundErrorCode ────────────────

        [Fact]
        public void PlaySoundErrorCode_HasAllExpectedValues()
        {
            Assert.True(Enum.IsDefined(typeof(PlaySoundErrorCode), PlaySoundErrorCode.Unknown));
            Assert.True(Enum.IsDefined(typeof(PlaySoundErrorCode), PlaySoundErrorCode.SoundGroupNotExist));
            Assert.True(Enum.IsDefined(typeof(PlaySoundErrorCode), PlaySoundErrorCode.SoundGroupHasNoAgent));
            Assert.True(Enum.IsDefined(typeof(PlaySoundErrorCode), PlaySoundErrorCode.LoadAssetFailure));
            Assert.True(Enum.IsDefined(typeof(PlaySoundErrorCode), PlaySoundErrorCode.IgnoredDueToLowPriority));
            Assert.True(Enum.IsDefined(typeof(PlaySoundErrorCode), PlaySoundErrorCode.SetSoundAssetFailure));
        }

        [Fact]
        public void PlaySoundErrorCode_UnknownIsZero()
        {
            Assert.Equal(0, (int)PlaySoundErrorCode.Unknown);
        }

        // ──────────────── PlaySoundParams ────────────────

        [Fact]
        public void PlaySoundParams_Create_ReturnsNonNull()
        {
            PlaySoundParams sut = PlaySoundParams.Create();
            Assert.NotNull(sut);
            ReferencePool.Release(sut);
        }

        [Fact]
        public void PlaySoundParams_Create_DefaultsMatchConstants()
        {
            PlaySoundParams sut = PlaySoundParams.Create();
            Assert.Equal(0f, sut.Time);
            Assert.False(sut.MuteInSoundGroup);
            Assert.False(sut.Loop);
            Assert.Equal(0, sut.Priority);
            Assert.Equal(1f, sut.VolumeInSoundGroup);
            Assert.Equal(0f, sut.FadeInSeconds);
            Assert.Equal(1f, sut.Pitch);
            Assert.Equal(0f, sut.PanStereo);
            Assert.Equal(0f, sut.SpatialBlend);
            Assert.Equal(100f, sut.MaxDistance);
            Assert.Equal(1f, sut.DopplerLevel);
            ReferencePool.Release(sut);
        }

        [Fact]
        public void PlaySoundParams_CreateWithLoop_SetsLoop()
        {
            PlaySoundParams sut = PlaySoundParams.Create(true);
            Assert.True(sut.Loop);
            ReferencePool.Release(sut);
        }

        [Fact]
        public void PlaySoundParams_Properties_SetAndGet()
        {
            PlaySoundParams sut = PlaySoundParams.Create();

            sut.Time = 1.5f;
            Assert.Equal(1.5f, sut.Time);

            sut.MuteInSoundGroup = true;
            Assert.True(sut.MuteInSoundGroup);

            sut.Loop = true;
            Assert.True(sut.Loop);

            sut.Priority = 128;
            Assert.Equal(128, sut.Priority);

            sut.VolumeInSoundGroup = 0.5f;
            Assert.Equal(0.5f, sut.VolumeInSoundGroup);

            sut.FadeInSeconds = 0.3f;
            Assert.Equal(0.3f, sut.FadeInSeconds);

            sut.Pitch = 2f;
            Assert.Equal(2f, sut.Pitch);

            sut.PanStereo = 0.5f;
            Assert.Equal(0.5f, sut.PanStereo);

            sut.SpatialBlend = 1f;
            Assert.Equal(1f, sut.SpatialBlend);

            sut.MaxDistance = 500f;
            Assert.Equal(500f, sut.MaxDistance);

            sut.DopplerLevel = 0f;
            Assert.Equal(0f, sut.DopplerLevel);

            ReferencePool.Release(sut);
        }

        [Fact]
        public void PlaySoundParams_Clear_ResetsDefaults()
        {
            PlaySoundParams sut = PlaySoundParams.Create();

            sut.Time = 1.5f;
            sut.Priority = 128;
            sut.VolumeInSoundGroup = 0.5f;

            sut.Clear();

            Assert.Equal(0f, sut.Time);
            Assert.Equal(0, sut.Priority);
            Assert.Equal(1f, sut.VolumeInSoundGroup);

            ReferencePool.Release(sut);
        }

        [Fact]
        public void PlaySoundParams_ReferencePoolReusesInstance()
        {
            PlaySoundParams first = PlaySoundParams.Create();
            first.Priority = 99;
            ReferencePool.Release(first);

            PlaySoundParams second = PlaySoundParams.Create();
            Assert.Equal(0, second.Priority); // Release 时 Clear 已重置
            Assert.Same(first, second); // ReferencePool 应复用同一实例
            ReferencePool.Release(second);
        }

        // ──────────────── SoundPlayContext ────────────────

        [Fact]
        public void SoundPlayContext_Create_SetsProperties()
        {
            object userData = "test";
            SoundPlayContext sut = SoundPlayContext.Create(null, Vector3.One, userData);

            Assert.Null(sut.BindingEntity);
            Assert.Equal(Vector3.One, sut.WorldPosition);
            Assert.Equal("test", sut.UserData);

            ReferencePool.Release(sut);
        }

        [Fact]
        public void SoundPlayContext_Clear_ResetsProperties()
        {
            object userData = "test";
            SoundPlayContext sut = SoundPlayContext.Create(null, Vector3.One, userData);

            sut.Clear();

            Assert.Null(sut.BindingEntity);
            Assert.Equal(Vector3.Zero, sut.WorldPosition);
            Assert.Null(sut.UserData);

            ReferencePool.Release(sut);
        }

        // ──────────────── PlaySoundFailureEventArgs ────────────────

        [Fact]
        public void PlaySoundFailureEventArgs_Create_SetsProperties()
        {
            PlaySoundParams soundParams = PlaySoundParams.Create();
            object userData = "test";

            PlaySoundFailureEventArgs sut = PlaySoundFailureEventArgs.Create(
                1, "test_asset", "test_group", soundParams,
                PlaySoundErrorCode.SoundGroupNotExist, "Group not found", userData);

            Assert.Equal(1, sut.SerialId);
            Assert.Equal("test_asset", sut.SoundAssetName);
            Assert.Equal("test_group", sut.SoundGroupName);
            Assert.Same(soundParams, sut.PlaySoundParams);
            Assert.Equal(PlaySoundErrorCode.SoundGroupNotExist, sut.ErrorCode);
            Assert.Equal("Group not found", sut.ErrorMessage);
            Assert.Equal("test", sut.UserData);
            Assert.Equal(typeof(PlaySoundFailureEventArgs).FullName, sut.Id);

            ReferencePool.Release(sut);
            ReferencePool.Release(soundParams);
        }

        [Fact]
        public void PlaySoundFailureEventArgs_Clear_ResetsProperties()
        {
            PlaySoundParams soundParams = PlaySoundParams.Create();
            PlaySoundFailureEventArgs sut = PlaySoundFailureEventArgs.Create(
                1, "test", "group", soundParams,
                PlaySoundErrorCode.Unknown, "error", null);

            sut.Clear();

            Assert.Equal(0, sut.SerialId);
            Assert.Null(sut.SoundAssetName);
            Assert.Null(sut.SoundGroupName);
            Assert.Null(sut.PlaySoundParams);
            Assert.Equal(PlaySoundErrorCode.Unknown, sut.ErrorCode);
            Assert.Null(sut.ErrorMessage);
            Assert.Null(sut.UserData);

            ReferencePool.Release(soundParams);
            ReferencePool.Release(sut);
        }

        // ──────────────── PlaySoundSuccessEventArgs ────────────────

        [Fact]
        public void PlaySoundSuccessEventArgs_Create_SetsProperties()
        {
            PlaySoundSuccessEventArgs sut = PlaySoundSuccessEventArgs.Create(
                42, "success_asset.wav", null, 1.5f, "user");

            Assert.Equal(42, sut.SerialId);
            Assert.Equal("success_asset.wav", sut.SoundAssetName);
            Assert.Null(sut.SoundAgent);
            Assert.Equal(1.5f, sut.Duration);
            Assert.Equal("user", sut.UserData);
            Assert.Equal(typeof(PlaySoundSuccessEventArgs).FullName, sut.Id);

            ReferencePool.Release(sut);
        }

        [Fact]
        public void PlaySoundSuccessEventArgs_Clear_ResetsProperties()
        {
            PlaySoundSuccessEventArgs sut = PlaySoundSuccessEventArgs.Create(
                42, "asset", null, 1.5f, "user");

            sut.Clear();

            Assert.Equal(0, sut.SerialId);
            Assert.Null(sut.SoundAssetName);
            Assert.Null(sut.SoundAgent);
            Assert.Equal(0f, sut.Duration);
            Assert.Null(sut.UserData);

            ReferencePool.Release(sut);
        }

        // ──────────────── PlaySoundUpdateEventArgs ────────────────

        [Fact]
        public void PlaySoundUpdateEventArgs_Create_SetsProperties()
        {
            PlaySoundParams soundParams = PlaySoundParams.Create();
            PlaySoundUpdateEventArgs sut = PlaySoundUpdateEventArgs.Create(
                3, "loading_asset", "bgm", soundParams, 0.75f, "ctx");

            Assert.Equal(3, sut.SerialId);
            Assert.Equal("loading_asset", sut.SoundAssetName);
            Assert.Equal("bgm", sut.SoundGroupName);
            Assert.Same(soundParams, sut.PlaySoundParams);
            Assert.Equal(0.75f, sut.Progress);
            Assert.Equal("ctx", sut.UserData);
            Assert.Equal(typeof(PlaySoundUpdateEventArgs).FullName, sut.Id);

            ReferencePool.Release(sut);
            ReferencePool.Release(soundParams);
        }

        [Fact]
        public void PlaySoundUpdateEventArgs_Clear_ResetsProperties()
        {
            PlaySoundParams soundParams = PlaySoundParams.Create();
            PlaySoundUpdateEventArgs sut = PlaySoundUpdateEventArgs.Create(
                3, "asset", "group", soundParams, 0.75f, "ctx");

            sut.Clear();

            Assert.Equal(0, sut.SerialId);
            Assert.Null(sut.SoundAssetName);
            Assert.Null(sut.SoundGroupName);
            Assert.Null(sut.PlaySoundParams);
            Assert.Equal(0f, sut.Progress);
            Assert.Null(sut.UserData);

            ReferencePool.Release(soundParams);
            ReferencePool.Release(sut);
        }

        // ──────────────── ResetSoundAgentEventArgs ────────────────

        [Fact]
        public void ResetSoundAgentEventArgs_Create_ReturnsNonNull()
        {
            ResetSoundAgentEventArgs sut = ResetSoundAgentEventArgs.Create();
            Assert.NotNull(sut);
            Assert.Equal(typeof(ResetSoundAgentEventArgs).FullName, sut.Id);
            ReferencePool.Release(sut);
        }

        [Fact]
        public void ResetSoundAgentEventArgs_Clear_DoesNotThrow()
        {
            ResetSoundAgentEventArgs sut = ResetSoundAgentEventArgs.Create();
            // xunit 无 DoesNotThrow，直接调用即可：抛异常则用例失败
            sut.Clear();
            ReferencePool.Release(sut);
        }
    }
}
