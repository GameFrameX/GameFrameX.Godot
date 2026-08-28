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

using System;
using GameFrameX.Runtime;

namespace GameFrameX.Setting.Runtime
{
    /// <summary>
    /// 创建当前运行环境使用的配置存储后端。
    /// </summary>
    internal static class SettingStorageBackendFactory
    {
        private static Func<ISettingStorageBackend> s_BackendFactory;

        /// <summary>
        /// 注册平台适配程序集使用的配置存储后端工厂。平台适配层通过原生 SDK 实现后端，再在启动阶段注册到 setting 程序集。
        /// </summary>
        /// <param name="backendFactory">配置存储后端工厂。</param>
        internal static void RegisterBackend(Func<ISettingStorageBackend> backendFactory)
        {
            if (backendFactory == null)
            {
                throw new GameFrameworkException("Storage backend factory is invalid.");
            }

            s_BackendFactory = backendFactory;
        }

        /// <summary>
        /// 创建当前运行环境使用的配置存储后端。未注册平台后端时使用默认的 Godot ConfigFile 后端。
        /// </summary>
        /// <returns>配置存储后端。</returns>
        internal static ISettingStorageBackend Create()
        {
            return s_BackendFactory == null ? new GodotConfigFileSettingStorage() : s_BackendFactory();
        }

        /// <summary>
        /// 重置已注册的后端工厂，仅供测试使用。
        /// </summary>
        internal static void ResetBackendForTests()
        {
            s_BackendFactory = null;
        }
    }
}
