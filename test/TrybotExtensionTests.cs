using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot.Operations;
using Trybot.Retry.Exceptions;
using Xunit;

namespace Trybot.Extensions.Http.Tests
{
    public class TrybotExtensionTests
    {
        [Fact]
        public async Task HttpClientBuilderTests()
        {
            var mockPolicy = new Mock<IBotPolicy<HttpResponseMessage>>();
            mockPolicy.Setup(p => p.ExecuteAsync(It.IsAny<IAsyncBotOperation<HttpResponseMessage>>(),
                    "correlationId", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)).Verifiable("Expected to call ExecuteAsync()");

            using (var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient<TestController>()
                        .AddTrybotPolicy(mockPolicy.Object);
                })
                .UseStartup<TestStartup>()))
            using (var client = server.CreateClient())
            using (var response = await client.GetAsync("api/test/policy"))
                response.EnsureSuccessStatusCode();

        }

        [Fact]
        public async Task HttpClientBuilderTests_Retry_With_WrongUrl()
        {
            var retryCount = 0;
            using (var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient<TestController>()
                        .AddTrybotPolicy(options => options
                            .Timeout(timeoutOptions => timeoutOptions
                                .After(TimeSpan.FromSeconds(3)))
                            .Retry(retryOptions => retryOptions
                                .OnRetry((ex, ctx) => retryCount++)
                                .WhenExceptionOccurs(ex => true)
                                .WithMaxAttemptCount(3)));
                })
                .UseStartup<TestStartup>()))
            using (var client = server.CreateClient())
                await Assert.ThrowsAsync<MaxRetryAttemptsReachedException>(() =>
                    client.GetAsync("api/test/policy2"));

            Assert.Equal(3, retryCount);
        }
    }

    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddApplicationPart(typeof(TestController).Assembly).AddControllersAsServices();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvc();
        }
    }

    [Route("api/test")]
    public class TestController : Controller
    {
        private readonly HttpClient client;

        public TestController(HttpClient client)
        {
            this.client = client;
        }

        [HttpGet("policy")]
        public async Task Policy()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            request.SetCorrelationId("correlationId");
            await this.client.SendAsync(request);
        }

        [HttpGet("policy2")]
        public async Task Policy2()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            await this.client.SendAsync(request);
        }
    }
}
