using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.RateLimiting;

public class CorvidHttpClientRateLimiterToken : ICorvidHttpClientRateLimiterToken {
    public bool Rejected { get; set; }

    public bool Expired { get; set; }

    public bool Completed { get; set; }

    public bool Cancelled { get; private set; }

    public void Cancel () => Cancelled = true;
}