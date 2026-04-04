using System;
using System.Text;
using UnityEngine;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public enum ERemoteCommand
    {
        /// <summary>
        /// 采样一次
        /// </summary>
        SampleOnce = 0,
    }

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class RemoteCommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        public int CommandType;

        /// <summary>
        /// 命令附加参数
        /// </summary>
        public string CommandParam;


        /// <summary>
        /// 序列化
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static byte[] Serialize(RemoteCommand command)
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(command));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static RemoteCommand Deserialize(byte[] data)
        {
            return JsonUtility.FromJson<RemoteCommand>(Encoding.UTF8.GetString(data));
        }

        [UnityEngine.Scripting.Preserve]
        public static bool TryParseCommandType(string command, out int commandType)
        {
            commandType = -1;
            if (string.IsNullOrEmpty(command))
            {
                return false;
            }

            var normalized = command.Trim().ToLowerInvariant();
            if (normalized == "sample_once")
            {
                commandType = (int)ERemoteCommand.SampleOnce;
                return true;
            }

            return int.TryParse(normalized, out commandType);
        }

        [UnityEngine.Scripting.Preserve]
        public static string ToCommandName(int commandType)
        {
            if (commandType == (int)ERemoteCommand.SampleOnce)
            {
                return "sample_once";
            }

            return commandType.ToString();
        }
    }
}
