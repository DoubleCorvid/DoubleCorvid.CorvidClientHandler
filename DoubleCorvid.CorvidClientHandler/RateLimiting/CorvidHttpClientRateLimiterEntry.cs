using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.RateLimiting;

public class CorvidHttpClientRateLimiterEntry : ICorvidHttpClientRateLimiterEntry {
    public required ICorvidHttpClientRequestConfig RequestConfig { get; set; }

    public required Func<ICorvidHttpClientRequestConfig, ICorvidHttpClientRequestResponse> Requester { get; set; }

    public required Action<ICorvidHttpClientRequestResponse> Callback { get; set; }
}
