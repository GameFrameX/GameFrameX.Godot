using System;

namespace GameFrameX.Web.Runtime
{
    public partial class WebManager
    {
        /// <summary>
        /// Web请求数据的基类，包含请求的基本信息
        /// </summary>
        public class WebData : IDisposable
        {
            /// <summary>
            /// 获取用户自定义数据
            /// </summary>
            public object UserData { get; }

            /// <summary>
            /// 获取是否为GET请求
            /// </summary>
            public bool IsGet { get; }

            /// <summary>
            /// 获取请求URL
            /// </summary>
            public string URL { get; }

            /// <summary>
            /// 初始化Web请求数据
            /// </summary>
            /// <param name="isGet">是否为GET请求</param>
            /// <param name="url">请求URL</param>
            /// <param name="userData">用户自定义数据</param>
            protected WebData(bool isGet, string url, object userData = null)
            {
                UserData = userData;
                IsGet = isGet;
                URL = url;
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            public virtual void Dispose()
            {
            }
        }
    }
}