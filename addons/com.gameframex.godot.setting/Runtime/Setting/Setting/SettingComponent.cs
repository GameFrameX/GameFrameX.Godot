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
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Setting.Runtime
{
    /// <summary>
    /// 游戏配置组件。
    /// </summary>
    [GlobalClass]
    public sealed partial class SettingComponent : GameFrameworkComponent
    {
        [Export] private string m_SettingHelperTypeName = "GameFrameX.Setting.Runtime.DefaultSettingHelper";

        private ISettingManager m_SettingManager = null;

        /// <summary>
        /// 获取游戏配置项数量。
        /// </summary>
        public int Count => m_SettingManager.Count;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(ISettingManager);
            base._Ready();
            m_SettingManager = GameFrameworkEntry.GetModule<ISettingManager>();
            if (m_SettingManager == null)
            {
                Log.Fatal("Setting manager is invalid.");
                return;
            }

            ISettingHelper settingHelper = Helper.CreateHelper<SettingHelperBase>(this, m_SettingHelperTypeName, null, 0);
            if (settingHelper == null)
            {
                Log.Fatal("Setting helper is invalid.");
                return;
            }

            m_SettingManager.SetSettingHelper(settingHelper);
            CallDeferred(nameof(LoadSettingsInternal));
        }

        private void LoadSettingsInternal()
        {
            if (!m_SettingManager.Load())
            {
                Log.Warning("Load setting failure.");
            }
        }

        /// <summary>
        /// 保存游戏配置。
        /// </summary>
        public void Save()
        {
            m_SettingManager.Save();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public string[] GetAllSettingNames()
        {
            return m_SettingManager.GetAllSettingNames();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <param name="results">所有游戏配置项的名称。</param>
        public void GetAllSettingNames(List<string> results)
        {
            m_SettingManager.GetAllSettingNames(results);
        }

        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public bool HasSetting(string settingName)
        {
            return m_SettingManager.HasSetting(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public bool RemoveSetting(string settingName)
        {
            return m_SettingManager.RemoveSetting(settingName);
        }

        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        public void RemoveAllSettings()
        {
            m_SettingManager.RemoveAllSettings();
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        public bool GetBool(string settingName)
        {
            return m_SettingManager.GetBool(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        public bool GetBool(string settingName, bool defaultValue)
        {
            return m_SettingManager.GetBool(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入布尔值。
        /// </summary>
        public void SetBool(string settingName, bool value)
        {
            m_SettingManager.SetBool(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        public int GetInt(string settingName)
        {
            return m_SettingManager.GetInt(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        public int GetInt(string settingName, int defaultValue)
        {
            return m_SettingManager.GetInt(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入整数值。
        /// </summary>
        public void SetInt(string settingName, int value)
        {
            m_SettingManager.SetInt(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        public float GetFloat(string settingName)
        {
            return m_SettingManager.GetFloat(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        public float GetFloat(string settingName, float defaultValue)
        {
            return m_SettingManager.GetFloat(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入浮点数值。
        /// </summary>
        public void SetFloat(string settingName, float value)
        {
            m_SettingManager.SetFloat(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        public string GetString(string settingName)
        {
            return m_SettingManager.GetString(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        public string GetString(string settingName, string defaultValue)
        {
            return m_SettingManager.GetString(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入字符串值。
        /// </summary>
        public void SetString(string settingName, string value)
        {
            m_SettingManager.SetString(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        public T GetObject<T>(string settingName) where T : class, new()
        {
            return m_SettingManager.GetObject<T>(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        public object GetObject(Type objectType, string settingName)
        {
            return m_SettingManager.GetObject(objectType, settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        public T GetObject<T>(string settingName, T defaultObj) where T : class, new()
        {
            return m_SettingManager.GetObject(settingName, defaultObj);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            return m_SettingManager.GetObject(objectType, settingName, defaultObj);
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        public void SetObject<T>(string settingName, T obj) where T : class, new()
        {
            m_SettingManager.SetObject(settingName, obj);
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        public void SetObject(string settingName, object obj)
        {
            m_SettingManager.SetObject(settingName, obj);
        }
    }
}
