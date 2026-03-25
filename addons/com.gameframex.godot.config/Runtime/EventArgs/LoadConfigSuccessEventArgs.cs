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
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Config.Runtime
{
    /// <summary>
    /// 加载全局配置成功事件。
    /// </summary>
    public sealed class LoadConfigSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 加载全局配置成功事件编号。
        /// </summary>
        public static readonly string EventId = typeof(LoadConfigSuccessEventArgs).FullName;

        /// <summary>
        /// 初始化加载全局配置成功事件编号的新实例。
        /// </summary>
        public LoadConfigSuccessEventArgs()
        {
            ConfigAssetName = null;
            Duration = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取加载全局配置成功事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 获取全局配置资源名称。
        /// </summary>
        public string ConfigAssetName { get; private set; }

        /// <summary>
        /// 获取加载持续时间。
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// 创建加载全局配置成功事件。
        /// </summary>
        /// <param name="dataAssetName"></param>
        /// <param name="duration"></param>
        /// <param name="userData"></param>
        /// <returns>创建的加载全局配置成功事件。</returns>
        public static LoadConfigSuccessEventArgs Create(string dataAssetName, float duration, object userData)
        {
            LoadConfigSuccessEventArgs loadConfigSuccessEventArgs = ReferencePool.Acquire<LoadConfigSuccessEventArgs>();
            loadConfigSuccessEventArgs.ConfigAssetName = dataAssetName;
            loadConfigSuccessEventArgs.Duration = duration;
            loadConfigSuccessEventArgs.UserData = userData;
            return loadConfigSuccessEventArgs;
        }

        /// <summary>
        /// 清理加载全局配置成功事件。
        /// </summary>
        public override void Clear()
        {
            ConfigAssetName = null;
            Duration = 0f;
            UserData = null;
        }
    }
}