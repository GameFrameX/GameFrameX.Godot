using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Web.Runtime
{
    /// <summary>
    /// Web请求管理器的ProtoBuf部分实现
    /// </summary>
    public partial class WebManager : GameFrameworkModule, IWebManager
    {
        /// <summary>
        /// 等待处理的Binary请求队列
        /// </summary>
        private readonly Queue<WebBinaryData> m_WaitingBinaryQueue = new Queue<WebBinaryData>(256);

        /// <summary>
        /// 正在处理的Binary请求列表
        /// </summary>
        private readonly List<WebBinaryData> m_SendingBinaryList = new List<WebBinaryData>(16);

        /// <summary>
        /// Binary内容类型常量
        /// </summary>
        private const string BinaryContentType = "application/octet-stream";

        /// <summary>
        /// 更新处理ProtoBuf请求队列
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位</param>
        void UpdateBinary(float elapseSeconds, float realElapseSeconds)
        {
            lock (m_StringBuilder)
            {
                if (m_SendingBinaryList.Count < MaxConnectionPerServer)
                {
                    if (m_WaitingBinaryQueue.Count > 0)
                    {
                        var webBinaryData = m_WaitingBinaryQueue.Dequeue();

                        MakeBinaryBytesRequest(webBinaryData);

                        m_SendingBinaryList.Add(webBinaryData);
                    }
                }
            }
        }

        /// <summary>
        /// 关闭ProtoBuf请求处理，清理资源
        /// </summary>
        private void ShutdownBinary()
        {
            while (m_WaitingBinaryQueue.Count > 0)
            {
                var webData = m_WaitingBinaryQueue.Dequeue();
                webData.Dispose();
            }

            m_WaitingBinaryQueue.Clear();
            while (m_SendingBinaryList.Count > 0)
            {
                var webData = m_SendingBinaryList[0];
                m_SendingBinaryList.RemoveAt(0);
                webData.Dispose();
            }

            m_SendingBinaryList.Clear();

            m_MemoryStream.Dispose();
        }

        /// <summary>
        /// 执行ProtoBuf字节请求
        /// </summary>
        /// <param name="webData">ProtoBuf请求数据</param>
        private async void MakeBinaryBytesRequest(WebBinaryData webData)
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(webData.URL);
                request.Method = webData.IsGet ? WebRequestMethods.Http.Get : WebRequestMethods.Http.Post;
                request.Timeout = (int)RequestTimeout.TotalMilliseconds; // 设置请求超时时间
                request.ContentType = BinaryContentType;
                byte[] postData = webData.SendData;
                request.ContentLength = postData.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(postData, 0, postData.Length);
                }

                if (webData.Header != null && webData.Header.Count > 0)
                {
                    foreach (var kv in webData.Header)
                    {
                        request.Headers[kv.Key] = kv.Value;
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        var transferEncoding = response.Headers.Get("Transfer-Encoding");
                        if (string.IsNullOrWhiteSpace(transferEncoding))
                        {
                            m_MemoryStream.SetLength(response.ContentLength);
                        }
                        else
                        {
                            m_MemoryStream.SetLength(0);
                        }

                        m_MemoryStream.Position = 0;
                        await responseStream.CopyToAsync(m_MemoryStream);
                        var resultData = m_MemoryStream.ToArray();
                        webData.Task.SetResult(new WebBufferResult(webData.UserData, resultData)); // 将流的内容复制到内存流中并转换为byte数组 
                    }
                }
            }
            catch (WebException e)
            {
                // 捕获超时异常
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    webData.Task.SetException(new TimeoutException(e.Message));
                    return;
                }

                webData.Task.SetException(e);
            }
            catch (IOException e)
            {
                webData.Task.SetException(e);
            }
            catch (Exception e)
            {
                webData.Task.SetException(e);
            }
            finally
            {
                m_SendingBinaryList.Remove(webData);
            }
        }


        /// <summary>
        /// 发送字节数组Post请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">要发送的字节数组数据</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        public Task<WebBufferResult> PostToBytes(string url, byte[] from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            var uniTaskCompletionSource = new TaskCompletionSource<WebBufferResult>();
            queryString = MergeQueryString(queryString);
            header = MergeHeader(header);
            url = UrlHandler(url, queryString);
            var webData = new WebBinaryData(url, header, from, uniTaskCompletionSource, userData);
            m_SendingBinaryList.Add(webData);
            MakeBinaryBytesRequest(webData);
            return uniTaskCompletionSource.Task;
        }
    }
}
