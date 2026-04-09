using System;
using System.Text;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public enum ERemoteCommand
    {
        /// <summary>
        /// 采样一次
        /// </summary>
        SampleOnce = 0,
    }

    [AssetSystemPreserve]
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
        [AssetSystemPreserve]
        public static byte[] Serialize(RemoteCommand command)
        {
            return Encoding.UTF8.GetBytes(AssetSystemJson.ToJson(command));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        [AssetSystemPreserve]
        public static RemoteCommand Deserialize(byte[] data)
        {
            return AssetSystemJson.FromJson<RemoteCommand>(Encoding.UTF8.GetString(data));
        }

        [AssetSystemPreserve]
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

        [AssetSystemPreserve]
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
