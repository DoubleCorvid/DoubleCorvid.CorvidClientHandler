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

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientHandlerFactory (ICorvidHttpClientHandlerFactoryConfig config) : ICorvidHttpClientHandlerFactory {
    public ICorvidHttpClientHandlerFactoryConfig Config { get; } = config;

    protected readonly ConcurrentDictionary<string, ICorvidHttpClientHandler> _handlers = new ();

    public IReadOnlyDictionary<string, ICorvidHttpClientHandler> CreatedHandlers => _handlers;

    public virtual ICorvidHttpClientHandler CreateHandler (string name) {
        return _handlers.GetOrAdd (name, n => BuildHandler (n));
    }

    private CorvidHttpClientHandler BuildHandler (string name) {
        var handlerConfig = Config.GetClientHandlerConfig (name);

        return new CorvidHttpClientHandler (handlerConfig, Config.ClientFactory.CreateClient (name));
    }
}
