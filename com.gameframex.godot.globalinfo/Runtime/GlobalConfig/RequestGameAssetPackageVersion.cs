namespace GameFrameX.GlobalConfig.Runtime
{
    /// <summary>
    /// 游戏资源版本请求对象,可以自己继承实现自己的字段
    /// </summary>
    public class RequestGameAssetPackageVersion : RequestBase
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string AssetPackageName { get; set; }
    }
}