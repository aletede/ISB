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
        public static Dictionary<string, string> _remoteStatus = new Dictionary<string, string>();  // da rivedere struttura e anche passaggio come argomento
        public static List<LocalEntry> _remoteEntries = null;
        public static BackgroundWorker _worker = new BackgroundWorker();
        private static IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.serverIP), Properties.Settings.Default.serverPort);
        private static TcpClient _client;

        public static void Init(object s, DoWorkEventArgs args)
        {
            Connect();
            //LoadRemoteData((string)args.Argument);
            Monitoring.Start((string)args.Argument);
        }

        public static void Connect()
        {
            if (_client!=null && _client.Connected) return;
            // rivedere se inserire tutte le istruzioni seguenti in un blocco try-catch
            _client = new TcpClient();
            do
            {
                try
                {
                    _worker.ReportProgress(1, "Trying to connect to the server...");
                    _client.Connect(_serverEndPoint);
                    if (!_client.Connected) System.Threading.Thread.Sleep(Constants.RECONNECT);
                }
                catch (Exception err)
                {
                    // rivedere gestione eccezioni e log file
                }
            }
            while (!_client.Connected);
            _worker.ReportProgress(1, "Client connected to the server!");
        }

        public static void LoadRemoteData(string localDirPath)
        {

        }

        public static void LoadRemoteFiles(string localDirPath)
        {
            // Chiamare LoadRemoteData nel caso in cui per qualsiasi motivo non è stato chiamato con successo in precedenza
        }

        public static bool Sync(FileStream fs, string fullpath, string checksum)
        {
            bool TransactionCompleted = false;
            return TransactionCompleted;
        }

        public static bool SyncDelete(string fullpath)
        {
            bool TransactionCompleted = false;
            return TransactionCompleted;
        }
    }
}
