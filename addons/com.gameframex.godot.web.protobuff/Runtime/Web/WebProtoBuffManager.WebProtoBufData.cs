using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;

namespace GameFrameX.Web.ProtoBuff.Runtime
{
    public partial class WebProtoBuffManager
    {
        private sealed class WebProtoBufData : WebManager.WebData
        {
            public readonly TaskCompletionSource<WebBufferResult> Task;
            public readonly byte[] SendData;

            public WebProtoBufData(string url, byte[] sendData, TaskCompletionSource<WebBufferResult> task, object userData) : base(false, url, userData)
            {
                task.CheckNull(nameof(task));
                SendData = sendData;
                Task = task;
            }
        }
    }
}
