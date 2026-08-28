using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Startup.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// URL 故障转移执行器测试（迁移自 Unity com.gameframex.unity.startup Tests/Runtime/UrlFailoverRunnerTests.cs）。
    /// UniTask 协程壳改为 xunit 的 async Task。
    /// </summary>
    public sealed class UrlFailoverRunnerTests
    {
        [Fact]
        public async Task ExecuteAsync_SingleUrlSucceeds_ReturnsSuccessImmediately()
        {
            var attempts = 0;

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a" },
                3,
                0,
                url =>
                {
                    attempts++;
                    return Task.FromResult(UrlAttemptResult.Succeed());
                });

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.FailedUrl);
            Assert.Equal(string.Empty, result.ErrorMessage);
            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task ExecuteAsync_RetriesCurrentUrlBeforeSuccess_UsesOneBasedProgress()
        {
            var attempts = 0;
            var progress = new List<int>();

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a" },
                3,
                0,
                url =>
                {
                    attempts++;
                    return Task.FromResult(attempts == 3
                        ? UrlAttemptResult.Succeed()
                        : UrlAttemptResult.Fail("temporary"));
                },
                (url, attempt, total) => progress.Add(attempt));

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, progress);
            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task ExecuteAsync_FirstUrlFailsSecondSucceeds_FailoversInOrder()
        {
            var attemptsByUrl = new Dictionary<string, int>();

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a", "http://b" },
                2,
                0,
                url =>
                {
                    attemptsByUrl.TryGetValue(url, out var count);
                    attemptsByUrl[url] = count + 1;

                    return Task.FromResult(url == "http://b"
                        ? UrlAttemptResult.Succeed()
                        : UrlAttemptResult.Fail("down"));
                });

            Assert.True(result.Success);
            Assert.Equal(2, attemptsByUrl["http://a"]);
            Assert.Equal(1, attemptsByUrl["http://b"]);
        }

        [Fact]
        public async Task ExecuteAsync_AllUrlsFail_ReturnsLastFailureWithoutThrowing()
        {
            var attempts = 0;
            var progressCalls = 0;

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a", "http://b" },
                3,
                0,
                url =>
                {
                    attempts++;
                    return Task.FromResult(UrlAttemptResult.Fail("failed " + url));
                },
                (url, attempt, total) => progressCalls++);

            Assert.False(result.Success);
            Assert.Equal("http://b", result.FailedUrl);
            Assert.Equal("failed http://b", result.ErrorMessage);
            Assert.Equal(6, attempts);
            Assert.Equal(6, progressCalls);
        }

        [Fact]
        public void ExecuteAsync_InvalidArgs_ThrowSynchronously()
        {
            // 参数校验在非 async 入口方法中同步抛出；语句体 lambda 丢弃返回的 Task，
            // 保持 Assert.Throws 的同步语义（与 Unity 版测试一致）。
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = UrlFailoverRunner.ExecuteAsync(null, 1, 0, url => Task.FromResult(UrlAttemptResult.Succeed()));
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = UrlFailoverRunner.ExecuteAsync(new string[0], 1, 0, url => Task.FromResult(UrlAttemptResult.Succeed()));
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = UrlFailoverRunner.ExecuteAsync(new[] { "http://a" }, 0, 0, url => Task.FromResult(UrlAttemptResult.Succeed()));
            });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = UrlFailoverRunner.ExecuteAsync(new[] { "http://a" }, 1, -1, url => Task.FromResult(UrlAttemptResult.Succeed()));
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = UrlFailoverRunner.ExecuteAsync(new[] { "http://a" }, 1, 0, null);
            });
        }

        [Fact]
        public async Task ExecuteAsync_MaxAttemptsOne_DoesNotDelayOrRetry()
        {
            var attempts = 0;

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a" },
                1,
                1000,
                url =>
                {
                    attempts++;
                    return Task.FromResult(UrlAttemptResult.Fail("once"));
                });

            Assert.False(result.Success);
            Assert.Equal(1, attempts);
        }
    }
}
