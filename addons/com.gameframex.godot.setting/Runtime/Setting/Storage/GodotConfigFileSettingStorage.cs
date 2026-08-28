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
using System.IO;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Setting.Runtime
{
    /// <summary>
    /// 基于 Godot ConfigFile 的默认配置存储后端。
    /// </summary>
    internal sealed class GodotConfigFileSettingStorage : ISettingStorageBackend
    {
        private const string ConfigFilePath = "user://GameFrameXPlayerPrefs.cfg";
        private const string SectionName = "Settings";

        private ConfigFile m_ConfigFile;

        /// <summary>
        /// 惰性获取 ConfigFile 实例，避免在无 Godot 原生运行时的环境（如单元测试）中构造时崩溃。
        /// </summary>
        private ConfigFile ConfigFileInstance
        {
            get
            {
                if (m_ConfigFile == null)
                {
                    m_ConfigFile = new ConfigFile();
                }

                return m_ConfigFile;
            }
        }

        /// <summary>
        /// 加载存储文件。
        /// </summary>
        /// <returns>是否加载成功。</returns>
        public bool Load()
        {
            Error error = ConfigFileInstance.Load(ConfigFilePath);
            return error == Error.Ok || error == Error.FileNotFound;
        }

        /// <summary>
        /// 保存存储文件。
        /// </summary>
        /// <returns>是否保存成功。</returns>
        public bool Save()
        {
            Error error = ConfigFileInstance.Save(ConfigFilePath);
            if (error != Error.Ok)
            {
                Log.Warning("Save settings failure with error code '{0}'.", error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否存在指定键。
        /// </summary>
        /// <param name="key">要检查的键。</param>
        /// <returns>是否存在指定键。</returns>
        public bool HasKey(string key)
        {
            return ConfigFileInstance.HasSectionKey(SectionName, key);
        }

        /// <summary>
        /// 删除指定键。
        /// </summary>
        /// <param name="key">要删除的键。</param>
        /// <returns>键存在并删除时返回 true，键不存在时返回 false。</returns>
        public bool DeleteKey(string key)
        {
            if (!HasKey(key))
            {
                return false;
            }

            ConfigFileInstance.EraseSectionKey(SectionName, key);
            return true;
        }

        /// <summary>
        /// 删除所有键并移除存储文件。
        /// </summary>
        public void DeleteAll()
        {
            m_ConfigFile = new ConfigFile();
            string absolutePath = ProjectSettings.GlobalizePath(ConfigFilePath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }

        /// <summary>
        /// 读取整数。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <returns>读取的整数。</returns>
        public int GetInt(string key, int defaultValue)
        {
            return Convert.ToInt32(ConfigFileInstance.GetValue(SectionName, key, defaultValue));
        }

        /// <summary>
        /// 写入整数。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public void SetInt(string key, int value)
        {
            ConfigFileInstance.SetValue(SectionName, key, value);
        }

        /// <summary>
        /// 读取浮点数。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <returns>读取的浮点数。</returns>
        public float GetFloat(string key, float defaultValue)
        {
            return Convert.ToSingle(ConfigFileInstance.GetValue(SectionName, key, defaultValue));
        }

        /// <summary>
        /// 写入浮点数。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public void SetFloat(string key, float value)
        {
            ConfigFileInstance.SetValue(SectionName, key, value);
        }

        /// <summary>
        /// 读取字符串。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="defaultValue">默认值。</param>
        /// <returns>读取的字符串。</returns>
        public string GetString(string key, string defaultValue)
        {
            return Convert.ToString(ConfigFileInstance.GetValue(SectionName, key, defaultValue));
        }

        /// <summary>
        /// 写入字符串。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public void SetString(string key, string value)
        {
            ConfigFileInstance.SetValue(SectionName, key, value);
        }
    }
}
