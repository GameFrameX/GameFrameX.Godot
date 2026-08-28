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
    /// 默认消息序列化器，作为未注册时的兜底实现，调用时抛出异常以提示用户注册实际的序列化器。
    /// </summary>
    /// <remarks>
    /// Default message serializer that serves as a fallback when no serializer is registered.
    /// Throws an exception to remind the user to register an actual serializer.
    /// </remarks>
    public class DefaultMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// 默认兜底实例。
        /// </summary>
        /// <remarks>
        /// Default fallback instance.
        /// </remarks>
        public static readonly DefaultMessageSerializer Instance = new DefaultMessageSerializer();

        private DefaultMessageSerializer() { }

        /// <summary>
        /// 序列化消息对象，未注册实际序列化器时始终抛出异常。
        /// </summary>
        /// <remarks>
        /// Serializes a message object; always throws when no actual serializer is registered.
        /// </remarks>
        /// <param name="message">要序列化的消息对象 / The message object to serialize</param>
        /// <returns>此方法不会返回 / This method never returns</returns>
        /// <exception cref="InvalidOperationException">未注册消息序列化器时抛出 / Thrown when no message serializer is registered</exception>
        public byte[] Serialize<T>(T message) where T : MessageObject
        {
            throw new InvalidOperationException(
                "No IMessageSerializer registered. " +
                "Call MessageSerializerRegistry.RegisterGlobal() or provide a channel-level serializer.");
        }

        /// <summary>
        /// 反序列化消息对象，未注册实际序列化器时始终抛出异常。
        /// </summary>
        /// <remarks>
        /// Deserializes a message object; always throws when no actual serializer is registered.
        /// </remarks>
        /// <param name="data">要反序列化的字节数组 / The byte array to deserialize</param>
        /// <param name="targetType">目标消息类型 / The target message type</param>
        /// <returns>此方法不会返回 / This method never returns</returns>
        /// <exception cref="InvalidOperationException">未注册消息序列化器时抛出 / Thrown when no message serializer is registered</exception>
        public object Deserialize(byte[] data, Type targetType)
        {
            throw new InvalidOperationException(
                "No IMessageSerializer registered. " +
                "Call MessageSerializerRegistry.RegisterGlobal() or provide a channel-level serializer.");
        }
    }
}
