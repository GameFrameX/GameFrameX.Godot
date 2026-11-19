//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Runtime;

namespace GameFrameX.Download.Runtime
{
    /// <summary>
    /// 下载模块启动配置（模块级 Options）。
    /// </summary>
    /// <remarks>
    /// 默认值即开箱值（与原 DownloadComponent 序列化字段默认值同源）；游戏在首场景加载前经 <c>GameFrameXOptions.Configure(new DownloadOptions{...})</c> 注入差异值，
    /// 由 <see cref="DownloadComponent"/> 在初始化时一次性消费。
    /// </remarks>
    public sealed class DownloadOptions
    {
        /// <summary>
        /// 下载代理辅助器数量（默认 3）。
        /// </summary>
        /// <remarks>启动期型：仅在组件初始化创建代理时消费，运行期不可修改。</remarks>
        [GameFrameXStartupOnly]
        public int AgentHelperCount = 3;

        /// <summary>
        /// 下载超时时长，以秒为单位（默认 30 秒）。
        /// </summary>
        /// <remarks>实时型：运行期可经 <see cref="DownloadComponent.Timeout"/> 修改，立即生效。</remarks>
        public float Timeout = 30f;

        /// <summary>
        /// 将缓冲区写入磁盘的临界大小，仅当开启断点续传时有效（默认 1MB = 1048576 字节）。
        /// </summary>
        /// <remarks>启动期型：仅模块首次实例化前经 <c>GameFrameXOptions.Configure</c> 生效；组件运行期修改该值会被拒绝。</remarks>
        [GameFrameXStartupOnly]
        public int FlushSize = OneMegaBytes;

        /// <summary>
        /// 下载代理辅助器类型全名（默认 GodotWebRequest 实现）。
        /// </summary>
        /// <remarks>启动期型：仅在组件初始化创建代理时消费，运行期不可修改。</remarks>
        [GameFrameXStartupOnly]
        public string DownloadAgentHelperTypeName = "GameFrameX.Download.Runtime.GodotWebRequestDownloadAgentHelper";

        /// <summary>
        /// 一兆字节对应的字节数（1048576），与原组件常量同值。
        /// </summary>
        public const int OneMegaBytes = 1024 * 1024;
    }
}
