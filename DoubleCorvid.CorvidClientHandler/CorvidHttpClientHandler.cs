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

using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientHandler (ICorvidHttpClientHandlerConfig config, HttpClient client) : ICorvidHttpClientHandler {
    public const string RetryAfter = "Retry-After";

    public ICorvidHttpClientHandlerConfig Config { get; } = config;

    public HttpClient HttpClient { get; } = client;

    #region Get
    public ICorvidHttpClientRequestToken Get (ICorvidHttpClientRequestConfig requestConfig) {
        return ExecuteRequest (requestConfig, ExecuteGetAsync);
    }

    public async Task<ICorvidHttpClientRequestToken> GetAndWaitAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteAndWaitAsync (requestConfig, ExecuteGetAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteGetAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return new CorvidHttpClientRequestResponse {
            HttpResponseMessage = await HttpClient.GetAsync (requestConfig.Route, requestConfig.HttpCompletionOption, requestConfig.CancellationToken)
        };
    }
    #endregion

    #region Patch
    public ICorvidHttpClientRequestToken Patch (ICorvidHttpClientRequestConfig requestConfig) {
        return ExecuteRequest (requestConfig, ExecutePatchAsync);
    }

    public async Task<ICorvidHttpClientRequestToken> PatchAndWaitAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteAndWaitAsync (requestConfig, ExecutePatchAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePatchAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PatchAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Post
    public ICorvidHttpClientRequestToken Post (ICorvidHttpClientRequestConfig requestConfig) {
        return ExecuteRequest (requestConfig, ExecutePostAsync);
    }

    public async Task<ICorvidHttpClientRequestToken> PostAndWaitAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteAndWaitAsync (requestConfig, ExecutePostAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePostAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PostAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Put
    public ICorvidHttpClientRequestToken Put (ICorvidHttpClientRequestConfig requestConfig) {
        return ExecuteRequest (requestConfig, ExecutePutAsync);
    }

    public async Task<ICorvidHttpClientRequestToken> PutAndWaitAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteAndWaitAsync (requestConfig, ExecutePutAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecutePutAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.PutAsync (requestConfig.Route, requestConfig.Content, requestConfig.CancellationToken)
    };
    #endregion

    #region Delete
    public ICorvidHttpClientRequestToken Delete (ICorvidHttpClientRequestConfig requestConfig) {
        return ExecuteRequest (requestConfig, ExecuteDeleteAsync);
    }

    public async Task<ICorvidHttpClientRequestToken> DeleteAndWaitAsync (ICorvidHttpClientRequestConfig requestConfig) {
        return await ExecuteAndWaitAsync (requestConfig, ExecuteDeleteAsync);
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteDeleteAsync (ICorvidHttpClientRequestConfig requestConfig) => new CorvidHttpClientRequestResponse {
        HttpResponseMessage = await HttpClient.DeleteAsync (requestConfig.Route, requestConfig.CancellationToken)
    };
    #endregion

    protected ICorvidHttpClientRequestToken ExecuteRequest (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        return Config.RateLimiter.EnqueueRequest (requestConfig, func);
    }

    protected async Task<ICorvidHttpClientRequestToken> ExecuteAndWaitAsync (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        var token = ExecuteRequest (requestConfig, func);

        await token.WaitUntilCompleteAsync ();

        return token;
    }
}