using System;
using System.Net.Http;
using Trybot;
using Trybot.Extensions.Http;
using Trybot.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains the extension methods for configuring <see cref="TrybotMessageHandler"/> in the <see cref="HttpClient"/> message handler chain.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Wraps the request execution of the underlying <see cref="HttpClient"/> 
        /// with the given <see cref="IBotPolicy{HttpResponseMessage}"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="policy">The bot policy.</param>
        /// <returns>The builder to continue the configuration of the <see cref="HttpClient"/>.</returns>
        public static IHttpClientBuilder AddTrybotPolicy(this IHttpClientBuilder builder, IBotPolicy<HttpResponseMessage> policy)
        {
            Shield.EnsureNotNull(policy, nameof(policy));
            return builder.AddHttpMessageHandler(() => new TrybotMessageHandler(policy));
        }

        /// <summary>
        /// Wraps the request execution of the underlying <see cref="HttpClient"/> 
        /// with the given <see cref="IBotPolicy{HttpResponseMessage}"/>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="policyBuilder">The policy builder delegate used to create and configure the <see cref="IBotPolicy{HttpResponseMessage}"/>.</param>
        /// <returns>The builder to continue the configuration of the <see cref="HttpClient"/>.</returns>
        public static IHttpClientBuilder AddTrybotPolicy(this IHttpClientBuilder builder, Action<IBotPolicyBuilder<HttpResponseMessage>> policyBuilder)
        {
            Shield.EnsureNotNull(policyBuilder, nameof(policyBuilder));

            return builder.AddHttpMessageHandler(() => new TrybotMessageHandler(policyBuilder));
        }
    }
}
