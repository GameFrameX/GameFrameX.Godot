namespace GameFrameX.Web.Runtime
{
    /// <summary>
    /// Web字符串请求结果类，用于封装HTTP请求返回的字符串数据
    /// </summary>
    public sealed class WebStringResult
    {
        /// <summary>
        /// 初始化Web字符串请求结果
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="result">请求返回的字符串结果</param>
        public WebStringResult(object userData, string result)
        {
            UserData = userData;
            Result = result;
        }

        /// <summary>
        /// 获取请求返回的字符串结果
        /// </summary>
        public string Result { get; }

        /// <summary>
        /// 获取用户自定义数据，在请求时传入的数据会原样返回
        /// </summary>
        public object UserData { get; }

        /// <summary>
        /// 将请求结果转换为字符串表示形式
        /// </summary>
        /// <returns>返回格式化的结果字符串</returns>
        public override string ToString()
        {
            return $"[Result]:{Result}";
        }
    }
}
