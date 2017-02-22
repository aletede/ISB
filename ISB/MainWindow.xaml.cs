using System;
using System.Collections.Generic;
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

namespace ISB
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            this.Loaded += new RoutedEventHandler(LoadInitValueComponent);
        }

        List<DirectoryEntry> localEntries = new List<DirectoryEntry>();

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWin = new SettingWindow();
            bool? res = settingWin.ShowDialog();    // possible exception InvalidOperationException
            if (res.HasValue)
                if ((bool)res)
                    MessageBox.Show("I nuovi parametri saranno applicati al riavvio dell'applicazione.");
        }

        private void LoadInitValueComponent(object sender, RoutedEventArgs e)
        {
            // eccezioni non gestite, dimensione in bytes
            pathTextBox.Text = Properties.Settings.Default.directory;
            try
            {
                foreach (string d in Directory.GetDirectories(pathTextBox.Text))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    DirectoryEntry de = new DirectoryEntry(di.Name, di.FullName, null, di.LastWriteTime, EntryType.Cartella, new Uri("pack://application:,,,/images/Folder.png"));
                    localEntries.Add(de);

                }
                foreach (string f in Directory.GetFiles(pathTextBox.Text))
                {
                    FileInfo fi = new FileInfo(f);
                    DirectoryEntry de = new DirectoryEntry(fi.Name, fi.FullName, fi.Length.ToString(), fi.LastWriteTime, EntryType.File, new Uri("pack://application:,,,/images/File.png"));
                    localEntries.Add(de);
                }
                localDir.ItemsSource = localEntries;
            }
            catch (Exception err)
            {
                // write in a log file
            }
        }

        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // ottimizzare il codice, pezzi di codici uguali a LoadInitvalueComponent
            DataGridRow row = sender as DataGridRow;
            DirectoryEntry entry = row.DataContext as DirectoryEntry;
            if (entry.Type == EntryType.Cartella)
            {
                localEntries.Clear();
                try
                {
                    if (entry.Fullpath != pathTextBox.Text)
                    {
                        DirectoryInfo di = new DirectoryInfo(entry.Fullpath);
                        DirectoryEntry de = new DirectoryEntry("...", di.Parent.FullName, null, null, EntryType.Cartella, new Uri("pack://application:,,,/images/OpenFolder.png"));
                        localEntries.Add(de);
                    }

                    foreach (string d in Directory.GetDirectories(entry.Fullpath))
                    {
                        DirectoryInfo di = new DirectoryInfo(d);
                        DirectoryEntry de = new DirectoryEntry(di.Name, di.FullName, null, di.LastWriteTime, EntryType.Cartella, new Uri("pack://application:,,,/images/Folder.png"));
                        localEntries.Add(de);

                    }
                    foreach (string f in Directory.GetFiles(entry.Fullpath))
                    {
                        FileInfo fi = new FileInfo(f);
                        DirectoryEntry de = new DirectoryEntry(fi.Name, fi.FullName, fi.Length.ToString(), fi.LastWriteTime, EntryType.File, new Uri("pack://application:,,,/images/File.png"));
                        localEntries.Add(de);
                    }
                    localDir.ItemsSource = null;
                    localDir.ItemsSource = localEntries;
                    
                }
                catch (Exception err)
                {
                    // write in a log file
                }
            }
        }
    }
}
