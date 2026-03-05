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

using DoubleCorvid.CorvidClientHandler.Framework.Config;

namespace DoubleCorvid.CorvidClientHandler.Config;

public class CorvidHttpClientHandlerConfig : ICorvidHttpClientHandlerConfig {
    public required string Name { get; set; }

    public required HttpClient Client { get; set; }

    public bool RateLimitRequests { get; set; } = true;

    public int RequestDelayInMilliseconds { get; set; } = 1000;

    public int RetryDelayPerAttemptInMilliseconds { get; set; } = 10000;

    public int MaxRetryAttempts { get; set; }  = 5;

    public int CalculateDelayInMillisecondsForRetryAttempt (int attempt) {
        if (attempt > MaxRetryAttempts) {
            return -1;
        }

        return RetryDelayPerAttemptInMilliseconds * (int) MathF.Pow (2, attempt - 1);
    }
}
