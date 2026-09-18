namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 寻址规则(枚举子集,语义对齐 GameFrameX Godot 版 DefaultAddressRule)
    /// </summary>
    public enum EAddressRule
    {
        /// <summary>
        /// 不启用寻址,定位地址为空。
        /// </summary>
        AddressDisable,

        /// <summary>
        /// 以文件名(去扩展名)作为定位地址。
        /// </summary>
        AddressByFileName,

        /// <summary>
        /// 以 分组名_文件名 作为定位地址。
        /// </summary>
        AddressByGroupAndFileName,

        /// <summary>
        /// 以资源路径作为定位地址。
        /// </summary>
        AddressByAssetPath,
    }
}
