using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ISB
{
    static class Monitoring
    {
        private static SHA1Managed sha = new SHA1Managed();
        public static bool pause = false;

        public static void Start(string localDirPath)
        {
            while (Client._remoteStatus == null)
                Client.LoadRemoteFiles();
            int interval = (int)Properties.Settings.Default.frequency;
            do
            {
                try
                {
                    if (Directory.Exists(localDirPath))
                    {
                        string[] files = Directory.GetFiles(localDirPath, "*.*", SearchOption.AllDirectories);
                        var removeList = Client._remoteStatus.Keys.Except(files);
                        if (removeList.Any())   // remove file list
                            foreach (string fullpath in removeList)
                                if (Client.SyncDelete(fullpath))    // da implementare
                                {
                                    Client._remoteStatus.Remove(fullpath);
                                    Client._worker.ReportProgress(3, fullpath + " removed from the server!");
                                    Client.LoadBackupData();
                                }
                        Dictionary<string, string> _currentStatus = new Dictionary<string, string>();  // da rivedere struttura
                        foreach (string fullpath in files)
                        {
                            // controllare se cancelpending è settata 
                            try
                            {
                                FileInfo fi = new FileInfo(fullpath);
                                if (fi.Exists)
                                {
                                    bool TransactionCompleted = false;
                                    string remoteChecksum, currentChecksum;
                                    using (FileStream fs = fi.OpenRead())
                                    {
                                        currentChecksum = computeChecksum(fs);
                                        fs.Seek(0, SeekOrigin.Begin);
                                        if (Client._remoteStatus.TryGetValue(fi.FullName, out remoteChecksum))
                                        {
                                            if (!currentChecksum.Equals(remoteChecksum, StringComparison.Ordinal)) 
                                                TransactionCompleted = Client.SyncFile(fs, fi.FullName, currentChecksum); 
                                            else
                                                _currentStatus[fi.FullName] = currentChecksum;
                                        }
                                        else // sync as new local file
                                            TransactionCompleted = Client.SyncFile(fs, fi.FullName, currentChecksum);
                                    }
                                    if (TransactionCompleted)
                                    {
                                        _currentStatus[fi.FullName] = currentChecksum;
                                        Client._worker.ReportProgress(1, fullpath + " uploaded to the server!");
                                        Client.LoadBackupData();
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                // rivedere gestione eccezioni e log file
                                Client._worker.ReportProgress(3, err.Message);
                            }
                        }
                        Client._remoteStatus = _currentStatus;
                    }
                    pause = true;
                    System.Threading.Thread.Sleep(interval);    // controllare se cancelpending è settata
                    Client._mre.WaitOne();
                    pause = false;
                }
                catch (Exception err)
                {
                    // rivedere gestione eccezioni e log file
                    Client._worker.ReportProgress(3, err.Message);
                }

            }
            while (true);   // da rivedere condizione di terminazione IMPORTANTE!!!!!!!!    // controllare se cancelpending è settata
        }

        private static string computeChecksum(FileStream fs)
        {
            byte[] checksum = sha.ComputeHash(fs);
            return Convert.ToBase64String(checksum);
        }
    }
}
