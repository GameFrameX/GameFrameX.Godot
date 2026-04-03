using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// 扩展图片组件，支持通过资源路径设置图标。
    /// </summary>
    public partial class UIImage : TextureRect
    {
        private string m_Icon;

        /// <summary>
        /// 获取或设置图标资源路径。
        /// </summary>
        [Export]
        public string Icon
        {
            get { return m_Icon; }
            set
            {
                if (m_Icon == value)
                {
                    return;
                }

                m_Icon = value;
                _ = this.SetIconAsync(m_Icon);
            }
        }
    }
}
