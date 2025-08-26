using System.Text.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    public class PubSubEnvelope<T>
    {
        public PubSubEnvelope(
            T data,
            string eventType,
            string schemaVersion = "v1",
            string? traceId = null,
            string? correlationId = null,
            DateTimeOffset? sentAt = null,
            Dictionary<string, string>? extraAttributes = null)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            SchemaVersion = schemaVersion;
            TraceId = traceId ?? Guid.NewGuid().ToString("N");
            CorrelationId = correlationId ?? Guid.NewGuid().ToString("N");
            SentAt = sentAt ?? DateTimeOffset.UtcNow;

            if (extraAttributes == null) return;

            foreach (var extraAttribute in extraAttributes)
            {
                ExtraAttributes.TryAdd(extraAttribute.Key, extraAttribute.Value);
            }
        }

        public T Data { get; }

        public string EventType { get; }

        public string SchemaVersion { get; }

        public string TraceId { get; }

        public string CorrelationId { get; }

        public DateTimeOffset SentAt { get; }

        public Dictionary<string, string> ExtraAttributes { get; } = new();

        public PubsubMessage ToPubsubMessage()
        {
            var message = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(Data))
            };

            // 標準化 Attributes
            message.Attributes["event_type"] = EventType;
            message.Attributes["schema_version"] = SchemaVersion;
            message.Attributes["trace_id"] = TraceId;
            message.Attributes["correlation_id"] = CorrelationId;
            message.Attributes["sent_at"] = SentAt.ToString("o");

            // 額外 Attributes
            foreach (var kv in ExtraAttributes)
            {
                message.Attributes[kv.Key] = kv.Value;
            }

            return message;
        }
    }
}