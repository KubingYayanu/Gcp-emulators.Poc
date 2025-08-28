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
                SetEnvironmentVariableEmulatorHost();
            }
        }

        /// <summary>
        /// 若是使用 Emulator, 必須設定環境變數 PUBSUB_EMULATOR_HOST
        /// </summary>
        private void SetEnvironmentVariableEmulatorHost()
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST",
                !Emulated
                    ? null
                    : EmulatorHost);
        }

        public long DefaultAckDeadline { get; set; }

        private string? _secret;

        public string? Secret
        {
            get => _secret;
            set
            {
                var decryptedSecret = DecryptSecret(value);
                _secret = decryptedSecret;
            }
        }

        private string? DecryptSecret(string? encrypted)
        {
            if (Emulated || string.IsNullOrEmpty(encrypted))
            {
                return null;
            }

            // TODO: Implement your decryption logic here
            var decrypted = encrypted;
            return decrypted;
        }
    }
}