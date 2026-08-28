using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameFrameX.Timer.Runtime
{
    /// <summary>
    /// 为 <see cref="ITimerManager"/> 提供 async/await 等待扩展方法。
    /// 所有等待均在 Godot 主线程完成，续体也回到主线程。
    /// </summary>
    public static class TimerAsyncExtensions
    {
        /// <summary>
        /// 等待指定的秒数后完成。
        /// </summary>
        /// <param name="timer">定时器管理器</param>
        /// <param name="seconds">等待的秒数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>当等待时间到达时完成的任务</returns>
        public static Task WaitForSecondsAsync(this ITimerManager timer, float seconds, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

            timer.AddOnce(seconds, _ =>
            {
                registration.Dispose();
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }

        /// <summary>
        /// 等待下一帧更新后完成。
        /// </summary>
        /// <param name="timer">定时器管理器</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下一帧到达时完成的任务</returns>
        public static Task WaitForNextFrameAsync(this ITimerManager timer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

            // AddOnce(0) — interval=0 即"每帧"，repeat=1 即"执行一次后自动移除"
            timer.AddOnce(0, _ =>
            {
                registration.Dispose();
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }

        /// <summary>
        /// 等待指定的帧数后完成。
        /// </summary>
        /// <param name="timer">定时器管理器</param>
        /// <param name="frameCount">要等待的帧数（必须 &gt; 0）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>指定帧数到达时完成的任务</returns>
        public static Task WaitForFramesAsync(this ITimerManager timer, int frameCount, CancellationToken cancellationToken = default)
        {
            if (frameCount <= 0)
            {
                return Task.CompletedTask;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

            // Add(0, frameCount) — 每帧执行一次，frameCount 次后自动移除
            timer.Add(0, frameCount, _ =>
            {
                registration.Dispose();
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }
    }
}
