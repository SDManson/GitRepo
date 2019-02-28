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



        private int? _eveClientId = null;
        public IntPtr? _eveMainWindow = null;


        private List<String> Players = new List<string>()
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


        public MainWindow()
        {
            //string sss = Extensions.Julian4();
            log4net.Config.XmlConfigurator.Configure();
            InitializeComponent();
            _eveClientId = Extensions.GetEveOnlineClientProcessId();
            _eveMainWindow = Extensions.GetEveMainWindow();
            ComboBox.ItemsSource = Players;
            ComboBox.SelectedIndex = 0;
            sensor = new Sensor();
            string fmt = $"Inside MainWindow eveClient {_eveClientId:X8} eveMainWindow {_eveMainWindow:X8}";
            Log.Debug(fmt);
           
        }


        public string ClientID = $"272ec715901c47a3b555c77ee7ed5e3f";
        public string SecretKey = $"Zf8YKDFe8GbbTBEPu8wpC7CIl513YUXFtZqMu8Dk";
        public string CallBack = $"https://localhost/callback";

        private void EveSSO_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("inside SSO_Click");
            System.Timers.Timer tm = new System.Timers.Timer();
            tm.Interval = 10 * 60_0000; // 10 minutes
            tm.Elapsed += TimerFired;
            tm.Start();


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

            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            var response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
            do
            {
                response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
            } while (null == response);
            MeasurementReceived(response?.MemoryMeasurement);
        }

        public void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
            Log.Debug(new StackFrame(0, true));
            WindowsInput.InputSimulator sim = new WindowsInput.InputSimulator();
            var overview = measurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.Where(x =>
                Regex.IsMatch(x.LabelText.ElementAt(2).Text, @"Wormhole [A-Z]"));
            string toSearch = entry.First().LabelText.ElementAt(2).Text.Split(' ')[1];
            string Hole = (toSearch.Contains("K162")) ? "UNK" : Worm.GetHole(toSearch);
            var scanResults = measurement?.Value.WindowProbeScanner.First().ScanResultView.Entry.FirstOrDefault();
            string scanID = scanResults?.LabelText.ElementAt(1).Text.Substring(0, 3);

            string ClipString = "{0} {1} {2}";
            if (Regex.IsMatch(Hole, @"Barbican|Conflux|Redoubt|Sentinel|Vidette")) scanID = "Drifter";
            ClipString = string.Format(ClipString, scanID, Hole, Extensions.Julian4());
            
            if (Hole.Contains("UNK"))
            {
                var motor = new WindowMotor(_eveMainWindow.Value);
                ShowInfo(overview.ListView.Entry.FirstOrDefault(x => x.LabelText.ElementAt(2).Text.Contains("Wormhole")),measurement);
                Sanderling.Interface.FromInterfaceResponse response = null;
                do
                {
                    response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
                } while (null == response);

                var InfoWindow =
                    response.MemoryMeasurement?.Value.WindowOther.First(x => x.Caption.Contains("formation"));
                try
                {
                    motor.MouseClick(InfoWindow.RegionCenter().Value, MouseButtonIdEnum.Left);

                    
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_A).Sleep(200);
                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_A).KeyUp(VirtualKeyCode.CONTROL).Sleep(200);
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_C).Sleep(200);
                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_C).KeyUp(VirtualKeyCode.CONTROL).Sleep(200);
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL).KeyDown(VirtualKeyCode.VK_W).Sleep(200);
                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_W).KeyUp(VirtualKeyCode.CONTROL);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                bool isEOL = false;
                Results = Classify(ClipString,out isEOL);
                try
                {
                    RunAsStaThread(() =>
                    {
                        Clipboard.SetText(string.IsNullOrEmpty(Results)
                            ? ClipString + (isEOL ? "eol" : "")
                            : Results + (isEOL ? "eol" : ""));
                    });

                }
                catch (COMException e)
                {
                    IntPtr hwnd=GetOpenClipboardWindow();
                    Log.Debug(hwnd.ToString());
                }

              
            }
            else
            {
                try
                {
                    RunAsStaThread(() => Clipboard.SetText(ClipString));
                }
                catch (COMException  e)
                {
                     
                }
            }
            //Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            Bookmark(string.IsNullOrEmpty(Results) ? ClipString : Results);
            /// Use this as a way of saying my program is done reset the 
            /// ScanHelper program foreground
           
            Thread.Sleep(500);
            SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle);
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

        public void Bookmark(string BookmarkName)
        {
            Sanderling.Interface.FromInterfaceResponse response = null;
            do
            {
                response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
                Thread.Sleep(250);
            } while (null == response);

            var overview = response.MemoryMeasurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.FirstOrDefault();//x => x.LabelText.ElementAt(2).Text.Contains("Concentrated"));
            entry.MouseClick(MouseButtonIdEnum.Left);

            WindowsInput.InputSimulator sim = new InputSimulator();
            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            sim.Keyboard.KeyPress(VirtualKeyCode.CAPITAL).Sleep(500).TextEntry(BookmarkName).KeyPress(VirtualKeyCode.RETURN);

        }


        public string Classify(string work,out bool isEol)
        {
            Log.Debug(new StackFrame(0, true));
            string tempstring = Clipboard.GetText();
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

        private static void SelectAndCopy(WindowsInput.InputSimulator sim)
        {
            Log.Debug(new StackFrame(0, true));
            List<VirtualKeyCode> selectall = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A };
            List<VirtualKeyCode> copyit = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C };

            List<VirtualKeyCode> workAll = new List<VirtualKeyCode>(selectall);
            workAll.AddRange(selectall);
            selectall.ForEach((p) => sim.Keyboard.KeyDown(p));
            selectall.Reverse();
            selectall.ForEach((p) => sim.Keyboard.KeyUp(p));

        }

        private void Bookmark_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug(new StackFrame(0, true));
            Sanderling.Interface.FromInterfaceResponse response = null;
            do
            {
                response = sensor?.MeasurementTakeNewRequest(_eveClientId.Value);
                Thread.Sleep(250);
            } while (null == response);

            var overview = response.MemoryMeasurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.FirstOrDefault();//x => x.LabelText.ElementAt(2).Text.Contains("Concentrated"));
            entry.MouseClick(MouseButtonIdEnum.Left);

            WindowsInput.InputSimulator sim = new InputSimulator();
            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(_eveMainWindow.Value);
            sim.Keyboard.KeyPress(VirtualKeyCode.CAPITAL).Sleep(500).TextEntry(DateTime.Now.ToString("HHmm")).KeyPress(VirtualKeyCode.RETURN);
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
        return DateTime.Now.ToString("yy").Substring(1) + DateTime.Now.DayOfYear.ToString("000");
    }

};

