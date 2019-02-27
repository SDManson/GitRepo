using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Wormholes
{
    class Program
    {

        static void Main(string[] args)
        {
            string toSearch = args[0];
            string test = TestQuery(toSearch);
            if (string.IsNullOrEmpty(test))
                {
                MessageBox.Show("oops");
               }
        }
    static public string TestQuery(string toSearch = "K162")
    {
        if (toSearch == "K162") return string.Empty;
        var test = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Wormholes.txt");
        return new StreamReader(test).ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToDictionary(v => v.Split(' ')[0], vv => vv.Split(' ')[1])
            .Where(p => p.Key.Contains(toSearch)).Select(p => p.Value).First();
        //return ss.Where(p => p.Key.Contains(toSearch)).Select(p => p.Value).First();
    }
}
}
