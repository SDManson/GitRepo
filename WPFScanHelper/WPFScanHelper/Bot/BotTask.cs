using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanderling.Motor;

namespace WPFScanHelper.Bot
{
    public interface IBotTask
    {
        IEnumerable<IBotTask> Component { get; }
        IEnumerable<MotionParam> Effects { get; }

    }

    public class BotTask : IBotTask

    {
        public IEnumerable<IBotTask> Component { set; get; }
        public IEnumerable<MotionParam> Effects { get; set; }
    }
}
