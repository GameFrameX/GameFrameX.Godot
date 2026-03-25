using System.Collections.Generic;
using GameFrameX.Asset.Runtime;
using Xunit;

namespace GameFrameX.Asset.Tests.Unit;

public sealed class OperationInfrastructureTests
{
    [Fact]
    public void OperationStatus_ShouldFlowToSucceed()
    {
        var scheduler = new OperationScheduler();
        var operation = new StepOperation(priority: 0, runCountToSucceed: 2, consumedMilliseconds: 0.1d);
        scheduler.AddOperation(operation);

        scheduler.Tick(0.016d);
        Assert.Equal(OperationStatus.Running, operation.Status);

        scheduler.Tick(0.016d);
        Assert.Equal(OperationStatus.Succeed, operation.Status);
    }

    [Fact]
    public void OperationStatus_ShouldFlowToFailed()
    {
        var scheduler = new OperationScheduler();
        var operation = new FailedOperation(priority: 0, "network error");
        scheduler.AddOperation(operation);

        scheduler.Tick(0.016d);

        Assert.Equal(OperationStatus.Failed, operation.Status);
        Assert.Equal("network error", operation.Error);
    }

    [Fact]
    public void Tick_ShouldRespectTimeSlice()
    {
        var scheduler = new OperationScheduler
        {
            MaxTimeSliceMilliseconds = 5d
        };

        var first = new StepOperation(priority: 0, runCountToSucceed: 1, consumedMilliseconds: 6d);
        var second = new StepOperation(priority: 0, runCountToSucceed: 1, consumedMilliseconds: 1d);
        scheduler.AddOperation(first);
        scheduler.AddOperation(second);

        scheduler.Tick(0.016d);

        Assert.Equal(1, first.UpdateCount);
        Assert.Equal(0, second.UpdateCount);

        scheduler.Tick(0.016d);

        Assert.Equal(1, second.UpdateCount);
    }

    [Fact]
    public void Tick_ShouldRunHigherPriorityFirst()
    {
        var callOrder = new List<string>();
        var scheduler = new OperationScheduler();
        var low = new OrderedOnceOperation(priority: 1, name: "low", callOrder);
        var high = new OrderedOnceOperation(priority: 100, name: "high", callOrder);
        var middle = new OrderedOnceOperation(priority: 10, name: "middle", callOrder);
        scheduler.AddOperation(low);
        scheduler.AddOperation(high);
        scheduler.AddOperation(middle);

        scheduler.Tick(0.016d);

        Assert.Equal(new[] { "high", "middle", "low" }, callOrder);
    }

    private sealed class StepOperation : AsyncOperationBase
    {
        private readonly int _runCountToSucceed;
        private readonly double _consumedMilliseconds;
        private int _runCount;

        public StepOperation(int priority, int runCountToSucceed, double consumedMilliseconds) : base(priority)
        {
            _runCountToSucceed = runCountToSucceed;
            _consumedMilliseconds = consumedMilliseconds;
        }

        public int UpdateCount => _runCount;

        protected override double OnUpdate(double deltaSeconds, double remainingTimeSliceMilliseconds)
        {
            _runCount++;
            if (_runCount >= _runCountToSucceed)
            {
                Succeed();
            }

            return _consumedMilliseconds;
        }
    }

    private sealed class FailedOperation : AsyncOperationBase
    {
        private readonly string _error;

        public FailedOperation(int priority, string error) : base(priority)
        {
            _error = error;
        }

        protected override double OnUpdate(double deltaSeconds, double remainingTimeSliceMilliseconds)
        {
            Fail(_error);
            return 0d;
        }
    }

    private sealed class OrderedOnceOperation : AsyncOperationBase
    {
        private readonly string _name;
        private readonly List<string> _callOrder;

        public OrderedOnceOperation(int priority, string name, List<string> callOrder) : base(priority)
        {
            _name = name;
            _callOrder = callOrder;
        }

        protected override double OnUpdate(double deltaSeconds, double remainingTimeSliceMilliseconds)
        {
            _callOrder.Add(_name);
            Succeed();
            return 0d;
        }
    }
}
