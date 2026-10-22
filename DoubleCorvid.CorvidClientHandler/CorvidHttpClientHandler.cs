/* Copyright © 2026 Raven Crowe
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
    public const string RetryAfter = "Retry-After";

    public ICorvidHttpClientHandlerConfig Config { get; } = config;

    public HttpClient HttpClient { get; } = client;

    #region Get
    public async Task<ICorvidHttpClientRequestToken> GetAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecuteGetAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteGetAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.GetAsync (requestConfig.Route, requestConfig.HttpCompletionOption, requestConfig.CancellationToken)
    };
    #endregion

    #region Patch
    public async Task<ICorvidHttpClientRequestToken> PatchAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePatchAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePatchAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PatchAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Post
    public async Task<ICorvidHttpClientRequestToken> PostAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePostAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePostAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PostAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Put
    public async Task<ICorvidHttpClientRequestToken> PutAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecutePutAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePutAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PutAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Delete
    public async Task<ICorvidHttpClientRequestToken> DeleteAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteRequest (requestConfig, ExecuteDeleteAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteDeleteAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.DeleteAsync (requestConfig.Route, requestConfig.CancellationToken)
    };
    #endregion

    protected async Task<ICorvidHttpClientRequestToken> ExecuteRequest (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        if (requestConfig.WithRetry) {
            return await ExecuteRequestWithRateLimitAndRetryAsync (requestConfig, func);
        }
        else {
            return ExecuteRequestWithRateLimit (requestConfig, func);
        }
    }

    protected async Task<ICorvidHttpClientRequestToken> ExecuteRequestWithRateLimitAndRetryAsync (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        var retryCount = 0;

        ICorvidHttpClientRequestToken token;

        Func<int, int> delayCalc = requestConfig.UseDefaultRetryDelay
                                   ? Config.CalculateDelayInMillisecondsForRetryAttempt
                                   : requestConfig.CalculateDelayInMillisecondsForRetryAttempt;

        int delay = -1;

        do {
            token = ExecuteRequestWithRateLimit (requestConfig, func);

            if (requestConfig.CancellationToken.IsCancellationRequested) {
                return token;
            }

            await token.WaitUntilCompleteAsync ();

            if (token.IsSuccess) {
                return token;
            }
            else if (DoesResponseHaveRetryAfterHeader (token.Response?.HttpResponseMessage)) {
                delay = GetReponseRetryAfterInSeconds (token.Response?.HttpResponseMessage.Headers.GetValues (RetryAfter) ?? []);
            }
            else if (requestConfig.UseDefaultRetryDelay) {
                delay = Config.CalculateDelayInMillisecondsForRetryAttempt (retryCount++);
            }
            else {
                delay = requestConfig.CalculateDelayInMillisecondsForRetryAttempt (retryCount++);
            }

            await Task.Delay (delay, requestConfig.CancellationToken);
        } while (delay > 0 && !requestConfig.CancellationToken.IsCancellationRequested);

        return token;
    }

    private static bool DoesResponseHaveRetryAfterHeader (HttpResponseMessage? response) => response?.Headers.Contains ("Try-After") ?? false;

    private static int GetReponseRetryAfterInSeconds (IEnumerable<string> retryAfterHeaders) {
        if (!retryAfterHeaders.Any ()) {
            return 0;
        }

        int delay = 0;

        foreach (var value in retryAfterHeaders) {
            delay += ParseDelayInSeconds (value);
        }

        return delay;
    }

    private static int ParseDelayInSeconds (string header) {
        int delay = 0;

        if (int.TryParse (header, out int parsed)) {
            delay = parsed;
        }
        else if (DateTime.TryParse (header, out var date)) {
            var span = date - DateTime.UtcNow;

            delay = span.Seconds;
        }

        return delay;
    }

    protected bool ShouldRetryRequest (HttpStatusCode code) => Config.NonretryStatusCodes.Contains (code);

    protected ICorvidHttpClientRequestToken ExecuteRequestWithRateLimit (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        return Config.RateLimiter.EnqueueRequest (requestConfig, func);
    }
}