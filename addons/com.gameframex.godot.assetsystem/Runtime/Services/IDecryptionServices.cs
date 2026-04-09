using System.IO;
namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public struct DecryptFileInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件加载路径
        /// </summary>
        public string FileLoadPath;

        /// <summary>
        /// Unity引擎用于内容校验的CRC
        /// </summary>
        public uint FileLoadCRC;
    }

    [AssetSystemPreserve]
    public interface IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        [AssetSystemPreserve]
        BundleFile LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream);

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        [AssetSystemPreserve]
        BundleFileCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream);

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        [AssetSystemPreserve]
        byte[] ReadFileData(DecryptFileInfo fileInfo);

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        [AssetSystemPreserve]
        string ReadFileText(DecryptFileInfo fileInfo);
    }
}
