using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ConsoleAppender
{
    internal class Program
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Logger.InfoFormat($"Running as {WindowsIdentity.GetCurrent().Name}");
            Logger.Error($"This will appear as red in console");

        }
    }
}
