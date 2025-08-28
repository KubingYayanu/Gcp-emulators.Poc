namespace Gcp.PubSub.Poc.Application.Interfaces.PubSub
{
    public class PubSubOptions
    {
        public const string GcpPubSub = "Gcp:PubSub";

        public bool Emulated { get; set; }

        private string _emulatorHost;

        public string EmulatorHost
        {
            get => _emulatorHost;
            set
            {
                _emulatorHost = value;
                SetEnvironmentVariable();
            }
        }

        public long DefaultAckDeadline { get; set; }

        /// <summary>
        /// 若是使用 Emulator, 必須設定環境變數 PUBSUB_EMULATOR_HOST
        /// </summary>
        private void SetEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST",
                !Emulated
                    ? null
                    : EmulatorHost);
        }
    }
}