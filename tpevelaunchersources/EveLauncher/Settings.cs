using System;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace EveLauncher
{
    public class AppSettings<T> where T : new()
    {
        private const string DefaultFileName = "settings.json";
        public static string ProgrammPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public void Save(string fileName = DefaultFileName)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(this));
        }

        public static void Save(T pSettings, string fileName = DefaultFileName)
        {
            File.WriteAllText(fileName, (new JavaScriptSerializer()).Serialize(pSettings));
        }

        public static T Load(string fileName = DefaultFileName)
        {
            T t = new T();
            var fullFileName = ProgrammPath + "\\" + fileName;
            Console.WriteLine("______Check this file: " + fullFileName);
            if (File.Exists(fullFileName))
            {
                t = (new JavaScriptSerializer()).Deserialize<T>(File.ReadAllText(fullFileName));
            }
            else
            {
#if DEBUG
                Console.WriteLine("File " + fileName + " not exist!");
                Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory() + Environment.NewLine +
                                  "check this file: " + fullFileName);
#endif
                Save(t);
            }

            return t;
        }
    }


    public class Settings : AppSettings<Settings>
    {
        public string DefaultHttpString = "http://www.geoplugin.net/xml.gp?";
        public string CountryToHide = "Somalia";
        public string IpToHide = "127.0.0.1";

        public int DelayBeforeRecheckOurIp = 10;
        public int DelayBeforeLookingEveClient = 100;
        public int DelayBetweenLoadingCharacterAttempts = 30;
        public string PathToBotExe = @"C:\Eve\Tools\Sanderling.Exe.exe";

        public string GetLauncherExe()
        {
            if (string.IsNullOrEmpty(EveSharedCachePath))
                return null;
            var resultPath = Path.GetDirectoryName(Path.GetDirectoryName(EveSharedCachePath));
            if (string.IsNullOrEmpty(resultPath))
                return null;
            var x = Path.Combine(resultPath, "Launcher\\evelauncher.exe");
            return x;
        }

        private string EveSharedCachePath
        {
            get
            {
                RegistryKey hkcu = Registry.CurrentUser;
                RegistryKey pathKey = hkcu.OpenSubKey(@"SOFTWARE\CCP\EVEONLINE");
                if (pathKey != null)
                {
                    string value = pathKey.GetValue("CACHEFOLDER", Environment.SpecialFolder.CommonApplicationData + @"\CCP\EVE\SharedCache\") as string;
                    return value;
                }
                return null;
            }
        }

    }

}
