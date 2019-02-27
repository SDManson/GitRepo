using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotEngine.Interface;

namespace WPFScanHelper.Bot
{

    public class MotionResult
    {
        public Int64 Id;

        public bool Success;
    }

    public class BotStepInput
    {
        public Int64 TimeMilli;

        public BotEngine.Interface.FromProcessMeasurement<Sanderling.Interface.MemoryStruct.IMemoryMeasurement>
            FromProcessMemoryMeasurement;

        public StringAtPath ConfigSerial;

        public MotionResult[] StepLastMotionResult;

        public IEnumerable<IBotTask> RootTaskListComponentOverride;
    }
}
