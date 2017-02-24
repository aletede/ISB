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
        private IPEndPoint _serverIP;
        private Socket _server;

        public Server()
        {
            _server = null;
            _serverIP = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.serverIP), Properties.Settings.Default.serverPort);
        }

        public bool Connect()
        {
            try
            {

                return true;
            }
            catch (Exception err) { return false; };
        }
    }
}
