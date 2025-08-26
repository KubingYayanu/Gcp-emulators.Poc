namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub.Publisher
{
    public class PubSubPublisherPayload
    {
        /// <summary>
        /// 發布的訊息內容, 需序列化
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 可設定訊息的屬性, 例如: 事件類型、來源等
        /// 可作為 Subscription 上針對訊息的過濾條件
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}