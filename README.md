# trybot-extensions-http [![Appveyor build status](https://img.shields.io/appveyor/ci/pcsajtai/trybot-extensions-http/master.svg?label=appveyor)](https://ci.appveyor.com/project/pcsajtai/trybot-extensions-http/branch/master) [![Travis CI build status](https://img.shields.io/travis/z4kn4fein/trybot-extensions-http/master.svg?label=travis-ci)](https://travis-ci.org/z4kn4fein/trybot-extensions-http) [![NuGet Version](https://buildstats.info/nuget/Trybot.Extensions.Http)](https://www.nuget.org/packages/Trybot.Extensions.Http/)

This package is an ASP.NET Core integration for [Trybot](https://github.com/z4kn4fein/trybot) and contains extensions for the [IHttpClientBuilder](https://github.com/aspnet/HttpClientFactory/tree/master/src/Microsoft.Extensions.Http/DependencyInjection) to apply resilience on HttpClient calls.

## Usage
From ASP.NET Core 2.1 you can configure your `HttpClient` instances via an `IHttpClientBuilder` offered by the `AddHttpClient()` function on the `IServiceCollection`. This package extends this builder interface for configuring transient fault handling around the calls initiated by those HttpClients. 

1. Configure your `HttpClient`
    ```c#
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<CustomController>()
            .AddTrybotPolicy(options => options
                .Timeout(timeoutOptions => timeoutOptions
                    .After(TimeSpan.FromSeconds(10)))
                .Retry(retryOptions => retryOptions
                    .WhenExceptionOccurs(exception => exception is HttpRequestException)
                    .WhenResultIs(result => result.StatusCode != HttpStatusCode.Ok)
                    .WaitBetweenAttempts((attempt, result, exception) => TimeSpan.FromSeconds(5))
                    .WithMaxAttemptCount(3)));
    }
    ```

2. Then you can take the configured `HttpClient` by dependency injection in your controller:
    ```c#
    public class CustomController : Controller
    {
        private readonly HttpClient client;

        public CustomController(HttpClient client)
        {
            this.client = client;
        }

        public async Task CustomAction()
        {
            return Ok(await client.GetStringAsync("/something"));
        }
    }
    ```

## Documentation
- [Trybot documentaion](https://github.com/z4kn4fein/trybot/blob/master/README.md)
- [IHttpClientFactory documentaion](https://docs.microsoft.com/en-gb/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1)
- [Resiliency patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/category/resiliency)
