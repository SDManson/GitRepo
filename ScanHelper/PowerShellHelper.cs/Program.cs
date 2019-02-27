using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Sanderling.Interface;
using Sanderling;
using Sanderling.Script.Impl;
using System.Threading;
using LINQPad;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using Sanderling.Interface.MemoryStruct;

namespace PowerShellHelper.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            var eveClientId = Extension.GetEveOnlineClientProcessId();
            var sensor = new Sensor();
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
                    break;
                }
                 Thread.Sleep(1000);
            }
         }

        public static void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
        {
            //Console.WriteLine("\nMeasurement received");
            // Console.WriteLine("measurement time: " + ((measurement?.End)?.ToString("### ### ### ### ###")?.Trim() ?? "null"));

            var ListUIElement =
                measurement?.Value?.EnumerateReferencedUIElementTransitive()
                ?.GroupBy(uiElement => uiElement.Id)
                ?.Select(group => group?.FirstOrDefault())
                ?.ToArray();


            


            var overview = measurement.Value.WindowOverview.First();
            var entry = overview.ListView.Entry.Where(x => x.LabelText.ElementAt(2).Text.Contains("Wormhole"));
            var work = entry.First().LabelText;
            string ToSearch = entry.First().LabelText.ElementAt(2).Text.Split(' ')[1];
            var SelectedItem = measurement.Value.WindowSelectedItemView.FirstOrDefault();
            var labelText = SelectedItem.LabelText.Last();
            
            //Sanderling.Motor.MotionParamExtension.MouseMove(labelText, new[] { BotEngine.Motor.MouseButtonIdEnum.Left });
            //labelText.Dump();


            return;
            







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

        public static IEnumerable<System.Text.RegularExpressions.Group> AsEnumerable(this System.Text.RegularExpressions.GroupCollection gc)
        {
            foreach (System.Text.RegularExpressions.Group g in gc) yield return g;
        }
        public static IEnumerable<System.Text.RegularExpressions.Match> AsEnumeable(this System.Text.RegularExpressions.MatchCollection mc)
        {
            foreach (System.Text.RegularExpressions.Match m in mc) yield return m;
        }

    }
}
