using System;
using System.Collections.Generic;
using System.Threading;
using GameFrameX.Runtime;
using GameFrameX.Sound.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 声音组与代理选择算法测试（Unity 侧 Tests 未覆盖此逻辑，此处自建）。
    /// SoundManager.cs 已通过 InternalsVisibleTo("GameFrameX.UnitTests") 暴露 SoundGroup/SoundAgent 内部类型；
    /// ISoundHelper/ISoundGroupHelper/ISoundAgentHelper 均为纯 C# 接口，用嵌套 Fake 驱动，全程无引擎调用。
    /// 引擎耦合说明：SoundManager.PlaySound 公共入口依赖 IAssetManager.LoadAssetAsync（资源系统 + AudioStream）
    /// 与 m_Serial 序列号自增，归引擎/资源系统测试；此处只测 SoundGroup.PlaySound 的纯代理选择逻辑。
    /// </summary>
    public sealed class SoundGroupTests
    {
        private sealed class FakeSoundGroupHelper : ISoundGroupHelper
        {
        }

        private sealed class FakeSoundHelper : ISoundHelper
        {
            public List<object> ReleasedAssets { get; } = new List<object>();

            public void ReleaseSoundAsset(object soundAsset)
            {
                ReleasedAssets.Add(soundAsset);
            }
        }

        /// <summary>
        /// 声音代理辅助器 Fake：IsPlaying/属性由测试直接控制，Play/Stop 模拟播放状态切换，
        /// CurrentAsset/LastFadeInSeconds 记录透传结果。
        /// </summary>
        private sealed class FakeSoundAgentHelper : ISoundAgentHelper
        {
            public bool IsPlaying { get; set; }

            public float Length { get; set; }

            public float Time { get; set; }

            public bool Mute { get; set; }

            public bool Loop { get; set; }

            public int Priority { get; set; }

            public float Volume { get; set; }

            public float Pitch { get; set; }

            public float PanStereo { get; set; }

            public float SpatialBlend { get; set; }

            public float MaxDistance { get; set; }

            public float DopplerLevel { get; set; }

            /// <summary>SetSoundAsset 的返回值开关，用于模拟设置资源失败。</summary>
            public bool SetSoundAssetResult { get; set; }

            public object CurrentAsset { get; private set; }

            public float LastFadeInSeconds { get; private set; }

            public event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent
            {
                add { }
                remove { }
            }

            public void Play(float fadeInSeconds)
            {
                LastFadeInSeconds = fadeInSeconds;
                IsPlaying = true;
            }

            public void Stop(float fadeOutSeconds)
            {
                IsPlaying = false;
            }

            public void Pause(float fadeOutSeconds)
            {
            }

            public void Resume(float fadeInSeconds)
            {
            }

            public void Reset()
            {
            }

            public bool SetSoundAsset(object soundAsset)
            {
                CurrentAsset = soundAsset;
                return SetSoundAssetResult;
            }
        }

        // ──────────────── 组构造与管理（SoundManager 公共 API） ────────────────

        [Fact]
        public void SoundGroup_Constructor_InvalidArgs_Throw()
        {
            Assert.Throws<GameFrameworkException>(() => new SoundManager.SoundGroup(null, new FakeSoundGroupHelper()));
            Assert.Throws<GameFrameworkException>(() => new SoundManager.SoundGroup(string.Empty, new FakeSoundGroupHelper()));
            Assert.Throws<GameFrameworkException>(() => new SoundManager.SoundGroup("bgm", null));
        }

        [Fact]
        public void SoundManager_AddSoundGroup_RegistersAndDeduplicates()
        {
            var manager = new SoundManager();
            var groupHelper = new FakeSoundGroupHelper();

            Assert.Equal(0, manager.SoundGroupCount);
            Assert.True(manager.AddSoundGroup("bgm", groupHelper));

            // 同名重复添加返回 false
            Assert.False(manager.AddSoundGroup("bgm", groupHelper));

            Assert.True(manager.HasSoundGroup("bgm"));
            Assert.False(manager.HasSoundGroup("sfx"));
            Assert.Equal(1, manager.SoundGroupCount);

            ISoundGroup group = manager.GetSoundGroup("bgm");
            Assert.NotNull(group);
            Assert.Equal("bgm", group.Name);
            Assert.Null(manager.GetSoundGroup("sfx"));

            ISoundGroup[] allGroups = manager.GetAllSoundGroups();
            Assert.Single(allGroups);
            Assert.Equal("bgm", allGroups[0].Name);

            var results = new List<ISoundGroup>();
            manager.GetAllSoundGroups(results);
            Assert.Single(results);

            // 无效参数抛异常
            Assert.Throws<GameFrameworkException>(() => manager.AddSoundGroup(null, groupHelper));
            Assert.Throws<GameFrameworkException>(() => manager.AddSoundGroup("bgm2", null));
        }

        [Fact]
        public void SoundManager_AddSoundGroup_WithOptions_AppliesSettings()
        {
            var manager = new SoundManager();

            Assert.True(manager.AddSoundGroup("bgm", true, true, 0.5f, new FakeSoundGroupHelper()));

            ISoundGroup group = manager.GetSoundGroup("bgm");
            Assert.True(group.AvoidBeingReplacedBySamePriority);
            Assert.True(group.Mute);
            Assert.Equal(0.5f, group.Volume);
        }

        [Fact]
        public void SoundManager_AddSoundAgentHelper_RequiresSoundHelperAndKnownGroup()
        {
            var manager = new SoundManager();
            manager.AddSoundGroup("bgm", new FakeSoundGroupHelper());
            var agentHelper = new FakeSoundAgentHelper();

            // 未先 SetSoundHelper
            Assert.Throws<GameFrameworkException>(() => manager.AddSoundAgentHelper("bgm", agentHelper));

            manager.SetSoundHelper(new FakeSoundHelper());

            // 不存在的组
            Assert.Throws<GameFrameworkException>(() => manager.AddSoundAgentHelper("unknown", agentHelper));

            manager.AddSoundAgentHelper("bgm", agentHelper);
            Assert.Equal(1, manager.GetSoundGroup("bgm").SoundAgentCount);
        }

        // ──────────────── 代理选择算法 ────────────────

        [Fact]
        public void PlaySound_SelectsFirstIdleAgent()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 2);

            ISoundAgent agent = Play(group, 1, new object(), 0);

            Assert.NotNull(agent);
            Assert.Equal(1, agent.SerialId);
            Assert.NotNull(helpers[0].CurrentAsset); // 第一个空闲代理被占用
            Assert.Null(helpers[1].CurrentAsset); // 第二个未被触碰
            Assert.Empty(soundHelper.ReleasedAssets);
        }

        [Fact]
        public void PlaySound_PrefersIdleAgentOverPreemption()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 2);

            ISoundAgent busyAgent = Play(group, 1, new object(), 0); // agent1 占用，priority 0
            object secondAsset = new object();

            // agent1 忙且 priority 0 远低于请求值 100，但 agent2 空闲 → 应选 agent2 而非抢占 agent1
            ISoundAgent agent = Play(group, 2, secondAsset, 100);

            Assert.NotSame(busyAgent, agent);
            Assert.Same(helpers[1], agent.Helper);
            Assert.DoesNotContain(soundHelper.ReleasedAssets, asset => ReferenceEquals(asset, helpers[0].CurrentAsset));
            Assert.Equal(1, busyAgent.SerialId); // 未被顶替，序列号不变
        }

        [Fact]
        public void PlaySound_WithoutAgents_ReturnsIgnoredDueToLowPriority()
        {
            var group = new SoundManager.SoundGroup("empty", new FakeSoundGroupHelper());
            PlaySoundParams playSoundParams = PlaySoundParams.Create();

            ISoundAgent agent = group.PlaySound(1, new object(), playSoundParams, out PlaySoundErrorCode? errorCode);

            Assert.Null(agent);
            Assert.Equal(PlaySoundErrorCode.IgnoredDueToLowPriority, errorCode);
            ReferencePool.Release(playSoundParams);
        }

        [Fact]
        public void PlaySound_AllBusyLowerPriority_ReturnsIgnoredDueToLowPriority()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 1);

            ISoundAgent busyAgent = Play(group, 1, new object(), 10); // 唯一代理占用，priority 10

            // 请求 priority 5 < 10，不可抢占
            ISoundAgent agent = Play(group, 2, new object(), 5);

            Assert.Null(agent);
            Assert.Equal(1, busyAgent.SerialId); // 原代理未被顶替
            Assert.True(helpers[0].IsPlaying);
            Assert.Empty(soundHelper.ReleasedAssets);
        }

        [Fact]
        public void PlaySound_HigherPriority_PreemptsLowestPriorityAgent()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 3);

            // 依次占用三个空闲代理，优先级分别为 5 / 3 / 8
            object asset1 = new object();
            object asset2 = new object();
            object asset3 = new object();
            Play(group, 1, asset1, 5);
            Play(group, 2, asset2, 3);
            Play(group, 3, asset3, 8);

            // 全忙，请求 priority 10 → 应抢占优先级最低（3）的第二个代理
            object newAsset = new object();
            ISoundAgent agent = Play(group, 4, newAsset, 10);

            Assert.NotNull(agent);
            Assert.Same(helpers[1], agent.Helper);
            Assert.Equal(4, agent.SerialId);
            Assert.Same(newAsset, helpers[1].CurrentAsset);
            Assert.Same(asset1, helpers[0].CurrentAsset); // 其余代理不受影响
            Assert.Same(asset3, helpers[2].CurrentAsset);
            Assert.Equal(10, helpers[1].Priority); // 新优先级写入代理
            // 被顶替代理的旧资源被释放
            Assert.Single(soundHelper.ReleasedAssets);
            Assert.Same(asset2, soundHelper.ReleasedAssets[0]);
        }

        [Fact]
        public void PlaySound_SamePriority_ReplacesOldestAgent()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 2);

            object asset1 = new object();
            object asset2 = new object();
            Play(group, 1, asset1, 5);
            Thread.Sleep(20); // 保证 SetSoundAssetTime 递增，使「最旧」可判定
            Play(group, 2, asset2, 5);
            Thread.Sleep(20);

            // 全忙且同优先级（Avoid=false）→ 顶替 SetSoundAssetTime 最旧的第一个代理
            object newAsset = new object();
            ISoundAgent agent = Play(group, 3, newAsset, 5);

            Assert.NotNull(agent);
            Assert.Same(helpers[0], agent.Helper);
            Assert.Same(newAsset, helpers[0].CurrentAsset);
            Assert.Same(asset2, helpers[1].CurrentAsset); // 较新的代理未被顶替
            Assert.Contains(asset1, soundHelper.ReleasedAssets); // 最旧代理的旧资源被释放
        }

        [Fact]
        public void PlaySound_AvoidBeingReplacedBySamePriority_BlocksSamePriority()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 2);
            group.AvoidBeingReplacedBySamePriority = true;

            object asset1 = new object();
            object asset2 = new object();
            Play(group, 1, asset1, 5);
            Thread.Sleep(20);
            Play(group, 2, asset2, 5);
            Thread.Sleep(20);

            // 全忙且同优先级，Avoid 开启 → 拒绝顶替
            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = 5;
            ISoundAgent agent = group.PlaySound(3, new object(), playSoundParams, out PlaySoundErrorCode? errorCode);

            Assert.Null(agent);
            Assert.Equal(PlaySoundErrorCode.IgnoredDueToLowPriority, errorCode);
            Assert.Same(asset1, helpers[0].CurrentAsset);
            Assert.Same(asset2, helpers[1].CurrentAsset);
            Assert.Empty(soundHelper.ReleasedAssets);
            ReferencePool.Release(playSoundParams);
        }

        [Fact]
        public void PlaySound_AvoidBeingReplacedBySamePriority_HigherPriorityStillPreempts()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 1);
            group.AvoidBeingReplacedBySamePriority = true;

            Play(group, 1, new object(), 5);

            // Avoid 只挡同优先级，更高优先级仍可抢占
            object newAsset = new object();
            ISoundAgent agent = Play(group, 2, newAsset, 10);

            Assert.NotNull(agent);
            Assert.Same(helpers[0], agent.Helper);
            Assert.Same(newAsset, helpers[0].CurrentAsset);
        }

        [Fact]
        public void PlaySound_SetSoundAssetFails_ReturnsSetSoundAssetFailure()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 1);
            helpers[0].SetSoundAssetResult = false;

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            ISoundAgent agent = group.PlaySound(1, new object(), playSoundParams, out PlaySoundErrorCode? errorCode);

            Assert.Null(agent);
            Assert.Equal(PlaySoundErrorCode.SetSoundAssetFailure, errorCode);
            ReferencePool.Release(playSoundParams);
        }

        [Fact]
        public void PlaySound_AssignsSerialIdAndParamsToAgent()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 1);
            group.Volume = 1f;

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = 7;
            playSoundParams.Loop = true;
            playSoundParams.Pitch = 2f;
            playSoundParams.VolumeInSoundGroup = 0.5f;
            playSoundParams.FadeInSeconds = 0.3f;

            ISoundAgent agent = group.PlaySound(42, new object(), playSoundParams, out PlaySoundErrorCode? errorCode);

            Assert.NotNull(agent);
            Assert.Null(errorCode);
            Assert.Equal(42, agent.SerialId);
            Assert.Equal(7, helpers[0].Priority);
            Assert.True(helpers[0].Loop);
            Assert.Equal(2f, helpers[0].Pitch);
            Assert.Equal(0.5f, helpers[0].Volume); // 组音量 1 × 组内音量 0.5
            Assert.Equal(0.3f, helpers[0].LastFadeInSeconds); // 淡入时间透传给 Play
            Assert.True(helpers[0].IsPlaying);
            ReferencePool.Release(playSoundParams);
        }

        // ──────────────── 组行为 ────────────────

        [Fact]
        public void StopSound_BySerialId_StopsAgent()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 1);

            Play(group, 1, new object(), 0);
            Assert.True(group.IsPlaying(1));

            Assert.True(group.StopSound(1, 0f));
            Assert.False(helpers[0].IsPlaying);
            Assert.False(group.IsPlaying(1));
            Assert.False(group.StopSound(999, 0f)); // 未知序列号
        }

        [Fact]
        public void PauseAndResumeSound_BySerialId()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 1);

            Play(group, 1, new object(), 0);

            Assert.True(group.PauseSound(1, 0f));
            Assert.True(group.ResumeSound(1, 0f));
            Assert.False(group.PauseSound(999, 0f));
            Assert.False(group.ResumeSound(999, 0f));
        }

        [Fact]
        public void SoundGroup_MuteAndVolume_PropagateToAgents()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 2);

            group.Mute = true;
            Assert.True(helpers[0].Mute);
            Assert.True(helpers[1].Mute);

            group.Volume = 0.5f;
            Assert.Equal(0.5f, helpers[0].Volume); // 组音量 0.5 × 组内默认音量 1
            Assert.Equal(0.5f, helpers[1].Volume);

            group.Mute = false;
            Assert.False(helpers[0].Mute);
        }

        [Fact]
        public void StopAllLoadedSounds_StopsAllPlayingAgents()
        {
            SoundManager.SoundGroup group = CreateGroupWithAgents(out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> helpers, 2);

            Play(group, 1, new object(), 0);
            Play(group, 2, new object(), 0);
            Assert.True(helpers[0].IsPlaying);
            Assert.True(helpers[1].IsPlaying);

            group.StopAllLoadedSounds(0f);

            Assert.False(helpers[0].IsPlaying);
            Assert.False(helpers[1].IsPlaying);
        }

        // ──────────────── 辅助 ────────────────

        /// <summary>创建含指定数量代理的声音组，返回各 Fake 引用。</summary>
        private static SoundManager.SoundGroup CreateGroupWithAgents(
            out FakeSoundHelper soundHelper, out List<FakeSoundAgentHelper> agentHelpers, int agentCount)
        {
            soundHelper = new FakeSoundHelper();
            SoundManager.SoundGroup group = new SoundManager.SoundGroup("test", new FakeSoundGroupHelper());
            agentHelpers = new List<FakeSoundAgentHelper>();
            for (int i = 0; i < agentCount; i++)
            {
                FakeSoundAgentHelper agentHelper = new FakeSoundAgentHelper();
                agentHelper.SetSoundAssetResult = true;
                group.AddSoundAgentHelper(soundHelper, agentHelper);
                agentHelpers.Add(agentHelper);
            }

            return group;
        }

        /// <summary>以指定优先级播放并释放临时参数（SoundGroup.PlaySound 不负责释放参数）。</summary>
        private static ISoundAgent Play(SoundManager.SoundGroup group, int serialId, object asset, int priority)
        {
            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = priority;
            ISoundAgent agent = group.PlaySound(serialId, asset, playSoundParams, out PlaySoundErrorCode? errorCode);
            ReferencePool.Release(playSoundParams);
            return agent;
        }
    }
}
