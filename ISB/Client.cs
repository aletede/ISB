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
        private static BinaryReader _reader;
        private static BinaryWriter _writer;

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
            InitStreams();
        }

        private static void InitStreams()
        {
            try
            {
                NetworkStream _stream = _client.GetStream();
                _stream.ReadTimeout = Constants.READTIMEOUT;
                _stream.WriteTimeout = Constants.WRITETIMEOUT;
                _reader = new BinaryReader(_stream);
                _writer = new BinaryWriter(_stream);
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
            }
        }

        public static void LoadRemoteData(string localDirPath)
        {
            // scope: receiving all backup filenames in the server
        }

        public static void LoadRemoteFiles(string localDirPath)
        {
            // Chiamare LoadRemoteData nel caso in cui per qualsiasi motivo non è stato chiamato con successo in precedenza
            // scope: receiving last directory status stored on the server
        }

        public static bool SyncFile(FileStream fs, string fullpath, string checksum, string localDirPath)
        {
            bool TransactionCompleted = false;
            try
            {
                Int32 serverAnswer;
                // notify server about SyncFile
                _writer.Write(Constants.SYNCFILE);
                serverAnswer = _reader.ReadInt32(); // wait for server answer
                if (serverAnswer == Constants.INIT)
                {
                    // for some reasons, server init has not been done before
                    LoadRemoteData(localDirPath);
                    serverAnswer = _reader.ReadInt32();
                }
                if (serverAnswer == Constants.SRVREADY)
                {
                    // send to server path of the file
                    _writer.Write(fullpath);
                    // send to server checksum of the file
                    _writer.Write(checksum);
                    // send file content
                   // _writer.Write(fs.Length);
                    byte[] buffer = new byte[Constants.BUFFSIZE];
                    while (fs.Read(buffer, 0, Constants.BUFFSIZE) > 0)
                        _writer.Write(buffer);
                    serverAnswer = _reader.ReadInt32();   // wait for server answer
                    if (serverAnswer == Constants.POS_ANS)
                        TransactionCompleted = true;
                }
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
            }
            return TransactionCompleted;
        }

        public static bool SyncDelete(string fullpath)
        {
            bool TransactionCompleted = false;
            return TransactionCompleted;
        }
    }
}
