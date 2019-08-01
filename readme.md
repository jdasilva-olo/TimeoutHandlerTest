This sample project demonstrate how the Timeout when using `HttpClientTimeoutHandler` does not include reading the response unlike the default `HttpClient` behavior. _It's similar to using `HttpCompletionOption.ResponseHeadersRead`._

HttpClientTimeoutHandler is from https://www.thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/.
Also, the code directly in the post does not use `.ConfigureAwait(false)` and it should.

Related articles:
* https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore (find `HttpClientHandler`)
* https://medium.com/@szntb/getting-burnt-with-httpclient-9c1712151039
