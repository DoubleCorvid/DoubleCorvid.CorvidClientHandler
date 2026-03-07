using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.RateLimiting;

public class CorivdHttpClientRateLimiter : ICorvidHttpClientRateLimiter {
    public ICorvidHttpClientRateLimiterToken QueueRequest (ICorvidHttpClientRateLimiterEntry rateLimitEntry) {
        throw new NotImplementedException ();
    }

    public async Task StartProcessingRequestsAsync () {
        throw new NotImplementedException ();
    }

    public async Task StopProcessingRequestsAsync () {
        throw new NotImplementedException ();
    }
}
