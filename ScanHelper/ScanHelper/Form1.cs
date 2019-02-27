using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;
using Sanderling;
using Sanderling.Script.Impl;
using System.Threading;
using LINQPad;
using System.Data.Sql;
using System.Data.SqlClient;
using BotSharp;
using System.IO;
using System.Reflection; 
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;
using Clipboard = System.Windows.Forms.Clipboard;
using TextDataFormat = System.Windows.Forms.TextDataFormat;


namespace ScanHelper
{
   
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        public string script = @"Process, Exist, Exefile.exe
                                PID:=ErrorLevel
                                WinActivate ahk_pid %PID%
                                MouseClick, left,1598,106
                                Sleep,500
                                MouseClick, left,594,565
                                Sleep,700
                                Send, ^a
                                Sleep,700
                                Send, ^c
                                Sleep,700
                                Send, ^w";
        int? eveClientId = null;
        //string CntrlC = @"^c";
        //string CntrlA = @"^a";
        //string CntrlW = @"^w";    





        public Form1()
        {
            InitializeComponent();

            //Sanderling.ABot.Bot.Bot bot = new Sanderling.ABot.Bot.Bot();
            //bot.Dump();


            //this.textBox1.Text=TestQuery("B735");
            // return;
            eveClientId = Extension.GetEveOnlineClientProcessId();
            ProcID.Text = eveClientId.ToString();
           // Form1_Load(null,null);
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            var sensor = new Sensor();

            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(Extension.GetEveMainWindow().Value);
            for (; ; )
            {
                var response = sensor?.MeasurementTakeNewRequest(eveClientId.Value);
                if (null == response)
                {
                    continue;
                }
                else
                {
                    MeasurementReceived(response?.MemoryMeasurement);
                    sensor = null;
                    break;
                }
            }
        }
        

        

        public string TestQuery(string toSearch="H296")
        {
            var test = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Wormholes.txt");
            return new StreamReader(test).ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToDictionary(v => v.Split(' ')[0], vv => vv.Split(' ')[1])
                .Where(p => p.Key.Contains(toSearch)).Select(p => p.Value).First();
            //return ss.Where(p => p.Key.Contains(toSearch)).Select(p => p.Value).First();
        }
        public void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
           
            var ListUIElement =
                measurement?.Value?.EnumerateReferencedUIElementTransitive()
                ?.GroupBy(uiElement => uiElement.Id)
                ?.Select(group => group?.FirstOrDefault())
                ?.ToArray();

            //Classify("K346");
            //return;
            var overview = measurement.Value.WindowOverview.First();
            var entry = overview.ListView.Entry.Where(x => x.LabelText.ElementAt(2).Text.Contains("Wormhole"));
            var work = entry.First().LabelText;
            string ToSearch = entry.First().LabelText.ElementAt(2).Text.Split(' ')[1];
            string Hole = string.Empty;
            Hole = (ToSearch.Contains("K162")) ? "UNK" : WormLib.Worm.GetHole(ToSearch);
            var ScanResult = measurement.Value.WindowProbeScanner.First().ScanResultView.Entry.First();
            string ID = ScanResult?.LabelText.ElementAt(1).Text.Substring(0, 3);
            string ClipString = "{0} {1}";

            ClipString = string.Format(ClipString, ID, Hole);
            RunAsStaThread(() => { Clipboard.SetText(ClipString);});
                      
            if (Hole.Equals("UNK"))
            {
                textBox1.Text = Classify(ClipString);
                RunAsStaThread(() => { Clipboard.SetText(textBox1.Text); });
            }
            else
            textBox1.Text = ClipString;
            //{
            //var id = Process.GetProcesses()
            //?.FirstOrDefault(pr => pr.MainWindowTitle.Contains("Sanderling")).Id;
            //SetForegroundWindow(Process.GetProcessById(id.Value).MainWindowHandle);
            //SendKeys.SendWait("{F5}");
            //while (ClipString.Equals(Clipboard.GetText())) { Thread.Sleep(100); }
            //ClipboardString.Add(Clipboard.GetText());

            //this.textBox1.Lines = ClipboardString.ToArray();
            //    this.textBox1.Text = ClipString;
            //    string Rgx = ClipboardString.First();
            //    string sss = string.Empty;


            //    if (System.Text.RegularExpressions.Regex.IsMatch(ClipboardString.Last().ToString(), @"medium"))
            //        sss = Rgx.Replace("UNK", "C1");

            //    if (System.Text.RegularExpressions.Regex.IsMatch(ClipboardString.Last().ToString(), @"into unknown") &&
            //        !sss.Contains("C1"))
            //        sss = Rgx.Replace("UNK", "C23");

