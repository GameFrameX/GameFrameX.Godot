// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//   This project is dual-licensed under the MIT License and Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any legal disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:  https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Godot.Startup.Procedure;

/// <summary>
/// 配置加载流程（预留配置表/本地化等初始化入口）。
/// </summary>
public sealed class ProcedureConfigState : ProcedureBase
{
    private const string ConfigRootPath = "res://Assets/Bundles/Config";

    /// <summary>
    /// 进入流程时执行。
    /// </summary>
    /// <param name="procedureOwner">流程持有者。</param>
    protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        Log.Info("进入流程：ProcedureConfigState");

        bool loaded = TryLoadConfigTables(out string summary);
        if (loaded)
        {
            Log.Info("[Config] {0}", summary);
        }
        else
        {
            Log.Warning("[Config] {0}", summary);
        }

        ChangeState<ProcedureGameLauncherState>(procedureOwner);
    }

    private static bool TryLoadConfigTables(out string summary)
    {
        try
        {
            string configRoot = ProjectSettings.GlobalizePath(ConfigRootPath);
            if (!Directory.Exists(configRoot))
            {
                summary = $"配置目录不存在：{configRoot}";
                return false;
            }

            string[] files = Directory
                .EnumerateFiles(configRoot, "*.json", SearchOption.AllDirectories)
                .OrderBy(static m => m, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (files.Length == 0)
            {
                summary = $"配置目录为空：{configRoot}";
                return false;
            }

            int totalRows = 0;
            foreach (string filePath in files)
            {
                string jsonText = File.ReadAllText(filePath);
                using JsonDocument jsonDocument = JsonDocument.Parse(jsonText);
                int rowCount = GetRowCount(jsonDocument.RootElement);
                totalRows += rowCount;
                Log.Info("[Config] Loaded table: {0}, rows={1}", filePath, rowCount);
            }

            summary = $"配置表加载完成。tableCount={files.Length}, totalRows={totalRows}, root={configRoot}";
            return true;
        }
        catch (Exception exception)
        {
            summary = $"配置表加载异常：{exception.Message}";
            return false;
        }
    }

    private static int GetRowCount(JsonElement rootElement)
    {
        if (rootElement.ValueKind == JsonValueKind.Array)
        {
            return rootElement.GetArrayLength();
        }

        if (rootElement.ValueKind == JsonValueKind.Object)
        {
            return rootElement.EnumerateObject().Count();
        }

        return 0;
    }
}
