using System;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 控件路径标记特性。
    /// </summary>
    public sealed class GDGUIElementPropertyAttribute : Attribute
    {
        /// <summary>
        /// 初始化标记特性。
        /// </summary>
        /// <param name="path">控件节点路径。</param>
        public GDGUIElementPropertyAttribute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// 获取控件路径。
        /// </summary>
        public string Path { get; }
    }
}
