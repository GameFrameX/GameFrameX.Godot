using Godot;

namespace LuBan.Runtime
{
    /// <summary>
    /// 防止代码运行时发生裁剪。将这个脚本添加到启动场景中，不会对逻辑有任何影响。
    /// Unity 版为 UnityEngine.MonoBehaviour + Start；Godot 版等价替换为 Node + _Ready，
    /// 并去除 UnityEngine.Scripting.Preserve（Godot/.NET 无 IL2CPP 裁剪管理）。
    /// </summary>
    public partial class LuBanCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(LuBan.Runtime.BeanBase);
            _ = typeof(LuBan.Runtime.EDeserializeError);
            _ = typeof(LuBan.Runtime.SerializationException);
            _ = typeof(LuBan.Runtime.SegmentSaveState);
            _ = typeof(LuBan.Runtime.ByteBuf);
            _ = typeof(LuBan.Runtime.ITypeId);
            _ = typeof(LuBan.Runtime.StringUtil);
        }
    }
}
