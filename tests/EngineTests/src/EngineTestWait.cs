using System;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试等待工具：轮询驱动 OperationSystem 的异步操作与句柄。
    /// 与 AssetSystemRuntimeVerifier 相同的 Task.Delay 轮询模式。
    /// </summary>
    public static class EngineTestWait
    {
        public static async Task OperationAsync(AsyncOperationBase operation, string step, int timeoutMilliseconds = 15000)
        {
            await UntilDoneAsync(() => operation.IsDone, step, timeoutMilliseconds);
            if (operation.Status != EOperationStatus.Succeed)
            {
                throw new EngineTestFailureException(step + " 失败：" + operation.Error);
            }
        }

        public static async Task HandleAsync(HandleBase handle, string step, int timeoutMilliseconds = 15000)
        {
            await UntilDoneAsync(() => handle.IsDone, step, timeoutMilliseconds);
            if (handle.Status != EOperationStatus.Succeed)
            {
                throw new EngineTestFailureException(step + " 失败：" + handle.LastError);
            }
        }

        public static async Task HandleFailureAsync(HandleBase handle, string step, int timeoutMilliseconds = 15000)
        {
            await UntilDoneAsync(() => handle.IsDone, step, timeoutMilliseconds);
            if (handle.Status == EOperationStatus.Succeed)
            {
                throw new EngineTestFailureException(step + " 期望失败却成功了（假成功回归）");
            }
        }

        public static async Task UntilDoneAsync(Func<bool> done, string step, int timeoutMilliseconds)
        {
            var start = Environment.TickCount;
            while (done() == false)
            {
                if (Environment.TickCount - start > timeoutMilliseconds)
                {
                    throw new EngineTestFailureException(step + " 等待超时(" + timeoutMilliseconds + "ms)");
                }

                await Task.Delay(1);
            }
        }

        public static async Task FramesAsync(int frameCount)
        {
            for (var i = 0; i < frameCount; i++)
            {
                await Task.Delay(16);
            }
        }
    }
}
