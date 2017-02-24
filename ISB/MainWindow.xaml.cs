using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
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
        private ObservableCollection<DirectoryEntry> localEntries = new ObservableCollection<DirectoryEntry>();

        public MainWindow()
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.directory))
            {
                DirWindow dirWin = new DirWindow();
                bool? res = dirWin.ShowDialog();
                if (res.HasValue)
                    if ((bool)res)
                        InitializeComponent();
                    else
                        Application.Current.Shutdown();
                else
                    Application.Current.Shutdown();
            }
            else InitializeComponent();
            this.Loaded += new RoutedEventHandler(LoadInitValueComponent); // approfondire concetto delegato loaded (IMPORTANTE!!!!!!!!)
        }

        private void LoadNewSettings()
        {
            Properties.Settings.Default.directory = Properties.Settings.Default.newDir;
            Properties.Settings.Default.serverIP = Properties.Settings.Default.newIP;
            Properties.Settings.Default.serverPort = Properties.Settings.Default.newPort;
            Properties.Settings.Default.frequency = Properties.Settings.Default.newFreq;
        }

        private void LoadInitValueComponent(object sender, RoutedEventArgs e)
        {
            LoadNewSettings();
            pathTextBox.Text = Properties.Settings.Default.directory;
            localDir.ItemsSource = localEntries;    // ObservableCollection
            LoadLocalDataGrid(pathTextBox.Text);

            // BackgroundWorker
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                for (int x = 1; x <= 10; x++)
                {
                    System.Threading.Thread.Sleep(1000);
                    worker.ReportProgress(0, new LogMessage("test "+x.ToString()+" log console from backgroundWorker", DateTime.Now));
                }
            };
            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                LogMessage lm2 = args.UserState as LogMessage;
                eventLogConsole.AppendText(lm2.ToString());
                eventLogConsole.ScrollToEnd();
            };
            worker.RunWorkerAsync();

            // test logmessage e event log console
            LogMessage lm = new LogMessage("prova logMessage class", DateTime.Now);
            eventLogConsole.AppendText(lm.ToString());
            lm.Message = "prova2 logMessage class";
            lm.Time = DateTime.Now;
            eventLogConsole.AppendText(lm.ToString());
            eventLogConsole.ScrollToEnd();
        }

        public void LoadLocalDataGrid(string localDirPath)
        {
            // eccezioni non gestite, dimensione in bytes
            try
            {
                localEntries.Clear();    // se un eccezione viene lanciata dopo che le entries sono state cancellate, la UI sarà vuota
                if (localDirPath != pathTextBox.Text)
                {
                    DirectoryInfo di = new DirectoryInfo(localDirPath);
                    DirectoryEntry de = new DirectoryEntry("...", di.Parent.FullName, null, null, EntryType.Cartella, new Uri("pack://application:,,,/images/OpenFolder.png"));
                    localEntries.Add(de);
                }

                foreach (string d in Directory.GetDirectories(localDirPath))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    DirectoryEntry de = new DirectoryEntry(di.Name, di.FullName, null, di.LastWriteTime.ToString(), EntryType.Cartella, new Uri("pack://application:,,,/images/Folder.png"));
                    localEntries.Add(de);

                }
                foreach (string f in Directory.GetFiles(localDirPath))
                {
                    FileInfo fi = new FileInfo(f);
                    DirectoryEntry de = new DirectoryEntry(fi.Name, fi.FullName, fi.Length.ToString(), fi.LastWriteTime.ToString(), EntryType.File, new Uri("pack://application:,,,/images/File.png"));
                    localEntries.Add(de);
                }
                localDir.Tag = localDirPath;
            }
            catch (Exception err)
            {
                // do something and write in a log file
            }
        }

        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            DirectoryEntry entry = row.DataContext as DirectoryEntry;
            if (entry.Type == EntryType.Cartella)
                if (Directory.Exists(entry.Fullpath))
                    LoadLocalDataGrid(entry.Fullpath);
                else
                {
                    LoadLocalDataGrid(localDir.Tag.ToString()); // ricarica la UI nel caso in cui una cartella non esiste più
                    eventLogConsole.AppendText(new LogMessage("La cartella " + entry.Name + " non esiste.", DateTime.Now).ToString());
                    eventLogConsole.ScrollToEnd();
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
    }
}
