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

using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientHandler (ICorvidHttpClientHandlerConfig config, HttpClient client) : ICorvidHttpClientHandler {
    public ICorvidHttpClientHandlerConfig Config { get; } = config;

    public HttpClient HttpClient { get; } = client;

    protected readonly SemaphoreSlim _timestampSemaphore = new (1);

    protected readonly ReaderWriterLock _timestampLock = new ();

    protected readonly int _timestampLockTimeout = 10;

    protected DateTime _lastRequestTimestamp = DateTime.UtcNow;

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
    public async Task<ICorvidHttpClientRequestResponse> GetAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecuteGetAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteGetAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.GetAsync (requestConfig.Route, requestConfig.HttpCompletionOption, requestConfig.CancellationToken)
    };
    #endregion

    #region Patch
    public async Task<ICorvidHttpClientRequestResponse> PatchAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePatchAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePatchAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PatchAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Post
    public async Task<ICorvidHttpClientRequestResponse> PostAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePostAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePostAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PostAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Put
    public async Task<ICorvidHttpClientRequestResponse> PutAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePutAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePutAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PutAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Delete
    public async Task<ICorvidHttpClientRequestResponse> DeleteAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecuteDeleteAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteDeleteAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.DeleteAsync (requestConfig.Route, requestConfig.CancellationToken)
    };
    #endregion

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteRequest (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        if (requestConfig.WithRetry) {
            return await ExecuteRequestWithRateLimitAndRetry (requestConfig, func);
        }
        else {
            return await ExecuteRequestWithRateLimit (requestConfig, func);
        }
    }
    
    protected async Task<ICorvidHttpClientRequestResponse> ExecuteRequestWithRateLimitAndRetry (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        var retryCount = 0;

        ICorvidHttpClientRequestResponse response;

        Func<int, int> delayCalc = requestConfig.UseDefaultRetryDelay 
                                   ? Config.CalculateDelayInMillisecondsForRetryAttempt 
                                   : requestConfig.CalculateDelayInMillisecondsForRetryAttempt;

        int delay = -1;

        do {
            response = await ExecuteRequestWithRateLimit (requestConfig, func);

            if (!response.HttpResponseMessage.IsSuccessStatusCode 
                && ShouldRetryRequest (response.HttpResponseMessage.StatusCode)) {
                
                delay = delayCalc (retryCount);

                retryCount++;

                if (delay > 0) {
                    await Task.Delay (delay, requestConfig.CancellationToken);
                }
            }
        } while (!response.HttpResponseMessage.IsSuccessStatusCode && delay > 0);

        return response;
    }

    protected bool ShouldRetryRequest (HttpStatusCode code) => Config.NonretryStatusCodes.Contains (code);

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteRequestWithRateLimit (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        var delay = Config.RequestDelayInMilliseconds;

        delay -= (delay > 0 ? GetMilisecondsSinceLastRequest (requestConfig.CancellationToken) : 0);

        if (delay > 0) {
            await Task.Delay (delay, requestConfig.CancellationToken);
        }


        var response = await func (requestConfig);

        await UpdateLastRequestTimeStamp (requestConfig.CancellationToken);

        return response;
    }

    private async Task UpdateLastRequestTimeStamp (CancellationToken token) {
        await _timestampSemaphore.WaitAsync (token);

        _lastRequestTimestamp = DateTime.UtcNow;

        _timestampSemaphore.Release ();
    }


    protected int GetMilisecondsSinceLastRequest (CancellationToken token) {
        _timestampSemaphore.Wait (token);

        var lastRequestTimestamp = _lastRequestTimestamp;

        _timestampSemaphore.Release ();

        var span = DateTime.UtcNow - lastRequestTimestamp;

        return span.Milliseconds;
    }
}
