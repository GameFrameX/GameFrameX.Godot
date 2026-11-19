// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe any legal rights or interests of others as prohibited by laws and regulations!
//
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository:  https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using GameFrameX.Runtime;

namespace GameFrameX.Setting.Runtime
{
    /// <summary>
    /// 创建当前运行环境使用的存档文件存储后端。默认返回本地磁盘后端（根目录为 <c>{user://}/SaveSlotFileStorage/</c>）；平台适配包可通过 <see cref="RegisterChannelBackend"/> 注册平台后端覆盖。
    /// </summary>
    public static class SaveSlotFileStorageBackendFactory
    {
        private static Func<ISaveSlotFileStorageBackend> s_ChannelBackendFactory;

        private static string s_RootDirectory;

        /// <summary>
        /// 获取存档文件存储根目录。未设置时默认 <c>{user://}/SaveSlotFileStorage</c>。
        /// </summary>
        public static string SaveSlotFileStorageRoot
        {
            get
            {
                if (s_RootDirectory == null)
                {
                    s_RootDirectory = PathHelper.Combine(PathHelper.GetPersistentDataPath(), "SaveSlotFileStorage");
                }

                return s_RootDirectory;
            }
        }

        /// <summary>
        /// 设置存档文件存储根目录（启动期配置）。必须在首次创建后端前调用；重复调用保留首个并输出 Warning。
        /// </summary>
        /// <param name="rootDirectory">根目录。</param>
        public static void SetSaveSlotFileStorageRoot(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory))
            {
                throw new SaveSlotFileStorageException("Save slot file storage root directory is invalid.");
            }

            if (s_RootDirectory != null)
            {
                Log.Warning("Save slot file storage root is already configured. The duplicate configuration is ignored.");
                return;
            }

            s_RootDirectory = rootDirectory;
        }

        /// <summary>
        /// 注册平台存档文件存储后端工厂。平台适配包在启动阶段自注册（引用即注册），注册时机早于 <see cref="SettingComponent"/> 首次创建后端。重复注册保留首个并输出 Warning。
        /// </summary>
        /// <param name="backendFactory">后端工厂，返回平台存储后端实例。</param>
        public static void RegisterChannelBackend(Func<ISaveSlotFileStorageBackend> backendFactory)
        {
            if (backendFactory == null)
            {
                throw new SaveSlotFileStorageException("Storage backend factory is invalid.");
            }

            if (s_ChannelBackendFactory != null)
            {
                Log.Warning("Channel save slot file storage backend is already registered. The duplicate registration is ignored.");
                return;
            }

            s_ChannelBackendFactory = backendFactory;
        }

        /// <summary>
        /// 创建当前运行环境使用的存档文件存储后端。优先取平台注册，未注册时返回 <see cref="LocalDiskSaveSlotFileStorageBackend"/>。
        /// </summary>
        public static ISaveSlotFileStorageBackend Create()
        {
            return s_ChannelBackendFactory == null ? new LocalDiskSaveSlotFileStorageBackend(SaveSlotFileStorageRoot) : s_ChannelBackendFactory();
        }

        /// <summary>
        /// 重置平台后端注册与根目录配置。仅供单元测试在用例间清理注册状态使用。
        /// </summary>
        internal static void ResetForTests()
        {
            s_ChannelBackendFactory = null;
            s_RootDirectory = null;
        }
    }
}
