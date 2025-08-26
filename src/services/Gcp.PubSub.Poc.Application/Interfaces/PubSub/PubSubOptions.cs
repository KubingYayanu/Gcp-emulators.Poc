namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        /// <summary>
        /// 若是使用 Emulator, 必須設定環境變數 PUBSUB_EMULATOR_HOST
        /// 否則會報錯
        /// </summary>
        public string? Host => Emulated
            ? null
            : Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");

        public long SubscriberAckDeadline { get; set; }
    }
}