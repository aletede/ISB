using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Logica di interazione per DirWindow.xaml
    /// </summary>
    public partial class DirWindow : Window
    {
        public DirWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string dirPath = pathTextBox.Text;
            if (String.IsNullOrEmpty(dirPath))
                pathTextBox.BorderBrush = Brushes.Red;
            else
            {
                if (Directory.Exists(dirPath))
                {
                    Properties.Settings.Default.newDir = dirPath;
                    Properties.Settings.Default.Save();
                    this.DialogResult = true;   // possible execption (System.InvalidOperationException)
                }
                else
                    pathTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult res = fbd.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
                pathTextBox.Text = fbd.SelectedPath;
        }
    }
}
