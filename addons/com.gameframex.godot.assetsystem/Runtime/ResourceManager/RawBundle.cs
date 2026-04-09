namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class RawBundle
    {
        private readonly IFileSystem _fileSystem;
        private readonly PackageBundle _packageBundle;
        private readonly string _filePath;

        [AssetSystemPreserve]
        internal RawBundle(IFileSystem fileSystem, PackageBundle packageBundle, string filePath)
        {
            _fileSystem = fileSystem;
            _packageBundle = packageBundle;
            _filePath = filePath;
        }

        [AssetSystemPreserve]
        public string GetFilePath()
        {
            return _filePath;
        }

        [AssetSystemPreserve]
        public byte[] ReadFileData()
        {
            if (_fileSystem != null)
            {
                return _fileSystem.ReadFileData(_packageBundle);
            }
            else
            {
                return FileUtility.ReadAllBytes(_filePath);
            }
        }

        [AssetSystemPreserve]
        public string ReadFileText()
        {
            if (_fileSystem != null)
            {
                return _fileSystem.ReadFileText(_packageBundle);
            }
            else
            {
                return FileUtility.ReadAllText(_filePath);
            }
        }
    }
}