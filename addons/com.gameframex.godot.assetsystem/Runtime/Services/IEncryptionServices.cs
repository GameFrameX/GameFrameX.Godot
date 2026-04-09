namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public struct EncryptResult
    {
        /// <summary>
        /// 文件是否加密
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 加密后的文件数据
        /// </summary>
        public byte[] EncryptedData;
    }

    [AssetSystemPreserve]
    public struct EncryptFileInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FileLoadPath;
    }

    [AssetSystemPreserve]
    public interface IEncryptionServices
    {
        [AssetSystemPreserve]
        EncryptResult Encrypt(EncryptFileInfo fileInfo);
    }
}