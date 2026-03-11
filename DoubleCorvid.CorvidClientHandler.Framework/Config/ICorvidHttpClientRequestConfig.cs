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

namespace DoubleCorvid.CorvidClientHandler.Framework.Config;

public interface ICorvidHttpClientRequestConfig {
    /// <summary>
    /// This request's relative route.
    /// </summary>
    string Route { get; }

    /// <summary>
    /// This requests `HttpCompleteionOption`.
    /// </summary>
    HttpCompletionOption HttpCompletionOption { get; }

    /// <summary>
    /// The content to be sent with this request.
    /// </summary>
    HttpContent Content { get; }

    /// <summary>
    /// TShould this request be retries?
    /// </summary>
    bool WithRetry { get; }

    /// <summary>
    /// Should this request use the default retry parameters?
    /// </summary>
    bool UseDefaultRetryDelay { get; }

    /// <summary>
    /// The delay between each retry request in milliseconds.
    /// </summary>
    int RetryDelayPerAttemptInMilliseconds { get; }

    /// <summary>
    /// The max amount of retries for this request.
    /// </summary>
    int MaxRetryAttempts { get; }

    /// <summary>
    /// This request's cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Calculate the delay for attempt number `attempt` in milliseconds.
    /// </summary>
    /// <param name="attempt">Which attempt to calculate for, starting at 1.</param>
    /// <returns>How long to to elay for in milliseconds.</returns>
    int CalculateDelayInMillisecondsForRetryAttempt (int attempt);
}
