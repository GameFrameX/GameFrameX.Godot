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
    /// 管理 GameFrameX 自动运行时 Host（唯一启动路径）。
    /// </summary>
    /// <remarks>
    /// 接管语义：包被引用即由本 Host 拥有框架组件所有权。场景里手工摆放的 GFX 组件在 <see cref="IsAutoInjectionClaimed"/>
    /// 为真时自我抑制并告警，不注册、不驱动、不关停会话——框架的组件所有权始终唯一。
    /// Godot 时序对应：Host 节点挂在 SceneTree 根下，场景切换只替换当前场景，Host 天然常驻（等价 Unity DontDestroyOnLoad）。
    /// </remarks>
    public static class GameFrameXRuntimeHost
    {
        private const string HostName = "[GameFrameX]";

        private static readonly Dictionary<Type, GameFrameXComponentRuntimeConfig> PendingConfigs = new Dictionary<Type, GameFrameXComponentRuntimeConfig>(32);

        private static Node s_Host;

        private static bool s_Started;

        private static bool s_AutoInjectionClaimed;

        private static GameFrameXRuntimePlan s_LastPlan;

        /// <summary>
        /// 获取是否已宣告自动注入接管（Bootstrap 在主场景加载前置位、Host 启动时清除）。
        /// </summary>
        /// <remarks>
        /// 场景内组件的 <c>_Ready</c> 据此判定自身是否为残留入口：为真即自我抑制。
        /// </remarks>
        public static bool IsAutoInjectionClaimed
        {
            get { return s_AutoInjectionClaimed; }
        }

        /// <summary>
        /// 获取最近一次启动计划。
        /// </summary>
        public static GameFrameXRuntimePlan LastPlan
        {
            get { return s_LastPlan; }
        }

        /// <summary>
        /// 宣告框架将接管全部 GFX 组件（由 <see cref="GameFrameXRuntimeBootstrap"/> 在主场景加载前调用）。
        /// </summary>
        /// <remarks>
        /// 宣告后、<see cref="EnsureStarted"/> 清标记前 <c>_Ready</c> 的 GFX 组件一律视为场景残留并被抑制。
        /// 业务代码不应调用本方法——它是 Bootstrap 与组件抑制之间的内部契约，公开仅为可测试性。
        /// </remarks>
        public static void ClaimAutoInjection()
        {
            s_AutoInjectionClaimed = true;
        }

        /// <summary>
        /// 重置自动运行时 Host 状态。
        /// </summary>
        public static void Reset()
        {
            if (s_Host != null && GodotObject.IsInstanceValid(s_Host))
            {
                s_Host.QueueFree();
            }

            s_Host = null;
            s_Started = false;
            s_AutoInjectionClaimed = false;
            s_LastPlan = null;
            PendingConfigs.Clear();
            GameFrameXRuntimeManagerResolver.Reset();
        }

        /// <summary>
        /// 确保自动运行时已经启动：创建 <c>[GameFrameX]</c> Host 并挂载扫描到的全部组件。
        /// </summary>
        /// <remarks>
        /// 幂等：重复调用只追加一条诊断。启动前先清除接管标记，使 Host 自己挂载的组件正常注册。
        /// Host 在加入场景树之前挂好全部组件子节点，再加入根节点——组件 <c>_Ready</c> 统一在入树时按挂载顺序触发
        /// （等价 Unity Host 先 SetActive(false) 挂组件再 SetActive(true) 的语义）。
        /// </remarks>
        public static void EnsureStarted()
        {
            if (s_Started)
            {
                if (s_LastPlan != null)
                {
                    s_LastPlan.Diagnostics.Add("Auto runtime startup was requested again and skipped.");
                    WriteDiagnostics();
                }

                return;
            }

            // 清标记必须早于创建 Host：场景内残留组件已在更早的 _Ready 阶段抑制完毕，
            // 此后 _Ready 的组件只可能是 Host 自己挂载的，必须正常注册。
            s_AutoInjectionClaimed = false;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var scanResult = GameFrameXRuntimeScanner.Scan();
            var overrideContext = GameFrameXRuntimeOverrideProvider.CollectOverrides();
            GameFrameXRuntimeManagerResolver.RegisterDescriptors(scanResult.ManagerDescriptors);
            GameFrameXRuntimeManagerResolver.RegisterOverrides(overrideContext.GetManagerOverrides());

            s_LastPlan = GameFrameXRuntimePlanner.CreatePlan(scanResult, overrideContext);
            s_LastPlan.Diagnostics.Add(Utility.Text.Format(
                "Scan result: {0} component(s), {1} manager implementation(s), {2} manager override(s).",
                scanResult.ComponentTypes.Count,
                scanResult.ManagerDescriptors.Count,
                overrideContext.GetManagerOverrides().Count));

            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree == null)
            {
                s_LastPlan.Diagnostics.Add("Auto runtime requires a SceneTree main loop; startup aborted.");
                WriteDiagnostics();
                return;
            }

            s_Host = new Node();
            s_Host.Name = HostName;

            foreach (var descriptor in s_LastPlan.Components)
            {
                AddComponent(descriptor);
            }

            // 挂载全部组件后入树：组件 _Ready 按子节点顺序统一触发；根下节点跨场景切换常驻。
            sceneTree.Root.AddChild(s_Host);
            s_Started = true;
            s_LastPlan.Diagnostics.Add(Utility.Text.Format("Startup elapsed: {0} ms.", stopwatch.ElapsedMilliseconds));
            WriteDiagnostics();
        }

        /// <summary>
        /// 获取或创建 Host 下的分组节点（按 <c>/</c> 分层，逐段查找，缺段即建纯组织节点）。
        /// </summary>
        /// <remarks>
        /// 供卫星包把自建常驻节点归位到 <c>[GameFrameX]</c> 层级（如 <c>UI/Keyboard</c>）。
        /// Host 启动前调用返回 null，调用方应在后续帧重试；已存在的同名节点直接复用，绝不重复创建。
        /// 分组节点不承载组件，生命周期与 Host 一致。
        /// </remarks>
        /// <param name="nodePath">分组节点路径（相对于 Host，<c>/</c> 分层，如 <c>UI/Keyboard</c>）。</param>
        /// <returns>分组节点；Host 未启动或路径无有效段时为 null。</returns>
        public static Node GetOrCreateHostNode(string nodePath)
        {
            if (string.IsNullOrEmpty(nodePath) || s_Host == null || !GodotObject.IsInstanceValid(s_Host))
            {
                return null;
            }

            Node current = s_Host;
            var matched = false;
            foreach (var segment in nodePath.Split('/'))
            {
                if (segment.Length == 0)
                {
                    continue;
                }

                var child = current.GetNodeOrNull(segment);
                if (child == null)
                {
                    var childNode = new Node();
                    childNode.Name = segment;
                    current.AddChild(childNode);
                    child = childNode;
                }

                current = child;
                matched = true;
            }

            return matched ? current : null;
        }

        /// <summary>
        /// 获取 Host 根节点（未启动时为 null）。
        /// </summary>
        public static Node Host
        {
            get { return s_Host != null && GodotObject.IsInstanceValid(s_Host) ? s_Host : null; }
        }

        /// <summary>
        /// 获取组件待应用的运行时配置。
        /// </summary>
        /// <param name="componentType">组件类型。</param>
        /// <returns>运行时配置。</returns>
        internal static GameFrameXComponentRuntimeConfig GetPendingConfig(Type componentType)
        {
            if (componentType == null)
            {
                return null;
            }

            PendingConfigs.TryGetValue(componentType, out var config);
            return config;
        }

        /// <summary>
        /// 禁用场景中残留的 GFX 组件入口并输出告警（由组件在 _Ready 中经 <see cref="IsAutoInjectionClaimed"/> 判定后调用）。
        /// </summary>
        /// <param name="component">被抑制的组件。</param>
        internal static void SuppressSceneEntry(GameFrameworkComponent component)
        {
            if (component == null)
            {
                return;
            }

            component.SetProcess(false);
            GD.PushWarning(Utility.Text.Format(
                "[GameFrameX] Scene entry '{0}' on '{1}' was suppressed because the auto runtime owns all GameFrameX components. Remove the hand-placed GameFrameX node from the scene; the framework injects it automatically.",
                component.GetType().FullName,
                component.GetPath()));
        }

        private static void AddComponent(GameFrameXRuntimeComponentDescriptor descriptor)
        {
            if (descriptor == null || descriptor.ComponentType == null)
            {
                return;
            }

            if (descriptor.Config != null)
            {
                PendingConfigs[descriptor.ComponentType] = descriptor.Config;
            }

            // Host 子树上可能已存在同类型组件（前序组件手动补挂的兄弟组件），此时直接复用该实例并应用配置，
            // 避免再创建一个会在注册阶段被判重复的实例。
            var existingComponent = s_Host != null ? FindComponentOfType(s_Host, descriptor.ComponentType) : null;
            if (existingComponent != null)
            {
                if (existingComponent is IGameFrameXComponentRuntimeConfigurable configurable && descriptor.Config != null)
                {
                    configurable.ApplyRuntimeConfig(descriptor.Config);
                }

                return;
            }

            var componentNodeName = GetComponentNodeName(descriptor.ComponentType);
            var component = Activator.CreateInstance(descriptor.ComponentType) as GameFrameworkComponent;
            if (component == null)
            {
                s_LastPlan.Diagnostics.Add(Utility.Text.Format(
                    "Component '{0}' can not be instantiated (public parameterless constructor required).",
                    descriptor.ComponentType.FullName));
                return;
            }

            component.Name = componentNodeName;
            if (component is IGameFrameXComponentRuntimeConfigurable configurable2 && descriptor.Config != null)
            {
                configurable2.ApplyRuntimeConfig(descriptor.Config);
            }

            s_Host.AddChild(component);
        }

        private static GameFrameworkComponent FindComponentOfType(Node root, Type componentType)
        {
            foreach (var child in root.GetChildren())
            {
                if (componentType.IsAssignableFrom(child.GetType()))
                {
                    return child as GameFrameworkComponent;
                }

                var nested = FindComponentOfType(child, componentType);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static string GetComponentNodeName(Type componentType)
        {
            const string componentSuffix = "Component";
            var componentName = componentType.Name;
            if (componentName.Length <= componentSuffix.Length || !componentName.EndsWith(componentSuffix, StringComparison.Ordinal))
            {
                return componentName;
            }

            return componentName.Substring(0, componentName.Length - componentSuffix.Length);
        }

        private static void WriteDiagnostics()
        {
            if (s_LastPlan == null)
            {
                return;
            }

            GD.Print(Utility.Text.Format("GameFrameX auto runtime started. Components: {0}.", s_LastPlan.Components.Count));
            foreach (var component in s_LastPlan.Components)
            {
                GD.Print(Utility.Text.Format(
                    "GameFrameX auto runtime enabled component: {0}, node: {1}/{2}, order: {3}.",
                    component.ComponentType.FullName,
                    HostName,
                    GetComponentNodeName(component.ComponentType),
                    component.Order));
            }

            foreach (var diagnostic in s_LastPlan.Diagnostics)
            {
                GD.PushWarning(Utility.Text.Format("GameFrameX auto runtime: {0}", diagnostic));
            }
        }
    }
}
