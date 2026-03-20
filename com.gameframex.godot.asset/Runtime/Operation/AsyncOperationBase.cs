using System;

namespace GameFrameX.Asset.Runtime
{
    [UnityEngine.Scripting.Preserve]
    public abstract class AsyncOperationBase
    {
        protected AsyncOperationBase(int priority = 0)
        {
            Priority = priority;
            Status = OperationStatus.None;
            Error = string.Empty;
        }

        public int Priority { get; }
        public OperationStatus Status { get; private set; }
        public string Error { get; private set; }
        internal long EnqueueOrder { get; set; }

        internal double UpdateOperation(double deltaSeconds, double remainingTimeSliceMilliseconds)
        {
            if (Status == OperationStatus.Succeed || Status == OperationStatus.Failed)
            {
                return 0d;
            }

            if (Status == OperationStatus.None)
            {
                Status = OperationStatus.Running;
                OnStart();
            }

            if (Status != OperationStatus.Running)
            {
                return 0d;
            }

            var consumed = OnUpdate(deltaSeconds, remainingTimeSliceMilliseconds);
            return Math.Max(0d, consumed);
        }

        protected virtual void OnStart()
        {
        }

        protected abstract double OnUpdate(double deltaSeconds, double remainingTimeSliceMilliseconds);

        protected void Succeed()
        {
            Status = OperationStatus.Succeed;
            Error = string.Empty;
        }

        protected void Fail(string error)
        {
            Status = OperationStatus.Failed;
            Error = error ?? string.Empty;
        }
    }
}
