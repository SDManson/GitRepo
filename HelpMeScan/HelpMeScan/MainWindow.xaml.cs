using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Media;
using System.Runtime.CompilerServices;
using Sanderling;
using Sanderling.Motor;
using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using WindowsInput;
using WindowsInput.Native;
using WormLib;
using System.Text.RegularExpressions;
using System.Timers;
using Bib3.Geometrik;
using BotEngine.Client;
using BotEngine.Common;
using BotEngine.Common.Motor;
using Window = System.Windows.Window;
using LINQPad;
using MouseButton = System.Windows.Input.MouseButton;
using Clipboard = System.Windows.Forms.Clipboard;


namespace HelpMeScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        private IntPtr? _eveMainWindow;
        private int? _eveClientId;

        public IntPtr? EveMainWindow
        {
            get => _eveMainWindow;
            set => _eveMainWindow = value; 
        }

        public int? EveClientId
        {
            get => _eveClientId;
            set => _eveClientId = value;
        }


        public string ClipString = "{0} {1}";


        public List<String> Players = new List<string>()
        {
            "Studley Maniac",
            "Charles Manson-666",
            "Charlie Manson-666",
            "Lieutenant JG Studley Maniac",
        };

        private string Results = string.Empty;

        public static void RunAsStaThread(Action goForIt)
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

        public bool UseSanderling = false;

        public Sensor sensor;

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Process that's holding the clipboard
        /// </summary>
        /// <returns>A Process object holding the clipboard, or null</returns>
        ///-----------------------------------------------------------------------------
        public Process ProcessHoldingClipboard()
        {
            Process theProc = null;

            IntPtr hwnd = GetOpenClipboardWindow();

            if (hwnd != IntPtr.Zero)
            {
                uint processId;
                uint threadId = GetWindowThreadProcessId(hwnd, out processId);

                Process[] procs = Process.GetProcesses();
                foreach (Process proc in procs)
                {
                    IntPtr handle = proc.MainWindowHandle;

                    if (handle == hwnd)
                    {
                        theProc = proc;
                    }
                    else if (processId == proc.Id)
                    {
                        theProc = proc;
                    }
                }
            }

            return theProc;
        }




        public MainWindow()
        {


            this.Left = 13.0;
            this.Top = 198.0;

            log4net.Config.XmlConfigurator.Configure();
            InitializeComponent();

            //Clipboard.Clear();
            
            EveClientId = Extensions.GetEveOnlineClientProcessId();
            EveMainWindow = Extensions.GetEveMainWindow();
            ComboBox.ItemsSource = Players;
            ComboBox.SelectedIndex = 0;
            sensor = new Sensor();
            string fmt = $"Inside MainWindow eveClient {EveClientId:X8} eveMainWindow {_eveMainWindow:X8}";
            Log.Debug(fmt);
           
        }


        public string ClientID = $"272ec715901c47a3b555c77ee7ed5e3f";
        public string SecretKey = $"Zf8YKDFe8GbbTBEPu8wpC7CIl513YUXFtZqMu8Dk";
        public string CallBack = $"https://localhost/callback";
        

        private void EveSSO_Click(object sender, RoutedEventArgs e)
        {
            //Log.Debug("inside SSO_Click");
            //System.Timers.Timer tm = new System.Timers.Timer();
            //tm.Interval = 10 * 60_0000; // 10 minutes
            //tm.Elapsed += TimerFired;
            //tm.Start();
        }
        private void BookmarkByOverview(string BookmarkName)
        { 
            IMemoryMeasurement measurement;
            WindowMotor motor = new WindowMotor(_eveMainWindow.Value);
            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            var response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response);

            measurement = response?.MemoryMeasurement?.Value;
            var overview = measurement.WindowOverview.FirstOrDefault();

            var entry = overview.ListView.Entry.FirstOrDefault();
            var motionParam = entry.MouseClick(MouseButtonIdEnum.Right);

            motor.ActSequenceMotion(motionParam.AsSequenceMotion(measurement));


            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response);
            measurement = response?.MemoryMeasurement?.Value;
            Sanderling.Interface.MemoryStruct.IMenu menu = measurement.Menu.FirstOrDefault();
            BookmarkByMenu(measurement,menu,"Save Location",BookmarkName);



        }

        private void BookmarkByMenu(IMemoryMeasurement measure,IMenu menu,string WhichOne,string BookName)
        {
            WindowMotor motor = new WindowMotor(_eveMainWindow.Value);
            IMemoryMeasurement measureme;
            MotionParam motionParam;
            var response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            if (null == menu)
            {
                do
                {
                    //motionParam = menuEntry.MouseClick(MouseButtonIdEnum.Left);
                    //motor.ActSequenceMotion(motionParam.AsSequenceMotion(measure));
                    response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);

                } while (null == response.MemoryMeasurement.Value.Menu);

                menu = response.MemoryMeasurement.Value.Menu.First();
            }

            //measureme = response?.MemoryMeasurement?.Value;
            //var mParam = measureme.WindowOverview.First().ListView.Entry.FirstOrDefault().MouseClick(MouseButtonIdEnum.Right);
            //motor.ActSequenceMotion(mParam.AsSequenceMotion(measureme));
            //    menu = measure.Menu.First();
            //}
            //IMenu menu = measure.Menu.First();
            var menuEntry = menu.Entry.FirstOrDefault(x=> Regex.IsMatch(x.Text, WhichOne));
            //x => x.Text.Contains(WhichOne));
            motionParam = menuEntry.MouseClick(MouseButtonIdEnum.Left);
            motor.ActSequenceMotion(motionParam.AsSequenceMotion(measure));
            //var response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
            //do
            //{
            //    response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
            //} while (null == response);
            //measure = response?.MemoryMeasurement?.Value;
            //var BookWindow = measure.WindowOther.First(x => Regex.IsMatch(x.Caption, "New Location"));
            //var BookWindowLabel = BookWindow.InputText;
            WindowsInput.InputSimulator sim = new InputSimulator();
            sim.Keyboard.TextEntry(BookName).Sleep(200).KeyPress(VirtualKeyCode.RETURN);
            




        }
        private static void TimerFired(object sender, ElapsedEventArgs e)
        {
            Log.Debug(new StackFrame(0,true));
            string SoundFile = @"C:\Windows\media\Alarm01.wav";
            System.Media.SoundPlayer play = new SoundPlayer();
            play.SoundLocation = SoundFile;
            play.Play();
            (sender as System.Timers.Timer).Stop();
        }

        private void Classify_OnClick(object sender, RoutedEventArgs e)
        {
            Log.Debug(new StackFrame(0, true));

           /// Rect r = this.RestoreBounds;
            //double Top, Left, Right, Bottom;
            //Top = this.Top;
            //Left = this.Left;
            
            Bookmarker bm=new Bookmarker(EveMainWindow.Value,EveClientId.Value);


            //sensor = new Sensor();
            //Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            //var response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            //do
            //{
            //    response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            //} while (null == response);
            //MeasurementReceived(response?.MemoryMeasurement);
            //sensor = null;
        }

        public void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
            Log.Debug(new StackFrame(0, true));
            

            WindowsInput.InputSimulator sim = new WindowsInput.InputSimulator();
            var overview = measurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.First(x =>
                Regex.IsMatch(x.LabelText.ElementAt(2).Text, @"Wormhole [A-Z]"));
            string toSearch = entry.LabelText.ElementAt(2).Text.Split(' ')[1];
            string Hole = (toSearch.Contains("K162")) ? "UNK" : Worm.GetHole(toSearch);
            var scanResults = measurement?.Value.WindowProbeScanner.First().ScanResultView.Entry.FirstOrDefault();
            string scanID = scanResults?.LabelText.ElementAt(1).Text.Substring(0, 3);

            
            if (Regex.IsMatch(Hole, @"Barbican|Conflux|Redoubt|Sentinel|Vidette")) scanID = "Drifter";
            ClipString = string.Format(ClipString, scanID, Hole );//, Extensions.Julian4());
            bool isEOL = false;
           
            if (Hole.Contains("UNK"))
            {
                var motor = new WindowMotor(_eveMainWindow.Value);
                ShowInfo(overview.ListView.Entry.First(x =>Regex.IsMatch(x.LabelText.ElementAt(2).Text, @"Wormhole [A-Z]")),measurement);
               Sanderling.Interface.FromInterfaceResponse response = null;
                do
                {
                    response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
                } while (null == response);

                var InfoWindow =
                    response.MemoryMeasurement?.Value.WindowOther.First(x => x.Caption.Contains("formation"));
                SelectAndCopy(sim,InfoWindow);
                
                //try
//                {
//                    motor.MouseClick(InfoWindow.RegionCenter().Value, MouseButtonIdEnum.Left);

//#if true
//                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_A).Sleep(200);
//                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_A).KeyUp(VirtualKeyCode.CONTROL).Sleep(200);
//                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_C).Sleep(200);
//                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_C).KeyUp(VirtualKeyCode.CONTROL).Sleep(200);
                    
//                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_W).Sleep(200);
//                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_W).KeyUp(VirtualKeyCode.CONTROL);
//#else
//                    SelectAndCopy(sim);
//                    sim.Keyboard.Sleep(200).KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_W).Sleep(200);
//                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_W).KeyUp(VirtualKeyCode.CONTROL);
//#endif
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);
//                    throw;
//                }

                Results = Classify(ClipString, out isEOL);
                //try
                //{
                //    Process pr = ProcessHoldingClipboard();
                //    if(pr!=null)
                //    Log.Debug(pr.MainModule.FileName);
                //   //RunAsStaThread(() => Clipboard.SetText(string.IsNullOrEmpty(Results) ? ClipString : Results));
                  
                //}
                //catch (Exception e)
                //{
                    
                    
                //}

              
            }
            else
            {
            //    try
            //    {
            //        Process pr = ProcessHoldingClipboard();
            //        if(pr!=null)
            //        Log.Debug(pr.MainModule.FileName);
            //      //  RunAsStaThread(() => Clipboard.SetText(string.IsNullOrEmpty(Results) ? ClipString : Results)); 
            //    }
            //    catch (COMException  e)
            //    {
            //        IntPtr hwnd = GetOpenClipboardWindow();
            //        Log.Debug(hwnd.ToString());
            //    }
            }
             //Bookmark(string.IsNullOrEmpty(Results) ? ClipString : Results,isEOL);
             string Name = (string.IsNullOrEmpty(Results)) ? ClipString : Results;
             Name+= ((isEOL) ? " eol" + Extensions.Julian4() : Extensions.Julian4());
            BookmarkByOverview(Name);
            /// Use this as a way of saying my program is done reset the 
            /// ScanHelper program foreground
           
            Thread.Sleep(50);
            SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle);
         //   App.Current.MainWindow.Close();
        }

        public void ShowInfo(Sanderling.Interface.MemoryStruct.IOverviewEntry entry, BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
            Log.Debug(new StackFrame(0, true));
            WindowMotor motor = new WindowMotor(_eveMainWindow.Value);
            var motionParam = entry.MouseClick(MouseButtonIdEnum.Left);
            motor.ActSequenceMotion(motionParam.AsSequenceMotion(measurement?.Value));
            motionParam.KeyDown = motionParam.KeyUp = new[] { VirtualKeyCode.VK_T };
            motor.ActSequenceMotion(motionParam.AsSequenceMotion(measurement?.Value));

            return;
        }

        public void Bookmark(string BookmarkName,bool isEOL)
        {
            // sensor = new Sensor();
            WindowMotor m = new WindowMotor(_eveMainWindow.Value);
            Sanderling.Interface.FromInterfaceResponse response = null;
            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response);
            
            BookmarkName += (isEOL) ? " eol" + Extensions.Julian4() : Extensions.Julian4();
            var overview = response.MemoryMeasurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.FirstOrDefault();
            var motionParam = entry.MouseClick(MouseButtonIdEnum.Left);
            m.ActSequenceMotion(motionParam.AsSequenceMotion(response.MemoryMeasurement?.Value));
            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response);

            var menu = response.MemoryMeasurement?.Value.Menu.FirstOrDefault();
            BookmarkByMenu(response.MemoryMeasurement?.Value,menu,"Save Location",BookmarkName+" 00");


            //WindowsInput.InputSimulator sim = new InputSimulator();
            //Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            //sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_B).Sleep(200);
            //sim.Keyboard.KeyUp(VirtualKeyCode.VK_B).KeyUp(VirtualKeyCode.CONTROL).Sleep(200).TextEntry(BookmarkName).KeyPress(VirtualKeyCode.RETURN);

            //sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            //sensor = null;


        }


        public string Classify(string work,out bool isEol)
        {
            Log.Debug(new StackFrame(0, true));
            string tempstring = Clipboard.GetText();
            Clipboard.Clear();
            isEol= Regex.IsMatch(tempstring, @"reaching the end");

            if (Replace(ref work, tempstring, out string replace))
                return replace;
            else
                return string.Empty;


        }

        private static bool Replace(ref string start, string tempstring, out string replace)
        {
            Log.Debug(new StackFrame(0, true));
            bool frigate = Regex.IsMatch(tempstring, @"Only the smallest");
            //bool eol = Regex.IsMatch(tempstring, @"reaching the end");
            replace = string.Empty;

            if (Regex.IsMatch(tempstring, @"high")) replace = start.Replace("UNK", "HS");

            if (Regex.IsMatch(tempstring, @"low")) replace = start.Replace("UNK", "LS");

            if (Regex.IsMatch(tempstring, @"null")) replace = start.Replace("UNK", "NS");

            if (Regex.IsMatch(tempstring, @"medium")) replace = start.Replace("UNK", "C1");

            if (Regex.IsMatch(tempstring, @"into unknown")) replace = start.Replace("UNK", "C23");

            if (Regex.IsMatch(tempstring, @"D|dangerous")) replace = start.Replace("UNK", "C45");

            if (Regex.IsMatch(tempstring, @"D|deadly")) replace = start.Replace("UNK", "C6");

            if (frigate) replace += "F";

            //if (eol) replace += " eol";
            return !string.IsNullOrEmpty(replace);
        }

        private void SelectAndCopy(WindowsInput.InputSimulator sim,IWindow infoWindow)
        {
            var motor = new WindowMotor(EveMainWindow.Value);
            motor.MouseClick(infoWindow.RegionCenter().Value, MouseButtonIdEnum.Left);
            Log.Debug(new StackFrame(0, true));
            List<VirtualKeyCode> selectall = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A };
            List<VirtualKeyCode> copyit = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C };

            List<VirtualKeyCode> workAll = new List<VirtualKeyCode>(selectall);
            workAll.AddRange(selectall);
            selectall.ForEach((p) => sim.Keyboard.KeyDown(p).Sleep(200));
            selectall.Reverse();
            selectall.ForEach((p) => sim.Keyboard.KeyUp(p).Sleep(200));

        }

        private void Bookmark_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug(new StackFrame(0, true));
            //sensor = new Sensor();
            Sanderling.Interface.FromInterfaceResponse response;
            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response);

            var overview = response.MemoryMeasurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.FirstOrDefault();//x => x.LabelText.ElementAt(2).Text.Contains("Concentrated"));
            entry.MouseClick(MouseButtonIdEnum.Left);

            WindowsInput.InputSimulator sim = new InputSimulator();
            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_B).Sleep(200);
            sim.Keyboard.KeyUp(VirtualKeyCode.VK_B).KeyUp(VirtualKeyCode.CONTROL).Sleep(200).TextEntry(DateTime.Now.ToString("HHmm")).KeyPress(VirtualKeyCode.RETURN);
            //sim.Keyboard.KeyPress(VirtualKeyCode.CAPITAL).Sleep(500).TextEntry(DateTime.Now.ToString("HHmm")).KeyPress(VirtualKeyCode.RETURN);
           // sensor = null;
        }
    }
}


