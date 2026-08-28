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
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================



using System;
using GameFrameX.AssetSystem;
using GameFrameX.Entity.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 默认声音代理辅助器。
    /// </summary>
    /// <remarks>
    /// Godot 迁移说明（对应 Unity 版 AudioSource 实现）：
    /// - 持有 2D 播放器（AudioStreamPlayer）与 3D 播放器（AudioStreamPlayer3D）两个子节点，按需择一播放：
    ///   Play 时 SpatialBlend &gt; 0 使用 3D，否则 2D；播放后收到非零 WorldPosition 或 BindingEntity 再迁移到 3D。
    /// - 音量为线性 0..1（与 Unity 语义一致），写入播放器时经 Mathf.LinearToDb 换算为 VolumeDb。
    /// - 淡入淡出由 CreateTween + TweenMethod 对 VolumeDb 线性插值实现（替代 Unity 协程）。
    /// - 播放结束检测使用 finished 信号（Unity 无此信号，用 Update 轮询 isPlaying）。
    /// - 循环播放：Godot 播放器无 loop 属性，finished 后重播实现。
    /// - 静音：Godot 播放器无 mute 属性，软件合成（音量置 0dB 之下限 -80dB）。
    /// - 优先级：纯软件字段（供代理选择算法使用），不再映射 AudioSource.priority。
    /// - 暂停：Godot 播放器无 Pause，记录播放进度后 Stop，恢复时从记录进度 Play。
    /// </remarks>
    public partial class DefaultSoundAgentHelper : SoundAgentHelperBase
    {
        private AudioStreamPlayer m_Player2D = null;
        private AudioStreamPlayer3D m_Player3D = null;
        private AudioStream m_Stream = null;
        private bool m_Is3D = false;
        private bool m_IsPause = false;
        private EntityLogic m_BindingEntityLogic = null;
        private float m_VolumeWhenPause = 0f;
        private Tween m_CurrentTween = null;
        private bool m_PlayEndedFired = false;
        private bool m_Mute = false;
        private float m_Volume = Constant.DefaultVolume;
        private float m_PanStereo = Constant.DefaultPanStereo;
        private bool m_Loop = Constant.DefaultLoop;
        private int m_Priority = Constant.DefaultPriority;
        private float m_MaxDistance = Constant.DefaultMaxDistance;
        private float m_DopplerLevel = Constant.DefaultDopplerLevel;
        private float m_SpatialBlend = Constant.DefaultSpatialBlend;
        private float m_DesiredTime = Constant.DefaultTime;
        private float m_PausedPosition = 0f;
        private EventHandler<ResetSoundAgentEventArgs> m_ResetSoundAgentEventHandler = null;

        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        public override bool IsPlaying
        {
            get { return m_Is3D ? m_Player3D.Playing : m_Player2D.Playing; }
        }

        /// <summary>
        /// 获取声音长度。
        /// </summary>
        public override float Length
        {
            get { return m_Stream != null ? (float)m_Stream.GetLength() : 0f; }
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public override float Time
        {
            get { return m_Is3D ? m_Player3D.GetPlaybackPosition() : m_Player2D.GetPlaybackPosition(); }
            set
            {
                if (IsPlaying)
                {
                    if (m_Is3D)
                    {
                        m_Player3D.Play(value);
                    }
                    else
                    {
                        m_Player2D.Play(value);
                    }
                }
                else
                {
                    m_DesiredTime = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置是否静音。
        /// </summary>
        public override bool Mute
        {
            get { return m_Mute; }
            set
            {
                m_Mute = value;
                ApplyVolume();
            }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public override bool Loop
        {
            get { return m_Loop; }
            set { m_Loop = value; }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public override int Priority
        {
            get { return m_Priority; }
            set { m_Priority = value; }
        }

        /// <summary>
        /// 获取或设置音量大小（线性 0..1）。
        /// </summary>
        public override float Volume
        {
            get { return m_Volume; }
            set
            {
                m_Volume = value;
                ApplyVolume();
            }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public override float Pitch
        {
            get { return m_Player2D.PitchScale; }
            set
            {
                m_Player2D.PitchScale = value;
                m_Player3D.PitchScale = value;
            }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public override float PanStereo
        {
            get { return m_PanStereo; }
            set
            {
                m_PanStereo = value;
                ApplyPan();
            }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public override float SpatialBlend
        {
            get { return m_SpatialBlend; }
            set { m_SpatialBlend = value; }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public override float MaxDistance
        {
            get { return m_MaxDistance; }
            set
            {
                m_MaxDistance = value;
                m_Player3D.MaxDistance = value;
            }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public override float DopplerLevel
        {
            get { return m_DopplerLevel; }
            set
            {
                m_DopplerLevel = value;
                m_Player3D.DopplerTracking = value > 0f
                    ? AudioStreamPlayer3D.DopplerTrackingEnum.IdleStep
                    : AudioStreamPlayer3D.DopplerTrackingEnum.Disabled;
            }
        }

        /// <summary>
        /// 重置声音代理事件。
        /// </summary>
        public override event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent
        {
            add { m_ResetSoundAgentEventHandler += value; }
            remove { m_ResetSoundAgentEventHandler -= value; }
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public override void Play(float fadeInSeconds)
        {
            StopCurrentTween();
            m_PlayEndedFired = false;
            m_IsPause = false;

            // 按空间混合量选择播放器（&gt;0 视为 3D 声音）
            SwitchPlayer(m_SpatialBlend > 0f);

            float startPosition = m_DesiredTime;
            m_DesiredTime = Constant.DefaultTime;
            PlayActive(startPosition);

            if (fadeInSeconds > 0f)
            {
                float targetVolumeDb = TargetVolumeDb();
                SetVolumeDb(Mathf.LinearToDb(0f));
                FadeVolumeTo(targetVolumeDb, fadeInSeconds, null);
            }
            else
            {
                ApplyVolume();
            }
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public override void Stop(float fadeOutSeconds)
        {
            StopCurrentTween();

            if (fadeOutSeconds > 0f && IsInsideTree() && IsPlaying)
            {
                FadeVolumeTo(Mathf.LinearToDb(0f), fadeOutSeconds, DoStop);
            }
            else
            {
                DoStop();
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public override void Pause(float fadeOutSeconds)
        {
            StopCurrentTween();

            m_VolumeWhenPause = m_Volume;
            if (fadeOutSeconds > 0f && IsInsideTree() && IsPlaying)
            {
                FadeVolumeTo(Mathf.LinearToDb(0f), fadeOutSeconds, DoPause);
            }
            else
            {
                DoPause();
            }
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public override void Resume(float fadeInSeconds)
        {
            StopCurrentTween();

            if (m_IsPause)
            {
                m_IsPause = false;
                PlayActive(m_PausedPosition);
            }

            if (fadeInSeconds > 0f)
            {
                float targetVolumeDb = TargetVolumeDb();
                SetVolumeDb(Mathf.LinearToDb(0f));
                FadeVolumeTo(targetVolumeDb, fadeInSeconds, null);
            }
            else
            {
                m_Volume = m_VolumeWhenPause > 0f ? m_VolumeWhenPause : m_Volume;
                ApplyVolume();
            }
        }

        /// <summary>
        /// 重置声音代理辅助器。
        /// </summary>
        public override void Reset()
        {
            StopCurrentTween();
            m_IsPause = false;
            m_PlayEndedFired = false;
            m_DesiredTime = Constant.DefaultTime;
            m_PausedPosition = 0f;
            m_VolumeWhenPause = 0f;
            m_Player2D.Stop();
            m_Player3D.Stop();
            m_Player2D.Stream = null;
            m_Player3D.Stream = null;
            m_Player3D.Position = Vector3.Zero;
            m_Stream = null;
            m_BindingEntityLogic = null;
        }

        /// <summary>
        /// 设置声音资源。
        /// </summary>
        /// <param name="soundAsset">声音资源（资源系统句柄 AssetHandle）。</param>
        /// <returns>是否设置声音资源成功。</returns>
        public override bool SetSoundAsset(object soundAsset)
        {
            if (soundAsset is AssetHandle assetHandle)
            {
                AudioStream audioStream = assetHandle.GetAssetObject<AudioStream>();
                if (audioStream == null)
                {
                    return false;
                }

                m_Stream = audioStream;
                m_Player2D.Stream = audioStream;
                m_Player3D.Stream = audioStream;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置声音绑定的实体。
        /// </summary>
        /// <param name="bindingEntity">声音绑定的实体。</param>
        public override void SetBindingEntity(GameFrameX.Entity.Runtime.Entity bindingEntity)
        {
            m_BindingEntityLogic = bindingEntity != null ? bindingEntity.Logic : null;
            if (m_BindingEntityLogic != null)
            {
                // 实体绑定的声音按 3D 处理，需要跟随实体位置
                SwitchPlayer(true);
                UpdateAgentPosition();
                return;
            }

            RaiseResetSoundAgent();
        }

        /// <summary>
        /// 设置声音所在的世界坐标。
        /// </summary>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        public override void SetWorldPosition(Vector3 worldPosition)
        {
            // 纯 2D 播放（默认路径）会收到 Vector3.Zero，此时不切换播放器
            if (worldPosition != Vector3.Zero)
            {
                SwitchPlayer(true);
            }

            m_Player3D.GlobalPosition = worldPosition;
        }

        public override void _Ready()
        {
            m_Player2D = new AudioStreamPlayer
            {
                Name = "Player2D",
            };
            m_Player3D = new AudioStreamPlayer3D
            {
                Name = "Player3D",
            };
            AddChild(m_Player2D);
            AddChild(m_Player3D);
            m_Player2D.Finished += OnPlaybackFinished;
            m_Player3D.Finished += OnPlaybackFinished;
            MaxDistance = m_MaxDistance;
            DopplerLevel = m_DopplerLevel;
            Pitch = Constant.DefaultPitch;
            ApplyVolume();
        }

        public override void _Process(double delta)
        {
            if (m_BindingEntityLogic != null)
            {
                UpdateAgentPosition();
            }
        }

        public override void _ExitTree()
        {
            StopCurrentTween();
            base._ExitTree();
        }

        private void DoStop()
        {
            m_Player2D.Stop();
            m_Player3D.Stop();
        }

        private void DoPause()
        {
            m_IsPause = true;
            m_PausedPosition = m_Is3D ? m_Player3D.GetPlaybackPosition() : m_Player2D.GetPlaybackPosition();
            if (m_Is3D)
            {
                m_Player3D.Stop();
            }
            else
            {
                m_Player2D.Stop();
            }
        }

        /// <summary>
        /// 在 2D / 3D 播放器之间切换，保留播放进度、音量与音调。
        /// </summary>
        /// <param name="want3D">是否使用 3D 播放器。</param>
        private void SwitchPlayer(bool want3D)
        {
            if (want3D == m_Is3D)
            {
                return;
            }

            bool wasPlaying = IsPlaying;
            float position = m_Is3D ? m_Player3D.GetPlaybackPosition() : m_Player2D.GetPlaybackPosition();
            float volumeDb = m_Is3D ? m_Player3D.VolumeDb : m_Player2D.VolumeDb;
            float pitchScale = m_Is3D ? m_Player3D.PitchScale : m_Player2D.PitchScale;
            bool was3D = m_Is3D;

            m_Is3D = want3D;
            SyncPlayerState(volumeDb, pitchScale);
            if (wasPlaying)
            {
                if (was3D)
                {
                    m_Player3D.Stop();
                }
                else
                {
                    m_Player2D.Stop();
                }

                PlayActive(position);
            }
        }

        /// <summary>
        /// 从指定位置播放当前活动播放器。
        /// </summary>
        /// <param name="position">播放起始位置。</param>
        private void PlayActive(float position)
        {
            if (m_Is3D)
            {
                m_Player3D.Play(position);
            }
            else
            {
                m_Player2D.Play(position);
            }
        }

        /// <summary>
        /// 将音量、音调、流同步到当前活动播放器。
        /// </summary>
        private void SyncPlayerState(float volumeDb, float pitchScale)
        {
            if (m_Is3D)
            {
                m_Player3D.Stream = m_Stream;
                m_Player3D.VolumeDb = volumeDb;
                m_Player3D.PitchScale = pitchScale;
            }
            else
            {
                m_Player2D.Stream = m_Stream;
                m_Player2D.VolumeDb = volumeDb;
                m_Player2D.PitchScale = pitchScale;
            }
        }

        private void ApplyVolume()
        {
            SetVolumeDb(TargetVolumeDb());
        }

        private float TargetVolumeDb()
        {
            if (m_Mute || m_Volume <= 0f)
            {
                return Mathf.LinearToDb(0f);
            }

            return Mathf.LinearToDb(m_Volume);
        }

        private void SetVolumeDb(float volumeDb)
        {
            m_Player2D.VolumeDb = volumeDb;
            m_Player3D.VolumeDb = volumeDb;
        }

        private void ApplyPan()
        {
            // Unity panStereo(-1..1) 与 Godot panning_strength(0..3) 语义不同，取绝对值近似。
            // Godot 的非位置播放器 AudioStreamPlayer 无声相属性，仅 3D 播放器支持。
            m_Player3D.PanningStrength = Mathf.Clamp(Mathf.Abs(m_PanStereo), 0f, 3f);
        }

        /// <summary>
        /// 对 VolumeDb 做线性插值淡变（替代 Unity 的 FadeToVolume 协程）。
        /// </summary>
        private void FadeVolumeTo(float targetVolumeDb, float duration, Action onFinished)
        {
            if (!IsInsideTree())
            {
                SetVolumeDb(targetVolumeDb);
                if (onFinished != null)
                {
                    onFinished();
                }

                return;
            }

            float startVolumeDb = m_Is3D ? m_Player3D.VolumeDb : m_Player2D.VolumeDb;
            m_CurrentTween = CreateTween();
            m_CurrentTween.TweenMethod(Callable.From<float>(volumeDb => SetVolumeDb(volumeDb)), startVolumeDb, targetVolumeDb, duration);
            if (onFinished != null)
            {
                m_CurrentTween.TweenCallback(Callable.From(onFinished));
            }
        }

        private void StopCurrentTween()
        {
            if (m_CurrentTween != null && m_CurrentTween.IsValid())
            {
                m_CurrentTween.Kill();
            }

            m_CurrentTween = null;
        }

        /// <summary>
        /// 播放结束回调（Godot finished 信号，Unity 版用 Update 轮询实现）。
        /// </summary>
        private void OnPlaybackFinished()
        {
            if (m_Loop)
            {
                // Godot 播放器无 loop 属性，重播实现循环播放
                PlayActive(0f);
                return;
            }

            if (m_PlayEndedFired)
            {
                return;
            }

            m_PlayEndedFired = true;
            RaiseResetSoundAgent();
        }

        /// <summary>
        /// 更新代理位置到绑定实体位置。
        /// </summary>
        private void UpdateAgentPosition()
        {
            if (m_BindingEntityLogic.Available)
            {
                Node cachedTransform = m_BindingEntityLogic.CachedTransform;
                if (cachedTransform is Node3D node3D)
                {
                    m_Player3D.GlobalPosition = node3D.GlobalPosition;
                }
                else if (cachedTransform is Node2D node2D)
                {
                    m_Player3D.GlobalPosition = new Vector3(node2D.GlobalPosition.X, node2D.GlobalPosition.Y, 0f);
                }

                return;
            }

            RaiseResetSoundAgent();
        }

        private void RaiseResetSoundAgent()
        {
            if (m_ResetSoundAgentEventHandler != null)
            {
                ResetSoundAgentEventArgs resetSoundAgentEventArgs = ResetSoundAgentEventArgs.Create();
                m_ResetSoundAgentEventHandler(this, resetSoundAgentEventArgs);
                ReferencePool.Release(resetSoundAgentEventArgs);
            }
        }
    }
}
