using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Web.ProtoBuff.Runtime
{
    public sealed partial class WebProtoBuffComponent : GameFrameworkComponent
    {
        private IWebProtoBuffManager m_WebProtoBuffManager;

        [Export(PropertyHint.Range, "0.5,120,0.1")]
        private float m_Timeout = 5f;

        public float Timeout
        {
            get { return m_WebProtoBuffManager.Timeout; }
            set { m_WebProtoBuffManager.Timeout = m_Timeout = value; }
        }

        public override void _Ready()
        {
            if (string.IsNullOrEmpty(componentType))
            {
                // 代码动态创建（LauncherAuto 场景）无场景序列化值，空值兜底默认实现，避免注册 Guard 中断
                componentType = typeof(WebProtoBuffManager).FullName;
            }
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IWebProtoBuffManager);
            base._Ready();
            m_WebProtoBuffManager = GameFrameworkEntry.GetModule<IWebProtoBuffManager>();
            if (m_WebProtoBuffManager == null)
            {
                Log.Fatal("Web ProtoBuff manager is invalid.");
                return;
            }

            m_WebProtoBuffManager.Timeout = m_Timeout;
        }

#if ENABLE_GAME_FRAME_X_WEB_PROTOBUF_NETWORK
        public Task<T> Post<T>(string url, GameFrameX.Network.Runtime.MessageObject message) where T : GameFrameX.Network.Runtime.MessageObject, GameFrameX.Network.Runtime.IResponseMessage
        {
            return m_WebProtoBuffManager.Post<T>(url, message);
        }
#endif
    }
}
