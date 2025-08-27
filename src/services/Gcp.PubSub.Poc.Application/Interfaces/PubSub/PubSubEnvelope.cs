using System.Text.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;

namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubEnvelope<T>
    {
        public PubSubEnvelope(
            T data,
            string eventType,
            string schemaVersion = "v1",
            string? traceId = null,
            string? correlationId = null,
            string? orderingKey = null,
            DateTimeOffset? sentAt = null,
            Dictionary<string, string>? extraAttributes = null)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            SchemaVersion = schemaVersion;
            TraceId = traceId ?? Guid.NewGuid().ToString("N");
            CorrelationId = correlationId ?? Guid.NewGuid().ToString("N");
            SentAt = sentAt ?? DateTimeOffset.UtcNow;
            OrderingKey = orderingKey;

            if (extraAttributes == null) return;

            foreach (var extraAttribute in extraAttributes)
            {
                ExtraAttributes.TryAdd(extraAttribute.Key, extraAttribute.Value);
            }
        }

        /// <summary>
        /// Pub/Sub 訊息唯一識別 (由 Pub/Sub 產生)
        /// 只有在 Subscriber 收到訊息後才會有值
        /// </summary>
        public string MessageId { get; set; }

        public T Data { get; }

        public string EventType { get; }

        public string SchemaVersion { get; }

        public string TraceId { get; }

        public string CorrelationId { get; }

        public DateTimeOffset SentAt { get; }

        /// <summary>
        /// https://chatgpt.com/share/68ae6605-ad00-8013-9cd5-ed0461791a09
        /// 如果需要某些訊息一定要按照順序處理，就必須使用 ordering_key
        /// Publisher 端: 可以在 PubsubMessage.OrderingKey 設定一個字串值
        /// Subscriber 端: 建立 Subscription 時開啟 Message Ordering，同一個 ordering_key 的訊息會依序送達，並且在該 key 未 ack 完成前不會發送下一筆同 key 訊息
        /// </summary>
        public string? OrderingKey { get; set; }

        /// <summary>
        /// 可設定訊息的屬性, 例如: 事件類型、來源等
        /// 可作為 Subscription 上針對訊息的過濾條件
        /// </summary>
        public Dictionary<string, string> ExtraAttributes { get; } = new();

        public string GetExtraAttribute(string key, string defaultValue = "")
        {
            return ExtraAttributes.GetValueOrDefault(key, defaultValue);
        }

        public void SetExtraAttribute(string key, string value)
        {
            ExtraAttributes[key] = value;
        }

        public PubsubMessage ToPubsubMessage()
        {
            var message = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(Data))
            };

            if (!string.IsNullOrEmpty(OrderingKey))
            {
                message.OrderingKey = OrderingKey;
            }

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