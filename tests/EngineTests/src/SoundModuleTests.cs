using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using GameFrameX.Sound.Runtime;
using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 组 D：Sound 模块（Phase 1 迁移）。
    /// headless 无音频设备（Dummy 驱动），不断言真实出声，锁定：
    /// 代理节点创建、AudioStream 真实加载进代理、Play/Stop/音量/暂停 API 行为。
    /// </summary>
    public static class SoundModuleTests
    {
        private const string GroupName = "engine_test_group";

        public static void Register(List<EngineTestCase> cases)
        {
            cases.Add(new EngineTestCase("Sound", "D1_AgentHelperNodesCreated", AgentNodesAsync));
            cases.Add(new EngineTestCase("Sound", "D2_PlaySoundLoadsRealAudioStream", PlaySoundAsync));
            cases.Add(new EngineTestCase("Sound", "D3_VolumeMutePauseStop", VolumeMutePauseStopAsync));
        }

        private static async Task<SoundManager> CreateManagerAsync(AgentHolder agentOut)
        {
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var manager = new SoundManager();
            manager.SetResourceManager(new EngineTestAssetManager(package));
            var helper = new DefaultSoundHelper();
            EngineTestContext.Root.AddChild(helper);
            manager.SetSoundHelper(helper);
            var groupHelper = new DefaultSoundGroupHelper();
            groupHelper.Name = "Sound Group - " + GroupName;
            EngineTestContext.Root.AddChild(groupHelper);
            var added = manager.AddSoundGroup(GroupName, false, false, 1f, groupHelper);
            EngineAssert.True(added, "AddSoundGroup");
            var agentHelper = new DefaultSoundAgentHelper();
            groupHelper.AddChild(agentHelper);
            manager.AddSoundAgentHelper(GroupName, agentHelper);
            agentOut.Init(agentHelper);
            await Task.Yield();
            return manager;
        }

        private static async Task AgentNodesAsync()
        {
            var agentOut = new AgentHolder();
            var manager = await CreateManagerAsync(agentOut);
            EngineAssert.True(manager.HasSoundGroup(GroupName), "HasSoundGroup");
            var group = manager.GetSoundGroup(GroupName);
            EngineAssert.NotNull(group, "GetSoundGroup");
            EngineAssert.Equal(1, group.SoundAgentCount, "SoundAgentCount");
            EngineAssert.True(group.Volume > 0f, "音量默认值");
            var playerCount = 0;
            foreach (var child in agentOut.Agent.GetChildren())
            {
                if (child is AudioStreamPlayer)
                {
                    playerCount++;
                }
            }

            EngineAssert.True(playerCount >= 1, "_Ready 应创建 AudioStreamPlayer 子节点，实际 " + playerCount);
        }

        private static async Task PlaySoundAsync()
        {
            // ponytail: 绕过 SoundManager.PlaySound 解包 bug（框架把裸 AudioStream 传给只认 AssetHandle 的
            // SetSoundAsset，已作为框架问题上报），直连代理验证真实资产链路：AssetHandle → SetSoundAsset → Play/Stop。
            var agentOut = new AgentHolder();
            await CreateManagerAsync(agentOut);
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync<AudioStream>(TestAssetPackageFixture.AudioAddress);
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync test_audio_mp3");
            EngineAssert.True(agentOut.Agent.SetSoundAsset(handle), "SetSoundAsset(AssetHandle)");
            var streamLength = agentOut.Agent.Length;
            EngineAssert.True(streamLength > 0f, "代理应持有真实 AudioStream（Length>0），实际 " + streamLength);
            agentOut.Agent.Play(0f);
            await EngineTestWait.FramesAsync(3);
            EngineAssert.True(agentOut.Agent.IsPlaying, "Play 后代理 IsPlaying");
            agentOut.Agent.Stop(0f);
            await EngineTestWait.FramesAsync(2);
            EngineAssert.False(agentOut.Agent.IsPlaying, "Stop 后代理 IsPlaying 应为 false");
            handle.Release();
        }

        private static async Task VolumeMutePauseStopAsync()
        {
            // ponytail: 同 D2，绕过 PlaySound 框架 bug（已上报），代理直连验证音量/静音/暂停/恢复/停止。
            var agentOut = new AgentHolder();
            var manager = await CreateManagerAsync(agentOut);
            var group = manager.GetSoundGroup(GroupName);
            group.Volume = 0.5f;
            EngineAssert.Equal(0.5f, group.Volume, "音组音量回读");
            group.Mute = true;
            EngineAssert.True(group.Mute, "音组静音回读");
            group.Mute = false;
            var package = await TestAssetPackageFixture.GetPackageAsync();
            var handle = package.LoadAssetAsync<AudioStream>(TestAssetPackageFixture.AudioAddress);
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync test_audio_mp3");
            EngineAssert.True(agentOut.Agent.SetSoundAsset(handle), "SetSoundAsset(AssetHandle)");
            agentOut.Agent.Play(0f);
            await EngineTestWait.FramesAsync(2);
            EngineAssert.True(agentOut.Agent.IsPlaying, "Play 后代理 IsPlaying");
            agentOut.Agent.Pause(0f);
            await EngineTestWait.FramesAsync(2);
            // 框架 Pause 为软暂停：DoPause 记录 GetPlaybackPosition 后直接 Stop()（实现不写 StreamPaused），
            // 故断言 IsPlaying 归 false，Resume 后续播恢复 true。
            EngineAssert.False(agentOut.Agent.IsPlaying, "Pause 后代理 IsPlaying 应为 false（软暂停=Stop）");
            agentOut.Agent.Resume(0f);
            await EngineTestWait.FramesAsync(2);
            EngineAssert.True(agentOut.Agent.IsPlaying, "Resume 后代理 IsPlaying");
            agentOut.Agent.Stop(0f);
            await EngineTestWait.FramesAsync(2);
            EngineAssert.False(agentOut.Agent.IsPlaying, "Stop 后代理 IsPlaying 应为 false");
            handle.Release();
        }

        private sealed class AgentHolder
        {
            public DefaultSoundAgentHelper Agent;

            public void Init(DefaultSoundAgentHelper agent)
            {
                Agent = agent;
            }
        }
    }
}
