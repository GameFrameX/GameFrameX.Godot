using System.Threading;
using System.Threading.Tasks;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public interface IHttpTransport
    {
        [AssetSystemPreserve]
        Task<HttpResponse> GetTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken);

        [AssetSystemPreserve]
        Task<HttpResponse> GetDataAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken);
    }
}
