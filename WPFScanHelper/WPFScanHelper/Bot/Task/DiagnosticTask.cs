using System;
using System.Collections.Generic;
using Sanderling.Motor;
namespace WPFScanHelper.Bot.Task
{
    public class DiagnosticTask : IBotTask
    {
        public IEnumerable<IBotTask> Component => null;
        public IEnumerable<MotionParam> Effects => null;
        public string MessageText;
    }
}
