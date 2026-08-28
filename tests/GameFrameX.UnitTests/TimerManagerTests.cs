using System;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Timer.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 定时器管理器行为测试（迁移自 Unity com.gameframex.unity.timer Tests/UnitTests.cs）。
    /// Godot 侧 GameFrameworkModule.Update/Shutdown 为 public，直接步进驱动，无需反射。
    /// 注：Update 的处理顺序为「遍历 _items → 处理 _toRemove → 最后把 _toAdd 刷入 _items」，
    /// 因此新 Add 的定时器从下一次 Update 开始计时，相关用例的步进按此时序修正。
    /// </summary>
    public sealed class TimerManagerTests : IDisposable
    {
        private readonly TimerManager _timerManager;

        public TimerManagerTests()
        {
            _timerManager = new TimerManager();
        }

        public void Dispose()
        {
            _timerManager.Shutdown();
        }

        // ──────────────── Add / AddOnce / AddUpdate ────────────────

        [Fact]
        public void Add_IntervalTimer_FiresAfterInterval()
        {
            bool called = false;
            var callback = new Action<object>(_ => called = true);

            _timerManager.Add(1f, 1, callback);
            _timerManager.Update(0f, 0.5f); // 刷入 _items
            Assert.False(called, "Should not fire before interval elapses");

            _timerManager.Update(0f, 0.6f); // 累计 0.6s，不足 1s
            Assert.False(called, "Should not fire at 0.6s");

            _timerManager.Update(0f, 0.6f); // 累计 1.2s
            Assert.True(called, "Should fire after interval elapses");
        }

        [Fact]
        public void AddOnce_FiresOnceThenAutoRemoves()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.AddOnce(1f, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount);

            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount); // 自动移除后不再触发
        }

        [Fact]
        public void AddUpdate_FiresEveryFrame()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.AddUpdate(callback);

            _timerManager.Update(0f, 0.016f); // 刷入 _items
            _timerManager.Update(0f, 0.016f);
            _timerManager.Update(0f, 0.016f);
            _timerManager.Update(0f, 0.016f);

            Assert.Equal(3, callCount);
        }

        [Fact]
        public void AddUpdate_WithParam_PassesParamToCallback()
        {
            object received = null;
            var expected = new object();
            var callback = new Action<object>(p => received = p);

            _timerManager.AddUpdate(callback, expected);
            _timerManager.Update(0f, 0.016f); // 刷入 _items
            _timerManager.Update(0f, 0.016f);

            Assert.Same(expected, received);
        }

        [Fact]
        public void AddUpdate_FiresOnZeroDeltaFrame()
        {
            // Godot 版新增：Interval<=0 走专用每帧路径（不再用 0.001f hack），与时间累积无关
            int callCount = 0;
            _timerManager.AddUpdate(_ => callCount++);

            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 0f); // delta 为 0 也应触发

            Assert.Equal(1, callCount);
        }

        // ──────────────── Remove ────────────────

        [Fact]
        public void Remove_StopsTimerFromFiring()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.Add(1f, 0, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount);

            _timerManager.Remove(callback);

            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Remove_WhileTimerInToAdd_DropsWithoutAdding()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            // Add 后、Update 刷入前立刻 Remove
            _timerManager.Add(1f, 1, callback);
            _timerManager.Remove(callback);
            _timerManager.Update(0f, 2f);

            Assert.Equal(0, callCount);
            Assert.False(_timerManager.Exists(callback));
        }

        // ──────────────── Exists ────────────────

        [Fact]
        public void Exists_ReturnsFalse_BeforeAdd()
        {
            Assert.False(_timerManager.Exists(_ => { }));
        }

        [Fact]
        public void Exists_ReturnsTrue_AfterAdd()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 1, callback);
            Assert.True(_timerManager.Exists(callback));
        }

        [Fact]
        public void Exists_ReturnsFalse_AfterTimerExpires()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.AddOnce(1f, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 2f);
            Assert.False(_timerManager.Exists(callback));
        }

        [Fact]
        public void Exists_ReturnsFalse_AfterRemove()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback);
            _timerManager.Remove(callback);
            Assert.False(_timerManager.Exists(callback));
        }

        // ──────────────── Repeat count ────────────────

        [Fact]
        public void RepeatCount_TimerFiresSpecifiedNumberOfTimes()
        {
            int callCount = 0;
            int repeat = 3;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.Add(0.5f, repeat, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items

            for (int i = 1; i <= 5; i++)
            {
                _timerManager.Update(0f, 0.6f);

                if (i < repeat)
                {
                    Assert.Equal(i, callCount);
                    Assert.True(_timerManager.Exists(callback));
                }
                else
                {
                    Assert.Equal(repeat, callCount);
                    Assert.False(_timerManager.Exists(callback));
                    break;
                }
            }
        }

        [Fact]
        public void RepeatCount_Zero_MeansInfinite()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.Add(0.5f, 0, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items
            Assert.True(_timerManager.Exists(callback));

            for (int i = 0; i < 10; i++)
            {
                _timerManager.Update(0f, 0.6f);
            }

            Assert.Equal(10, callCount);
            Assert.True(_timerManager.Exists(callback));
        }

        // ──────────────── Re-add same callback ────────────────

        [Fact]
        public void ReAddSameCallback_UpdatesIntervalAndParam_InPlace()
        {
            string received = null;
            var callback = new Action<object>(p => received = (string)p);

            _timerManager.Add(1f, 1, callback, "first");
            // 第一次 Update 前重复添加：应原位更新 _toAdd 中的条目
            _timerManager.Add(2f, 0, callback, "updated");

            _timerManager.Update(0f, 1.5f); // 刷入 _items（不计时）
            Assert.Null(received);

            _timerManager.Update(0f, 1.5f); // 累计 1.5s < 2s：间隔已更新为 2s，不应触发
            Assert.Null(received);

            _timerManager.Update(0f, 1.5f); // 累计 3.0s > 2s：触发并携带更新后的参数
            Assert.Equal("updated", received);
        }

        [Fact]
        public void ReAddSameCallback_AfterFirstFire_ResetsTimer()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.AddOnce(1f, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount);
            Assert.False(_timerManager.Exists(callback));

            // 过期后用同一回调重新添加：应像新定时器一样工作
            _timerManager.AddOnce(1f, callback);
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);
            Assert.Equal(2, callCount);
        }

        // ──────────────── Callback param ────────────────

        [Fact]
        public void Param_IsPassedCorrectly()
        {
            object received = null;
            var expected = new object();
            _timerManager.AddOnce(1f, p => received = p, expected);

            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);

            Assert.Same(expected, received);
        }

        [Fact]
        public void Param_WhenNull_DoesNotThrow()
        {
            bool called = false;
            _timerManager.AddOnce(1f, _ => called = true);

            _timerManager.Update(0f, 0f); // 刷入 _items
            var exception = Record.Exception(() => _timerManager.Update(0f, 1.5f));

            Assert.Null(exception);
            Assert.True(called);
        }

        // ──────────────── CatchCallbackExceptions ────────────────

        [Fact]
        public void CatchCallbackExceptions_On_DoesNotThrow_OnException()
        {
            var original = TimerManager.CatchCallbackExceptions;
            TimerManager.CatchCallbackExceptions = true;
            try
            {
                _timerManager.AddOnce(1f, _ => throw new InvalidOperationException("expected test error"));
                _timerManager.Update(0f, 0f); // 刷入 _items
                var exception = Record.Exception(() => _timerManager.Update(0f, 1.5f));

                Assert.Null(exception);
            }
            finally
            {
                TimerManager.CatchCallbackExceptions = original;
            }
        }

        [Fact]
        public void CatchCallbackExceptions_On_DoesNotRemoveTimer()
        {
            // 行为变更点（对齐 Unity 1.3.1）：开启捕获时仅告警，不删除定时器
            int callCount = 0;
            Action<object> callback = _ =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("expected test error");
                }
            };
            var original = TimerManager.CatchCallbackExceptions;
            TimerManager.CatchCallbackExceptions = true;
            try
            {
                _timerManager.Add(1f, 2, callback);
                _timerManager.Update(0f, 0f); // 刷入 _items

                _timerManager.Update(0f, 1.5f); // 第 1 次：抛异常被吞，定时器保留
                Assert.Equal(1, callCount);
                Assert.True(_timerManager.Exists(callback));

                _timerManager.Update(0f, 1.5f); // 第 2 次：定时器仍在并正常触发
                Assert.Equal(2, callCount);
            }
            finally
            {
                TimerManager.CatchCallbackExceptions = original;
            }
        }

        [Fact]
        public void CatchCallbackExceptions_Off_ThrowsToCaller()
        {
            var original = TimerManager.CatchCallbackExceptions;
            TimerManager.CatchCallbackExceptions = false;
            try
            {
                _timerManager.AddOnce(1f, _ => throw new InvalidOperationException("expected test error"));
                _timerManager.Update(0f, 0f); // 刷入 _items

                // Godot 侧直接调用 Update，异常不经反射包装，原样抛出
                Assert.Throws<InvalidOperationException>(() => _timerManager.Update(0f, 1.5f));
            }
            finally
            {
                TimerManager.CatchCallbackExceptions = original;
            }
        }

        // ──────────────── Object pool ────────────────

        [Fact]
        public void ObjectPool_ReusesTimerItem_AfterTimerExpires()
        {
            // 反复添加并触发一次性定时器 — 每个都回收复用 TimerItem
            for (int i = 0; i < 5; i++)
            {
                bool called = false;
                var callback = new Action<object>(_ => called = true);
                _timerManager.AddOnce(0.1f, callback);
                _timerManager.Update(0f, 0f); // 刷入 _items
                _timerManager.Update(0f, 0.2f);
                Assert.True(called);
            }

            // 无崩溃、无脏状态 — 池正确回收
        }

        // ──────────────── Shutdown ────────────────

        [Fact]
        public void Shutdown_ClearsAllTimers()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback);
            _timerManager.AddOnce(2f, _ => { });
            Assert.True(_timerManager.Exists(callback));

            _timerManager.Shutdown();
            Assert.False(_timerManager.Exists(callback));
        }

        // ──────────────── Multiple timers ────────────────

        [Fact]
        public void MultipleTimers_AllFireIndependently()
        {
            int countA = 0, countB = 0, countC = 0;
            var cbA = new Action<object>(_ => countA++);
            var cbB = new Action<object>(_ => countB++);
            var cbC = new Action<object>(_ => countC++);

            _timerManager.AddUpdate(cbA);     // 每帧
            _timerManager.Add(0.5f, 4, cbB);  // 4 次，0.5s 间隔
            _timerManager.AddOnce(1.5f, cbC); // 1.5s 一次

            _timerManager.Update(0f, 0f); // 刷入 _items

            _timerManager.Update(0f, 0.016f); // 帧 1
            Assert.Equal(1, countA);

            _timerManager.Update(0f, 0.016f); // 帧 2：cbB 累计 0.032s
            Assert.Equal(2, countA);

            _timerManager.Update(0f, 0.6f); // 帧 3：cbB 首次触发（0.632s）
            Assert.Equal(1, countB);
            Assert.Equal(0, countC);

            _timerManager.Update(0f, 0.016f); // 帧 4
            Assert.Equal(1, countB);

            _timerManager.Update(0f, 0.6f); // 帧 5：cbB 第 2 次
            Assert.Equal(2, countB);

            _timerManager.Update(0f, 0.6f); // 帧 6：cbB 第 3 次 + cbC 触发（1.848s）
            Assert.Equal(3, countB);
            Assert.Equal(1, countC);
            Assert.False(_timerManager.Exists(cbC));

            _timerManager.Update(0f, 0.6f); // 帧 7：cbB 第 4 次（最后一次）
            Assert.Equal(4, countB);
            Assert.False(_timerManager.Exists(cbB));

            // cbA 每帧照常触发
            Assert.True(countA >= 4);
        }

        // ──────────────── ID system ────────────────

        [Fact]
        public void Add_ReturnsPositiveId()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            Assert.True(id > 0);
        }

        [Fact]
        public void Add_EachCallReturnsUniqueId()
        {
            int id1 = _timerManager.Add(1f, 1, _ => { });
            int id2 = _timerManager.Add(1f, 1, _ => { });
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void Exists_ById_ReturnsTrue_AfterAdd()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            Assert.True(_timerManager.Exists(id));
        }

        [Fact]
        public void Exists_ById_ReturnsFalse_AfterRemove()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Remove(id);
            Assert.False(_timerManager.Exists(id));
        }

        [Fact]
        public void Exists_ById_ReturnsFalse_ForUnknownId()
        {
            Assert.False(_timerManager.Exists(99999));
        }

        [Fact]
        public void Remove_ById_StopsTimer()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 0, _ => callCount++);
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount);

            _timerManager.Remove(id);
            _timerManager.Update(0f, 1.5f);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Remove_ById_AlsoCleansCallbackLookup()
        {
            var callback = new Action<object>(_ => { });
            int id = _timerManager.Add(1f, 1, callback);
            _timerManager.Remove(id);
            Assert.False(_timerManager.Exists(callback));
        }

        [Fact]
        public void AddOnce_ReturnsPositiveId()
        {
            int id = _timerManager.AddOnce(1f, _ => { });
            Assert.True(id > 0);
        }

        [Fact]
        public void AddUpdate_ReturnsPositiveId()
        {
            int id = _timerManager.AddUpdate(_ => { });
            Assert.True(id > 0);
        }

        // ──────────────── Pause / Resume ────────────────

        [Fact]
        public void Pause_PreventsTimerFromFiring()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 0, _ => callCount++);

            _timerManager.Update(0f, 0.5f); // 刷入 _items
            _timerManager.Pause(id);

            // 即使时间足够，暂停中的定时器也不触发
            _timerManager.Update(0f, 2f);
            Assert.Equal(0, callCount);
        }

        [Fact]
        public void PauseThenResume_TimerContinuesFromWhereItStopped()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 2, _ => callCount++);

            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 0.6f); // 累计 0.6s（不足以触发）
            _timerManager.Pause(id);

            _timerManager.Update(0f, 2f); // 暂停期间不累积
            Assert.Equal(0, callCount);

            _timerManager.Resume(id);
            _timerManager.Update(0f, 0.5f); // 补足剩余 0.4s
            Assert.Equal(1, callCount);
        }

        // ──────────────── Tag system ────────────────

        [Fact]
        public void Tag_HasTag_ReturnsTrueAfterAdd()
        {
            _timerManager.Add(1f, 1, _ => { }, tag: "test-tag");
            Assert.True(_timerManager.HasTag("test-tag"));
        }

        [Fact]
        public void Tag_HasTag_ReturnsFalse_ForUnknownTag()
        {
            Assert.False(_timerManager.HasTag("nonexistent"));
        }

        [Fact]
        public void Tag_PauseByTag_PausesAllTaggedTimers()
        {
            int countA = 0, countB = 0;
            _timerManager.Add(1f, 0, _ => countA++, tag: "group1");
            _timerManager.Add(1f, 0, _ => countB++, tag: "group1");

            _timerManager.Update(0f, 0.5f); // 刷入 _items
            _timerManager.PauseByTag("group1");

            _timerManager.Update(0f, 2f);
            Assert.Equal(0, countA);
            Assert.Equal(0, countB);
        }

        [Fact]
        public void Tag_ResumeByTag_ResumesAllTaggedTimers()
        {
            int callCount = 0;
            _timerManager.Add(1f, 0, _ => callCount++, tag: "group1");

            _timerManager.Update(0f, 0.5f); // 刷入 _items
            _timerManager.PauseByTag("group1");
            _timerManager.Update(0f, 2f); // 暂停不触发
            Assert.Equal(0, callCount);

            _timerManager.ResumeByTag("group1");
            _timerManager.Update(0f, 1.5f); // flush 帧不计时，恢复后从头累积 1.5s ≥ 1s → 触发
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Tag_RemoveByTag_RemovesAllTaggedTimers()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback, tag: "group1");
            Assert.True(_timerManager.Exists(callback));

            _timerManager.RemoveByTag("group1");
            Assert.False(_timerManager.Exists(callback));
            Assert.False(_timerManager.HasTag("group1"));
        }

        [Fact]
        public void Tag_UntaggedTimers_NotAffectedByTagOperations()
        {
            int untaggedCount = 0, taggedCount = 0;
            _timerManager.Add(1f, 0, _ => untaggedCount++); // 无标签
            _timerManager.Add(1f, 0, _ => taggedCount++, tag: "group1");

            _timerManager.Update(0f, 0.5f); // 刷入 _items
            _timerManager.PauseByTag("group1");

            _timerManager.Update(0f, 1.5f);
            Assert.True(untaggedCount > 0);
            Assert.Equal(0, taggedCount);
        }

        [Fact]
        public void ReAdd_WithDifferentTag_UpdatesTagIndex()
        {
            // 用同一回调引用重复添加（Unity 原用例误用了两个不同 lambda 实例，此处修正）
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 1, callback, tag: "old-tag");
            Assert.True(_timerManager.HasTag("old-tag"));

            _timerManager.Add(1f, 1, callback, tag: "new-tag");
            Assert.False(_timerManager.HasTag("old-tag"));
            Assert.True(_timerManager.HasTag("new-tag"));
        }

        // ──────────────── Time scale mode ────────────────

        [Fact]
        public void TimeScale_Scaled_UsesElapseSeconds()
        {
            int callCount = 0;
            // Scaled 定时器：使用 elapseSeconds（Update 第一参数）
            _timerManager.Add(1f, 1, _ => callCount++, timeScale: TimerTimeScale.Scaled);

            _timerManager.Update(0.5f, 0.016f); // 刷入 _items
            Assert.Equal(0, callCount);

            _timerManager.Update(0.6f, 0.016f); // scaled 累计 0.6s < 1s
            Assert.Equal(0, callCount);

            _timerManager.Update(0.6f, 0.016f); // scaled 累计 1.2s → 触发
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void TimeScale_DefaultIsUnscaled_UsesRealElapseSeconds()
        {
            int callCount = 0;
            // 默认 Unscaled：使用 realElapseSeconds（Update 第二参数）
            _timerManager.Add(1f, 1, _ => callCount++);

            _timerManager.Update(0f, 0.6f); // 刷入 _items
            Assert.Equal(0, callCount);

            _timerManager.Update(0f, 0.5f); // real 累计 0.5s < 1s
            Assert.Equal(0, callCount);

            _timerManager.Update(0f, 0.6f); // real 累计 1.1s → 触发
            Assert.Equal(1, callCount);
        }

        // ──────────────── Query API ────────────────

        [Fact]
        public void GetRemaining_ReturnsCorrectValue()
        {
            int id = _timerManager.Add(2f, 1, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 0.5f);

            float remaining = _timerManager.GetRemaining(id);
            Assert.InRange(remaining, 0f, 1.5f);
        }

        [Fact]
        public void GetRemaining_ReturnsNegative_ForUnknownId()
        {
            Assert.Equal(-1f, _timerManager.GetRemaining(99999));
        }

        [Fact]
        public void GetRemaining_ReturnsNegative_BeforeFlushed()
        {
            // 尚未刷入 _items 的定时器：Query 返回 -1（对齐 Unity 实现）
            int id = _timerManager.Add(2f, 1, _ => { });
            Assert.Equal(-1f, _timerManager.GetRemaining(id));
        }

        [Fact]
        public void GetElapsed_ReturnsNegative_ForUnknownId()
        {
            Assert.Equal(-1f, _timerManager.GetElapsed(99999));
        }

        [Fact]
        public void GetRepeatLeft_ReturnsCorrectValue()
        {
            int id = _timerManager.Add(1f, 5, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            Assert.Equal(5, _timerManager.GetRepeatLeft(id));

            _timerManager.Update(0f, 1.5f);
            Assert.Equal(4, _timerManager.GetRepeatLeft(id));
        }

        [Fact]
        public void GetRepeatLeft_ReturnsNegative_ForUnknownId()
        {
            Assert.Equal(-1, _timerManager.GetRepeatLeft(99999));
        }

        // ──────────────── OnComplete ────────────────

        [Fact]
        public void OnComplete_Fires_WhenTimerNaturallyCompletes()
        {
            bool completed = false;
            _timerManager.Add(1f, 3, _ => { }, onComplete: () => completed = true);

            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f); // 第 1 次
            Assert.False(completed);

            _timerManager.Update(0f, 1.5f); // 第 2 次
            Assert.False(completed);

            _timerManager.Update(0f, 1.5f); // 第 3 次 → 完成
            Assert.True(completed);
        }

        [Fact]
        public void OnComplete_Fires_WhenTimerRemovedByCallback()
        {
            bool completed = false;
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback, onComplete: () => completed = true);

            _timerManager.Remove(callback);
            Assert.True(completed);
        }

        [Fact]
        public void OnComplete_DoesNotFire_ForInfiniteRepeatTimer()
        {
            bool completed = false;
            _timerManager.Add(1f, 0, _ => { }, onComplete: () => completed = true);

            _timerManager.Update(0f, 0f); // 刷入 _items
            for (int i = 0; i < 10; i++)
            {
                _timerManager.Update(0f, 1.5f);
            }

            Assert.False(completed);
        }

        // ──────────────── IsPaused ────────────────

        [Fact]
        public void IsPaused_ReturnsFalse_ForActiveTimer()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            Assert.False(_timerManager.IsPaused(id));
        }

        [Fact]
        public void IsPaused_ReturnsTrue_AfterPause()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Pause(id);
            Assert.True(_timerManager.IsPaused(id));
        }

        [Fact]
        public void IsPaused_ReturnsFalse_AfterResume()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Pause(id);
            _timerManager.Resume(id);
            Assert.False(_timerManager.IsPaused(id));
        }

        [Fact]
        public void IsPaused_ReturnsFalse_ForUnknownId()
        {
            Assert.False(_timerManager.IsPaused(99999));
        }

        // ──────────────── Edge cases ────────────────

        [Fact]
        public void Remove_UnknownId_DoesNotThrow()
        {
            var exception = Record.Exception(() => _timerManager.Remove(99999));
            Assert.Null(exception);
        }

        [Fact]
        public void Pause_AfterTimerExpired_IsNoOp()
        {
            int id = _timerManager.AddOnce(0.1f, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1f); // 触发并移除
            Assert.False(_timerManager.Exists(id));

            var exception = Record.Exception(() => _timerManager.Pause(id));
            Assert.Null(exception);
        }

        [Fact]
        public void Resume_AfterTimerExpired_IsNoOp()
        {
            int id = _timerManager.AddOnce(0.1f, _ => { });
            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1f);

            var exception = Record.Exception(() => _timerManager.Resume(id));
            Assert.Null(exception);
        }

        [Fact]
        public void NegativeInterval_TreatedAsPerFrame()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);
            _timerManager.Add(-1f, 3, callback);

            _timerManager.Update(0f, 0.016f); // 刷入 _items
            Assert.Equal(0, callCount);

            _timerManager.Update(0f, 0.016f); // 第 1 帧
            Assert.Equal(1, callCount);

            _timerManager.Update(0f, 0.016f); // 第 2 帧
            Assert.Equal(2, callCount);

            _timerManager.Update(0f, 0.016f); // 第 3 帧后停止（Unity 原用例 Exists 误传了新 lambda，此处用回调引用修正）
            Assert.Equal(3, callCount);
            Assert.False(_timerManager.Exists(callback));
        }

        [Fact]
        public void GetRemaining_ForInfiniteRepeat_ReturnsZero()
        {
            int id = _timerManager.Add(1f, 0, _ => { }); // 无限
            _timerManager.Update(0f, 0f); // 刷入 _items
            Assert.Equal(0f, _timerManager.GetRemaining(id));
        }

        [Fact]
        public void GetRemaining_ForPerFrameTimer_ReturnsZero()
        {
            int id = _timerManager.Add(0f, 1, _ => { }); // 每帧、一次
            _timerManager.Update(0f, 0f); // 刷入 _items
            Assert.Equal(0f, _timerManager.GetRemaining(id));
        }

        // ──────────────── Async extensions ────────────────

        [Fact]
        public async Task WaitForSecondsAsync_CompletesAfterInterval()
        {
            var task = _timerManager.WaitForSecondsAsync(1f);
            Assert.False(task.IsCompleted);

            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 1.5f);

            await task;
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WaitForNextFrameAsync_CompletesOnNextUpdate()
        {
            var task = _timerManager.WaitForNextFrameAsync();
            Assert.False(task.IsCompleted);

            _timerManager.Update(0f, 0f); // 刷入 _items
            _timerManager.Update(0f, 0.016f);

            await task;
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task WaitForFramesAsync_CompletesOnFirstFrame()
        {
            // 对齐 Unity 1.3.1 实现：回调无条件 TrySetResult，Add(0, frameCount) 首次触发即完成任务，
            // frameCount 只决定内部 timer 的存活帧数（上游怪癖，与其名义语义不符，如实迁移）
            var task = _timerManager.WaitForFramesAsync(3);
            Assert.False(task.IsCompleted);

            _timerManager.Update(0f, 0f); // 刷入 _items
            Assert.False(task.IsCompleted);

            _timerManager.Update(0f, 0.016f); // 帧 1：回调首次触发即完成
            await task;
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Fact]
        public void WaitForSecondsAsync_PreCanceledToken_ReturnsCanceledTask()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task = _timerManager.WaitForSecondsAsync(1f, cts.Token);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void WaitForFramesAsync_ZeroFrames_ReturnsCompletedTask()
        {
            var task = _timerManager.WaitForFramesAsync(0);
            Assert.Same(Task.CompletedTask, task);
        }
    }
}
