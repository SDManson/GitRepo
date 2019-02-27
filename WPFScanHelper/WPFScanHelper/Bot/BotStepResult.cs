using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanderling.Motor;

namespace WPFScanHelper.Bot
{
    public class MotionRecommendation
    {
        public int Id;

        public MotionParam MotionParam;
    }

    public class BotStepResult
    {
        public Exception Exception;

        public MotionRecommendation[] ListMotion;

        public IBotTask[][] OutputListTaskPath;
    }
}
