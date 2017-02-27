using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    static class Client
    {
        public static BackgroundWorker _worker = new BackgroundWorker();
        private static IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.serverIP), Properties.Settings.Default.serverPort);
        private static TcpClient client;

        public static void Connect()
        {
            if (client!=null && client.Connected) return;
            // rivedere se inserire tutte le istruzioni seguenti in un blocco try-catch
            client = new TcpClient();
            do
            {
                try
                {
                    _worker.ReportProgress(1, "Trying to connect to the server...");
                    client.Connect(_serverEndPoint);
                    if (!client.Connected) System.Threading.Thread.Sleep(Constants.RECONNECT);
                }
                catch (Exception err)
                {
                    // rivedere gestione eccezioni e log file
                }
            }
            while (!client.Connected);
            _worker.ReportProgress(1, "Client connected to the server!");
        }

        public static void LoadRemoteData(string remoteDirPath)
        {
            string answer;
            try
            {
                var stream = client.GetStream();
                var writer = new BinaryWriter(stream);
                var reader = new BinaryReader(stream);
                writer.Write(100);
                answer = reader.ReadString();
                _worker.ReportProgress(2, answer);
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
                _worker.ReportProgress(2, null);
                Connect();
            }
        }
    }
}
