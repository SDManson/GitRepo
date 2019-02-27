using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WpfClipboardMonitor;

namespace WPFScanHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ClipboardMonitor monitor = new ClipboardMonitor(this);
            monitor.ClipboardUpdate += Monitor_ClipboardUpdate;
            monitor.Start();
        }

        private void Monitor_ClipboardUpdate(object sender, EventArgs e)
        {
            Debug.WriteLine(sender.GetType());
            MessageBox.Show("Clipboard has changed");
            string ss = Clipboard.GetText();
        }

        public void MonitorClipboard()
        {

        }
    }
}
