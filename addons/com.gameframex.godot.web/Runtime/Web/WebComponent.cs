// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
// 
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
// 
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
// 
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
// 
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Web.Runtime
{
    /// <summary>
    /// Web 请求组件。
    /// 提供HTTP GET和POST请求功能的Godot组件。
    /// 支持字符串和字节数组格式的请求结果。
    /// 可以设置请求超时时间。
    /// </summary>
    public sealed partial class WebComponent : GameFrameworkComponent
    {
        /// <summary>
        /// Web请求管理器实例
        /// </summary>
        private IWebManager m_WebManager;

        /// <summary>
        /// 请求超时时间配置
        /// </summary>
        [Export]
        private float m_Timeout = 5f;

        /// <summary>
        /// 获取或设置下载超时时长，以秒为单位。
        /// 当请求超过此时间未完成时会自动终止。
        /// </summary>
        public float Timeout
        {
            get { return m_WebManager.Timeout; }
            set { m_WebManager.Timeout = m_Timeout = value; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// 在此方法中初始化Web管理器并设置超时时间。
        /// </summary>
        public override void _Ready()
        {
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IWebManager);
            base._Ready();
            m_WebManager = GameFrameworkEntry.GetModule<IWebManager>();
            if (m_WebManager == null)
            {
                Log.Fatal("Web manager is invalid.");
                return;
            }

            m_WebManager.Timeout = m_Timeout;
        }

        /// <summary>
        /// 发送Get请求，返回字符串结果。
        /// 这是最基础的GET请求方法，不包含任何查询参数和请求头。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字符串结果的WebStringResult异步任务</returns>
        public Task<WebStringResult> GetToString(string url, object userData = null)
        {
            return m_WebManager.GetToString(url, userData);
        }

        /// <summary>
        /// 发送带查询参数的Get请求，返回字符串结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字符串结果的WebStringResult异步任务</returns>
        public Task<WebStringResult> GetToString(string url, Dictionary<string, string> queryString, object userData = null)
        {
            return m_WebManager.GetToString(url, queryString, userData);
        }

        /// <summary>
        /// 发送带查询参数和请求头的Get请求，返回字符串结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字符串结果的WebStringResult异步任务</returns>
        public Task<WebStringResult> GetToString(string url, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            return m_WebManager.GetToString(url, queryString, header, userData);
        }


        /// <summary>
        /// 发送Get请求，返回字节数组结果。
        /// 适用于下载二进制数据如图片、音频等。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> GetToBytes(string url, object userData = null)
        {
            return m_WebManager.GetToBytes(url, userData);
        }

        /// <summary>
        /// 发送带查询参数的Get请求，返回字节数组结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> GetToBytes(string url, Dictionary<string, string> queryString, object userData = null)
        {
            return m_WebManager.GetToBytes(url, queryString, userData);
        }

        /// <summary>
        /// 发送带查询参数和请求头的Get请求，返回字节数组结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> GetToBytes(string url, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            return m_WebManager.GetToBytes(url, queryString, header, userData);
        }


        /// <summary>
        /// 发送Post请求，返回字符串结果。
        /// 这是最基础的POST请求方法。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典，作为请求体发送</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字符串结果的WebStringResult异步任务</returns>
        public Task<WebStringResult> PostToString(string url, Dictionary<string, object> from = null, object userData = null)
        {
            return m_WebManager.PostToString(url, from, userData);
        }

        /// <summary>
        /// 发送带查询参数的Post请求，返回字符串结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典，作为请求体发送</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字符串结果的WebStringResult异步任务</returns>
        public Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, object userData = null)
        {
            return m_WebManager.PostToString(url, from, queryString, userData);
        }

        /// <summary>
        /// 发送带查询参数和请求头的Post请求，返回字符串结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典，作为请求体发送</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字符串结果的WebStringResult异步任务</returns>
        public Task<WebStringResult> PostToString(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            return m_WebManager.PostToString(url, from, queryString, header, userData);
        }


        /// <summary>
        /// 发送Post请求，返回字节数组结果。
        /// 适用于上传和下载二进制数据。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典，作为请求体发送</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, object userData = null)
        {
            return m_WebManager.PostToBytes(url, from, userData);
        }

        /// <summary>
        /// 发送带查询参数的Post请求，返回字节数组结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典，作为请求体发送</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, object userData = null)
        {
            return m_WebManager.PostToBytes(url, from, queryString, userData);
        }

        /// <summary>
        /// 发送带查询参数和请求头的Post请求，返回字节数组结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单数据字典，作为请求体发送</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> PostToBytes(string url, Dictionary<string, object> from, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            return m_WebManager.PostToBytes(url, from, queryString, header, userData);
        }

        /// <summary>
        /// 发送带查询参数和请求头的Post请求，返回字节数组结果。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="fromData">表单数据字节数组，作为请求体发送</param>
        /// <param name="queryString">URL查询参数字典，会被附加到URL后面</param>
        /// <param name="header">HTTP请求头字典</param>
        /// <param name="userData">用户自定义数据，会在结果中原样返回</param>
        /// <returns>返回包含字节数组的WebBufferResult异步任务</returns>
        public Task<WebBufferResult> PostToBytes(string url, byte[] fromData, Dictionary<string, string> queryString, Dictionary<string, string> header, object userData = null)
        {
            return m_WebManager.PostToBytes(url, fromData, queryString, header, userData);
        }

        /// <summary>
        /// 添加基础表单数据
        /// </summary>
        /// <param name="key">表单键</param>
        /// <param name="value">表单值</param>
        public void AddBaseForm(string key, object value)
        {
            m_WebManager.AddBaseForm(key, value);
        }

        /// <summary>
        /// 移除基础表单数据
        /// </summary>
        /// <param name="key">表单键</param>
        public void RemoveBaseForm(string key)
        {
            m_WebManager.RemoveBaseForm(key);
        }

        /// <summary>
        /// 清空基础表单数据
        /// </summary>
        public void ClearBaseForm()
        {
            m_WebManager.ClearBaseForm();
        }

        /// <summary>
        /// 添加基础请求头数据
        /// </summary>
        /// <param name="key">请求头键</param>
        /// <param name="value">请求头值</param>
        public void AddBaseHeader(string key, string value)
        {
            m_WebManager.AddBaseHeader(key, value);
        }

        /// <summary>
        /// 移除基础请求头数据
        /// </summary>
        /// <param name="key">请求头键</param>
        public void RemoveBaseHeader(string key)
        {
            m_WebManager.RemoveBaseHeader(key);
        }

        /// <summary>
        /// 清空基础请求头数据
        /// </summary>
        public void ClearBaseHeader()
        {
            m_WebManager.ClearBaseHeader();
        }

        /// <summary>
        /// 添加基础查询参数数据
        /// </summary>
        /// <param name="key">查询参数键</param>
        /// <param name="value">查询参数值</param>
        public void AddBaseQueryString(string key, string value)
        {
            m_WebManager.AddBaseQueryString(key, value);
        }

        /// <summary>
        /// 移除基础查询参数数据
        /// </summary>
        /// <param name="key">查询参数键</param>
        public void RemoveBaseQueryString(string key)
        {
            m_WebManager.RemoveBaseQueryString(key);
        }

        /// <summary>
        /// 清空基础查询参数数据
        /// </summary>
        public void ClearBaseQueryString()
        {
            m_WebManager.ClearBaseQueryString();
        }
    }
}