            //    if (System.Text.RegularExpressions.Regex.IsMatch(ClipboardString.Last().ToString(), @"D|dangerous"))
            //        sss = Rgx.Replace("UNK", "C45");

            //    if (System.Text.RegularExpressions.Regex.IsMatch(ClipboardString.Last().ToString(), @"D|deadly"))
            //        sss = Rgx.Replace("UNK", "C6");

            //    //if (System.Text.RegularExpressions.Regex.IsMatch(ID, @"[A-Z]00\d") ||
            //       if(System.Text.RegularExpressions.Regex.IsMatch(ClipboardString.Last().ToString(), @"O|only the smallest"))
            //        sss += ("F");

            //    if (System.Text.RegularExpressions.Regex.IsMatch(ClipboardString.Last().ToString(), @"reaching the end"))
            //        sss = Rgx += " eol";
            //    if (string.IsNullOrEmpty(sss)) sss = ClipString;
            //    RunAsSTAThread(() => { Clipboard.SetText(sss); });
            //    textBox1.Text = sss;
            //}

            //return;
        }


        public string Classify(string start)
        {
            string tempstring = string.Empty;
            var id = Process.GetProcesses()
                ?.FirstOrDefault(pr => pr.MainWindowTitle.Contains(@"v2018")).Id;
            SetForegroundWindow(Process.GetProcessById(id.Value).MainWindowHandle);
            InputSimulator sim = new InputSimulator();
            sim.Keyboard.KeyPress(new VirtualKeyCode[] {VirtualKeyCode.F5});
            //SendKeys.SendWait("{F5}");
            //IntPtr hWnd= Process.GetCurrentProcess().MainWindowHandle;
            //System.Windows.Window wnd = new System.Windows.Window();
           
            //ClipboardMonitor monitor = new ClipboardMonitor();



            while (start.Equals(Clipboard.GetText())) { Thread.Sleep(100); }
            tempstring = Clipboard.GetText();

            if (Replace(ref start, tempstring, out var replace)) return replace;
          
            return replace;
        }

        private static bool Replace(ref string start, string tempstring, out string replace)
        {
            if (Regex.IsMatch(tempstring, @"O|only the smallest"))
            {
                replace=start + "F";
            }

            if (Regex.IsMatch(tempstring, @"high"))
            {
                replace = start.Replace("UNK", "HS");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"low"))
            {
                replace = start.Replace("UNK", "LS");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"null"))
            {
                replace = start.Replace("UNK", "NS");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"medium"))
            {
                replace = start.Replace("UNK", "C1");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"into unknown"))
            {
                replace = start.Replace("UNK", "C23");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"D|dangerous"))
            {
                replace = start.Replace("UNK", "C45");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"D|deadly"))
            {
                replace = start.Replace("UNK", "C6");
                return true;
            }

            if (Regex.IsMatch(tempstring, @"reaching the end"))
                replace = start + " eol";

            replace = start;
            return false;
        }


        static void RunAsStaThread(Action goForIt)
        {
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    goForIt();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }

        

        private void Form1_DoubleClick(object sender, MouseEventArgs e)
        {
            Form1_Load(sender, null);
        }

    }
   
}
static public class Extension
{
    static public int? GetEveOnlineClientProcessId() =>
        System.Diagnostics.Process.GetProcesses()
        ?.FirstOrDefault(process =>
        {
            try
            {
                return string.Equals("ExeFile.exe", process?.MainModule?.ModuleName, StringComparison.InvariantCultureIgnoreCase);
            }
            catch { }

            return false;
        })
        ?.Id;

    static public IntPtr? GetSanderling() { return (IntPtr?) Process.GetProcesses()?.FirstOrDefault(pr => pr.MainWindowTitle.Contains("Sanderling")).Id; }

    static public IntPtr? GetEveMainWindow() =>
       System.Diagnostics.Process.GetProcesses()
        ?.FirstOrDefault(process =>
        {
            try
            {
                return string.Equals("ExeFile.exe", process?.MainModule?.ModuleName, StringComparison.InvariantCultureIgnoreCase);
            }
            catch { }

            return false;
        })
        ?.MainWindowHandle;

    public static IEnumerable<Group> AsEnumerable(this GroupCollection gc)
    {
        foreach (Group g in gc) yield return g;
    }
    public static IEnumerable<Match> AsEnumeable(this MatchCollection mc)
    {
        foreach (Match m in mc) yield return m;
    }

   

    public static string GetClipBoardData()
    {
        try
        {
            string ClipData = null;
            Exception e = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        ClipData = Clipboard.GetText(TextDataFormat.Text);
                    }
                    catch (Exception ex)
                    { e = ex; }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return ClipData;
        }
        catch (Exception )
        {
            return string.Empty;
        }
    }
}
