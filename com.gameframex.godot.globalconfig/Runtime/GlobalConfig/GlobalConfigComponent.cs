using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.GlobalConfig.Runtime
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [GlobalClass]
    public sealed partial class GlobalConfigComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 检测App版本地址接口
        /// </summary>
        [Export] private string m_checkAppVersionUrl = string.Empty;

        /// <summary>
        /// 检测App版本地址接口
        /// </summary>
        public string CheckAppVersionUrl
        {
            get { return m_checkAppVersionUrl; }
            set { m_checkAppVersionUrl = value; }
        }

        /// <summary>
        /// 检测资源版本地址接口
        /// </summary>
        [Export] private string m_checkResourceVersionUrl = string.Empty;

        /// <summary>
        /// 检测资源版本地址接口
        /// </summary>
        public string CheckResourceVersionUrl
        {
            get { return m_checkResourceVersionUrl; }
            set { m_checkResourceVersionUrl = value; }
        }

        /// <summary>
        /// AOT代码列表
        /// </summary>
        [Export] private string m_aotCodeList = string.Empty;

        /// <summary>
        /// AOT补充元数据列表
        /// </summary>
        private List<string> m_aotCodeLists = new List<string>();

        /// <summary>
        /// 补充元数据列表
        /// </summary>
        public List<string> AOTCodeLists
        {
            get { return m_aotCodeLists; }
        }

        /// <summary>
        /// AOT代码列表
        /// </summary>
        public string AOTCodeList
        {
            get { return m_aotCodeList; }
            set
            {
                m_aotCodeList = value;
                try
                {
                    m_aotCodeLists = Utility.Json.ToObject<List<string>>(value);
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                }
            }
        }

        /// <summary>
        /// 附加内容
        /// </summary>
        [Export] private string m_content = string.Empty;

        /// <summary>
        /// 附加内容
        /// </summary>
        public string Content
        {
            get => m_content;
            set => m_content = value;
        }

        /// <summary>
        /// 主机服务地址
        /// </summary>
        [Export] private string m_hostServerUrl = string.Empty;

        /// <summary>
        /// 原始数据
        /// </summary>
        [Export] private string m_originalData = string.Empty;

        /// <summary>
        /// 主机服务地址
        /// </summary>
        public string HostServerUrl
        {
            get { return m_hostServerUrl; }
            set { m_hostServerUrl = value; }
        }

        /// <summary>
        /// 获取原始数据
        /// </summary>
        public string OriginalData
        {
            get { return m_originalData; }
        }

        /// <summary>
        /// 设置原始数据
        /// </summary>
        /// <param name="data">原始数据</param>
        public void SetOriginalData(string data)
        {
            m_originalData = data;
        }

        /// <summary>
        /// 获取全局配置信息
        /// </summary>
        /// <returns>返回全局配置信息对象</returns>
        public ResponseGlobalInfo GlobalInfo
        {
            get { return m_responseGlobalInfo; }
        }

        /// <summary>
        /// 全局配置信息对象
        /// </summary>
        /// <remarks>
        /// 用于存储从服务器获取的全局配置数据，包含游戏运行所需的各种全局参数
        /// </remarks>
        private ResponseGlobalInfo m_responseGlobalInfo;

        /// <summary>
        /// 设置全局配置信息
        /// </summary>
        /// <param name="globalInfo">全局配置信息对象，包含从服务器获取的配置数据</param>
        /// <remarks>
        /// 该方法用于更新全局配置信息，通常在从服务器获取新的配置数据后调用
        /// </remarks>
        public void SetGlobalConfig(ResponseGlobalInfo globalInfo)
        {
            m_responseGlobalInfo = globalInfo;
        }

        public override void _Ready()
        {
            IsAutoRegister = false;
            base._Ready();
        }
    }
}