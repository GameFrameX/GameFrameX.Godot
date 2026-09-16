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

using GameFrameX.Network.Runtime;
using ProtoBuf.Meta;

namespace ProtoBuf
{
    /// <summary>
    /// Protobuf 序列化器初始化器：将 network 消息基类契约注册进 protobuf-net 运行时模型，
    /// 并可将 <see cref="ProtobufMessageSerializer"/> 注册为全局默认。
    /// network 基类契约由 <see cref="SerializerHelper"/> 惰性幂等注册，无需启动期显式调用；
    /// Register() 仅在需要把 Protobuf 设为全局默认序列化器时调用（如 GameApp 初始化）。
    /// </summary>
    /// <remarks>
    /// Protobuf serializer initializer: registers the network message base contracts into the
    /// protobuf-net runtime model, and optionally installs <see cref="ProtobufMessageSerializer"/>
    /// as the global default. Contracts are lazily and idempotently ensured by
    /// <see cref="SerializerHelper"/>; Register() is only needed to set the global default serializer.
    /// </remarks>
    public static class ProtobufSerializerInitializer
    {
        private static volatile bool s_NetworkMessageContractsRegistered;
        private static readonly object s_RegisterLock = new object();

        /// <summary>
        /// 初始化 Protobuf 序列化器并注册为全局默认消息序列化器。
        /// </summary>
        /// <remarks>
        /// Initializes the Protobuf serializer and registers it as the global default message serializer.
        /// </remarks>
        public static void Register()
        {
            EnsureNetworkMessageContracts();
            MessageSerializerRegistry.RegisterGlobal(new ProtobufMessageSerializer());
        }

        /// <summary>
        /// 幂等注册 network 包消息基类契约；<see cref="SerializerHelper"/> 每次序列化前调用，
        /// 保证不依赖任何启动期显式初始化（此前 network 源码内联 [ProtoContract] 注解，移除注解后由本方法等价接管）。
        /// </summary>
        /// <remarks>
        /// Idempotently registers the network package's message base contracts.
        /// MessageObject registers as an empty contract (inheritance root only);
        /// MessageHttpObject fields 1=Id, 2=UniqueId, 3=Body — wire-compatible with the removed inline attributes.
        /// </remarks>
        public static void EnsureNetworkMessageContracts()
        {
            if (s_NetworkMessageContractsRegistered)
            {
                return;
            }

            lock (s_RegisterLock)
            {
                if (s_NetworkMessageContractsRegistered)
                {
                    return;
                }

                var model = RuntimeTypeModel.Default;

                var messageType = typeof(MessageObject);
                if (model.CanSerialize(messageType) == false)
                {
                    model.Add(messageType, false);
                }

                var httpType = typeof(MessageHttpObject);
                if (model.CanSerialize(httpType) == false)
                {
                    var metaType = model.Add(httpType, false);
                    metaType.Add(1, nameof(MessageHttpObject.Id));
                    metaType.Add(2, nameof(MessageHttpObject.UniqueId));
                    metaType.Add(3, nameof(MessageHttpObject.Body));
                }

                s_NetworkMessageContractsRegistered = true;
            }
        }
    }
}
