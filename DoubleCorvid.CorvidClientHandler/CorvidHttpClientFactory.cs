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
using System.Net.Http.Headers;
using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientFactory (ICorvidHttpClientFactoryConfig config) : ICorvidHttpClientFactory {
    public ICorvidHttpClientFactoryConfig Config { get; } = config;

    protected readonly ConcurrentDictionary<string, HttpClient> _clients = new ();

    public IReadOnlyDictionary<string, HttpClient> CreatedClients => _clients;

    protected readonly Lazy<HttpMessageHandler> _lazyHandler = new (() => new HttpClientHandler ());

    private bool _disposed = false;

    public virtual HttpClient CreateClient (string name) {
        return _clients.GetOrAdd (name, n => BuildNewClient (n));
    }

    protected HttpClient BuildNewClient (string name) {
        var clientConfig = Config.GetClientConfig (name);

        var client = new HttpClient (_lazyHandler.Value, disposeHandler: false) {
            BaseAddress = new Uri (clientConfig.BaseUrl)
        };

        foreach (var header in clientConfig.DefaultRequestHeaders) {
            client.DefaultRequestHeaders.Add (header.Key, header.Value);
        }

        foreach (var type in clientConfig.AcceptedMediaTypes) {
            client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue (type));
        }

        if (!string.IsNullOrEmpty (clientConfig.UserAgent)) {
            client.DefaultRequestHeaders.Add ("User-Agent", clientConfig.UserAgent);
        }

        return client;
    }

    public bool RemoveClient (string name) {
        var removed = _clients.TryRemove (name, out var client);

        if (removed && client is not null) {
            client.Dispose ();
        }

        return removed;
    }

    public virtual void Dispose () {
        if (_disposed) {
            return;
        }

        GC.SuppressFinalize (this);

        // This isn't needed as the clients use the underlying HttpMessageHandler,
        // which is disposed of later, but it terminates any open connections, etc,
        // so it's worth it imho.
        foreach (var client in _clients) {
            client.Value.Dispose ();
        }

        _clients.Clear ();

        if (_lazyHandler.IsValueCreated) {
            _lazyHandler.Value.Dispose ();
        }

        _disposed = true;
    }
}
