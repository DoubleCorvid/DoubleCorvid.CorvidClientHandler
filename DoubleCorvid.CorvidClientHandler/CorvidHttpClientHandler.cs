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
using System.Runtime.CompilerServices;

using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientHandler (ICorvidHttpClientHandlerConfig config, HttpClient client) : ICorvidHttpClientHandler {
    public ICorvidHttpClientHandlerConfig Config { get; } = config;

    public HttpClient HttpClient { get; } = client;

    protected List<HttpStatusCode> _oneShotStatusCodes = [
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

    #region Get
    public async Task<ICorvidHttpClientRequestToken?> GetAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecuteGetAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteGetAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.GetAsync (requestConfig.Route, requestConfig.HttpCompletionOption, requestConfig.CancellationToken)
    };
    #endregion

    #region Patch
    public async Task<ICorvidHttpClientRequestToken?> PatchAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePatchAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePatchAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PatchAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Post
    public async Task<ICorvidHttpClientRequestToken?> PostAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePostAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePostAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PostAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Put
    public async Task<ICorvidHttpClientRequestToken?> PutAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePutAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePutAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PutAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Delete
    public async Task<ICorvidHttpClientRequestToken?> DeleteAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecuteDeleteAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteDeleteAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.DeleteAsync (requestConfig.Route, requestConfig.CancellationToken)
    };
    #endregion

    protected async Task<ICorvidHttpClientRequestToken> ExecuteRequest (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        if (requestConfig.WithRetry) {
            return await ExecuteRequestWithRateLimitAndRetry (requestConfig, func);
        }
        else {
            return await ExecuteRequestWithRateLimit (requestConfig, func);
        }
    }
    
    protected async Task<ICorvidHttpClientRequestToken> ExecuteRequestWithRateLimitAndRetry (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        var retryCount = 0;

        ICorvidHttpClientRequestToken token;

        Func<int, int> delayCalc = requestConfig.UseDefaultRetryDelay 
                                   ? Config.CalculateDelayInMillisecondsForRetryAttempt 
                                   : requestConfig.CalculateDelayInMillisecondsForRetryAttempt;

        int delay = -1;

        do {
            token = await ExecuteRequestWithRateLimit (requestConfig, func);

            if (requestConfig.CancellationToken.IsCancellationRequested) {
                return token;
            }

            await token.WaitForResponseAsync ();

            if (token.IsSuccess) {
                delay = -1;
            }
            else if (requestConfig.UseDefaultRetryDelay) {
                delay = Config.CalculateDelayInMillisecondsForRetryAttempt (retryCount++);
            }
            else {
                delay = requestConfig.CalculateDelayInMillisecondsForRetryAttempt (retryCount++);
            }
        } while (delay > 0 && !requestConfig.CancellationToken.IsCancellationRequested);

        return token;
    }

    protected bool ShouldRetryRequest (HttpStatusCode code) => Config.NonretryStatusCodes.Contains (code);

    protected async Task<ICorvidHttpClientRequestToken> ExecuteRequestWithRateLimit (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        return Config.RateLimiter.EnqueueRequest (requestConfig, func);
    }
}
