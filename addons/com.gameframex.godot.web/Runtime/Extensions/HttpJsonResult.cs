using GameFrameX.Runtime;
using System.Text.Json.Serialization;

namespace GameFrameX.Web.Runtime
{
    /// <summary>
    /// HTTP网页请求的消息响应结构
    /// </summary>
    public sealed class HttpJsonResult
    {
        /// <summary>
        /// 响应码0 为成功
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// 响应数据.
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; }

        public override string ToString()
        {
            return Utility.Json.ToJson(this);
        }
    }
}
