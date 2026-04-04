using System.Threading;
using System.Threading.Tasks;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public interface IHttpTransport
    {
        [UnityEngine.Scripting.Preserve]
        Task<HttpResponse> GetTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken);

        [UnityEngine.Scripting.Preserve]
        Task<HttpResponse> GetDataAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken);
    }
}
