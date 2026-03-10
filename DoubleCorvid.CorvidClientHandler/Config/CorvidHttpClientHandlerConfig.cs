/* Copyright © 2025 Raven Crowe
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System.Net;

using DoubleCorvid.CorvidClientHandler.Framework.Config;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.Config;

public class CorvidHttpClientHandlerConfig : ICorvidHttpClientHandlerConfig {
    public required string Name { get; init; }

    public bool RateLimitRequests { get; init; } = false;

    public required ICorvidHttpClientRateLimiter RateLimiter { get; init; }

    public List<HttpStatusCode> NonretryStatusCodes { get; init; } = [
        HttpStatusCode.MovedPermanently,
        HttpStatusCode.TemporaryRedirect,
        HttpStatusCode.SeeOther,
        HttpStatusCode.NotModified,
        HttpStatusCode.TemporaryRedirect,
        HttpStatusCode.PermanentRedirect,
        HttpStatusCode.BadRequest,
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden,
        HttpStatusCode.NotFound,
        HttpStatusCode.MethodNotAllowed,
        HttpStatusCode.NotAcceptable,
        HttpStatusCode.Gone,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.NotImplemented,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.HttpVersionNotSupported,
        HttpStatusCode.VariantAlsoNegotiates,
        HttpStatusCode.InsufficientStorage,
        HttpStatusCode.LoopDetected,
        HttpStatusCode.NetworkAuthenticationRequired,
    ];

    public int MaxRetryAttempts { get; init; } = 4;

    public int CalculateDelayInMillisecondsForRetryAttempt (int attempt) {
        if (attempt > MaxRetryAttempts) {
            return -1;
        }

        int jitter = DateTime.UtcNow.Millisecond % (int) MathF.Pow (2, attempt - 1);

        return (int) MathF.Pow (2, attempt - 1) + jitter;
    }
}
