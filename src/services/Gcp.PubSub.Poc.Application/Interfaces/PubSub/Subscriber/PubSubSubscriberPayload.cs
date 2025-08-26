namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Subscriber
{
    public class PubSubSubscriberPayload
    {
        /// <summary>
        /// Pub/Sub 訊息唯一識別 (由 Pub/Sub 產生)
        /// </summary>
        public string MessageId { get; set; }
        
        /// <summary>
        /// 接收到的訊息內容, 需反序列化
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 可設定訊息的屬性, 例如: 事件類型、來源等
        /// 可作為 Subscription 上針對訊息的過濾條件
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}