using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole
{
    // See https://www.thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
    public class HttpClientTimeoutHandler : DelegatingHandler
    {
        public TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(100);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var cts = GetCancellationTokenSource(request, cancellationToken))
            {
                try
                {
                    cancellationToken = cts?.Token ?? cancellationToken;
                    var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                      // This doesn't cancel the Load which will continue in the background, HttpClient has access
                      // to an internal method that takes a cancellation token.
//                    await Task.WhenAny(IsCanceledAsync(), response.Content.LoadIntoBufferAsync()).ConfigureAwait(false);
//                    if (cancellationToken.IsCancellationRequested)
//                    {
//                        response.Dispose();
//                        throw new TimeoutException();
//                    }
//
                    return response;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }
            }

            Task IsCanceledAsync()
            {
                var tcs = new TaskCompletionSource<bool>();
                cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
                return tcs.Task;
            }
        }

        private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? DefaultTimeout;
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                // No need to create a CTS if there's no timeout
                return null;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            return cts;
        }
    }

    public static class HttpRequestExtensions
    {
        private const string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
            {
                return timeout;
            }

            return null;
        }
    }
}