public static class Extensions
{
    public static int? GetEveOnlineClientProcessId() =>
        System.Diagnostics.Process.GetProcesses()
            ?.FirstOrDefault(process =>
            {
                try
                {
                    return string.Equals("ExeFile.exe", process?.MainModule?.ModuleName,
                        StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                }

                return false;
            })
            ?.Id;

    public static IntPtr? GetSanderling()
    {
        return Process.GetProcesses()?.FirstOrDefault(pr => pr.MainWindowTitle.Contains("Sanderling")).MainWindowHandle;
    }

    public static IntPtr? GetEveMainWindow() =>
        System.Diagnostics.Process.GetProcesses()
            ?.FirstOrDefault(process =>
            {
                try
                {
                    return string.Equals("ExeFile.exe", process?.MainModule?.ModuleName,
                        StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                }

                return false;
            })
            ?.MainWindowHandle;
    public static IntPtr? GetEveMainWindow(string Player) =>
        System.Diagnostics.Process.GetProcesses()
            ?.FirstOrDefault(process =>
            {
                try
                {
                    return string.Equals("ExeFile.exe", process?.MainModule?.ModuleName,
                        StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                }

                return false;
            })
            ?.MainWindowHandle;


    public static string Julian4()
    {
        return string.Format(" {0}{1}",DateTime.Now.ToString("yy").Substring(1), DateTime.Now.DayOfYear.ToString("000"));
    }

};

