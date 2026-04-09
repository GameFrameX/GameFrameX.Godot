using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 资源系统调试信息
    /// </summary>
    [AssetSystemPreserve]
    [Serializable]
    public class DebugReport
    {
        /// <summary>
        /// 游戏帧
        /// </summary>
        public int FrameCount;

        /// <summary>
        /// 调试的包裹数据列表
        /// </summary>
        public List<DebugPackageData> PackageDatas = new(10);


        /// <summary>
        /// 序列化
        /// </summary>
        [AssetSystemPreserve]
        public static byte[] Serialize(DebugReport debugReport)
        {
            return Encoding.UTF8.GetBytes(AssetSystemJson.ToJson(debugReport));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        [AssetSystemPreserve]
        public static DebugReport Deserialize(byte[] data)
        {
            return AssetSystemJson.FromJson<DebugReport>(Encoding.UTF8.GetString(data));
        }
    }
}
