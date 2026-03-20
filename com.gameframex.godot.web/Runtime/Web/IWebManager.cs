using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFrameX.Web.Runtime
{
    /// <summary>
    /// Web请求管理器接口，提供HTTP GET和POST请求的功能
    /// </summary>
    public interface IWebManager
    {
        /// <summary>
        /// 发送Get请求，返回字符串结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebStringResult类型的异步任务</returns>
        Task<WebStringResult> GetToString(string url, object userData = null);

        /// <summary>
        /// 发送Get请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> GetToBytes(string url, object userData = null);

        /// <summary>
        /// 发送带查询参数的Get请求，返回字符串结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebStringResult类型的异步任务</returns>
        Task<WebStringResult> GetToString(string url, Dictionary<string, string> queryString, object userData = null);


        /// <summary>
        /// 发送带查询参数的Get请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> GetToBytes(string url, Dictionary<string, string> queryString, object userData = null);


        /// <summary>
        /// 发送带查询参数和请求头的Get请求，返回字符串结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebStringResult类型的异步任务</returns>
        Task<WebStringResult> GetToString(string url, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null);


        /// <summary>
        /// 发送带查询参数和请求头的Get请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> GetToBytes(string url, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null);


        /// <summary>
        /// 发送简单Post请求，返回字符串结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebStringResult类型的异步任务</returns>
        Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, object userData = null);

        /// <summary>
        /// 发送带查询参数的Post请求，返回字符串结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebStringResult类型的异步任务</returns>
        Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, object userData = null);

        /// <summary>
        /// 发送带查询参数和请求头的Post请求，返回字符串结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebStringResult类型的异步任务</returns>
        Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null);

        /// <summary>
        /// 发送简单Post请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, object userData = null);

        /// <summary>
        /// 发送带查询参数的Post请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, object userData = null);

        /// <summary>
        /// 发送带查询参数和请求头的Post请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null);

        /// <summary>
        /// 发送字节数组Post请求，返回字节数组结果
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">要发送的字节数组数据</param>
        /// <param name="queryString">URL查询参数字典</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>返回WebBufferResult类型的异步任务</returns>
        Task<WebBufferResult> PostToBytes(string url, byte[] from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null);

        /// <summary>
        /// 超时时间
        /// </summary>
        float Timeout { get; set; }

        /// <summary>
        /// 添加基础表单数据
        /// </summary>
        /// <param name="key">表单键</param>
        /// <param name="value">表单值</param>
        void AddBaseForm(string key, object value);

        /// <summary>
        /// 移除基础表单数据
        /// </summary>
        /// <param name="key">表单键</param>
        void RemoveBaseForm(string key);

        /// <summary>
        /// 清空基础表单数据
        /// </summary>
        void ClearBaseForm();

        /// <summary>
        /// 添加基础请求头数据
        /// </summary>
        /// <param name="key">请求头键</param>
        /// <param name="value">请求头值</param>
        void AddBaseHeader(string key, string value);

        /// <summary>
        /// 移除基础请求头数据
        /// </summary>
        /// <param name="key">请求头键</param>
        void RemoveBaseHeader(string key);

        /// <summary>
        /// 清空基础请求头数据
        /// </summary>
        void ClearBaseHeader();

        /// <summary>
        /// 添加基础查询参数数据
        /// </summary>
        /// <param name="key">查询参数键</param>
        /// <param name="value">查询参数值</param>
        void AddBaseQueryString(string key, string value);

        /// <summary>
        /// 移除基础查询参数数据
        /// </summary>
        /// <param name="key">查询参数键</param>
        void RemoveBaseQueryString(string key);

        /// <summary>
        /// 清空基础查询参数数据
        /// </summary>
        void ClearBaseQueryString();
    }
}
