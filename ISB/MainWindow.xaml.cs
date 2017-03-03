using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ISB
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<LocalEntry> localEntries = null; 

        public MainWindow()
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.directory))
            {
                DirWindow dirWin = new DirWindow();
                bool? res = dirWin.ShowDialog();
                if (res.HasValue)
                    if ((bool)res)
                    {
                        LoadNewSettings();
                        InitializeComponent();
                    }
                    else
                        Application.Current.Shutdown();
                else
                    Application.Current.Shutdown();
            }
            else
            {
                LoadNewSettings();
                InitializeComponent();
            }
            this.Loaded += new RoutedEventHandler(StartApplication);
        }
        
        private void LoadNewSettings()
        {
            Properties.Settings.Default.directory = Properties.Settings.Default.newDir;
            Properties.Settings.Default.serverIP = Properties.Settings.Default.newIP;
            Properties.Settings.Default.serverPort = Properties.Settings.Default.newPort;
            Properties.Settings.Default.frequency = Properties.Settings.Default.newFreq;
            Properties.Settings.Default.Save();
        }

        private void StartApplication(object sender, RoutedEventArgs e)
        {
            pathTextBox.Text = Properties.Settings.Default.directory;   // Directory path to be monitored

            // Local Data Grid initialization
            localDir.Tag = pathTextBox.Text;    // Current directory displayed into Local Data Grid
            LoadLocalData(pathTextBox.Text);    // Load entries into Data Grid

            // initializing restoring 
            Restoring._worker.WorkerReportsProgress = true;
            Restoring._worker.DoWork += new DoWorkEventHandler(Restoring.Start);
            Restoring._worker.ProgressChanged += new ProgressChangedEventHandler(ProgressResults);
            Restoring._worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                // aggiorna remote data e fai qualunque altra cosa e poi chiama _mre.set()
                string result = ((bool)args.Result) ? "Restoring end with success. Monitoring is resumed." : "Restoring failed. Monitoring is resumed.";
                eventLogConsole.AppendText(new LogMessage(result, DateTime.Now).ToString());  // modificare success/fail in base al valore di ritorno
                eventLogConsole.ScrollToEnd();
                Client._mre.Set();
            };

            // initializing Remote Data Grid and starting monitoring
            Client._worker.WorkerReportsProgress = true;
            Client._worker.DoWork += new DoWorkEventHandler(Client.Init);
            Client._worker.ProgressChanged += new ProgressChangedEventHandler(ProgressResults);
            Client._worker.RunWorkerAsync(pathTextBox.Text);
            Client._mre.Set();
        }

        private void ProgressResults(object s, ProgressChangedEventArgs args)
        {
            // da rivedere
            switch (args.ProgressPercentage)
            {
                case 1:
                    // Connect (print string)
                    eventLogConsole.AppendText(new LogMessage((string)args.UserState, DateTime.Now).ToString());
                    eventLogConsole.ScrollToEnd();
                    break;
                case 2:
                    // LoadBackupData
                    if (!(bool)args.UserState)
                        eventLogConsole.AppendText(new LogMessage("Exception raised on LoadBackupData", DateTime.Now).ToString());
                    else
                    {
                        remoteDir.ItemsSource = Client._remoteEntries;
                        eventLogConsole.AppendText(new LogMessage("LoadBackupData done", DateTime.Now).ToString());
                    }
                    eventLogConsole.ScrollToEnd();
                    break;
                case 3:
                    // file removed/uploaded 
                    eventLogConsole.AppendText(new LogMessage((string)args.UserState, DateTime.Now).ToString());
                    eventLogConsole.ScrollToEnd();
                    break;
                default:
                    break;
            }
        }

        private void LoadLocalData(string localDirPath)
        {
            try
            {
                List<LocalEntry> UpdateLocalEntries = new List<LocalEntry>();                
                if (localDirPath != pathTextBox.Text)
                {
                    DirectoryInfo di = new DirectoryInfo(localDirPath);
                    LocalEntry de = new LocalEntry("...", di.Parent.FullName, null, null, EntryType.Cartella, new Uri("pack://application:,,,/images/OpenFolder.png"));
                    UpdateLocalEntries.Add(de);
                }

                foreach (string d in Directory.GetDirectories(localDirPath))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    LocalEntry de = new LocalEntry(di.Name, di.FullName, null, di.LastWriteTime.ToString(), EntryType.Cartella, new Uri("pack://application:,,,/images/Folder.png"));
                    UpdateLocalEntries.Add(de);

                }
                foreach (string f in Directory.GetFiles(localDirPath))
                {
                    FileInfo fi = new FileInfo(f);
                    LocalEntry de = new LocalEntry(fi.Name, fi.FullName, (fi.Length >> 10).ToString(), fi.LastWriteTime.ToString(), EntryType.File, new Uri("pack://application:,,,/images/File.png"));
                    UpdateLocalEntries.Add(de);
                }
                localDir.Tag = localDirPath;    // Update current directory of Local Data Grid
                localEntries = UpdateLocalEntries;
                localDir.ItemsSource = localEntries;
            }
            catch (Exception err)
            {
                // rivedere gestione eccezioni e log file
                eventLogConsole.AppendText(new LogMessage("Source: " + err.Source + " Msg: " + err.Message, DateTime.Now).ToString());
                eventLogConsole.ScrollToEnd();
            }
        }

        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            LocalEntry entry = row.DataContext as LocalEntry;
            if (entry.Type == EntryType.Cartella)
                if (Directory.Exists(entry.Fullpath))
                    LoadLocalData(entry.Fullpath);
                else
                {
                    MessageBox.Show("Impossibile accedere alla cartella selezionata.");
                    LoadLocalData(localDir.Tag.ToString()); // ricarica la UI nel caso in cui una cartella non esiste più
                }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWin = new SettingWindow();
            bool? res = settingWin.ShowDialog();    // possible exception InvalidOperationException
            if (res.HasValue)
                if ((bool)res)
                    MessageBox.Show("I nuovi parametri saranno applicati al riavvio dell'applicazione.");
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (!Restoring._worker.IsBusy)
            {
                Client._mre.Reset();
                Restoring._worker.RunWorkerAsync(restoreButton.Tag.ToString());
            }
            restoreButton.IsEnabled = false;
        }

        private void Row_Selected(object sender, RoutedEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            RemoteEntry entry = row.DataContext as RemoteEntry;
            restoreButton.Tag = entry.ID;
            restoreButton.IsEnabled = true;
        }

        private void remoteDir_LostFocus(object sender, RoutedEventArgs e)
        {
            remoteDir.SelectedItem = null;
        }

        private void localDir_LostFocus(object sender, RoutedEventArgs e)
        {
            localDir.SelectedItem = null;
        }
    }
}
