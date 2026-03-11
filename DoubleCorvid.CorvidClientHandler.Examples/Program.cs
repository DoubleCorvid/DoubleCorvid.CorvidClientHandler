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

using System.Text.Json;
using DoubleCorvid.CorvidClientHandler.Builders;
using DoubleCorvid.CorvidClientHandler.Config;
using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;
using DoubleCorvid.CorvidClientHandler.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler.Examples;

public class Program {
    private static readonly string _clientName = "default";

    private static readonly CancellationTokenSource _cancellationTokenSource = new ();

    private static CancellationToken _cancelToken = _cancellationTokenSource.Token;

    public static async Task Main () {
        await ExecuteJsonPlaceholderExamples ();
    }

    private static async Task ExecuteJsonPlaceholderExamples () {
        var ratelimiter = new CorvidHttpClientRateLimiter (new CorvidHttpClientRateLimiterConfig ());

        var handlerFactory = BuildHandlerFactory (ratelimiter);

        var client = handlerFactory.CreateHandler (_clientName);

        _ = ratelimiter.RunAsync (_cancelToken);

        await ExecuteGetExampleAsync (client);

        await ExecutePatchExampleAsync (client);

        await ExecutePostExampleAsync (client);

        await ExecutePutExampleAsync (client);

        await ExecuteDeleteExampleAsync (client);

        ratelimiter.Stop ();

        _cancellationTokenSource.Cancel ();
    }

    private static async Task ExecuteGetExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("Executing get example...");

        var token = await client.GetAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken = _cancelToken
        });
        
        if (token is null || token.IsRejected) {
            Console.WriteLine ("Delete request was either rejected, or didn't return a token.");
            return;
        }

        var response = await token.WaitUntilCompleteAsync ();

        if (token.IsSuccess && (response?.HttpResponseMessage.IsSuccessStatusCode ?? false)) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Get example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Get example failed.");
        }
    }

    private static async Task ExecutePatchExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting patch example...");

        var token = await client.PatchAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            Content = new StringContent (JsonSerializer.Serialize<object> (new {
                Title = "foo"
            })),
            CancellationToken = _cancelToken
        });
        
        if (token is null || token.IsRejected) {
            Console.WriteLine ("Patch request was either rejected, or didn't return a token.");
            return;
        }

        var response = await token.WaitUntilCompleteAsync ();

        if (token.IsSuccess && (response?.HttpResponseMessage.IsSuccessStatusCode ?? false)) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Patch example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Patch example failed.");
        }
    }

    private static async Task ExecutePostExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting post example...");
        
        var token = await client.PostAsync (new CorvidHttpClientRequestConfig {
            Route = "posts",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            Content = new StringContent (JsonSerializer.Serialize<object> (new {
                Title = "foo",
                Body = "bar",
                UserId = 1,
            })),
            CancellationToken = _cancelToken
        });

        if (token is null || token.IsRejected) {
            Console.WriteLine ("Delete request was either rejected, or didn't return a token.");
            return;
        }

        var response = await token.WaitUntilCompleteAsync ();

        if (token.IsSuccess && (response?.HttpResponseMessage.IsSuccessStatusCode ?? false)) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Post example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Post example failed.");
        }
    }

    private static async Task ExecutePutExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting put example...");
        
        var token = await client.PutAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            Content = new StringContent (JsonSerializer.Serialize<object> (new {
                Id = 1,
                Title = "foo",
                Body = "bar",
                UserId = 1,
            })),
            CancellationToken = _cancelToken
        });

        if (token is null || token.IsRejected) {
            Console.WriteLine ("Put request was either rejected, or didn't return a token.");
            return;
        }

        var response = await token.WaitUntilCompleteAsync ();

        if (token.IsSuccess && (response?.HttpResponseMessage.IsSuccessStatusCode ?? false)){
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Put example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Put example failed.");
        }
    }

    private static async Task ExecuteDeleteExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting delete example...");
        
        var token = await client.DeleteAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken = _cancelToken
        });

        if (token is null || token.IsRejected) {
            Console.WriteLine ("Delete request was either rejected, or didn't return a token.");
            return;
        }

        var response = await token.WaitUntilCompleteAsync ();

        if (token.IsSuccess && (response?.HttpResponseMessage.IsSuccessStatusCode ?? false)) {
            Console.WriteLine ($"Delete example succeded. (It's hard to display an object that doesn't exist :) )");
        }
        else {
            Console.WriteLine ($"Delete example failed.");
        }
    }

    private static ICorvidHttpClientFactory BuildHttpClientFactory () {
        var builder = new CorvidHttpClientFactoryBuilder {
             HttpClientFactoryConfig = new CorvidHttpClientFactoryConfig {
                DefaultClientConfig = new CorvidHttpClientConfig {
                    Name = _clientName,
                    AcceptedMediaTypes = [ "application/json" ],
                    BaseUrl = @"https://jsonplaceholder.typicode.com/",
                    UserAgent = "Corvid Client Handler Example/0.1-ALPHA",
                    DefaultRequestHeaders = new Dictionary<string, string> ()
                }
             }
        };

        return builder.BuildClientFactory ();
    }

    private static ICorvidHttpClientHandlerFactory BuildHandlerFactory (ICorvidHttpClientRateLimiter ratelimiter) {
        var builder = new CorvidHttpClientHandlerFactoryBuilder {
            HttpClientHandlerFactoryConfig = new CorvidHttpClientHandlerFactoryConfig {
                ClientFactory = BuildHttpClientFactory (),
                DefaultHandlerConfig = new CorvidHttpClientHandlerConfig {
                    Name = _clientName,
                    RateLimiter = ratelimiter
                }
            }
        };

        return builder.BuildHandlerFactory ();
    }
}