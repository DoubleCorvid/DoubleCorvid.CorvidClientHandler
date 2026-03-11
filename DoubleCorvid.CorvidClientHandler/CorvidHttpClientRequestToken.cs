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

using DoubleCorvid.CorvidClientHandler.Framework;

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