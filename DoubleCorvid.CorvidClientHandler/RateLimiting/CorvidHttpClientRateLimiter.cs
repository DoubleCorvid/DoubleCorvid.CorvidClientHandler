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

using System.Collections.Concurrent;
using System.Net;

using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.RateLimiting;

public class CorvidHttpClientRateLimiter : ICorvidHttpClientRateLimiter {
    public const string RetryAfter = "Retry-After";

    public ICorvidHttpClientRateLimiterConfig Config { get; }

    private readonly ConcurrentQueue<ICorvidHttpClientRateLimiterEntry> _entries = [ ];

    private bool _processing = false;

    protected readonly ReaderWriterLock _timestampLock = new ();

    protected DateTime _lastRequestTimestamp = DateTime.UtcNow;

    private bool disposed;

    public Task? RunningTask { get; private set; }

    public CorvidHttpClientRateLimiter (ICorvidHttpClientRateLimiterConfig config) {
        Config = config;

        if (config.StartAfterConstruction) {
            RunningTask = Run (config.DefaultCancellationToken);
        }
    }


    public Task Run (CancellationToken? cancellationToken = null) {
        var token = cancellationToken is null ? Config.DefaultCancellationToken : (CancellationToken) cancellationToken;

        RunningTask = Task.Run (async () => {
            _processing = true;

            while (_processing && !token.IsCancellationRequested) {
                if (!_entries.IsEmpty
                && _entries.TryDequeue (out var entry)
                && entry is not null
                && !entry.Token.IsCancelled) {
                    await ProcessEntry (entry);
                }
            }
        }, token);

        return RunningTask;
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

    public void Stop () {
        _processing = false;
    }

    private async Task ProcessEntry (ICorvidHttpClientRateLimiterEntry entry) {
        var config = entry.RequestConfig;

        var token = entry.Token;

        if (config.CancellationToken.IsCancellationRequested || !token.IsValid) {
            token.Expire ();
        }
        else {
            var response = await ExecuteRequest (config, entry.Requester);

            token.Response = response;

            token.Complete ();
        }
    }

    public void ClearQueue () {
        foreach (var entry in _entries) {
            entry.Token.Expire ();
        }

        _entries.Clear ();
    }

    private async Task<ICorvidHttpClientRequestResponse?> ExecuteRequest (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
        int delay = Config.RateLimitDelay - GetMilisecondsSinceLastRequest (requestConfig.CancellationToken);

        int attempts = 0;

        int maxAttempts = requestConfig.MaxRetryAttempts > 0 ? requestConfig.MaxRetryAttempts : Config.MaxRetryAttempts;

        ICorvidHttpClientRequestResponse response;

        Func<int, int> delayCalc = requestConfig.UseDefaultRetryDelay
                                    ? Config.DefaultDelayInMillisecondsForRetryAttemptCalculator
                                    : requestConfig.CalculateDelayInMillisecondsForRetryAttempt;
        
        do {
            if (delay > 0) {
                await Task.Delay (delay, requestConfig.CancellationToken);

                delay = 0;
            }

            response = await func (requestConfig);

            await UpdateLastRequestTimeStamp (requestConfig.CancellationToken);

            if (requestConfig.WithRetry && ShouldRetryRequest (attempts, maxAttempts, response?.HttpResponseMessage.StatusCode)) {
                if (response is null) {
                    delay = Config.RateLimitDelay;
                }
                else if (DoesResponseHaveRetryAfterHeader (response?.HttpResponseMessage)) {
                    delay = GetReponseRetryAfterInSeconds (response?.HttpResponseMessage.Headers.GetValues (RetryAfter) ?? []);
                }
                else if (!response?.HttpResponseMessage.IsSuccessStatusCode ?? true) {
                    delay = delayCalc (attempts);
                }
            }

            attempts++;
        }
        while (delay > 0 && attempts <= maxAttempts && !requestConfig.CancellationToken.IsCancellationRequested);

        return response;
    }

    private async Task UpdateLastRequestTimeStamp (CancellationToken token) {
        lock (_timestampLock) {
            _lastRequestTimestamp = DateTime.UtcNow;
        }
    }

    private int GetMilisecondsSinceLastRequest (CancellationToken token) {
        TimeSpan span;

        lock (_timestampLock) {
            span = DateTime.UtcNow - _lastRequestTimestamp;
        }

        return span.Milliseconds;
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

    private bool ShouldRetryRequest (int currentAttempt, int maxAttempts, HttpStatusCode? code) {
        return code is null || (currentAttempt < maxAttempts &&!Config.NonretryStatusCodes.Contains ((HttpStatusCode) code));
    }

    // protected async Task<ICorvidHttpClientRequestToken> ExecuteRequestWithRateLimitAndRetryAsync (ICorvidHttpClientRequestConfig requestConfig, Func<ICorvidHttpClientRequestConfig, Task<ICorvidHttpClientRequestResponse>> func) {
    //     var retryCount = 0;

    //     ICorvidHttpClientRequestToken token;

    //     Func<int, int> delayCalc = requestConfig.UseDefaultRetryDelay
    //                                ? Config.CalculateDelayInMillisecondsForRetryAttempt
    //                                : requestConfig.CalculateDelayInMillisecondsForRetryAttempt;

    //     int delay = -1;

    //     do {
    //         token = ExecuteRequestWithRateLimit (requestConfig, func);

    //         if (requestConfig.CancellationToken.IsCancellationRequested) {
    //             return token;
    //         }

    //         await token.WaitUntilCompleteAsync ();

    //         if (token.IsSuccess) {
    //             return token;
    //         }
    //         else if (DoesResponseHaveRetryAfterHeader (token.Response?.HttpResponseMessage)) {
    //             delay = GetReponseRetryAfterInSeconds (token.Response?.HttpResponseMessage.Headers.GetValues (RetryAfter) ?? []);
    //         }
    //         else if (requestConfig.UseDefaultRetryDelay) {
    //             delay = Config.CalculateDelayInMillisecondsForRetryAttempt (retryCount++);
    //         }
    //         else {
    //             delay = requestConfig.CalculateDelayInMillisecondsForRetryAttempt (retryCount++);
    //         }

    //         await Task.Delay (delay, requestConfig.CancellationToken);
    //     } while (delay > 0 && !requestConfig.CancellationToken.IsCancellationRequested);

    //     return token;
    // }

    // protected bool ShouldRetryRequest (HttpStatusCode code) => Config.NonretryStatusCodes.Contains (code);m

    protected virtual void Dispose (bool disposing) {
        if (!disposed) {
            if (disposing) {
                ClearQueue ();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~CorvidHttpClientRateLimiter()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose () {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose (disposing: true);
        GC.SuppressFinalize (this);
    }

}