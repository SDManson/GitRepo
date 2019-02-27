using System;
using System.Threading;

namespace EveLauncher
{
    class Program
    {
        public static readonly int MiliSec = 1000;

        static void Main(string[] args)
        {
            TpEveLauncher.MoveConsoleToLeftTopPosition();
            TpEveLauncher eveLauncher = new TpEveLauncher();
            while (!eveLauncher.IsOurIpHidden(TpEveLauncher.Settings))
            {
                Console.WriteLine("_____Waiting [DelayBeforeRecheckOurIp]=" +
                                  TpEveLauncher.Settings.DelayBeforeRecheckOurIp + " sec while VPN create new connection.");
                Thread.Sleep(TpEveLauncher.Settings.DelayBeforeRecheckOurIp * MiliSec);
            }

            Console.WriteLine("_____VPN connected. Now starting launcher...");

            if (eveLauncher.LaunchExe(TpEveLauncher.Settings.GetLauncherExe()))
            {
                Console.WriteLine("_____Waiting update eve client: [DelayBeforeLookingEveClient] = " + TpEveLauncher.Settings.DelayBeforeLookingEveClient + " sec...");
                Thread.Sleep(TpEveLauncher.Settings.DelayBeforeLookingEveClient * MiliSec);

                if (!eveLauncher.SelectCharacter())
                {
                    TpEveLauncher.RebootSystem();
                }

                eveLauncher.CloseEveLauncher();

                if (eveLauncher.LaunchExe(TpEveLauncher.Settings.PathToBotExe))
                {
                    Console.WriteLine("_____Launching bot was successful.");
                }
            }
            else
            {
                TpEveLauncher.RebootSystem();
            }

#if DEBUG
            Console.WriteLine("Press any key...");
            Console.ReadLine(); // Debug
#endif
        }

    }
}
