using System.Threading.Tasks;

namespace GameFrameX.Web.ProtoBuff.Runtime
{
    public interface IWebProtoBuffManager
    {
#if ENABLE_GAME_FRAME_X_WEB_PROTOBUF_NETWORK
        Task<T> Post<T>(string url, GameFrameX.Network.Runtime.MessageObject message) where T : GameFrameX.Network.Runtime.MessageObject, GameFrameX.Network.Runtime.IResponseMessage;
#endif
        float Timeout { get; set; }
    }
}
