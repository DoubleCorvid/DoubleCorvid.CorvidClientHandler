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

using System.Collections.Concurrent;
using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.RateLimiting;

public class CorvidHttpClientRateLimiter (ICorvidHttpClientRateLimiterConfig config) : ICorvidHttpClientRateLimiter {
    public ICorvidHttpClientRateLimiterConfig Config { get; } = config;

    private readonly ConcurrentQueue<ICorvidHttpClientRateLimiterEntry> _entries = [];
    
    private bool _processing = false;

    protected readonly SemaphoreSlim _timestampSemaphore = new (1);

    protected readonly ReaderWriterLock _timestampLock = new ();

    protected readonly int _timestampLockTimeout = 10;

    protected DateTime _lastRequestTimestamp = DateTime.UtcNow;

    public async Task StartAsync (CancellationToken cancellationToken) {
        _processing = true;

        while (_processing && !cancellationToken.IsCancellationRequested) {
            if (!_entries.IsEmpty
               && _entries.TryDequeue (out var entry)
               && entry is not null
               && !entry.Token.IsCancelled) {
                await ProcessEntry (entry);
            }
        }
    }

    private async Task ProcessEntry (ICorvidHttpClientRateLimiterEntry entry) {
        var config = entry.RequestConfig;

        var token = entry.Token;

        if (config.CancellationToken.IsCancellationRequested) {
            token.Expire ();
        }
        else {
            var response = await ExecuteRequest (config, entry.Requester);

            token.Response = response;

            token.Complete ();
        }
    }

    protected async Task<ICorvidHttpClientRequestResponse> ExecuteRequest (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        var delay = Config.RateLimitDelay;

        if (delay > 0) {
            delay -= GetMilisecondsSinceLastRequest (requestConfig.CancellationToken);
        }

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
        var span = DateTime.UtcNow - _lastRequestTimestamp;

        return span.Milliseconds;
    }

    public void Stop () {
        _processing = false;
    }

    public void ClearQueue () {
        _entries.Clear ();
    }

    public ICorvidHttpClientRequestToken EnqueueRequest (ICorvidHttpClientRequestConfig requestConfig,
                                                           Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> requester) {
        var token = new CorvidHttpClientRequestToken ();

        var entry = new CorvidHttpClientRateLimiterEntry {
            RequestConfig = requestConfig,
            Requester = requester,
            Token = token,
        };

        _entries.Enqueue (entry);

        return token;
    }
}
