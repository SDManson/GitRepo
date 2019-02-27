using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanderling.Parse;
using Sanderling.Motor;

namespace WPFScanHelper.Bot.Task
{
    public class UndockTask : IBotTask
    {
        public IMemoryMeasurement MemoryMeasurement;

        public IEnumerable<IBotTask> Component => null;

        public IEnumerable<MotionParam> Effects
        {
            get
            {
                if (MemoryMeasurement?.IsUnDocking ?? false) yield break;

                if (!MemoryMeasurement?.IsDocked ?? false) yield break;

                yield return MemoryMeasurement?.WindowStation?.FirstOrDefault()?.UndockButton
                    ?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left);
            }
        }

    }
}
