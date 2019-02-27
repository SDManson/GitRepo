using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClipTime
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
           
            Clipboard.SetText(System.DateTime.Now.ToString("HHmm"));
        }
    }
}
