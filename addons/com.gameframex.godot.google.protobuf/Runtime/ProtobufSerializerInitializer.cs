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
    /// Protobuf 序列化器初始化器，将 <see cref="ProtobufMessageSerializer"/> 注册为全局默认。
    /// Godot 没有 Unity 的 RuntimeInitializeOnLoadMethod 自动初始化机制，需在游戏启动时显式调用（如 GameApp 初始化）。
    /// </summary>
    /// <remarks>
    /// Protobuf serializer initializer that registers <see cref="ProtobufMessageSerializer"/> as the global default.
    /// Godot has no Unity-style RuntimeInitializeOnLoadMethod, so Register() must be called explicitly
    /// during game startup (e.g. in GameApp initialization).
    /// </remarks>
    public static class ProtobufSerializerInitializer
    {
        /// <summary>
        /// 初始化 Protobuf 序列化器并注册为全局默认消息序列化器。
        /// </summary>
        /// <remarks>
        /// Initializes the Protobuf serializer and registers it as the global default message serializer.
        /// </remarks>
        public static void Register()
        {
            RegisterMessageHttpObject();
            MessageSerializerRegistry.RegisterGlobal(new ProtobufMessageSerializer());
        }

        private static void RegisterMessageHttpObject()
        {
            var model = RuntimeTypeModel.Default;
            var type = typeof(MessageHttpObject);
            if (model.CanSerialize(type))
            {
                return;
            }
            var metaType = model.Add(type, false);
            metaType.Add(1, nameof(MessageHttpObject.Id));
            metaType.Add(2, nameof(MessageHttpObject.UniqueId));
            metaType.Add(3, nameof(MessageHttpObject.Body));
        }
    }
}
