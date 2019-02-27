using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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
    }
}
