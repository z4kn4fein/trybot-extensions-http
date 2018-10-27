using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot.Utils;

namespace Trybot.Extensions.Http
{
    /// <inheritdoc />
    /// <summary>
    /// A custom <see cref="DelegatingHandler" /> implementation that wraps the underlying request execution with a <see cref="IBotPolicy{HttpResponseMessage}" />.
    /// </summary>
    public class TrybotMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// The <see cref="IBotPolicy{HttpResponseMessage}"/> which wraps the underlying request execution.
        /// </summary>
        protected readonly IBotPolicy<HttpResponseMessage> BotPolicy;

        /// <summary>
        /// Constructs a <see cref="TrybotMessageHandler"/>.
        /// </summary>
        /// <param name="policyBuilder">The policy builder delegate used to create and configure the <see cref="IBotPolicy{HttpResponseMessage}"/>.</param>
        public TrybotMessageHandler(Action<IBotPolicyBuilder<HttpResponseMessage>> policyBuilder)
        {
            Shield.EnsureNotNull(policyBuilder, nameof(policyBuilder));

            this.BotPolicy = new BotPolicy<HttpResponseMessage>();
            this.BotPolicy.Configure(policyBuilder);
        }

        /// <summary>
        /// Constructs a <see cref="TrybotMessageHandler"/>.
        /// </summary>
        /// <param name="botPolicy">The bot policy.</param>
        public TrybotMessageHandler(IBotPolicy<HttpResponseMessage> botPolicy)
        {
            Shield.EnsureNotNull(botPolicy, nameof(botPolicy));

            this.BotPolicy = botPolicy;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationdId = request.GetCorrelationId();
            return await this.BotPolicy.ExecuteAsync((req, ctx, token) =>
                this.WrappedSendAsync(req, token), correlationdId, request, cancellationToken);
        }

        /// <summary>
        /// The request execution wrapped by the <see cref="IBotPolicy{HttpResponseMessage}"/>.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task which executed the request.</returns>
        protected virtual Task<HttpResponseMessage> WrappedSendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            base.SendAsync(request, cancellationToken);
    }
}
