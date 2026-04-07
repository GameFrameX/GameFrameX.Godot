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
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository:  https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Procedure.Runtime
{
    /// <summary>
    /// 流程组件。
    /// </summary>
    public sealed partial class ProcedureComponent : GameFrameworkComponent
    {
        private IProcedureManager m_ProcedureManager = null;
        private ProcedureBase m_EntranceProcedure = null;

        [Export] private string[] m_AvailableProcedureTypeNames = Array.Empty<string>();
        [Export] private string m_EntranceProcedureTypeName = string.Empty;

        /// <summary>
        /// 获取当前流程管理器。
        /// </summary>
        public IProcedureManager Procedure
        {
            get { return m_ProcedureManager; }
        }

        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get { return m_ProcedureManager.CurrentProcedure; }
        }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        public float CurrentProcedureTime
        {
            get { return m_ProcedureManager.CurrentProcedureTime; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            ImplementationComponentType = Type.GetType(componentType);
            InterfaceComponentType = typeof(IProcedureManager);
            base._Ready();
            m_ProcedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            if (m_ProcedureManager == null)
            {
                Log.Fatal("Procedure manager is invalid.");
                return;
            }

            CallDeferred(nameof(StartProcedureInternal));
        }

        /// <summary>
        /// 启动流程。
        /// </summary>
        private void StartProcedureInternal()
        {
            var availableProcedureTypeNames = BuildValidProcedureTypeNames();
            if (availableProcedureTypeNames.Length == 0)
            {
                Log.Error("Available procedures are empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(m_EntranceProcedureTypeName))
            {
                Log.Error("Entrance procedure is invalid.");
                return;
            }

            if (!Array.Exists(availableProcedureTypeNames, m => string.Equals(m, m_EntranceProcedureTypeName, StringComparison.Ordinal)))
            {
                Log.Error("Entrance procedure '{0}' is not in available procedures.", m_EntranceProcedureTypeName);
                return;
            }

            m_AvailableProcedureTypeNames = availableProcedureTypeNames;
            m_EntranceProcedure = null;
            ProcedureBase[] procedures = new ProcedureBase[availableProcedureTypeNames.Length];
            for (int i = 0; i < availableProcedureTypeNames.Length; i++)
            {
                Type procedureType = Utility.Assembly.GetType(availableProcedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error("Can not find procedure type '{0}'.", availableProcedureTypeNames[i]);
                    return;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error("Can not create procedure instance '{0}'.", availableProcedureTypeNames[i]);
                    return;
                }

                if (m_EntranceProcedureTypeName == availableProcedureTypeNames[i])
                {
                    m_EntranceProcedure = procedures[i];
                }
            }

            if (m_EntranceProcedure == null)
            {
                Log.Error("Entrance procedure is invalid.");
                return;
            }

            m_ProcedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);
            m_ProcedureManager.StartProcedure(m_EntranceProcedure.GetType());
        }

        private string[] BuildValidProcedureTypeNames()
        {
            if (m_AvailableProcedureTypeNames == null || m_AvailableProcedureTypeNames.Length == 0)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>(m_AvailableProcedureTypeNames.Length);
            var deduplicate = new HashSet<string>(StringComparer.Ordinal);
            foreach (var typeName in m_AvailableProcedureTypeNames)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                if (!deduplicate.Add(typeName))
                {
                    continue;
                }

                result.Add(typeName);
            }

            const string configProcedureTypeName = "Godot.Startup.Procedure.ProcedureConfigState";
            if (!deduplicate.Contains(configProcedureTypeName))
            {
                Type configProcedureType = Utility.Assembly.GetType(configProcedureTypeName);
                if (configProcedureType != null && typeof(ProcedureBase).IsAssignableFrom(configProcedureType))
                {
                    deduplicate.Add(configProcedureTypeName);
                    result.Add(configProcedureTypeName);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            return m_ProcedureManager.HasProcedure<T>();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure(Type procedureType)
        {
            return m_ProcedureManager.HasProcedure(procedureType);
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            return m_ProcedureManager.GetProcedure<T>();
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            return m_ProcedureManager.GetProcedure(procedureType);
        }
    }
}
