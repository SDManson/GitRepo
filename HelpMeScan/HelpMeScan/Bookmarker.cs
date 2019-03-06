using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Sanderling;
using Sanderling.Motor;
using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using WindowsInput;
using WindowsInput.Native;
using WormLib;
using System.Text.RegularExpressions;
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
   partial class Bookmarker
   {
        public IntPtr? EveMainWindow { get; set; }
        public int? EveClientId { get; set; }

        public string ClipString = "{0} {1}";
        private Sensor sensor = null;
        
        public Bookmarker(IntPtr? mainWindow = null, int? clientID = null)
        {
            EveMainWindow = (null == mainWindow) ? Extensions.GetEveMainWindow() : mainWindow;
            EveClientId = (null == clientID) ? Extensions.GetEveOnlineClientProcessId() : clientID;
            sensor = new Sensor();
            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(EveMainWindow.Value);
            var response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response);
            MeasurementReceived(response?.MemoryMeasurement);

        }


       

        public void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
            InputSimulator sim=new InputSimulator();
            var overview = measurement?.Value.WindowOverview.FirstOrDefault();
            var entry = overview.ListView.Entry.First(x =>
                Regex.IsMatch(x.LabelText.ElementAt(2).Text, @"Wormhole [A-Z]"));
            string toSearch = entry.LabelText.ElementAt(2).Text.Split(' ')[1];
            string Hole = (toSearch.Contains("K162")) ? "UNK" : Worm.GetHole(toSearch);
            var scanResults = measurement?.Value.WindowProbeScanner.First().ScanResultView.Entry.FirstOrDefault();
            string scanID = scanResults?.LabelText.ElementAt(1).Text.Substring(0, 3);

            if (Regex.IsMatch(Hole, @"Barbican|Conflux|Redoubt|Sentinel|Vidette")) scanID = "Drifter";
            ClipString = string.Format(ClipString, scanID, Hole);
            bool isEol = false;
            if (Hole.Contains("UNK"))
            {
                var motor = new WindowMotor(EveMainWindow.Value);
                ShowInfo(overview.ListView.Entry.First(x => Regex.IsMatch(x.LabelText.ElementAt(2).Text, @"Wormhole [A-Z]")), measurement);
                //ShowInfo(overview.ListView.Entry.FirstOrDefault(x => x.LabelText.ElementAt(2).Text.Contains("Wormhole")),measurement);
                Sanderling.Interface.FromInterfaceResponse response = null;
                do {
                    response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
                } while (null == response);

                var InfoWindow =
                    response.MemoryMeasurement?.Value.WindowOther.First(x => x.Caption.Contains("formation"));
                SelectAndCopy(sim,InfoWindow);

                ClipString = string.Format(ClipString, scanID, Hole);//, Extensions.Julian4());
                bool isEOL = false;
                
                var results = Classify(ClipString, out isEOL);

                string Name = (string.IsNullOrEmpty(results)) ? ClipString : results;
                Name += ((isEOL) ? " eol" + Extensions.Julian4() : Extensions.Julian4());
                BookmarkByOverview(Name);
                ///// Use this as a way of saying my program is done reset the 
                ///// ScanHelper program foreground

                //Thread.Sleep(50);
                //SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle);
                //   App.Current.MainWindow.Close();
            }
            BookmarkByOverview(ClipString+Extensions.Julian4());

    }
        private void BookmarkByOverview(string BookmarkName)
        {
            IMemoryMeasurement measurement;
            WindowMotor motor = new WindowMotor(EveMainWindow.Value);
            Sanderling.Motor.WindowMotor.EnsureWindowIsForeground(EveMainWindow.Value);
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
            BookmarkByMenu(measurement, menu, "Save Location", BookmarkName);



        }

        private void BookmarkByMenu(IMemoryMeasurement measure, IMenu menu, string WhichOne, string BookName)
        {
            WindowMotor motor = new WindowMotor(EveMainWindow.Value);
            IMemoryMeasurement measureme;
            MotionParam motionParam;
            var response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);

            WindowsInput.InputSimulator sim = new InputSimulator();
            sim.Keyboard.KeyDown(VirtualKeyCode.MENU).Sleep(200).KeyPress(VirtualKeyCode.VK_D).Sleep(200).KeyUp(VirtualKeyCode.MENU);
            if (null == menu)
            {
                do
                {
                    response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
                } while (null == response.MemoryMeasurement.Value.Menu);

                menu = response.MemoryMeasurement.Value.Menu.First();
            }

           
            var menuEntry = menu.Entry.FirstOrDefault(x => Regex.IsMatch(x.Text, WhichOne));
           
            motionParam = menuEntry.MouseClick(MouseButtonIdEnum.Left);
            motor.ActSequenceMotion(motionParam.AsSequenceMotion(measure));

            
            do
            {
                response = sensor?.MeasurementTakeNewRequest(EveClientId.Value);
            } while (null == response.MemoryMeasurement.Value.WindowOther.First(p=> Regex.IsMatch(p.Caption,"Location")));

            IWindow other =
                response.MemoryMeasurement.Value.WindowOther.First(p => Regex.IsMatch(p.Caption, "Location"));
            //var inputText = other.InputText.First(p => Regex.IsMatch(p.Text, "Wormhole"));
            //WindowsInput.InputSimulator sim = new InputSimulator();
            sim.Keyboard.TextEntry(BookName).Sleep(200).KeyPress(VirtualKeyCode.RETURN);

            sim.Keyboard.Sleep(1000).KeyDown(VirtualKeyCode.MENU).Sleep(200).KeyPress(VirtualKeyCode.VK_D).Sleep(200).KeyUp(VirtualKeyCode.MENU);

        }

        public string Classify(string work, out bool isEol)
        {
           // Log.Debug(new StackFrame(0, true));
            string tempstring = Clipboard.GetText();
            //Clipboard.Clear();
            isEol = Regex.IsMatch(tempstring, @"reaching the end");

            if (Replace(ref work, tempstring, out string replace))
                return replace;
            else
                return string.Empty;


        }
        public void ShowInfo(IOverviewEntry entry,
            BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
            WindowMotor motor=new WindowMotor(EveMainWindow.Value);
            var motionParam = entry.MouseClick(MouseButtonIdEnum.Left);
            motionParam.KeyDown = motionParam.KeyUp = new[] {VirtualKeyCode.VK_T};
            motor.ActSequenceMotion(motionParam.AsSequenceMotion(measurement?.Value));
        }


        public void SelectAndCopy(WindowsInput.InputSimulator sim,IWindow infoWindow)
        {
            var motor = new WindowMotor(EveMainWindow.Value);
            motor.MouseClick(infoWindow.RegionCenter().Value, MouseButtonIdEnum.Left);
            List<VirtualKeyCode> selectall = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A };
            List<VirtualKeyCode> copyit = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C };
            List<VirtualKeyCode> workAll = new List<VirtualKeyCode>();
            List<VirtualKeyCode> closer = new List<VirtualKeyCode>() {VirtualKeyCode.CONTROL, VirtualKeyCode.VK_W };
            //workAll.AddRange(selectall);
            //selectall.Reverse();
            //workAll.AddRange(selectall);
            //workAll.AddRange(copyit);
            //copyit.Reverse();
            //workAll.AddRange(copyit);
            //workAll.AddRange(closer);
            //closer.Reverse();
            //workAll.AddRange(closer);

            //workAll.ForEach((p) => sim.Keyboard.Key);

            selectall.ForEach((p) => sim.Keyboard.KeyDown(p).Sleep(200));
            selectall.Reverse();
            selectall.ForEach((p) => sim.Keyboard.KeyUp(p).Sleep(200));

            copyit.ForEach((p) => sim.Keyboard.KeyDown(p).Sleep(200));
            copyit.Reverse();
            copyit.ForEach((p) => sim.Keyboard.KeyUp(p).Sleep(200));


            closer.ForEach((p) => sim.Keyboard.KeyDown(p).Sleep(200));
            closer.Reverse();
            closer.ForEach((p) => sim.Keyboard.KeyUp(p).Sleep(200));



            //sim.Keyboard.Sleep(200).KeyPress(VirtualKeyCode.VK_W).Sleep(200);
            //sim.Keyboard.KeyUp(VirtualKeyCode.VK_W);//.KeyUp(VirtualKeyCode.CONTROL);
        }
        private static bool Replace(ref string start, string tempstring, out string replace)
        {
            //Log.Debug(new StackFrame(0, true));
            bool frigate = Regex.IsMatch(tempstring, @"Only the smallest");
            //bool eol = Regex.IsMatch(tempstring, @"reaching the end");
            replace = string.Empty;

            if (Regex.IsMatch(tempstring, @"high")) replace = start.Replace("UNK", "HS");

            if (Regex.IsMatch(tempstring, @"low")) replace = start.Replace("UNK", "LS");

            if (Regex.IsMatch(tempstring, @"null")) replace = start.Replace("UNK", "NS");

            //if (Regex.IsMatch(tempstring, @"medium")) replace = start.Replace("UNK", "C1");

            if (Regex.IsMatch(tempstring, @"into unknown"))
            if (Regex.IsMatch(tempstring, @"medium")) replace = start.Replace("UNK", "C1");
            else
            {
                replace = start.Replace("UNK", "C23");
            }

            if (Regex.IsMatch(tempstring, @"D|dangerous")) replace = start.Replace("UNK", "C45");

            if (Regex.IsMatch(tempstring, @"D|deadly")) replace = start.Replace("UNK", "C6");

            if (frigate) replace += "F";

            //if (eol) replace += " eol";
            return !string.IsNullOrEmpty(replace);
        }
    }
   
}
