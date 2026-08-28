namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 过滤规则(枚举子集,语义对齐 YooAsset Unity 版 DefaultFilterRule,后缀按 Godot 资源类型适配)
    /// </summary>
    public enum EFilterRule
    {
        /// <summary>
        /// 收集所有资源文件
        /// </summary>
        CollectAll,

        /// <summary>
        /// 只收集场景文件(Godot 下为 .tscn)
        /// </summary>
        CollectScene,

        /// <summary>
        /// 只收集纹理文件(.png/.jpg/.jpeg/.webp/.svg/.bmp)
        /// </summary>
        CollectTexture,

        /// <summary>
        /// 只收集着色器文件(Godot 下为 .gdshader)
        /// </summary>
        CollectShader,
    }
}
