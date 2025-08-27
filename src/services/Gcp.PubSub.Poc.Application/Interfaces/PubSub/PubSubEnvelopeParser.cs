using System.Text.Json;
using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public static class PubSubEnvelopeParser
    {
        public static PubSubEnvelope<T> Parse<T>(PubsubMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // 解析 Data
            var payload = JsonSerializer.Deserialize<T>(message.Data.ToStringUtf8())
                          ?? throw new InvalidOperationException("Message data is null or cannot be deserialized.");

            // 解析標準屬性
            if (!message.Attributes.TryGetValue("event_type", out var eventType))
                throw new InvalidOperationException("Missing required attribute: event_type");

            var schemaVersion = message.Attributes
                .GetValueOrDefault("schema_version", "v1");
            var traceId = message.Attributes
                .GetValueOrDefault("trace_id", Guid.NewGuid().ToString("N"));
            var correlationId = message.Attributes
                .GetValueOrDefault("correlation_id", Guid.NewGuid().ToString("N"));

            if (!message.Attributes.TryGetValue("sent_at", out var sentAtRaw)
                || !DateTimeOffset.TryParse(sentAtRaw, out var sentAt))
            {
                sentAt = DateTimeOffset.UtcNow;
            }

            var orderingKey = !string.IsNullOrEmpty(message.OrderingKey)
                ? message.OrderingKey
                : null;

            // 建立 Envelope
            var envelope = new PubSubEnvelope<T>(
                data: payload,
                eventType: eventType,
                schemaVersion: schemaVersion,
                traceId: traceId,
                correlationId: correlationId,
                orderingKey: orderingKey,
                sentAt: sentAt);
            
            // 補上 PubSub MessageId
            envelope.MessageId = message.MessageId;

            // 放入額外 attributes
            foreach (var kv in message.Attributes)
            {
                if (kv.Key is "event_type" or "schema_version" or "trace_id" or "correlation_id" or "sent_at")
                    continue;

                envelope.ExtraAttributes[kv.Key] = kv.Value;
            }

            return envelope;
        }
    }
}