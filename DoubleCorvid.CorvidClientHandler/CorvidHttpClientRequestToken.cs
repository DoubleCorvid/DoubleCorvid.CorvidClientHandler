using DoubleCorvid.CorvidClientHandler.Framework;
using DoubleCorvid.CorvidClientHandler.Framework.RateLimiting;

namespace DoubleCorvid.CorvidClientHandler;

public class CorvidHttpClientRequestToken : ICorvidHttpClientRequestToken {
    private readonly TaskCompletionSource<ICorvidHttpClientRequestResponse?> taskCompletionSource = new ();

    public bool IsRejected { get; private set; }

    public bool IsExpired { get; private set; }

    public bool IsCompleted { get; private set; }

    public bool IsCancelled { get; private set; }

    public bool IsSuccess => Response?.HttpResponseMessage.IsSuccessStatusCode ?? false;

    public void Reject () { 
        taskCompletionSource.SetCanceled ();

        IsRejected = true; 
    }

    public void Expire () { 
        taskCompletionSource.SetCanceled ();

        IsExpired = true; 
    }

    public void Complete () {
        taskCompletionSource.SetResult (Response);

        IsCompleted = true; 
    }

    public void Cancel () {
        taskCompletionSource.SetCanceled ();
        
        IsCancelled = true; 
    }

    public ICorvidHttpClientRequestResponse? Response { get; set; }

    public Task<ICorvidHttpClientRequestResponse?> WaitForResponseAsync () => taskCompletionSource.Task;
}