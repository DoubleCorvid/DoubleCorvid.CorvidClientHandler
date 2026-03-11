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

namespace DoubleCorvid.CorvidClientHandler.Framework;

public interface ICorvidHttpClientFactory : IHttpClientFactory, IDisposable {
    /// <summary>
    /// This factory's configuration.
    /// </summary>
    ICorvidHttpClientFactoryConfig Config { get; }

    /// <summary>
    /// The clients created and managed by this factory.
    /// </summary>
    IReadOnlyDictionary<string, HttpClient> CreatedClients { get; }

    /// <summary>
    /// Remove a client from this factory.
    /// </summary>
    /// <param name="name">The name of the client to remove.</param>
    /// <returns>`true` if the client was found and removed, `false` otherwise.</returns>
    bool RemoveClient (string name);
}