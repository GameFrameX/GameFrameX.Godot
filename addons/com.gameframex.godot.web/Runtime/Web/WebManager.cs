using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Web.Runtime
{
    /// <summary>
    /// Web请求管理器,实现HTTP GET和POST请求功能
    /// </summary>
    public partial class WebManager : GameFrameworkModule, IWebManager
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        // 用于构建URL的StringBuilder
        private readonly StringBuilder m_StringBuilder = new StringBuilder(256);

        // 等待处理的普通请求队列
        private readonly Queue<WebJsonData> m_WaitingNormalQueue = new Queue<WebJsonData>(256);

        // 正在处理的普通请求列表
        private readonly List<WebJsonData> m_SendingNormalList = new List<WebJsonData>(16);

        // 用于存储请求和响应数据的内存流
        private readonly MemoryStream m_MemoryStream;

        // JSON内容类型常量
        private const string JsonContentType = "application/json; charset=utf-8";

        // 超时时间(秒)
        private float m_Timeout = 5f;

        private readonly Dictionary<string, object> m_BaseForm = new Dictionary<string, object>(64);
        private readonly Dictionary<string, string> m_BaseHeader = new Dictionary<string, string>(64);
        private readonly Dictionary<string, string> m_BaseQueryString = new Dictionary<string, string>(64);

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebManager()
        {
            MaxConnectionPerServer = 8;
            m_MemoryStream = new MemoryStream();
            Timeout = 5f;
        }

        /// <summary>
        /// 获取或设置超时时间(秒)
        /// </summary>
        public float Timeout
        {
            get { return m_Timeout; }
            set
            {
                m_Timeout = value;
                RequestTimeout = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// 获取或设置每个服务器的最大连接数
        /// </summary>
        public int MaxConnectionPerServer { get; set; }

        /// <summary>
        /// 获取或设置请求超时时间
        /// </summary>
        public TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// 更新处理请求队列
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (m_StringBuilder)
            {
                if (m_SendingNormalList.Count < MaxConnectionPerServer)
                {
                    if (m_WaitingNormalQueue.Count > 0)
                    {
                        var webJsonData = m_WaitingNormalQueue.Dequeue();

                        if (webJsonData.UniTaskCompletionStringSource != null)
                        {
                            MakeJsonStringRequest(webJsonData);
                        }
                        else
                        {
                            MakeJsonBytesRequest(webJsonData);
                        }


                        m_SendingNormalList.Add(webJsonData);
                    }
                }

                UpdateBinary(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭时清理资源
        /// </summary>
        public override void Shutdown()
        {
            while (m_WaitingNormalQueue.Count > 0)
            {
                var webData = m_WaitingNormalQueue.Dequeue();
                webData.Dispose();
            }

            m_WaitingNormalQueue.Clear();
            while (m_SendingNormalList.Count > 0)
            {
                var webData = m_SendingNormalList[0];
                m_SendingNormalList.RemoveAt(0);
                webData.Dispose();
            }

            m_SendingNormalList.Clear();
            ShutdownBinary();
            m_MemoryStream.Dispose();
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebStringResult> GetToString(string url, object userData = null)
        {
            return GetToString(url, null, null, userData);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebBufferResult> GetToBytes(string url, object userData = null)
        {
            return GetToBytes(url, null, null, userData);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebStringResult> GetToString(string url, Dictionary<string, string> queryString, object userData = null)
        {
            return GetToString(url, queryString, null, userData);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebBufferResult> GetToBytes(string url, Dictionary<string, string> queryString, object userData = null)
        {
            return GetToBytes(url, queryString, null, userData);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebStringResult> GetToString(string url, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            var uniTaskCompletionSource = new TaskCompletionSource<WebStringResult>();
            queryString = MergeQueryString(queryString);
            header = MergeHeader(header);
            url = UrlHandler(url, queryString);

            WebJsonData webJsonData = new WebJsonData(url, header, true, uniTaskCompletionSource, userData);
            m_WaitingNormalQueue.Enqueue(webJsonData);
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 处理JSON字符串请求
        /// </summary>
        private async void MakeJsonStringRequest(WebJsonData webJsonData)
        {
#if ENABLE_GAMEFRAMEX_WEB_SEND_LOG
            Log.Debug($"Web Request: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)}");
#endif

            try
            {
                using var request = new HttpRequestMessage(webJsonData.IsGet ? HttpMethod.Get : HttpMethod.Post, webJsonData.URL);
                if (webJsonData.Form != null && webJsonData.Form.Count > 0)
                {
                    string body = GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form);
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                if (webJsonData.Header != null && webJsonData.Header.Count > 0)
                {
                    foreach (var kv in webJsonData.Header)
                    {
                        if (!request.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                        {
                            if (request.Content == null)
                            {
                                request.Content = new ByteArrayContent(Array.Empty<byte>());
                            }

                            request.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                        }
                    }
                }

                using var timeoutCts = new System.Threading.CancellationTokenSource(RequestTimeout);
                using (var response = await HttpClient.SendAsync(request, timeoutCts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                    Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {content}");
#endif
                    webJsonData.UniTaskCompletionStringSource.SetResult(new WebStringResult(webJsonData.UserData, content));
                }
            }
            catch (TaskCanceledException e)
            {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {e.Message}");
#endif
                webJsonData.UniTaskCompletionStringSource.SetException(new TimeoutException(e.Message));
            }
            catch (IOException e)
            {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {e.Message}");
#endif
                webJsonData.UniTaskCompletionStringSource.SetException(e);
            }
            catch (Exception e)
            {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {e.Message}");
#endif
                webJsonData.UniTaskCompletionStringSource.SetException(e);
            }
            finally
            {
                m_SendingNormalList.Remove(webJsonData);
            }
        }

        /// <summary>
        /// 处理JSON字节数组请求
        /// </summary>
        private async void MakeJsonBytesRequest(WebJsonData webJsonData)
        {
#if ENABLE_GAMEFRAMEX_WEB_SEND_LOG
            Log.Debug($"Web Request: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)}");
#endif

            try
            {
                using var request = new HttpRequestMessage(webJsonData.IsGet ? HttpMethod.Get : HttpMethod.Post, webJsonData.URL);
                if (webJsonData.Header != null && webJsonData.Header.Count > 0)
                {
                    foreach (var kv in webJsonData.Header)
                    {
                        if (!request.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                        {
                            if (request.Content == null)
                            {
                                request.Content = new ByteArrayContent(Array.Empty<byte>());
                            }

                            request.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                        }
                    }
                }

                if (webJsonData.Form != null && webJsonData.Form.Count > 0)
                {
                    string body = GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form);
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                using var timeoutCts = new System.Threading.CancellationTokenSource(RequestTimeout);
                using (var response = await HttpClient.SendAsync(request, timeoutCts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    var resultData = await response.Content.ReadAsByteArrayAsync();
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                    Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {resultData}");
#endif
                    webJsonData.UniTaskCompletionBytesSource.SetResult(new WebBufferResult(webJsonData.UserData, resultData));
                }
            }
            catch (TaskCanceledException e)
            {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {e.Message}");
#endif
                webJsonData.UniTaskCompletionBytesSource.SetException(new TimeoutException(e.Message));
            }
            catch (IOException e)
            {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {e.Message}");
#endif
                webJsonData.UniTaskCompletionBytesSource.SetException(e);
            }
            catch (Exception e)
            {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
                Log.Debug($"Web Response: {webJsonData.URL} \n Header: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Header)} \n  Form: {GameFrameX.Runtime.Utility.Json.ToJson(webJsonData.Form)} \n Content: {e.Message}");
#endif
                webJsonData.UniTaskCompletionBytesSource.SetException(e);
            }
            finally
            {
                m_SendingNormalList.Remove(webJsonData);
            }
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebBufferResult> GetToBytes(string url, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            var uniTaskCompletionSource = new TaskCompletionSource<WebBufferResult>();
            queryString = MergeQueryString(queryString);
            header = MergeHeader(header);
            url = UrlHandler(url, queryString);

            WebJsonData webJsonData = new WebJsonData(url, header, true, uniTaskCompletionSource, userData);
            m_WaitingNormalQueue.Enqueue(webJsonData);
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, object userData = null)
        {
            return PostToString(url, from, null, null, userData);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, object userData = null)
        {
            return PostToBytes(url, from, null, null, userData);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, object userData = null)
        {
            return PostToString(url, from, queryString, null, userData);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, object userData = null)
        {
            return PostToBytes(url, from, queryString, null, userData);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            var uniTaskCompletionSource = new TaskCompletionSource<WebStringResult>();
            from = MergeForm(from);
            queryString = MergeQueryString(queryString);
            header = MergeHeader(header);
            url = UrlHandler(url, queryString);

            WebJsonData webJsonData = new WebJsonData(url, header, from, uniTaskCompletionSource, userData);
            m_WaitingNormalQueue.Enqueue(webJsonData);
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns></returns>
        public Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            var uniTaskCompletionSource = new TaskCompletionSource<WebBufferResult>();
            from = MergeForm(from);
            queryString = MergeQueryString(queryString);
            header = MergeHeader(header);
            url = UrlHandler(url, queryString);
            WebJsonData webJsonData = new WebJsonData(url, header, from, uniTaskCompletionSource, userData);
            m_WaitingNormalQueue.Enqueue(webJsonData);
            return uniTaskCompletionSource.Task;
        }

        #region Base Data Management

        /// <summary>
        /// 添加基础表单数据
        /// </summary>
        /// <param name="key">表单键</param>
        /// <param name="value">表单值</param>
        public void AddBaseForm(string key, object value)
        {
            m_BaseForm[key] = value;
        }

        /// <summary>
        /// 移除基础表单数据
        /// </summary>
        /// <param name="key">表单键</param>
        public void RemoveBaseForm(string key)
        {
            if (m_BaseForm.ContainsKey(key))
            {
                m_BaseForm.Remove(key);
            }
        }

        /// <summary>
        /// 清空基础表单数据
        /// </summary>
        public void ClearBaseForm()
        {
            m_BaseForm.Clear();
        }

        /// <summary>
        /// 添加基础请求头数据
        /// </summary>
        /// <param name="key">请求头键</param>
        /// <param name="value">请求头值</param>
        public void AddBaseHeader(string key, string value)
        {
            m_BaseHeader[key] = value;
        }

        /// <summary>
        /// 移除基础请求头数据
        /// </summary>
        /// <param name="key">请求头键</param>
        public void RemoveBaseHeader(string key)
        {
            if (m_BaseHeader.ContainsKey(key))
            {
                m_BaseHeader.Remove(key);
            }
        }

        /// <summary>
        /// 清空基础请求头数据
        /// </summary>
        public void ClearBaseHeader()
        {
            m_BaseHeader.Clear();
        }

        /// <summary>
        /// 添加基础查询参数数据
        /// </summary>
        /// <param name="key">查询参数键</param>
        /// <param name="value">查询参数值</param>
        public void AddBaseQueryString(string key, string value)
        {
            m_BaseQueryString[key] = value;
        }

        /// <summary>
        /// 移除基础查询参数数据
        /// </summary>
        /// <param name="key">查询参数键</param>
        public void RemoveBaseQueryString(string key)
        {
            if (m_BaseQueryString.ContainsKey(key))
            {
                m_BaseQueryString.Remove(key);
            }
        }

        /// <summary>
        /// 清空基础查询参数数据
        /// </summary>
        public void ClearBaseQueryString()
        {
            m_BaseQueryString.Clear();
        }

        #endregion

        #region Data Merging

        /// <summary>
        /// 合并表单数据
        /// </summary>
        /// <param name="form">本次请求的表单数据</param>
        /// <returns>合并后的表单数据</returns>
        private Dictionary<string, object> MergeForm(Dictionary<string, object> form)
        {
            if (m_BaseForm.Count == 0)
            {
                return form;
            }

            // 复制基础数据
            var result = new Dictionary<string, object>(m_BaseForm);

            // 覆盖/添加外部数据
            if (form != null)
            {
                foreach (var kv in form)
                {
                    result[kv.Key] = kv.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// 合并请求头数据
        /// </summary>
        /// <param name="header">本次请求的请求头数据</param>
        /// <returns>合并后的请求头数据</returns>
        private Dictionary<string, string> MergeHeader(Dictionary<string, string> header)
        {
            if (m_BaseHeader.Count == 0)
            {
                return header;
            }

            var result = new Dictionary<string, string>(m_BaseHeader);
            if (header != null)
            {
                foreach (var kv in header)
                {
                    result[kv.Key] = kv.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// 合并查询参数数据
        /// </summary>
        /// <param name="queryString">本次请求的查询参数数据</param>
        /// <returns>合并后的查询参数数据</returns>
        private Dictionary<string, string> MergeQueryString(Dictionary<string, string> queryString)
        {
            if (m_BaseQueryString.Count == 0)
            {
                return queryString;
            }

            var result = new Dictionary<string, string>(m_BaseQueryString);
            if (queryString != null)
            {
                foreach (var kv in queryString)
                {
                    result[kv.Key] = kv.Value;
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// URL 标准化
        /// </summary>
        /// <param name="url">原始URL</param>
        /// <param name="queryString">查询参数字典</param>
        /// <returns>标准化后的URL</returns>
        private string UrlHandler(string url, Dictionary<string, string> queryString)
        {
            m_StringBuilder.Clear();
            m_StringBuilder.Append((url));
            if (queryString != null && queryString.Count > 0)
            {
                if (!url.EndsWithFast("?"))
                {
                    m_StringBuilder.Append("?");
                }

                foreach (var kv in queryString)
                {
                    m_StringBuilder.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }

                url = m_StringBuilder.ToString(0, m_StringBuilder.Length - 1);
                m_StringBuilder.Clear();
            }

            return url;
        }
    }
}
