using System.Threading.Tasks;
using GameFrameX.AssetSystem;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（TextFile，对应 Unity TextAsset）。
    /// </summary>
    public static class AssetComponentTextFileExtensions
    {
        /// <summary>
        /// 异步加载文本资源（Unity TextAsset → Godot 原生文件 RawFileHandle，取内容使用 GetRawFileText()）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>原生文件句柄</returns>
        public static Task<RawFileHandle> LoadTextFileAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadRawFileAsync(path);
        }

        /// <summary>
        /// 同步加载文本资源（Unity TextAsset → Godot 原生文件 RawFileHandle，取内容使用 GetRawFileText()）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>原生文件句柄</returns>
        public static RawFileHandle LoadTextFileSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadRawFileSync(path);
        }
    }
}
