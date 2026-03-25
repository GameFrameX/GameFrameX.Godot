namespace GameFrameX.Web.Runtime
{
    public sealed class WebBufferResult
    {
        public WebBufferResult(object userData, byte[] result)
        {
            UserData = userData;
            Result = result;
        }

        /// <summary>
        /// 请求结果
        /// </summary>
        public byte[] Result { get; }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData { get; }
    }
}