using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Runtime;

namespace GameFrameX.Web.Runtime
{
    public partial class WebManager
    {
        /// <summary>
        /// Web Binary请求数据类，用于处理Binary格式的Web请求
        /// </summary>
        private sealed class WebBinaryData : WebData
        {
            /// <summary>
            /// 获取请求任务的完成源，用于异步操作的控制和结果返回
            /// </summary>
            public readonly TaskCompletionSource<WebBufferResult> Task;

            /// <summary>
            /// 获取要发送的Protocol Buffer序列化后的字节数组数据
            /// </summary>
            public readonly byte[] SendData;

            /// <summary>
            /// 获取请求头信息
            /// </summary>
            public readonly Dictionary<string, string> Header;

            /// <summary>
            /// 初始化Web Binary请求数据
            /// </summary>
            /// <param name="url">请求URL</param>
            /// <param name="header">请求头信息</param>
            /// <param name="sendData">要发送的Binary序列化数据</param>
            /// <param name="task">请求任务的完成源</param>
            /// <param name="userData">用户自定义数据</param>
            public WebBinaryData(string url, Dictionary<string, string> header, byte[] sendData, TaskCompletionSource<WebBufferResult> task, object userData) : base(false, url, userData)
            {
                task.CheckNull(nameof(task));
                SendData = sendData;
                Task = task;
                Header = header;
            }
        }
    }
}