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

public interface ICorvidHttpClientConfig {
    /// <summary>
    /// The name of this client.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The base URL for this client.
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// The user agent for this client.
    /// </summary>
    string UserAgent { get; }

    /// <summary>
    /// This client's default request headers.
    /// </summary>
    IDictionary<string, string> DefaultRequestHeaders { get; }

    /// <summary>
    /// The media types this client will accept.
    /// </summary>
    IList<string> AcceptedMediaTypes { get; }
}
