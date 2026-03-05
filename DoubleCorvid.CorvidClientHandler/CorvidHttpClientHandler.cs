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

using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientHandler (ICorvidHttpClientHandlerConfig config) : ICorvidHttpClientHandler {
    public ICorvidHttpClientHandlerConfig Config { get; } = config;

    public HttpClient HttpClient => Config.Client;

    protected readonly SemaphoreSlim _timestampSemaphore = new (1);

    protected DateTime _lastRequestTimestamp = DateTime.UtcNow;

    #region Get
    public async Task<ICorvidHttpResponse> GetAsync (ICorvidHttpClientRequestConfig config) {
        return await ExecuteRequestWithRateLimitAndRetry (config, ExecuteGetAsync);
    }

    protected async Task<ICorvidHttpResponse> ExecuteGetAsync (ICorvidHttpClientRequestConfig config) => new CorvidHttpResponse {
        HttpResponseMessage = await HttpClient.GetAsync (config.Route, config.HttpCompletionOption, config.CancellationToken)
    };
    #endregion

    #region Patch
    public async Task<ICorvidHttpResponse> PatchAsync (ICorvidHttpClientRequestConfig config) {
        return await ExecuteRequestWithRateLimitAndRetry (config, ExecutePatchAsync);
    }

    protected async Task<ICorvidHttpResponse> ExecutePatchAsync (ICorvidHttpClientRequestConfig config) => new CorvidHttpResponse {
        HttpResponseMessage = await HttpClient.PatchAsync (config.Route, config.Content, config.CancellationToken)
    };
    #endregion

    #region Post
    public async Task<ICorvidHttpResponse> PostAsync (ICorvidHttpClientRequestConfig config) {
        return await ExecuteRequestWithRateLimitAndRetry (config, ExecutePostAsync);
    }

    protected async Task<ICorvidHttpResponse> ExecutePostAsync (ICorvidHttpClientRequestConfig config) => new CorvidHttpResponse {
        HttpResponseMessage = await HttpClient.PostAsync (config.Route, config.Content, config.CancellationToken)
    };
    #endregion

    #region Put
    public async Task<ICorvidHttpResponse> PutAsync (ICorvidHttpClientRequestConfig config) {
        return await ExecuteRequestWithRateLimitAndRetry (config, ExecutePutAsync);
    }

    protected async Task<ICorvidHttpResponse> ExecutePutAsync (ICorvidHttpClientRequestConfig config) => new CorvidHttpResponse {
        HttpResponseMessage = await HttpClient.PutAsync (config.Route, config.Content, config.CancellationToken)
    };
    #endregion

    #region Delete
    public async Task<ICorvidHttpResponse> DeleteAsync (ICorvidHttpClientRequestConfig config) {
        return await ExecuteRequestWithRateLimitAndRetry (config, ExecuteDeleteAsync);
    }

    protected async Task<ICorvidHttpResponse> ExecuteDeleteAsync (ICorvidHttpClientRequestConfig config) => new CorvidHttpResponse {
        HttpResponseMessage = await HttpClient.DeleteAsync (config.Route, config.CancellationToken)
    };
    #endregion
    
    protected async Task<ICorvidHttpResponse> ExecuteRequestWithRateLimitAndRetry (ICorvidHttpClientRequestConfig config, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpResponse>> func) {
        var retryCount = 0;

        ICorvidHttpResponse response;

        int delay = -1;

        do {
            response = await ExecuteRequestWithRateLimit (config, func);

            if (!response.HttpResponseMessage.IsSuccessStatusCode) {
                if (config.UseDefaultRetryDelay) {
                    delay = Config.CalculateDelayInMillisecondsForRetryAttempt (retryCount);
                }
                else {
                    delay = config.CalculateDelayInMillisecondsForRetryAttempt (retryCount);
                }

                retryCount++;
                
                await Task.Delay (delay, config.CancellationToken);
            }
        } while (!response.HttpResponseMessage.IsSuccessStatusCode && delay > 0);

        return response;
    }

    protected async Task<ICorvidHttpResponse> ExecuteRequestWithRateLimit (ICorvidHttpClientRequestConfig config, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpResponse>> func) {
        int delay = Config.RequestDelayInMilliseconds - GetMilisecondsSinceLastRequest ();

        if (delay > 0) {
            await Task.Delay (delay, config.CancellationToken);
        }
        
        var response = await func (config);

        await _timestampSemaphore.WaitAsync ();

        _lastRequestTimestamp = DateTime.UtcNow;

        _timestampSemaphore.Release ();

        return response;
    }

    protected int GetMilisecondsSinceLastRequest () {
        _timestampSemaphore.Wait ();

        var lastRequestTimestamp = _lastRequestTimestamp;

        _timestampSemaphore.Release ();

        var span = DateTime.UtcNow - lastRequestTimestamp;

        return span.Milliseconds;
    }
}
