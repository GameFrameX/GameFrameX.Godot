using System;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Timer.Runtime
{
    /// <summary>
    /// 计时器组件。
    /// </summary>
    public sealed partial class TimerComponent : GameFrameworkComponent
    {
        private ITimerManager _timerManager;

        public override void _Ready()
        {
            if (string.IsNullOrEmpty(componentType))
            {
                // 代码动态创建（LauncherAuto 场景）无场景序列化值，空值兜底默认实现，避免注册 Guard 中断
                componentType = typeof(TimerManager).FullName;
            }
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(ITimerManager);
            base._Ready();
            _timerManager = GameFrameworkEntry.GetModule<ITimerManager>();
            if (_timerManager == null)
            {
                Log.Fatal("Timer manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 添加一个定时调用的任务
        /// </summary>
        /// <param name="interval">间隔时间（以秒为单位）</param>
        /// <param name="repeat">重复次数（0 表示无限重复）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        /// <param name="tag">标签（用于分组管理）</param>
        /// <param name="timeScale">时间缩放模式</param>
        /// <param name="onComplete">定时器完成/移除时触发的回调</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        public int Add(float interval, int repeat, Action<object> callback, object callbackParam = null, string tag = null, TimerTimeScale timeScale = TimerTimeScale.Unscaled, Action onComplete = null)
        {
            return _timerManager.Add(interval, repeat, callback, callbackParam, tag, timeScale, onComplete);
        }

        /// <summary>
        /// 添加一个只执行一次的任务
        /// </summary>
        /// <param name="interval">间隔时间（以秒为单位）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        public int AddOnce(float interval, Action<object> callback, object callbackParam = null)
        {
            return _timerManager.AddOnce(interval, callback, callbackParam);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        public int AddUpdate(Action<object> callback)
        {
            return _timerManager.AddUpdate(callback);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        public int AddUpdate(Action<object> callback, object callbackParam)
        {
            return _timerManager.AddUpdate(callback, callbackParam);
        }

        /// <summary>
        /// 检查指定的任务是否存在
        /// </summary>
        /// <param name="callback">要检查的回调函数</param>
        /// <returns>存在返回 true，不存在返回 false</returns>
        public bool Exists(Action<object> callback)
        {
            return _timerManager.Exists(callback);
        }

        /// <summary>
        /// 检查指定 ID 的任务是否存在且未被删除。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>存在且活跃返回 true，否则 false</returns>
        public bool Exists(int id)
        {
            return _timerManager.Exists(id);
        }

        /// <summary>
        /// 移除指定的任务
        /// </summary>
        /// <param name="callback">要移除的回调函数</param>
        public void Remove(Action<object> callback)
        {
            _timerManager.Remove(callback);
        }

        /// <summary>
        /// 按 ID 移除指定的任务。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        public void Remove(int id)
        {
            _timerManager.Remove(id);
        }

        /// <summary>
        /// 暂停指定 ID 的定时器（暂停期间不累积时间）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        public void Pause(int id)
        {
            _timerManager.Pause(id);
        }

        /// <summary>
        /// 恢复指定 ID 的定时器（从暂停处继续累积时间）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        public void Resume(int id)
        {
            _timerManager.Resume(id);
        }

        /// <summary>
        /// 检查指定 ID 的定时器是否处于暂停状态。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>暂停中返回 true，否则 false</returns>
        public bool IsPaused(int id)
        {
            return _timerManager.IsPaused(id);
        }

        /// <summary>
        /// 暂停指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        public void PauseByTag(string tag)
        {
            _timerManager.PauseByTag(tag);
        }

        /// <summary>
        /// 恢复指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        public void ResumeByTag(string tag)
        {
            _timerManager.ResumeByTag(tag);
        }

        /// <summary>
        /// 移除指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        public void RemoveByTag(string tag)
        {
            _timerManager.RemoveByTag(tag);
        }

        /// <summary>
        /// 检查指定标签是否有活跃的定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        /// <returns>存在至少一个定时器返回 true，否则 false</returns>
        public bool HasTag(string tag)
        {
            return _timerManager.HasTag(tag);
        }

        /// <summary>
        /// 获取指定 ID 定时器的剩余时间（秒）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>剩余秒数；定时器不存在返回 -1</returns>
        public float GetRemaining(int id)
        {
            return _timerManager.GetRemaining(id);
        }

        /// <summary>
        /// 获取指定 ID 定时器已累积的经过时间（秒）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>已累积的秒数；定时器不存在返回 -1</returns>
        public float GetElapsed(int id)
        {
            return _timerManager.GetElapsed(id);
        }

        /// <summary>
        /// 获取指定 ID 定时器的剩余重复次数。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>剩余次数（0 表示无限循环）；定时器不存在返回 -1</returns>
        public int GetRepeatLeft(int id)
        {
            return _timerManager.GetRepeatLeft(id);
        }

        /// <summary>
        /// 等待指定的秒数后完成（async/await 支持）。
        /// </summary>
        /// <param name="seconds">等待的秒数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>当等待时间到达时完成的任务</returns>
        public Task WaitForSecondsAsync(float seconds, CancellationToken cancellationToken = default)
        {
            return _timerManager.WaitForSecondsAsync(seconds, cancellationToken);
        }

        /// <summary>
        /// 等待下一帧更新后完成（async/await 支持）。
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下一帧到达时完成的任务</returns>
        public Task WaitForNextFrameAsync(CancellationToken cancellationToken = default)
        {
            return _timerManager.WaitForNextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 等待指定的帧数后完成（async/await 支持）。
        /// </summary>
        /// <param name="frameCount">要等待的帧数（必须 &gt; 0）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>指定帧数到达时完成的任务</returns>
        public Task WaitForFramesAsync(int frameCount, CancellationToken cancellationToken = default)
        {
            return _timerManager.WaitForFramesAsync(frameCount, cancellationToken);
        }
    }
}
