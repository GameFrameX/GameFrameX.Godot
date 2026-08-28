using GameFrameX.AssetSystem.Networking;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// UnityWebRequest Godot 桩行为测试：HttpTransport 未配置（走 Web* 桩链）时必须显式失败，
    /// 不允许假成功（Phase 2.3：SendWebRequest 返回 ConnectionError）。
    /// 纯 C# 桩，无 Godot native 依赖。
    /// </summary>
    public sealed class AssetSystemWebRequestStubTests
    {
        [Fact]
        public void SendWebRequest_WithoutHttpTransport_ReturnsConnectionError()
        {
            var request = new UnityWebRequest("http://127.0.0.1/test", UnityWebRequest.kHttpVerbGET);

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            Assert.True(operation.isDone);
            Assert.True(request.isDone);
            Assert.Equal(UnityWebRequest.Result.ConnectionError, request.result);
            Assert.True(request.isNetworkError);
            Assert.False(request.isHttpError);
            Assert.NotNull(request.error);
            Assert.Contains("GodotHttpTransport is null", request.error);
        }
    }
}
