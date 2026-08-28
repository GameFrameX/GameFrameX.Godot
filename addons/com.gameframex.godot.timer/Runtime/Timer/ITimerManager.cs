using System;

namespace GameFrameX.Timer.Runtime
{
    /// <summary>
    /// 定时器接口
    /// </summary>
    public interface ITimerManager
    {
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
        int Add(float interval, int repeat, Action<object> callback, object callbackParam = null, string tag = null, TimerTimeScale timeScale = TimerTimeScale.Unscaled, Action onComplete = null);

        /// <summary>
        /// 添加一个只执行一次的任务
        /// </summary>
        /// <param name="interval">间隔时间（以秒为单位）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        int AddOnce(float interval, Action<object> callback, object callbackParam = null);

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        int AddUpdate(Action<object> callback);

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        int AddUpdate(Action<object> callback, object callbackParam);

        /// <summary>
        /// 检查指定的任务是否存在（按回调）。
        /// </summary>
        /// <param name="callback">要检查的回调函数</param>
        /// <returns>存在返回 true，不存在返回 false</returns>
        bool Exists(Action<object> callback);

        /// <summary>
        /// 检查指定 ID 的任务是否存在且未被删除。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>存在且活跃返回 true，否则 false</returns>
        bool Exists(int id);

        /// <summary>
        /// 移除指定的任务（按回调）。
        /// </summary>
        /// <param name="callback">要移除的回调函数</param>
        void Remove(Action<object> callback);

        /// <summary>
        /// 按 ID 移除指定的任务。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        void Remove(int id);

        /// <summary>
        /// 暂停指定 ID 的定时器（暂停期间不累积时间）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        void Pause(int id);

        /// <summary>
        /// 恢复指定 ID 的定时器（从暂停处继续累积时间）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        void Resume(int id);

        /// <summary>
        /// 检查指定 ID 的定时器是否处于暂停状态。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>暂停中返回 true，否则 false</returns>
        bool IsPaused(int id);

        // ──────────────── Tag-based operations ────────────────

        /// <summary>
        /// 暂停指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        void PauseByTag(string tag);

        /// <summary>
        /// 恢复指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        void ResumeByTag(string tag);

        /// <summary>
        /// 移除指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        void RemoveByTag(string tag);

        /// <summary>
        /// 检查指定标签是否有活跃的定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        /// <returns>存在至少一个定时器返回 true，否则 false</returns>
        bool HasTag(string tag);

        // ──────────────── Query ────────────────

        /// <summary>
        /// 获取指定 ID 定时器的剩余时间（秒）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>剩余秒数；定时器不存在返回 -1</returns>
        float GetRemaining(int id);

        /// <summary>
        /// 获取指定 ID 定时器已累积的经过时间（秒）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>已累积的秒数；定时器不存在返回 -1</returns>
        float GetElapsed(int id);

        /// <summary>
        /// 获取指定 ID 定时器的剩余重复次数。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>剩余次数（0 表示无限循环）；定时器不存在返回 -1</returns>
        int GetRepeatLeft(int id);
    }
}
