using System.Collections.Concurrent;
using GameFrameX.AssetSystem;

namespace GameFrameX.Asset.Runtime
{
    public partial class AssetManager
    {
        private class RemoteServices : IRemoteServices
        {
            public string HostServer { get; }
            public string FallbackHostServer { get; }
            private readonly ConcurrentDictionary<string, string> _mapping = new ConcurrentDictionary<string, string>();

            public RemoteServices(string hostServer, string fallbackHostServer)
            {
                HostServer = hostServer;
                FallbackHostServer = fallbackHostServer;
            }

            public string GetRemoteMainURL(string fileName, string packageVersion)
            {
                return GetFileLoadURL(fileName);
            }

            public string GetRemoteFallbackURL(string fileName, string packageVersion)
            {
                return GetFileLoadURL(fileName, true);
            }

            private string GetFileLoadURL(string fileName, bool isFallback = false)
            {
                return _mapping.GetOrAdd(fileName, _ => PathUtility.Combine(isFallback ? FallbackHostServer : HostServer, fileName));
            }
        }
    }
}
