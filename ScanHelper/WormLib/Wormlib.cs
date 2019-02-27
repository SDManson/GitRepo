using System;
using System.Linq;
using System.IO;
using System.Reflection;
using Sanderling;

namespace WormLib
{
    public class Worm
    {

        static public string GetHole(string toSearch = "K162")
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("Wormholes.txt"));
            if (toSearch == "K162") return string.Empty;
            var test = assembly.GetManifestResourceStream(resourceName);
            //varr test = Assembly. "Wormholes.txt");

            //          return new StreamReader(test).ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToDictionary(v => v.Split(' ')[0], vv => vv.Split(' ')[1])
            return new StreamReader(test).ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToDictionary(v => v.Split(' ')[0], vv => vv.Split(' ')[1])
                        .Where(p => p.Key.Contains(toSearch)).Select(p => p.Value).First();
            //return ss.Where(p => p.Key.Contains(toSearch)).Select(p => p.Value).First();


        }



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

        //static public Sanderling.Sensor _Sensor;

        static public Sensor Sensor
        {
            get { return new Sensor(); }
        }

        static public Sanderling.Interface.FromInterfaceResponse Response()
        {
            return Sensor?.MeasurementTakeNewRequest(GetEveOnlineClientProcessId().Value);
        }

    }


}
