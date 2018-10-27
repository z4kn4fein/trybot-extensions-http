using Trybot.Extensions.Http;

namespace System.Net.Http
{
    /// <summary>
    /// Contains the extension methods for adding additional Trybot related data to the <see cref="HttpRequestMessage"/>.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        private const string CorrelationIdKey = "TrybotCorrelationIdKey";

        /// <summary>
        /// Gets the correlation id from the <see cref="HttpRequestMessage.Properties"/>.
        /// This is used by the <see cref="TrybotMessageHandler"/> to set the correlation id of the actual policy execution.
        /// </summary>
        /// <param name="message">The request message.</param>
        /// <returns>The correlation id.</returns>
        public static object GetCorrelationId(this HttpRequestMessage message) =>
            message.Properties.TryGetValue(CorrelationIdKey, out var value) ? value : null;

        /// <summary>
        /// Stores the correlation id in the <see cref="HttpRequestMessage.Properties"/>.
        /// This can be used to synchronize the correlation id between the application and the actual policy execution.
        /// </summary>
        /// <param name="message">The request message.</param>
        /// <param name="correlationId">The correlation id.</param>
        public static void SetCorrelationId(this HttpRequestMessage message, object correlationId) =>
            message.Properties[CorrelationIdKey] = correlationId;
    }
}
