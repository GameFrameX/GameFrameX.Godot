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

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 消息序列化器全局注册表，用于管理全局默认的消息序列化器实例。
    /// </summary>
    /// <remarks>
    /// Global registry for managing the default message serializer instance.
    /// </remarks>
    public static class MessageSerializerRegistry
    {
        private static volatile IMessageSerializer _global;

        /// <summary>
        /// 获取全局消息序列化器，若未注册则返回 null，由调用方自行回退到 JSON 兜底实现。
        /// </summary>
        /// <remarks>
        /// Gets the global message serializer; returns null when not registered,
        /// and the caller falls back to the JSON default implementation.
        /// </remarks>
        /// <value>全局消息序列化器实例 / The global message serializer instance</value>
        public static IMessageSerializer Global
        {
            get { return _global; }
        }

        /// <summary>
        /// 注册全局消息序列化器。
        /// </summary>
        /// <remarks>
        /// Registers the global message serializer.
        /// </remarks>
        /// <param name="serializer">要注册的消息序列化器实例 / The message serializer instance to register</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="serializer"/> 为 null 时抛出 / Thrown when <paramref name="serializer"/> is null</exception>
        public static void RegisterGlobal(IMessageSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            _global = serializer;
        }

        /// <summary>
        /// 重置全局消息序列化器为未注册状态。
        /// </summary>
        /// <remarks>
        /// Resets the global message serializer to the unregistered state.
        /// </remarks>
        public static void Reset()
        {
            _global = null;
        }
    }
}
