// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
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
//  or to infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//
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

#if ENABLE_GAME_FRAME_X_WEB_SOCKET || FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// WebSocket 连接抽象接口，隔离对 Godot 原生 <c>WebSocketPeer</c> 具体类型的依赖，便于单元测试注入桩实现。
    /// </summary>
    /// <remarks>
    /// 返回错误码的方法遵循 Godot 约定：0 表示成功（<c>Godot.Error.Ok</c>），非 0 为对应错误。
    /// </remarks>
    public interface IWebSocket
    {
        /// <summary>
        /// 获取当前连接就绪状态。
        /// </summary>
        WebSocketReadyState ReadyState { get; }

        /// <summary>
        /// 获取或设置入站缓冲区大小。
        /// </summary>
        int InboundBufferSize { get; set; }

        /// <summary>
        /// 获取或设置出站缓冲区大小。
        /// </summary>
        int OutboundBufferSize { get; set; }

        /// <summary>
        /// 发起连接到指定 URL（ws:// 或 wss://）。
        /// </summary>
        /// <param name="url">目标 URL。</param>
        /// <returns>错误码，0 表示请求已受理。</returns>
        int ConnectToUrl(string url);

        /// <summary>
        /// 轮询连接状态并驱动握手、收发与关闭流程，需在就绪前持续调用。
        /// </summary>
        void Poll();

        /// <summary>
        /// 获取待读取的消息帧数量。
        /// </summary>
        /// <returns>待读取的帧数量。</returns>
        int GetAvailablePacketCount();

        /// <summary>
        /// 取出最早到达的一帧，内容为二进制帧或文本帧的 UTF-8 字节。
        /// </summary>
        /// <returns>帧内容字节数组。</returns>
        byte[] GetPacket();

        /// <summary>
        /// 判断最近一次 <see cref="GetPacket"/> 取出的是否为文本帧。
        /// </summary>
        /// <returns>是文本帧返回 true，二进制帧返回 false。</returns>
        bool WasStringPacket();

        /// <summary>
        /// 以二进制帧发送数据。
        /// </summary>
        /// <param name="data">要发送的字节数组。</param>
        /// <returns>错误码，0 表示已入队。</returns>
        int Send(byte[] data);

        /// <summary>
        /// 以文本帧发送字符串。
        /// </summary>
        /// <param name="text">要发送的文本。</param>
        /// <returns>错误码，0 表示已入队。</returns>
        int SendText(string text);

        /// <summary>
        /// 主动关闭连接。
        /// </summary>
        /// <param name="code">关闭状态码。</param>
        /// <param name="reason">关闭原因。</param>
        void Close(int code, string reason);

        /// <summary>
        /// 获取对端返回的关闭状态码。
        /// </summary>
        /// <returns>关闭状态码，连接未关闭时为 -1。</returns>
        int GetCloseCode();

        /// <summary>
        /// 获取对端返回的关闭原因。
        /// </summary>
        /// <returns>关闭原因描述。</returns>
        string GetCloseReason();
    }
}

#endif
