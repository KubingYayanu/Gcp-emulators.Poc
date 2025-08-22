using System.Net;
using System.Net.Sockets;

namespace Gcp.PubSub.Poc.Worker.Serilog
{
    public class ContainerIdentity
    {
        public class ContainerInfo
        {
            /// <summary>
            /// 容器名稱 (從 HOSTNAME 環境變數或機器名稱取得)
            /// </summary>
            public string ContainerName { get; set; }

            /// <summary>
            /// 專案名稱 (從環境變數取得)
            /// </summary>
            public string ProjectName { get; set; }

            /// <summary>
            /// 容器的 IP 位址
            /// </summary>
            public string IpAddress { get; set; }
        }

        /// <summary>
        /// 取得容器身份資訊
        /// Docker 會自動設定 HOSTNAME 環境變數為容器名稱
        /// </summary>
        public static ContainerInfo GetContainerInfo()
        {
            // Docker 容器內部，HOSTNAME 環境變數會自動設定為容器名稱
            var containerName = Environment.GetEnvironmentVariable("HOSTNAME")
                                ?? Environment.MachineName;

            // 從 Docker Compose 環境變數取得服務資訊
            var projectName = Environment.GetEnvironmentVariable("COMPOSE_PROJECT_NAME") ?? "unknown";

            var containerInfo = new ContainerInfo
            {
                ContainerName = containerName,
                ProjectName = projectName,
                IpAddress = GetLocalIpAddress()
            };

            return containerInfo;
        }

        /// <summary>
        /// 取得容器的本地 IP 位址
        /// </summary>
        private static string GetLocalIpAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                foreach (IPAddress address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork
                        && !IPAddress.IsLoopback(address))
                    {
                        return address.ToString();
                    }
                }

                return "127.0.0.1";
            }
            catch
            {
                return "unknown";
            }
        }

        /// <summary>
        /// 產生包含容器身份的結構化日誌
        /// </summary>
        public static void LogWithContainerInfo(string message, string level = "INFO")
        {
            var containerInfo = GetContainerInfo();

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage =
                $"[{timestamp}] [{level}] [{containerInfo.ContainerName}] {message}";

            Console.WriteLine(logMessage);
        }

        /// <summary>
        /// 顯示容器啟動資訊
        /// </summary>
        public static void LogStartupInfo()
        {
            var containerInfo = GetContainerInfo();

            Console.WriteLine("=== 容器身份資訊 ===");
            Console.WriteLine($"容器名稱: {containerInfo.ContainerName}");
            Console.WriteLine($"專案名稱: {containerInfo.ProjectName}");
            Console.WriteLine($"IP 位址: {containerInfo.IpAddress}");
            Console.WriteLine("==================");
        }
    }
}