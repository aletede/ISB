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
            pathTextBox.Text = Properties.Settings.Default.newDir;  
            addrTextBox.Text = Properties.Settings.Default.newIP;
            portTextBox.Text = Properties.Settings.Default.newPort.ToString();
            freqTextBox.Text = Properties.Settings.Default.newFreq.ToString();
            pathTextBox.Tag = pathTextBox.Text;
            addrTextBox.Tag = addrTextBox.Text;
            portTextBox.Tag = portTextBox.Text;
            freqTextBox.Tag = freqTextBox.Text;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            UInt16 port;
            UInt32 freq;
            bool textChanged = false;

            if (String.IsNullOrEmpty(pathTextBox.Text) || !Directory.Exists(pathTextBox.Text)) {
                pathTextBox.BorderBrush = Brushes.Red;
                return;
            }
            else if (pathTextBox.Text != (string)pathTextBox.Tag) {
                textChanged = true;
                Properties.Settings.Default.newDir = pathTextBox.Text;
            }

            if (String.IsNullOrEmpty(addrTextBox.Text) || !isValidIPv4(addrTextBox.Text)) {
                addrTextBox.BorderBrush = Brushes.Red;
                return;
            }
            else if (addrTextBox.Text != (string)addrTextBox.Tag) {
                textChanged = true;
                Properties.Settings.Default.newIP = addrTextBox.Text;
            }

            if (String.IsNullOrEmpty(portTextBox.Text) || !UInt16.TryParse(portTextBox.Text, out port)) {
                portTextBox.BorderBrush = Brushes.Red;
                return;
            }
            else if (portTextBox.Text != (string)portTextBox.Tag) {
                textChanged = true;
                Properties.Settings.Default.newPort = port;
            }

            if (String.IsNullOrEmpty(freqTextBox.Text) || !UInt32.TryParse(freqTextBox.Text, out freq)) {
                freqTextBox.BorderBrush = Brushes.Red;
                return;
            }
            else if (freqTextBox.Text != (string)freqTextBox.Tag){
                textChanged = true;
                Properties.Settings.Default.newFreq = freq;
            }

            if (textChanged) this.DialogResult = true;   // possible execption (System.InvalidOperationException)
            else this.DialogResult = false; // possible execption (System.InvalidOperationException)
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
