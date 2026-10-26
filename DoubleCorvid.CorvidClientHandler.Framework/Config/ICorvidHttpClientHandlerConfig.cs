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
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

using Microsoft.Extensions.Logging;

namespace DoubleCorvid.CorvidClientHandler.Framework.Config;

public interface ICorvidHttpClientHandlerConfig {
    /// <summary>
    /// The logger that this handler should use.
    /// </summary>
    ILogger<ICorvidHttpClientHandler>? Logger { get; }

    /// <summary>
    /// The name of this handler.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// TShould requests be rate limited?
    /// </summary>
    bool RateLimitRequests { get; }

    /// <summary>
    /// The rate limiter that this client will use.
    /// </summary>
    ICorvidHttpClientRateLimiter RateLimiter { get; }
}