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

using System;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面接口。
    /// </summary>
    public interface IUIForm
    {
        /// <summary>
        /// 界面回收开始时间
        /// </summary>
        DateTime ReleaseStartTime { get; }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        int SerialId { get; }

        /// <summary>
        /// 获取界面完整名称。
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        string UIFormAssetName { get; }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        string AssetPath { get; }

        /// <summary>
        /// 是否禁用回收，禁用回收的界面不会被回收
        /// </summary>
        bool IsDisableRecycling { get; }

        /// <summary>
        /// 是否禁用关闭，禁用关闭的界面不会被关闭
        /// </summary>
        bool IsDisableClosing { get; }

        /// <summary>
        /// 是否可以回收，true:界面可以被回收，false:界面不可以被回收
        /// </summary>
        bool IsCanRecycle { get; }

        /// <summary>
        /// 界面回收间隔，单位：秒
        /// </summary>
        int RecycleInterval { get; }

        /// <summary>
        /// 是否开启组件居中，true:组件生成后默认父组件居中
        /// </summary>
        bool IsCenter { get; }

        /// <summary>
        /// 获取界面实例。
        /// </summary>
        object Handle { get; }

        /// <summary>
        /// 获取界面是否可用。
        /// </summary>
        bool Available { get; }

        /// <summary>
        /// 是否启用显示动画
        /// </summary>
        bool EnableShowAnimation { get; set; }

        /// <summary>
        /// 显示动画名称
        /// </summary>
        string ShowAnimationName { get; set; }

        /// <summary>
        /// 是否启用隐藏动画
        /// </summary>
        bool EnableHideAnimation { get; set; }

        /// <summary>
        /// 隐藏动画名称
        /// </summary>
        string HideAnimationName { get; set; }

        /// <summary>
        /// 获取界面是否可见。
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        IUIGroup UIGroup { get; set; }

        /// <summary>
        /// 获取界面在界面组中的深度。
        /// </summary>
        int DepthInUIGroup { get; }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        bool PauseCoveredUIForm { get; }

        /// <summary>
        /// 获取是否唤醒过
        /// </summary>
        bool IsAwake { get; }

        /// <summary>
        /// 界面初始化前执行
        /// </summary>
        void OnAwake();

        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="onInitAction">初始化界面前的委托。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="recycleInterval">界面回收间隔，单位：秒</param>
        void Init(int serialId, string uiFormAssetName, IUIGroup uiGroup, Action<IUIForm> onInitAction, bool pauseCoveredUIForm, bool isNewInstance, object userData, int recycleInterval, bool isFullScreen = false);

        /// <summary>
        /// 界面初始化。
        /// </summary>
        void OnInit();

        /// <summary>
        /// 界面回收。
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        void OnOpen(object userData);

        /// <summary>
        /// 绑定事件
        /// </summary>
        void BindEvent();

        /// <summary>
        /// 加载数据
        /// </summary>
        void LoadData();

        /// <summary>
        /// 界面更新本地化。
        /// </summary>
        void UpdateLocalization();

        /// <summary>
        /// 界面显示。
        /// </summary>
        /// <param name="handler">界面显示处理接口</param>
        /// <param name="complete">完成回调</param>
        void Show(IUIFormShowHandler handler, Action complete);

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        void OnClose(bool isShutdown, object userData);

        /// <summary>
        /// 界面隐藏。
        /// </summary>
        /// <param name="handler">界面隐藏处理接口</param>
        /// <param name="complete">完成回调</param>
        void Hide(IUIFormHideHandler handler, Action complete);

        /// <summary>
        /// 界面暂停。
        /// </summary>
        void OnPause();

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        void OnResume();

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        void OnCover();

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        void OnReveal();

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        void OnRefocus(object userData);

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void OnUpdate(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        void OnDepthChanged(int uiGroupDepth, int depthInUIGroup);
    }
}
