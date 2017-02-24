using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    class Server
    {
        private IPEndPoint _serverEndPoint;
        private Socket _server;

        public Server()
        {
            _server = null;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.serverIP), Properties.Settings.Default.serverPort);
        }

        public bool Connect()
        {
            try
            {
                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Connect(_serverEndPoint);
                _server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                return true;
            }
            catch (Exception err) { return false; };    // gestire eccezioni
        }
    }
}
