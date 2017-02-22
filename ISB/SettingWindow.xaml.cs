using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ISB
{
    /// <summary>
    /// Logica di interazione per SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            LoadSettingValues();
        }

        private void LoadSettingValues()
        {
            pathTextBox.Text = Properties.Settings.Default.directory;
            addrTextBox.Text = Properties.Settings.Default.serverIP;
            portTextBox.Text = Properties.Settings.Default.serverPort.ToString();
            freqTextBox.Text = Properties.Settings.Default.frequency.ToString();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            UInt16 port;
            UInt32 freq;
            if (String.IsNullOrEmpty(pathTextBox.Text) || !Directory.Exists(pathTextBox.Text))
                pathTextBox.BorderBrush = Brushes.Red;
            else if(String.IsNullOrEmpty(addrTextBox.Text) || !isValidIPv4(addrTextBox.Text))
                addrTextBox.BorderBrush = Brushes.Red;
            else if (String.IsNullOrEmpty(portTextBox.Text) || !UInt16.TryParse(portTextBox.Text, out port))
                portTextBox.BorderBrush = Brushes.Red;
            else if (String.IsNullOrEmpty(freqTextBox.Text) || !UInt32.TryParse(freqTextBox.Text, out freq))
                portTextBox.BorderBrush = Brushes.Red;
            else
            {
                // rivedere questa parte e il caso in cui non sono state eseguite modifiche
                Properties.Settings.Default.directory = pathTextBox.Text;
                Properties.Settings.Default.serverIP = addrTextBox.Text;
                Properties.Settings.Default.serverPort = port;
                Properties.Settings.Default.frequency = freq;
                this.DialogResult = true;   // possible execption (System.InvalidOperationException)
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult res = fbd.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
                pathTextBox.Text = fbd.SelectedPath;
        }

        private bool isValidIPv4(String ip)
        {
            if (ip.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Length == 4)
            {
                IPAddress ipAddr;
                if (IPAddress.TryParse(ip, out ipAddr)) return true;
                else return false;
            }
            else return false;
        }
    }
}
