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
        private Monitoring bgWorker;

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
            this.Loaded += new RoutedEventHandler(runBackgroundWorker);
            this.Loaded += new RoutedEventHandler(LoadInitValue);
        }

        private void runBackgroundWorker(object sender, RoutedEventArgs e)
        {
            bgWorker = new Monitoring(this);
            bgWorker.WorkerReportsProgress = true;
            bgWorker.start();
        }

        private void LoadInitValue(object sender, RoutedEventArgs e)
        {
            pathTextBox.Text = Properties.Settings.Default.directory;
            localDir.Tag = pathTextBox.Text;
            localDir.ItemsSource = localEntries;
            LoadLocalDataGrid(pathTextBox.Text);
        }

        private void LoadNewSettings()
        {
            Properties.Settings.Default.directory = Properties.Settings.Default.newDir;
            Properties.Settings.Default.serverIP = Properties.Settings.Default.newIP;
            Properties.Settings.Default.serverPort = Properties.Settings.Default.newPort;
            Properties.Settings.Default.frequency = Properties.Settings.Default.newFreq;
        }

        public void LoadLocalDataGrid(string localDirPath)
        {
            // eccezioni non gestite benissimo (da rivedere), dimensione in bytes
            int count = 2;
            try
            {
                localEntries.Clear();
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
                // write error in a log file
                eventLogConsole.AppendText(new LogMessage("Source: " + err.Source + " Msg: " + err.Message, DateTime.Now).ToString());
                eventLogConsole.ScrollToEnd();
                if (count != 0)
                {
                    count--;
                    LoadLocalDataGrid(localDir.Tag.ToString());
                }
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
                    MessageBox.Show("Impossibile accedere alla cartella selezionata.");
                    LoadLocalDataGrid(localDir.Tag.ToString()); // ricarica la UI nel caso in cui una cartella non esiste più
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
