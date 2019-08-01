using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const bool UseTimeoutHandler = true;

            Console.WriteLine("Press any key to make request.");
            Console.ReadKey(true);
            Console.WriteLine("Starting...");

            var handler = UseTimeoutHandler
                ? new HttpClientTimeoutHandler {InnerHandler = new HttpClientHandler()}
                : (HttpMessageHandler)new HttpClientHandler();
            var httpClient = new HttpClient(handler)
            {
                Timeout = UseTimeoutHandler ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(6),
            };

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:52587");
            if (UseTimeoutHandler)
            {
                request.SetTimeout(TimeSpan.FromSeconds(5));
            }

            var response = await httpClient.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();

            Console.WriteLine(text);
            Console.WriteLine("Done.");
        }
    }
}
