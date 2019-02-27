using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib3;
using Sanderling.Parse;
using BotEngine.Interface;
using Sanderling;
using Sanderling.Motor;
using WPFScanHelper.Bot.Memory;
using WPFScanHelper.Bot.Task;
using WPFScanHelper.Serialization;


namespace WPFScanHelper.Bot
{
    public class Bot
    {
        public static readonly Func<Int64> GetTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt;

        public BotStepInput StepLastInput { private set; get; }

        public PropertyGenTimespanInt64<BotStepResult> StepLastResult { private set; get; }

        int motionId;

        int stepIndex;

        public FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurementAtTime { private set; get; }

        public readonly Sanderling.Accumulator.MemoryMeasurementAccumulator MemoryMeasurementAccu = new Sanderling.Accumulator.MemoryMeasurementAccumulator();

        public readonly OverviewMemory OverviewMemory = new OverviewMemory();

        readonly IDictionary<Int64, int> MouseClickLastStepIndexFromUIElementId = new Dictionary<Int64, int>();

        readonly IDictionary<Sanderling.Accumulation.IShipUiModule, int> ToggleLastStepIndexFromModule = new Dictionary<Sanderling.Accumulation.IShipUiModule, int>();

        public KeyValuePair<Deserialization, Config> ConfigSerialAndStruct { private set; get; }

        public Int64? MouseClickLastAgeStepCountFromUIElement(Sanderling.Interface.MemoryStruct.IUIElement uiElement)
        {
            if (null == uiElement)
                return null;

            var interactionLastStepIndex = MouseClickLastStepIndexFromUIElementId?.TryGetValueNullable(uiElement.Id);

            return stepIndex - interactionLastStepIndex;
        }

        public Int64? ToggleLastAgeStepCountFromModule(Sanderling.Accumulation.IShipUiModule module) =>
            module == null ? null :
                stepIndex - ToggleLastStepIndexFromModule?.TryGetValueNullable(module);

        IEnumerable<IBotTask[]> StepOutputListTaskPath() =>
            ((IBotTask)new BotTask { Component = RootTaskListComponent() })
            ?.EnumeratePathToNodeFromTreeDFirst(node => node?.Component)
            ?.Where(taskPath => (taskPath?.LastOrDefault()).ShouldBeIncludedInStepOutput())
            ?.TakeSubsequenceWhileUnwantedInferenceRuledOut();

        void MemorizeStepInput(BotStepInput input)
        {
            ConfigSerialAndStruct = (input?.ConfigSerial?.String).DeserializeIfDifferent(ConfigSerialAndStruct);

            MemoryMeasurementAtTime = input?.FromProcessMemoryMeasurement?.MapValue(measurement => measurement?.Parse());

            MemoryMeasurementAccu.Accumulate(MemoryMeasurementAtTime);

            OverviewMemory.Aggregate(MemoryMeasurementAtTime);
        }

        void MemorizeStepResult(BotStepResult stepResult)
        {
            var setMotionMouseWaypointUIElement =
                stepResult?.ListMotion
                    ?.Select(motion => motion?.MotionParam)
                    ?.Where(motionParam => 0 < motionParam?.MouseButton?.Count())
                    ?.Select(motionParam => motionParam?.MouseListWaypoint)
                    ?.ConcatNullable()?.Select(mouseWaypoint => mouseWaypoint?.UIElement)?.WhereNotDefault();

            foreach (var mouseWaypointUIElement in setMotionMouseWaypointUIElement.EmptyIfNull())
                MouseClickLastStepIndexFromUIElementId[mouseWaypointUIElement.Id] = stepIndex;
        }

        public BotStepResult Step(BotStepInput input)
        {
            var beginTimeMilli = GetTimeMilli();

            StepLastInput = input;

            Exception exception = null;

            var listMotion = new List<MotionRecommendation>();

            IBotTask[][] outputListTaskPath = null;

            try
            {
                MemorizeStepInput(input);

                outputListTaskPath = StepOutputListTaskPath()?.ToArray();

                foreach (var moduleToggle in outputListTaskPath.ConcatNullable().OfType<ModuleToggleTask>().Select(moduleToggleTask => moduleToggleTask?.module).WhereNotDefault())
                    ToggleLastStepIndexFromModule[moduleToggle] = stepIndex;

                foreach (var taskPath in outputListTaskPath.EmptyIfNull())
                {
                    foreach (var effectParam in (taskPath?.LastOrDefault()?.ApplicableEffects()).EmptyIfNull())
                    {
                        listMotion.Add(new MotionRecommendation
                        {
                            Id = motionId++,
                            MotionParam = effectParam,
                        });
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }

            var stepResult = new BotStepResult
            {
                Exception = exception,
                ListMotion = listMotion?.ToArrayIfNotEmpty(),
                OutputListTaskPath = outputListTaskPath,
            };

            MemorizeStepResult(stepResult);

            StepLastResult = new PropertyGenTimespanInt64<BotStepResult>(stepResult, beginTimeMilli, GetTimeMilli());

            ++stepIndex;

            return stepResult;
        }

        IEnumerable<IBotTask> RootTaskListComponent() =>
            StepLastInput?.RootTaskListComponentOverride ??
            RootTaskListComponentDefault();

        IEnumerable<IBotTask> RootTaskListComponentDefault()
        {
            //yield return new BotTask {Component = EnumerateConfigDiagnostics()};

            //var moduleUnknown = MemoryMeasurementAccu?.ShipUiModule?.FirstOrDefault(m => null == m?.TooltipLast?.Value);
           // yield return new BotTask {Effects = new[] {moduleUnknown?.MouseMove()}};

            yield return new UndockTask {MemoryMeasurement = MemoryMeasurementAtTime?.Value};


        }
       

        IEnumerable<IBotTask> EnumerateConfigDiagnostics()
        {
            var configDeserializeException = ConfigSerialAndStruct.Key?.Exception;

            if (null != configDeserializeException)
                yield return new DiagnosticTask { MessageText = "error parsing configuration: " + configDeserializeException.Message };
            else
            if (null == ConfigSerialAndStruct.Value)
                yield return new DiagnosticTask { MessageText = "warning: no configuration supplied." };
        }
    }

}
