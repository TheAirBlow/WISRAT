using System.Net.Sockets;

namespace WISRAT.CLI
{
    /// <summary>
    /// User object
    /// </summary>
    public class User
    {
        public string Name;
        public string Password;
        public string IP;
        public TcpClient Client;
    }
}