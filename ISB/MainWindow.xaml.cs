﻿using System;
using System.Collections.Generic;
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
        private List<LocalEntry> remoteEntries = null;
        //private BackgroundWorker _worker = new BackgroundWorker();

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
        }

        private void StartApplication(object sender, RoutedEventArgs e)
        {
            pathTextBox.Text = Properties.Settings.Default.directory;   // Directory path to be monitored

            // Local Data Grid initialization
            localDir.Tag = pathTextBox.Text;    // Current directory displayed into Local Data Grid
            LoadLocalData(pathTextBox.Text);    // Load entries into Data Grid

            // initializing Remote Data Grid and starting monitoring
            Client._worker.WorkerReportsProgress = true;
            Client._worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                Client.Connect();
                while (true)
                {
                    Client.LoadRemoteData((string)args.Argument);
                    System.Threading.Thread.Sleep(500);
                }
                //Monitoring.Start();
            };
            Client._worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                switch (args.ProgressPercentage)
                {
                    case 1:
                        // Connect
                        eventLogConsole.AppendText(new LogMessage((string)args.UserState, DateTime.Now).ToString());
                        eventLogConsole.ScrollToEnd();
                        break;
                    case 2:
                        // LoadRemoteData
                        if (args.UserState == null)
                            eventLogConsole.AppendText(new LogMessage("Exception raised on LoadRemoteData", DateTime.Now).ToString());
                        else
                            eventLogConsole.AppendText(new LogMessage((string)args.UserState, DateTime.Now).ToString());
                        eventLogConsole.ScrollToEnd();
                        break;
                    default:
                        break;
                }
            };
            Client._worker.RunWorkerAsync(pathTextBox.Text);
        }   

        public void LoadLocalData(string localDirPath)
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
                localDir.ItemsSource = localEntries;    // Update source data for Local Data Grid
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
    }
}
