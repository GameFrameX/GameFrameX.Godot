using GameFrameX.Asset.Runtime;
using Xunit;

namespace GameFrameX.Asset.Tests.Unit;

public sealed class OperationSchedulerSmokeTests
{
    [Fact]
    public void Tick_ShouldRecordDeltaAndIncreaseTickCount()
    {
        var scheduler = new OperationScheduler();

        scheduler.Tick(0.16d);

        Assert.Equal(1L, scheduler.TickCount);
        Assert.Equal(0.16d, scheduler.LastDeltaSeconds, 3);
    }

    [Fact]
    public void GfAssetSystem_TickShouldDriveSchedulerAfterInitialize()
    {
        var scheduler = new OperationScheduler();
        var assetSystem = new GfAssetSystem();
        assetSystem.Initialize(scheduler);

        assetSystem.Tick(0.2d);

        Assert.True(assetSystem.IsInitialized);
        Assert.Equal(1L, scheduler.TickCount);
        Assert.Equal(0.2d, scheduler.LastDeltaSeconds, 3);
    }
}
