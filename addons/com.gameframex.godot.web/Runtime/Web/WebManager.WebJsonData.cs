using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFrameX.Web.Runtime
{
    public partial class WebManager
    {
        /// <summary>
        /// Web JSON请求数据类，用于处理JSON格式的Web请求
        /// </summary>
        private sealed class WebJsonData : WebData
        {
            /// <summary>
            /// 获取请求头信息
            /// </summary>
            public Dictionary<string, string> Header { get; }

            /// <summary>
            /// 获取表单数据
            /// </summary>
            public Dictionary<string, object> Form { get; }

            /// <summary>
            /// 字符串结果的任务完成源
            /// </summary>
            public readonly TaskCompletionSource<WebStringResult> UniTaskCompletionStringSource;

            /// <summary>
            /// 字节数组结果的任务完成源
            /// </summary>
            public readonly TaskCompletionSource<WebBufferResult> UniTaskCompletionBytesSource;

            /// <summary>
            /// 初始化Web JSON请求数据（用于字节数组结果的GET/POST请求）
            /// </summary>
            /// <param name="url">请求URL</param>
            /// <param name="header">请求头信息</param>
            /// <param name="isGet">是否为GET请求</param>
            /// <param name="source">字节数组结果的任务完成源</param>
            /// <param name="userData">用户自定义数据</param>
            public WebJsonData(string url, Dictionary<string, string> header, bool isGet, TaskCompletionSource<WebBufferResult> source, object userData = null) : base(isGet, url, userData)
            {
                Header = header;
                UniTaskCompletionBytesSource = source;
            }

            /// <summary>
            /// 初始化Web JSON请求数据（用于字符串结果的GET/POST请求）
            /// </summary>
            /// <param name="url">请求URL</param>
            /// <param name="header">请求头信息</param>
            /// <param name="isGet">是否为GET请求</param>
            /// <param name="source">字符串结果的任务完成源</param>
            /// <param name="userData">用户自定义数据</param>
            public WebJsonData(string url, Dictionary<string, string> header, bool isGet, TaskCompletionSource<WebStringResult> source, object userData = null) : base(isGet, url, userData)
            {
                Header = header;
                UniTaskCompletionStringSource = source;
            }

            /// <summary>
            /// 初始化Web JSON请求数据（用于带表单的字符串结果POST请求）
            /// </summary>
            /// <param name="url">请求URL</param>
            /// <param name="header">请求头信息</param>
            /// <param name="form">表单数据</param>
            /// <param name="source">字符串结果的任务完成源</param>
            /// <param name="userData">用户自定义数据</param>
            public WebJsonData(string url, Dictionary<string, string> header, Dictionary<string, object> form, TaskCompletionSource<WebStringResult> source, object userData = null) : base(false, url, userData)
            {
                Header = header;
                Form = form;
                UniTaskCompletionStringSource = source;
            }

            /// <summary>
            /// 初始化Web JSON请求数据（用于带表单的字节数组结果POST请求）
            /// </summary>
            /// <param name="url">请求URL</param>
            /// <param name="header">请求头信息</param>
            /// <param name="form">表单数据</param>
            /// <param name="source">字节数组结果的任务完成源</param>
            /// <param name="userData">用户自定义数据</param>
            public WebJsonData(string url, Dictionary<string, string> header, Dictionary<string, object> form, TaskCompletionSource<WebBufferResult> source, object userData = null) : base(false, url, userData)
            {
                Header = header;
                Form = form;
                UniTaskCompletionBytesSource = source;
            }

            /// <summary>
            /// 释放资源，取消未完成的任务
            /// </summary>
            public override void Dispose()
            {
                if (UniTaskCompletionStringSource != null)
                {
                    UniTaskCompletionStringSource.TrySetCanceled();
                }

                if (UniTaskCompletionBytesSource != null)
                {
                    UniTaskCompletionBytesSource.TrySetCanceled();
                }

                base.Dispose();
            }
        }
    }
}