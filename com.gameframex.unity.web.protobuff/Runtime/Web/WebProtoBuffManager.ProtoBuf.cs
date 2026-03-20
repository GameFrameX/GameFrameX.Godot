using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;

namespace GameFrameX.Web.ProtoBuff.Runtime
{
    public partial class WebProtoBuffManager
    {
        private readonly Queue<WebProtoBufData> m_WaitingProtoBufQueue = new Queue<WebProtoBufData>(256);
        private readonly List<WebProtoBufData> m_SendingProtoBufList = new List<WebProtoBufData>(16);
        private const string ProtoBufContentType = "application/x-protobuf";

        private void UpdateProtoBuf(float elapseSeconds, float realElapseSeconds)
        {
            lock (m_StringBuilder)
            {
                if (m_SendingProtoBufList.Count < MaxConnectionPerServer && m_WaitingProtoBufQueue.Count > 0)
                {
                    var webProtoBufData = m_WaitingProtoBufQueue.Dequeue();
                    MakeProtoBufBytesRequest(webProtoBufData);
                    m_SendingProtoBufList.Add(webProtoBufData);
                }
            }
        }

        private void ShutdownProtoBuf()
        {
            while (m_WaitingProtoBufQueue.Count > 0)
            {
                var webData = m_WaitingProtoBufQueue.Dequeue();
                webData.Dispose();
            }

            m_WaitingProtoBufQueue.Clear();
            while (m_SendingProtoBufList.Count > 0)
            {
                var webData = m_SendingProtoBufList[0];
                m_SendingProtoBufList.RemoveAt(0);
                webData.Dispose();
            }

            m_SendingProtoBufList.Clear();
            m_MemoryStream.Dispose();
        }

        private async void MakeProtoBufBytesRequest(WebProtoBufData webData)
        {
            try
            {
                var request = WebRequest.CreateHttp(webData.URL);
                request.Method = webData.IsGet ? WebRequestMethods.Http.Get : WebRequestMethods.Http.Post;
                request.Timeout = (int)RequestTimeout.TotalMilliseconds;
                request.ContentType = ProtoBufContentType;
                var postData = webData.SendData;
                request.ContentLength = postData.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(postData, 0, postData.Length);
                }

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (var responseStream = response.GetResponseStream())
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
                        webData.Task.SetResult(new WebBufferResult(webData.UserData, m_MemoryStream.ToArray()));
                    }
                }
            }
            catch (WebException e)
            {
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
                m_SendingProtoBufList.Remove(webData);
            }
        }

        public async Task<T> Post<T>(string url, MessageObject message) where T : MessageObject, IResponseMessage
        {
            DebugSendLog(message);
            var webBufferResult = await PostInner(url, message);
            if (webBufferResult.IsNotNull())
            {
                var messageObjectHttp = SerializerHelper.Deserialize(webBufferResult.Result, typeof(MessageHttpObject)) as MessageHttpObject;
                if (messageObjectHttp.IsNotNull() && messageObjectHttp.Id != default)
                {
                    var messageType = ProtoMessageIdHandler.GetRespTypeById(messageObjectHttp.Id);
                    if (messageType != typeof(T))
                    {
                        Log.Error($"Response message type is invalid. Expected '{typeof(T).FullName}', actual '{messageType.FullName}'.");
                        return default;
                    }

                    var messageObject = SerializerHelper.Deserialize(messageObjectHttp.Body, typeof(T)) as T;
                    DebugReceiveLog(messageObject);
                    return messageObject;
                }
            }

            return default;
        }

        private Task<WebBufferResult> PostInner(string url, MessageObject message, object userData = null)
        {
            var taskCompletionSource = new TaskCompletionSource<WebBufferResult>();
            url = UrlHandler(url, null);
            var id = ProtoMessageIdHandler.GetReqMessageIdByType(message.GetType());
            var messageHttpObject = new MessageHttpObject
            {
                Id = id,
                UniqueId = message.UniqueId,
                Body = SerializerHelper.Serialize(message),
            };
            var sendData = SerializerHelper.Serialize(messageHttpObject);
            var webData = new WebProtoBufData(url, sendData, taskCompletionSource, userData);
            m_WaitingProtoBufQueue.Enqueue(webData);
            return taskCompletionSource.Task;
        }

        private void DebugReceiveLog(MessageObject messageObject)
        {
#if ENABLE_GAMEFRAMEX_WEB_RECEIVE_LOG
            var messageId = ProtoMessageIdHandler.GetReqMessageIdByType(messageObject.GetType());
            Log.Debug($"接收消息 ID:[{messageId},{messageObject.UniqueId},{messageObject.GetType().Name}] 消息内容:{Utility.Json.ToJson(messageObject)}");
#endif
        }

        private void DebugSendLog(MessageObject messageObject)
        {
#if ENABLE_GAMEFRAMEX_WEB_SEND_LOG
            var messageId = ProtoMessageIdHandler.GetReqMessageIdByType(messageObject.GetType());
            Log.Debug($"发送消息 ID:[{messageId},{messageObject.UniqueId},{messageObject.GetType().Name}] 消息内容:{Utility.Json.ToJson(messageObject)}");
#endif
        }
    }
}
