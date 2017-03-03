using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ISB
{
    static class Client
    {
        public static Dictionary<string, string> _remoteStatus = null;  // da rivedere struttura e anche passaggio come argomento
        public static List<RemoteEntry> _remoteEntries = null;
        public static BackgroundWorker _worker = new BackgroundWorker();
        public static ManualResetEvent _mre = new ManualResetEvent(false);
        private static IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.serverIP), Properties.Settings.Default.serverPort);
        private static TcpClient _client;
        private static BinaryReader _reader;
        private static BinaryWriter _writer;
        public static string _dirPath;

        public static void Init(object s, DoWorkEventArgs args)
        {
            // qui inizia il bgWorker, forse è meglio mettere un try catch per evitare che eccezioni strane fanno crashare l'app
            _dirPath = (string)args.Argument;
            Connect();
            LoadBackupData();
            Monitoring.Start(_dirPath);
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
                    else
                    {
                        _worker.ReportProgress(1, "Client connected to the server!");
                        InitStreams();
                        InitServer();
                        _worker.ReportProgress(1, "After InitServer!");
                    }
                }
                catch (Exception err)
                {
                    // rivedere gestione eccezioni e log file
                    // catch eccezioni di InitStreams???
                    // catch eccezioni di InitServer???
                }
            }
            while (!_client.Connected);
        }

        private static void InitStreams()
        {
            NetworkStream _stream = _client.GetStream();
            _stream.ReadTimeout = Constants.READTIMEOUT;
            _stream.WriteTimeout = Constants.WRITETIMEOUT;
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
        }

        private static void InitServer()
        {
            _writer.Write(Constants.INIT);
            _writer.Write(_dirPath);
            _worker.ReportProgress(1, "Waiting for positive answer from server!");
            if (_reader.ReadInt32() == Constants.POS_ANS)
                _worker.ReportProgress(1, "Init server completed!");
        }

        public static void LoadBackupData()
        {
            // scope: receiving all backup filenames in the server
            try
            {
                Int32 answer;
                List<RemoteEntry> _temp = new List<RemoteEntry>();
                _writer.Write(Constants.RCVENTRIES);
                while ((answer = _reader.ReadInt32()) == Constants.INIT)
                    InitServer();
                for (Int32 i=0; i < answer; i++)
                {
                    string _id = _reader.ReadString();
                    string _date = _reader.ReadString();
                    _temp.Add(new RemoteEntry(_date, _id));
                }
                _remoteEntries = _temp;
                _worker.ReportProgress(2, true);    // rivedere
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
                // catch eccezioni initServer???
            }
        }

        public static void LoadRemoteFiles()
        {
            // scope: receiving last directory status stored on the server
            try
            {
                Int32 answer;
                Dictionary<string, string> _temp = new Dictionary<string, string>();
                _writer.Write(Constants.RCVDATAFILE);
                while ((answer = _reader.ReadInt32()) == Constants.INIT)
                    InitServer();
                for (Int32 i = 0; i < answer; i++)
                {
                    string _path = _reader.ReadString();
                    string _checksum = _reader.ReadString();
                    _temp.Add(_path, _checksum);
                }
                _remoteStatus = _temp;
                _worker.ReportProgress(1, "LoadRemoteFiles done!");
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
                // catch eccezioni initServer???
            }
        }

        public static bool SyncFile(FileStream fs, string fullpath, string checksum)
        {
            bool TransactionCompleted = false;
            try
            {
                Int32 serverAnswer;
                // notify server about SyncFile
                _writer.Write(Constants.SYNCFILE);
                while ((serverAnswer = _reader.ReadInt32()) == Constants.INIT)
                    InitServer();
                if (serverAnswer == Constants.SRVREADY)
                {
                    // send to server path of the file
                    _writer.Write(fullpath);
                    // send to server checksum of the file
                    _writer.Write(checksum);
                    // send file content
                    _writer.Write(fs.Length);
                    fs.CopyTo(_writer.BaseStream);  // da rivedere!!!!!!
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
            try
            {
                Int32 serverAnswer;
                _writer.Write(Constants.SYNCDELETE);
                while ((serverAnswer = _reader.ReadInt32()) == Constants.INIT)
                    InitServer();
                if (serverAnswer == Constants.SRVREADY)
                    _writer.Write(fullpath);
                serverAnswer = _reader.ReadInt32();
                if (serverAnswer == Constants.POS_ANS)
                    TransactionCompleted = true;
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
            }
            return TransactionCompleted;
        }

        public static Dictionary<string, string> LoadBackupFolder(string folderID)
        {
            Dictionary<string, string> _temp = null;
            try
            {
                Int32 answer;
                _temp = new Dictionary<string, string>();
                _writer.Write(Constants.RESTOREDIR);
                _writer.Write(folderID);
                while ((answer = _reader.ReadInt32()) == Constants.INIT)
                    InitServer();
                for (Int32 i = 0; i < answer; i++)
                {
                    string _path = _reader.ReadString();
                    string _checksum = _reader.ReadString();
                    _temp.Add(_path, _checksum);
                }
                Restoring._worker.ReportProgress(1, "LoadBackupFolder done!");
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
                // catch eccezioni initServer???
            }
            return _temp;
        }

        public static void RestoreFile(string fullpath, string tempfullpath)
        {
            Int64 filesize = _reader.ReadInt64();
            using (FileStream _fs = File.OpenWrite(tempfullpath))
            {
                int _buffersize;
                while (filesize > 0)
                {
                    _buffersize = Constants.BUFFSIZE;
                    if (filesize < Constants.BUFFSIZE) _buffersize = (int)filesize;
                    byte[] _buffer = new byte[_buffersize];
                    int size = _reader.BaseStream.Read(_buffer, 0, _buffersize);
                    _fs.Write(_buffer, 0, size);
                    filesize -= size;
                }
            }
        }
    }
}
