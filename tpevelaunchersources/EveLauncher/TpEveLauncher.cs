using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;

namespace EveLauncher
{

    public class TpEveLauncher
    {
        public static readonly int MiliSec = 1000;
        #region DllImport
        public static IntPtr HWND_BOTTOM = (IntPtr)1;
        public static IntPtr HWND_TOP = (IntPtr)0;

        public static uint SWP_NOSIZE = 1;
        public static uint SWP_NOZORDER = 4;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("advapi32.dll", EntryPoint = "InitiateSystemShutdownEx")]
        public static extern int InitiateSystemShutdownEx(string lpMachineName, string lpMessage, int dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown, int dwReason);
        #endregion DllImport

        static Settings _settings;
        public static Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    try
                    {
                        _settings = Settings.Load();
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        Console.WriteLine("File not found: \n" + Environment.NewLine + e);
                        _settings = new Settings();
                        _settings.Save();
                    }
                }
                return _settings;
            }
        }

        public bool IsOurIpHidden(Settings defaultData)
        {
            bool matchWithDefaultData = false;

            try
            {
                KeyValuePair<IPAddress, string> myReceivedData = FillMyIpData(
                    new KeyValuePair<IPAddress, string>(
                        IPAddress.Parse(defaultData.IpToHide), 
                        defaultData.CountryToHide));
                if (defaultData.IpToHide == myReceivedData.Key.ToString() ||
                    defaultData.CountryToHide == myReceivedData.Value)
                {
                    matchWithDefaultData = true;
                    Console.WriteLine("_____Your Ip belongs to your country!");
                }
            }
            catch (Exception e)
            {
                //TODO: use logs for this message
                Console.WriteLine("Site is unavailable!");
                Console.WriteLine(e.Message);
                return false;
            }

            return !matchWithDefaultData;
        }

        public bool LaunchExe(string pathToExeFile)
        {
            try
            {
                Process.Start(pathToExeFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("!!! Launch exe-file failed. We try launch from this path: \n" + pathToExeFile + "\n" + Environment.NewLine + e);
                return false;
            }
            return true;
        }

        public bool CloseEveLauncher()
        {
            try
            {
                var launchersList = Process.GetProcesses().Where(x => x.MainWindowTitle.Contains("EVE") && x.MainModule.ModuleName == "evelauncher.exe").ToList();
                if (launchersList.Count == 0) return false;
                foreach (var launcher in launchersList)
                {
                    launcher.Kill();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Close eve launcher was failed:" + e.Message);
                return false;
            }

            return true;
        }

        public bool SelectCharacter()
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;

            int sleep = 5;
            Console.WriteLine("_____Checking launched eve client.");
            bool result = false;
            int count = 10;
            bool isBroughtForeground = false;

            do
            {
                var eveClientHandle = FindHandleByTitle("EVE", "exefile.exe");

                if (eveClientHandle != IntPtr.Zero)
                {
                    isBroughtForeground = SetForegroundWindow(eveClientHandle);
                    string message = isBroughtForeground
                        ? "_____Choose a caracter complete"
                        : "!!! This is the " + count + " attempt. Choose a caracter...failed.";

                    //SetWindowPos(eveClientHandle, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER); // move window to left top corner
                    //Thread.Sleep(500);
                    Console.WriteLine(message);
                }

                if (isBroughtForeground)
                {
                    PostMessage(eveClientHandle, WM_KEYDOWN, (IntPtr)(13), (IntPtr)0);
                    Thread.Sleep(500);
                    PostMessage(eveClientHandle, WM_KEYUP, (IntPtr)(13), (IntPtr)0);

                    Console.WriteLine("_____We are waiting for eve to boot up after selecting a character...." + sleep +
                                      " sec.");
                    Thread.Sleep(sleep * 1000);

                    if (FindHandleByTitle("EVE -") != IntPtr.Zero)
                    {
                        Console.WriteLine("!!! Eve online client not found..");
                        result = true;
                    }
                }
                Console.WriteLine("_____DelayBetweenLoadingCharacterAttempts = " + Settings.DelayBetweenLoadingCharacterAttempts);
                Thread.Sleep(Settings.DelayBetweenLoadingCharacterAttempts * 1000);
            } while (!(result || count-- < 0));

            return result;
        }

        public static void RebootSystem()
        {
            try
            {
                Console.WriteLine("_____System will be rebooted. Press any key...");
#if DEBUG
                Console.Read();// Debug
#endif
                InitiateSystemShutdownEx("127.0.0.1", "Reboot windows after unsuccessful eve launch", 2, true, true, 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region SupportMethods

        public static void MoveConsoleToLeftTopPosition()
        {
            var consoleWnd = Process.GetCurrentProcess().MainWindowHandle;
            SetWindowPos(consoleWnd, 0, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        private IntPtr FindHandleByTitle(string title, string moduleName = "exefile.exe")
        {
            string message = string.Empty;
            IntPtr hWnd = IntPtr.Zero;
            Process eveProcess;
            int count = 0;
            List<Process> allProcesses;

            do
            {
                allProcesses = Process.GetProcesses().ToList();
                eveProcess = allProcesses.FirstOrDefault(x =>
                    x.MainWindowTitle.Contains(title) && x.MainModule.ModuleName == moduleName);
                Thread.Sleep(1000);
            } while (eveProcess == null && count++ < 10);

            if (eveProcess != null)
            {
                hWnd = eveProcess.MainWindowHandle;
                message = eveProcess.MainWindowTitle;
            }
            else
            {
                //debug
                //var eveProcesses = allProcesses.Where(x => x.MainWindowTitle.Contains(title));
                //var enumerable = eveProcesses.ToList();
                //Console.WriteLine("Count: "+ enumerable.Count);
                //foreach (var proc in enumerable)
                //{
                //    Console.WriteLine("Proc title: " + proc.MainWindowTitle);
                //    Console.WriteLine("Proc module: " + proc.MainModule.ModuleName);
                //}

                message = "...failed. ";
            }

            Console.WriteLine("_____FindHandleByTitle: " + title + "  " + message);
            return hWnd;
        }

        private KeyValuePair<IPAddress, string> FillMyIpData(KeyValuePair<IPAddress, string> defaultData)
        {
            IPAddress ip = defaultData.Key;
            string country = defaultData.Value;
            var locationResponse = new WebClient().DownloadString(Settings.DefaultHttpString);
            var responseXml = XDocument.Parse(locationResponse)
                .Element("geoPlugin");

            if (responseXml != null)
            {
                var element = responseXml.Element("geoplugin_request");
                if (element != null)
                {
                    IPAddress.TryParse(element.Value, out ip);
                }

                element = responseXml.Element("geoplugin_countryName");
                if (element != null)
                    country = element.Value;
            }

            return new KeyValuePair<IPAddress, string>(ip, country);

        }

        #endregion SupportMethods

    }
}
