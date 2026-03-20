using System;
using System.Collections.Generic;

namespace GameFrameX.Asset.Runtime
{
    [UnityEngine.Scripting.Preserve]
    public sealed class OperationScheduler
    {
        private readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>();
        private long _nextEnqueueOrder;

        public long TickCount { get; private set; }
        public double LastDeltaSeconds { get; private set; }
        public double MaxTimeSliceMilliseconds { get; set; } = 30d;
        public int OperationCount => _operations.Count;

        [UnityEngine.Scripting.Preserve]
        public void Tick(double deltaSeconds)
        {
            LastDeltaSeconds = deltaSeconds;
            TickCount++;
            if (_operations.Count == 0)
            {
                return;
            }

            _operations.Sort(CompareOperation);
            var remainingTimeSlice = MaxTimeSliceMilliseconds;
            for (var i = 0; i < _operations.Count; i++)
            {
                if (remainingTimeSlice <= 0d)
                {
                    break;
                }

                var operation = _operations[i];
                var consumed = operation.UpdateOperation(deltaSeconds, remainingTimeSlice);
                if (consumed > 0d)
                {
                    remainingTimeSlice -= consumed;
                }
            }

            _operations.RemoveAll(operation => operation.Status == OperationStatus.Succeed || operation.Status == OperationStatus.Failed);
        }

        public void AddOperation(AsyncOperationBase operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.Status == OperationStatus.Succeed || operation.Status == OperationStatus.Failed)
            {
                throw new InvalidOperationException("Completed operation cannot be scheduled.");
            }

            if (_operations.Contains(operation))
            {
                return;
            }

            operation.EnqueueOrder = _nextEnqueueOrder++;
            _operations.Add(operation);
        }

        private static int CompareOperation(AsyncOperationBase left, AsyncOperationBase right)
        {
            var priorityCompare = right.Priority.CompareTo(left.Priority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            return left.EnqueueOrder.CompareTo(right.EnqueueOrder);
        }
    }
}
