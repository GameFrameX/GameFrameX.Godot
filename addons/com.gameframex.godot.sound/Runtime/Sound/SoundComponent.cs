// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 声音组件。
    /// </summary>
    /// <remarks>
    /// Godot 迁移说明（对应 Unity 版 SoundComponent）：
    /// - MonoBehaviour 生命周期 Awake/Start 合并为 _Ready，OnDestroy 对应 _ExitTree。
    /// - Unity 版的 AudioMixer 查找（EnsureAudioMixer/CreateDefaultAudioMixer/FindMatchingGroups）整体省略：
    ///   组音量与静音在 Godot 版 SoundGroup/代理中已有软件合成（音量线性 0..1 换算 VolumeDb，静音置 -80dB 下限），
    ///   无需 AudioMixerGroup 路由。如后续需要硬件混音总线，可按组名接入 AudioServer.SetBusVolumeDb/SetBusMute。
    /// - Unity 版的 AudioListener 唯一性管理（RefreshAudioListener/OnSceneLoaded/OnSceneUnloaded）省略：
    ///   Godot 的 Camera3D 自带 Listener，由相机体系管理。
    /// - 检查器组配置由 [SerializeField] SoundGroup[] 改为平行数组导出（见 m_SoundGroupNames 等字段说明）。
    /// - UniTask 返回值统一替换为 Task。
    /// </remarks>
    [GlobalClass]
    public sealed partial class SoundComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private ISoundManager m_SoundManager = null;
        private EventComponent m_EventComponent = null;

        [Export] private Node m_InstanceRoot = null;
        [Export] private string m_SoundHelperTypeName = "GameFrameX.Sound.Runtime.DefaultSoundHelper";
        [Export] private string m_SoundGroupHelperTypeName = "GameFrameX.Sound.Runtime.DefaultSoundGroupHelper";
        [Export] private string m_SoundAgentHelperTypeName = "GameFrameX.Sound.Runtime.DefaultSoundAgentHelper";
        [Export] private SoundHelperBase m_CustomSoundHelper = null;
        [Export] private SoundGroupHelperBase m_CustomSoundGroupHelper = null;
        [Export] private SoundAgentHelperBase m_CustomSoundAgentHelper = null;

        // Godot 迁移说明：Unity 版为 [SerializeField] private SoundGroup[]（[Serializable] 嵌套类）。
        // Godot 4.5 无法在检查器导出普通嵌套类数组（需 Resource 化 + [GlobalClass]，嵌套类不适用），
        // 故拆为按索引对齐的平行数组，初始化时聚合为嵌套 SoundGroup 配置对象。
        // 数组长度不足的条目回退默认值（false/false/1f/1）。
        [Export] private string[] m_SoundGroupNames = Array.Empty<string>();
        [Export] private Godot.Collections.Array<bool> m_SoundGroupAvoidBeingReplacedBySamePriority = new Godot.Collections.Array<bool>();
        [Export] private Godot.Collections.Array<bool> m_SoundGroupMutes = new Godot.Collections.Array<bool>();
        [Export] private float[] m_SoundGroupVolumes = Array.Empty<float>();
        [Export] private int[] m_SoundGroupAgentHelperCounts = Array.Empty<int>();

        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int SoundGroupCount
        {
            get { return m_SoundManager.SoundGroupCount; }
        }

        /// <summary>
        /// 组件初始化（对应 Unity 版 Awake + Start 合并）。
        /// </summary>
        public override void _Ready()
        {
            base._Ready();
            m_SoundManager = GameFrameworkEntry.GetModule<ISoundManager>();
            if (m_SoundManager == null)
            {
                Log.Fatal("Sound manager is invalid.");
                return;
            }

            m_SoundManager.PlaySoundSuccess += OnPlaySoundSuccess;
            m_SoundManager.PlaySoundFailure += OnPlaySoundFailure;

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Error("Event component is invalid.");
                return;
            }

            m_SoundManager.SetResourceManager(GameFrameworkEntry.GetModule<IAssetManager>());

            SoundHelperBase soundHelper = CreateHelper(m_SoundHelperTypeName, m_CustomSoundHelper, 0);
            if (soundHelper == null)
            {
                Log.Error("Can not create sound helper.");
                return;
            }

            soundHelper.Name = "Sound Helper";
            AttachChild(soundHelper);
            m_SoundManager.SetSoundHelper(soundHelper);

            EnsureInstanceRoot();

            List<SoundGroup> soundGroups = BuildConfiguredSoundGroups();
            for (int i = 0; i < soundGroups.Count; i++)
            {
                SoundGroup group = soundGroups[i];
                if (!AddSoundGroup(group.Name, group.AvoidBeingReplacedBySamePriority, group.Mute, group.Volume, group.AgentHelperCount))
                {
                    Log.Warning($"Add sound group '{group.Name}' failure.");
                    continue;
                }
            }
        }

        /// <summary>
        /// 组件销毁（对应 Unity 版 OnDestroy，仅退订事件；声音模块生命周期由 GameFrameworkEntry 统一管理）。
        /// </summary>
        public override void _ExitTree()
        {
            base._ExitTree();
            if (m_SoundManager == null)
            {
                return;
            }

            m_SoundManager.PlaySoundSuccess -= OnPlaySoundSuccess;
            m_SoundManager.PlaySoundFailure -= OnPlaySoundFailure;
        }

        /// <summary>
        /// 检查是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>是否存在声音组。</returns>
        public bool HasSoundGroup(string soundGroupName)
        {
            return m_SoundManager.HasSoundGroup(soundGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public ISoundGroup GetSoundGroup(string soundGroupName)
        {
            return m_SoundManager.GetSoundGroup(soundGroupName);
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        public ISoundGroup[] GetAllSoundGroups()
        {
            return m_SoundManager.GetAllSoundGroups();
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        public void GetAllSoundGroups(List<ISoundGroup> results)
        {
            m_SoundManager.GetAllSoundGroups(results);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, int soundAgentHelperCount)
        {
            return AddSoundGroup(soundGroupName, false, false, 1f, soundAgentHelperCount);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">是否禁止同优先级声音替换。</param>
        /// <param name="soundGroupMute">是否静音。</param>
        /// <param name="soundGroupVolume">音量。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, int soundAgentHelperCount)
        {
            if (m_SoundManager.HasSoundGroup(soundGroupName))
            {
                return false;
            }

            SoundGroupHelperBase soundGroupHelper = CreateHelper(m_SoundGroupHelperTypeName, m_CustomSoundGroupHelper, SoundGroupCount);
            if (soundGroupHelper == null)
            {
                Log.Error("Can not create sound group helper.");
                return false;
            }

            soundGroupHelper.Name = $"Sound Group - {soundGroupName}";
            EnsureInstanceRoot();
            m_InstanceRoot.AddChild(soundGroupHelper);

            // Godot 迁移说明：Unity 版此处通过 AudioMixer.FindMatchingGroups 查找并设置组的 AudioMixerGroup，
            // Godot 版组音量/静音已由软件合成，此处省略（详见类备注）。
            if (!m_SoundManager.AddSoundGroup(soundGroupName, soundGroupAvoidBeingReplacedBySamePriority, soundGroupMute, soundGroupVolume, soundGroupHelper))
            {
                return false;
            }

            for (int i = 0; i < soundAgentHelperCount; i++)
            {
                if (!AddSoundAgentHelper(soundGroupName, soundGroupHelper, i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingSoundSerialIds()
        {
            return m_SoundManager.GetAllLoadingSoundSerialIds();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            m_SoundManager.GetAllLoadingSoundSerialIds(results);
        }

        /// <summary>
        /// 获取正在加载声音是否准备完成。
        /// </summary>
        /// <param name="serialId">要检查的序列编号。</param>
        /// <returns>正在加载声音是否准备完成。</returns>
        public bool IsLoadingSound(int serialId)
        {
            return m_SoundManager.IsLoadingSound(serialId);
        }

        /// <summary>
        /// 获取指定声音组中是否正在播放指定序列编号的声音。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="serialId">要检查的序列编号。</param>
        /// <returns>是否正在播放。</returns>
        public bool IsPlaying(string soundGroupName, int serialId)
        {
            ISoundGroup soundGroup = m_SoundManager.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                return false;
            }

            return soundGroup.IsPlaying(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, null, null, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, null, null, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, playSoundParams, null, null, null);
        }

        /// <summary>
        /// 播放绑定实体的声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="bindingEntity">声音绑定的实体。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, GameFrameX.Entity.Runtime.Entity bindingEntity)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, bindingEntity, null, null);
        }

        /// <summary>
        /// 播放指定世界坐标处的声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, Vector3 worldPosition)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, worldPosition, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, null, userData, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, null, null);
        }

        /// <summary>
        /// 播放绑定实体的声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingEntity">声音绑定的实体。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, GameFrameX.Entity.Runtime.Entity bindingEntity)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, bindingEntity, null, null);
        }

        /// <summary>
        /// 播放绑定实体的声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingEntity">声音绑定的实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="serialId">播放声音的序列编号（小于 0 时自动分配）。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, GameFrameX.Entity.Runtime.Entity bindingEntity, object userData, int serialId)
        {
            int? targetSerialId = serialId >= 0 ? serialId : (int?)null;
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, bindingEntity, userData, targetSerialId);
        }

        /// <summary>
        /// 播放指定世界坐标处的声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, Vector3 worldPosition)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, worldPosition, null);
        }

        /// <summary>
        /// 播放指定世界坐标处的声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, Vector3 worldPosition, object userData)
        {
            return m_SoundManager.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, SoundPlayContext.Create(null, worldPosition, userData), null);
        }

        /// <summary>
        /// 以播放选项播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="options">播放选项。</param>
        /// <returns>播放声音的序列编号。</returns>
        public async Task<int> PlaySound(string soundAssetName, string soundGroupName, SoundPlayOptions options)
        {
            if (options == null)
            {
                options = new SoundPlayOptions();
            }

            if (options.BindingEntity != null)
            {
                return await PlaySound(soundAssetName, soundGroupName, options.Priority, options.PlaySoundParams, options.BindingEntity, options.UserData, options.SerialId);
            }

            return await m_SoundManager.PlaySound(soundAssetName, soundGroupName, options.Priority, options.PlaySoundParams, SoundPlayContext.Create(null, options.WorldPosition.GetValueOrDefault(), options.UserData), options.SerialId);
        }

        /// <summary>
        /// 以指定序列编号播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="serialId">播放声音的序列编号。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySoundBySerialId(string soundAssetName, string soundGroupName, int serialId)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, null, null, null, serialId);
        }

        /// <summary>
        /// 以指定序列编号播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="serialId">播放声音的序列编号。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>播放声音的序列编号。</returns>
        public Task<int> PlaySoundBySerialId(string soundAssetName, string soundGroupName, int serialId, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, DefaultPriority, playSoundParams, null, null, serialId);
        }

        /// <summary>
        /// 停止播放指定序列编号的声音。
        /// </summary>
        /// <param name="serialId">要停止播放的声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId)
        {
            return m_SoundManager.StopSound(serialId);
        }

        /// <summary>
        /// 停止播放指定序列编号的声音。
        /// </summary>
        /// <param name="serialId">要停止播放的声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            return m_SoundManager.StopSound(serialId, fadeOutSeconds);
        }

        /// <summary>
        /// 停止播放所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            m_SoundManager.StopAllLoadedSounds();
        }

        /// <summary>
        /// 停止播放所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            m_SoundManager.StopAllLoadedSounds(fadeOutSeconds);
        }

        /// <summary>
        /// 停止播放所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingSounds()
        {
            m_SoundManager.StopAllLoadingSounds();
        }

        /// <summary>
        /// 暂停播放指定序列编号的声音。
        /// </summary>
        /// <param name="serialId">要暂停播放的声音的序列编号。</param>
        public void PauseSound(int serialId)
        {
            m_SoundManager.PauseSound(serialId);
        }

        /// <summary>
        /// 暂停播放指定序列编号的声音。
        /// </summary>
        /// <param name="serialId">要暂停播放的声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            m_SoundManager.PauseSound(serialId, fadeOutSeconds);
        }

        /// <summary>
        /// 恢复播放指定序列编号的声音。
        /// </summary>
        /// <param name="serialId">要恢复播放的声音的序列编号。</param>
        public void ResumeSound(int serialId)
        {
            m_SoundManager.ResumeSound(serialId);
        }

        /// <summary>
        /// 恢复播放指定序列编号的声音。
        /// </summary>
        /// <param name="serialId">要恢复播放的声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            m_SoundManager.ResumeSound(serialId, fadeInSeconds);
        }

        /// <summary>
        /// 将平台事件参数转发为全局事件（对应 Unity 版 OnPlaySoundSuccess）。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventArgs">事件参数。</param>
        private void OnPlaySoundSuccess(object sender, PlaySoundSuccessEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, PlaySoundSuccessEventArgs.Create(eventArgs.SerialId, eventArgs.SoundAssetName, eventArgs.SoundAgent, eventArgs.Duration, eventArgs.UserData));
        }

        /// <summary>
        /// 将平台事件参数转发为全局事件（对应 Unity 版 OnPlaySoundFailure）。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventArgs">事件参数。</param>
        private void OnPlaySoundFailure(object sender, PlaySoundFailureEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, PlaySoundFailureEventArgs.Create(eventArgs.SerialId, eventArgs.SoundAssetName, eventArgs.SoundGroupName, eventArgs.PlaySoundParams, eventArgs.ErrorCode, eventArgs.ErrorMessage, eventArgs.UserData));
        }

        /// <summary>
        /// 绑定实体播放核心实现：将实体、用户数据打包为 SoundPlayContext 后交给声音模块。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">播放优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingEntity">声音绑定的实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="serialId">播放声音的序列编号（为 null 时自动分配）。</param>
        /// <returns>播放声音的序列编号。</returns>
        private Task<int> PlaySound(string soundAssetName, string soundGroupName, int priority, PlaySoundParams playSoundParams, GameFrameX.Entity.Runtime.Entity bindingEntity, object userData, int? serialId)
        {
            return m_SoundManager.PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, SoundPlayContext.Create(bindingEntity, Vector3.Zero, userData), serialId);
        }

        /// <summary>
        /// 创建声音代理辅助器并挂载到声音组辅助器（对应 Unity 版 AddSoundAgentHelper 私有方法）。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <param name="index">代理索引。</param>
        /// <returns>是否创建成功。</returns>
        private bool AddSoundAgentHelper(string soundGroupName, SoundGroupHelperBase soundGroupHelper, int index)
        {
            SoundAgentHelperBase soundAgentHelper = CreateHelper(m_SoundAgentHelperTypeName, m_CustomSoundAgentHelper, index);
            if (soundAgentHelper == null)
            {
                Log.Error("Can not create sound agent helper.");
                return false;
            }

            soundAgentHelper.Name = $"Sound Agent Helper - {soundGroupName} - {index}";
            soundGroupHelper.AddChild(soundAgentHelper);
            m_SoundManager.AddSoundAgentHelper(soundGroupName, soundAgentHelper);
            return true;
        }

        /// <summary>
        /// 创建辅助器节点。
        /// </summary>
        /// <remarks>
        /// 对应 Unity 版 Helper.CreateHelper：自定义辅助器优先；否则按类型名反射创建。
        /// Godot 迁移说明：自定义辅助器仅首个（索引 0）使用，其余代理按类型名创建，
        /// 避免同一节点被重复挂到多个父节点（Godot 的 AddChild 不允许重复父节点）。
        /// </remarks>
        /// <typeparam name="T">辅助器基类型。</typeparam>
        /// <param name="helperTypeName">辅助器类型名。</param>
        /// <param name="customHelper">自定义辅助器。</param>
        /// <param name="helperIndex">辅助器索引。</param>
        /// <returns>创建的辅助器节点，失败时返回 null。</returns>
        private T CreateHelper<T>(string helperTypeName, T customHelper, int helperIndex) where T : Node
        {
            Node helperNode;
            if (customHelper != null && helperIndex == 0)
            {
                helperNode = customHelper;
            }
            else
            {
                if (string.IsNullOrEmpty(helperTypeName))
                {
                    Log.Error("Can not create helper with empty type name.");
                    return null;
                }

                Type helperType = Type.GetType(helperTypeName);
                if (helperType == null || !typeof(T).IsAssignableFrom(helperType))
                {
                    Log.Error($"Can not create helper with type name '{helperTypeName}'.");
                    return null;
                }

                helperNode = (Node)Activator.CreateInstance(helperType);
            }

            return (T)helperNode;
        }

        /// <summary>
        /// 挂载子节点，若节点已有父节点则先脱离原父节点。
        /// </summary>
        /// <param name="child">要挂载的子节点。</param>
        private void AttachChild(Node child)
        {
            Node parent = child.GetParent();
            if (parent != null)
            {
                parent.RemoveChild(child);
            }

            AddChild(child);
        }

        /// <summary>
        /// 确保声音实例根节点存在（对应 Unity 版 Start 中创建 "Sound Instances" 的逻辑）。
        /// </summary>
        private void EnsureInstanceRoot()
        {
            if (m_InstanceRoot != null)
            {
                return;
            }

            m_InstanceRoot = new Node();
            m_InstanceRoot.Name = "Sound Instances";
            AddChild(m_InstanceRoot);
        }

        /// <summary>
        /// 将检查器导出的平行数组聚合为声音组配置列表（对应 Unity 版 [SerializeField] SoundGroup[]）。
        /// </summary>
        /// <returns>声音组配置列表。</returns>
        private List<SoundGroup> BuildConfiguredSoundGroups()
        {
            List<SoundGroup> soundGroups = new List<SoundGroup>();
            if (m_SoundGroupNames == null)
            {
                return soundGroups;
            }

            for (int i = 0; i < m_SoundGroupNames.Length; i++)
            {
                if (string.IsNullOrEmpty(m_SoundGroupNames[i]))
                {
                    continue;
                }

                bool avoidBeingReplacedBySamePriority = i < m_SoundGroupAvoidBeingReplacedBySamePriority.Count && m_SoundGroupAvoidBeingReplacedBySamePriority[i];
                bool mute = i < m_SoundGroupMutes.Count && m_SoundGroupMutes[i];
                float volume = i < m_SoundGroupVolumes.Length ? m_SoundGroupVolumes[i] : 1f;
                int agentHelperCount = i < m_SoundGroupAgentHelperCounts.Length ? m_SoundGroupAgentHelperCounts[i] : 1;
                soundGroups.Add(new SoundGroup(m_SoundGroupNames[i], avoidBeingReplacedBySamePriority, mute, volume, agentHelperCount));
            }

            return soundGroups;
        }
    }
}
