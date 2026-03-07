
using System.Text.Json;

using DoubleCorvid.CorvidClientHandler.Builders;
using DoubleCorvid.CorvidClientHandler.Config;
using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.Config;

namespace DoubleCorvid.CorvidClientHandler.Examples;

public class Program {
    private static readonly string _clientName = "default";

    private static readonly CancellationTokenSource _cancellationTokenSource = new ();

    private static CancellationToken _cancelToken => _cancellationTokenSource.Token;

    public static async Task Main () {
        await ExecuteJsonPlaceholderExamples ();
    }

    private static async Task ExecuteJsonPlaceholderExamples () {
        var handlerFactory = BuildHandlerFactory ();

        var client = handlerFactory.CreateHandler (_clientName);

        await ExecuteGetExampleAsync (client);

        await ExecutePatchExampleAsync (client);

        await ExecutePostExampleAsync (client);

        await ExecutePutExampleAsync (client);

        await ExecuteDeleteExampleAsync (client);
    }

    private static async Task ExecuteGetExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("Executing get example...");

        var response = await client.GetAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken = _cancelToken
        });

        if (response.HttpResponseMessage.IsSuccessStatusCode) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Get example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Get example failed with status code: {response.HttpResponseMessage.StatusCode}");
        }
    }

    private static async Task ExecutePatchExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting patch example...");

        var response = await client.PatchAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            Content = new StringContent (JsonSerializer.Serialize<object> (new {
                Title = "foo"
            })),
            CancellationToken = _cancelToken
        });

        if (response.HttpResponseMessage.IsSuccessStatusCode) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Patch example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Patch example failed with status code: {response.HttpResponseMessage.StatusCode}");
        }
    }

    private static async Task ExecutePostExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting post example...");
        
        var response = await client.PostAsync (new CorvidHttpClientRequestConfig {
            Route = "posts",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            Content = new StringContent (JsonSerializer.Serialize<object> (new {
                Title = "foo",
                Body = "bar",
                UserId = 1,
            })),
            CancellationToken = _cancelToken
        });

        if (response.HttpResponseMessage.IsSuccessStatusCode) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Post example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Post example failed with status code: {response.HttpResponseMessage.StatusCode}");
        }
    }

    private static async Task ExecutePutExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting put example...");
        
        var response = await client.PutAsync (new CorvidHttpClientRequestConfig {
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

        if (response.HttpResponseMessage.IsSuccessStatusCode) {
            var str = await response.HttpResponseMessage.Content.ReadAsStringAsync ();

            Console.WriteLine ($"Put example succeded:\n{str}");
        }
        else {
            Console.WriteLine ($"Put example failed with status code: {response.HttpResponseMessage.StatusCode}");
        }
    }

    private static async Task ExecuteDeleteExampleAsync (ICorvidHttpClientHandler client) {
        Console.WriteLine ("\nExecuting delete example...");
        
        var response = await client.DeleteAsync (new CorvidHttpClientRequestConfig {
            Route = "posts/1",
            HttpCompletionOption = HttpCompletionOption.ResponseContentRead,
            CancellationToken = _cancelToken
        });

        if (response.HttpResponseMessage.IsSuccessStatusCode) {
            Console.WriteLine ($"Delete example succeded. (It's hard to display an object that doesn't exist :) )");
        }
        else {
            Console.WriteLine ($"Delete example failed with status code: {response.HttpResponseMessage.StatusCode}");
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

    private static ICorvidHttpClientHandlerFactory BuildHandlerFactory () {
        var builder = new CorvidHttpClientHandlerFactoryBuilder {
            HttpClientHandlerFactoryConfig = new CorvidHttpClientHandlerFactoryConfig {
                ClientFactory = BuildHttpClientFactory (),
                DefaultHandlerConfig = new CorvidHttpClientHandlerConfig {
                    Name = _clientName
                }
            }
        };

        return builder.BuildHandlerFactory ();
    }
}