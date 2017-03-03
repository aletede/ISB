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
    static class Restoring
    {
        public static BackgroundWorker _worker = new BackgroundWorker();
        private static SHA1Managed sha = new SHA1Managed();

        public static void Start(object s, DoWorkEventArgs args)
        {
            Dictionary<string, string> _matching = new Dictionary<string, string>();
            try
            {
                while (!Monitoring.pause)
                    System.Threading.Thread.Sleep(500);
                _worker.ReportProgress(1, "Monitoring is paused. Restoring is starting.");

                string _folderID = (string)args.Argument;

                Dictionary<string, string> _temp = Client.LoadBackupFolder(_folderID);
                List<string> downloadList = null;
                List<string> deleteList = null;
                List<string> listToCheck = null;
                if (Directory.Exists(Client._dirPath))
                {
                    string[] files = Directory.GetFiles(Client._dirPath, "*.*", SearchOption.AllDirectories);
                    downloadList = _temp.Keys.Except(files).ToList();  // scarica i file che si trovano sul server ma non sul client
                    deleteList = files.Except(_temp.Keys).ToList(); // rimuovi i file che si trovano sul client ma non sul server
                    listToCheck = files.Intersect(_temp.Keys).ToList();    // file da verificare se da scaricare o lasciare l'originale
                    foreach (string fullpath in listToCheck)
                    {
                        if (File.Exists(fullpath))
                        {
                            using (FileStream fs = File.OpenRead(fullpath))
                            {
                                string currentChecksum = Convert.ToBase64String(sha.ComputeHash(fs));
                                if (currentChecksum != _temp[fullpath])
                                    downloadList.Add(fullpath);
                            }
                        }
                        else downloadList.Add(fullpath);
                    }
                }
                else downloadList = new List<string>(_temp.Keys);

                foreach (string fullpath in downloadList)
                {
                    string tempfullpath = Path.GetTempFileName();
                    _matching.Add(fullpath, tempfullpath);
                    Client.RestoreFile(fullpath, tempfullpath);
                }
                // download files eseguito con successo
                if (deleteList != null)
                    foreach (string fullpath in deleteList)
                        File.Delete(fullpath);
                foreach (KeyValuePair<string, string> match in _matching)
                {
                    if (File.Exists(match.Key)) File.Delete(match.Key);
                    File.Move(match.Value, match.Key);
                }
                // ripristino cartella eseguito con successo
                Client._remoteStatus = _temp;
                Client.LoadBackupData();
                args.Result = true;
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
                foreach (string tempfile in _matching.Values)
                    if (File.Exists(tempfile)) File.Delete(tempfile);
                _worker.ReportProgress(1, "Restoring process failed!");
                _worker.ReportProgress(1, err.Message);
                args.Result = false;
            }
        }
    }
}
