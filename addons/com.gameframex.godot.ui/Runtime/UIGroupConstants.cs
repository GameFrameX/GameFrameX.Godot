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

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面组常量
    /// </summary>
    public static class UIGroupConstants
    {
        /// <summary>
        /// 隐藏
        /// </summary>
        public static readonly UIGroupDefine Hidden = new UIGroupDefine(40, UIGroupNameConstants.Hidden);

        /// <summary>
        /// 背景
        /// </summary>
        public static readonly UIGroupDefine Background = new UIGroupDefine(35, UIGroupNameConstants.Background);

        /// <summary>
        /// 场景
        /// </summary>
        public static readonly UIGroupDefine Scene = new UIGroupDefine(30, UIGroupNameConstants.Scene);

        /// <summary>
        /// 世界
        /// </summary>
        public static readonly UIGroupDefine World = new UIGroupDefine(27, UIGroupNameConstants.World);

        /// <summary>
        /// 战斗
        /// </summary>
        public static readonly UIGroupDefine Battle = new UIGroupDefine(25, UIGroupNameConstants.Battle);

        /// <summary>
        /// 头顶
        /// </summary>
        public static readonly UIGroupDefine Hud = new UIGroupDefine(22, UIGroupNameConstants.Hud);

        /// <summary>
        /// 地图
        /// </summary>
        public static readonly UIGroupDefine Map = new UIGroupDefine(20, UIGroupNameConstants.Map);

        /// <summary>
        /// 底板
        /// </summary>
        public static readonly UIGroupDefine Floor = new UIGroupDefine(15, UIGroupNameConstants.Floor);

        /// <summary>
        /// 正常
        /// </summary>
        public static readonly UIGroupDefine Normal = new UIGroupDefine(10, UIGroupNameConstants.Normal);

        /// <summary>
        /// 固定
        /// </summary>
        public static readonly UIGroupDefine Fixed = new UIGroupDefine(0, UIGroupNameConstants.Fixed);

        /// <summary>
        /// 窗口
        /// </summary>
        public static readonly UIGroupDefine Window = new UIGroupDefine(-10, UIGroupNameConstants.Window);

        /// <summary>
        /// 提示
        /// </summary>
        public static readonly UIGroupDefine Tip = new UIGroupDefine(-15, UIGroupNameConstants.Tip);

        /// <summary>
        /// 引导
        /// </summary>
        public static readonly UIGroupDefine Guide = new UIGroupDefine(-20, UIGroupNameConstants.Guide);

        /// <summary>
        /// 黑板
        /// </summary>
        public static readonly UIGroupDefine BlackBoard = new UIGroupDefine(-22, UIGroupNameConstants.BlackBoard);

        /// <summary>
        /// 对话
        /// </summary>
        public static readonly UIGroupDefine Dialogue = new UIGroupDefine(-23, UIGroupNameConstants.Dialogue);

        /// <summary>
        /// Loading
        /// </summary>
        public static readonly UIGroupDefine Loading = new UIGroupDefine(-25, UIGroupNameConstants.Loading);

        /// <summary>
        /// 通知
        /// </summary>
        public static readonly UIGroupDefine Notify = new UIGroupDefine(-30, UIGroupNameConstants.Notify);

        /// <summary>
        /// 系统顶级
        /// </summary>
        public static readonly UIGroupDefine System = new UIGroupDefine(-35, UIGroupNameConstants.System);
    }
}
