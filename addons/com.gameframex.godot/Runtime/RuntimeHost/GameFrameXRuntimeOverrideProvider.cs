// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the source code root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legal rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 运行时覆盖收集器：合并手动注册与扫描静默发现的 <see cref="IGameFrameXRuntimeOverrideProvider"/>。
    /// </summary>
    /// <remarks>
    /// 引用即注册：实现 <see cref="IGameFrameXRuntimeOverrideProvider"/> 的普通类由扫描静默发现、无参实例化后自动生效；
    /// 手动注册的同类型实例优先，扫描不重复实例化同类型（防止覆盖被收集两次）。缺公共无参构造 → 警告后跳过（响亮失败，不静默丢）。
    /// </remarks>
    public static class GameFrameXRuntimeOverrideProvider
    {
        private static readonly List<IGameFrameXRuntimeOverrideProvider> Providers = new List<IGameFrameXRuntimeOverrideProvider>(32);

        /// <summary>
        /// 注册运行时覆盖 Provider。
        /// </summary>
        /// <param name="provider">运行时覆盖 Provider。</param>
        public static void RegisterProvider(IGameFrameXRuntimeOverrideProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            Providers.Add(provider);
        }

        /// <summary>
        /// 退订运行时覆盖 Provider。
        /// </summary>
        /// <param name="provider">运行时覆盖 Provider。</param>
        public static void UnregisterProvider(IGameFrameXRuntimeOverrideProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            Providers.Remove(provider);
        }

        /// <summary>
        /// 清空全部运行时覆盖 Provider。
        /// </summary>
        public static void ClearProviders()
        {
            Providers.Clear();
        }

        /// <summary>
        /// 收集全部运行时覆盖（手动注册的先应用，扫描发现的按类型去重后应用）。
        /// </summary>
        /// <returns>运行时覆盖上下文。</returns>
        public static GameFrameXRuntimeOverrideContext CollectOverrides()
        {
            var context = new GameFrameXRuntimeOverrideContext();
            var appliedTypes = new HashSet<Type>();

            foreach (var provider in Providers)
            {
                ApplyProvider(provider, context);
                if (provider != null)
                {
                    appliedTypes.Add(provider.GetType());
                }
            }

            var scanResult = GameFrameXRuntimeScanner.Scan();
            foreach (var providerType in scanResult.OverrideProviderTypes)
            {
                if (appliedTypes.Contains(providerType))
                {
                    continue;
                }

                IGameFrameXRuntimeOverrideProvider discovered;
                try
                {
                    discovered = (IGameFrameXRuntimeOverrideProvider)Activator.CreateInstance(providerType);
                }
                catch (Exception exception)
                {
                    GD.PushWarning(Utility.Text.Format(
                        "[GameFrameX] Runtime override provider '{0}' can not be instantiated (public parameterless constructor required): {1}",
                        providerType.FullName,
                        exception.Message));
                    continue;
                }

                ApplyProvider(discovered, context);
                appliedTypes.Add(providerType);
            }

            foreach (var error in scanResult.Errors)
            {
                GD.PushWarning("[GameFrameX] Runtime override provider scan: " + error);
            }

            return context;
        }

        private static void ApplyProvider(IGameFrameXRuntimeOverrideProvider provider, GameFrameXRuntimeOverrideContext context)
        {
            if (provider == null)
            {
                return;
            }

            try
            {
                provider.CollectOverrides(context);
            }
            catch (Exception exception)
            {
                GD.PushWarning(Utility.Text.Format(
                    "[GameFrameX] Runtime override provider '{0}' threw during CollectOverrides and was skipped: {1}",
                    provider.GetType().FullName,
                    exception.Message));
            }
        }
    }
}
