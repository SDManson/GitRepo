using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib3;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.Parse;
using WPFScanHelper.Bot.Task;

namespace WPFScanHelper.Bot
{
    public static class BotExtension
    {
        private static readonly EWarTypeEnum[][] listEwarPriortyGroup = new[]
        {
            new[] {EWarTypeEnum.ECM},
            new[] {EWarTypeEnum.Web},
            new[] {EWarTypeEnum.WarpDisrupt, EWarTypeEnum.WarpScramble},
        };

        public static int AttackPriorityIndexForOverviewEntryEWar(IEnumerable<EWarTypeEnum> setEwar)
        {
            var setEwarRendered = setEwar?.ToArray();

            return listEwarPriortyGroup.FirstIndexOrNull(p => p.ContainsAny(setEwarRendered)) ??
                   (listEwarPriortyGroup.Length + (0 < setEwarRendered?.Length ? 0 : 1));
        }

        public static int AttackPriorityIndex(
            this Bot bot,
            Sanderling.Parse.IOverviewEntry entry) =>
            AttackPriorityIndexForOverviewEntryEWar(bot?.OverviewMemory?.SetEWarTypeFromOverviewEntry(entry));

        public static bool ShouldBeIncludedInStepOutput(this IBotTask task) =>
            (task?.ContainsEffect() ?? false) || task is DiagnosticTask;
        
        public static bool LastContainsEffect(this IEnumerable<IBotTask> listTask) =>
            listTask?.LastOrDefault()?.ContainsEffect() ?? false;

        public static IEnumerable<MotionParam> ApplicableEffects(this IBotTask task) =>
            task?.Effects?.WhereNotDefault();

        public static bool ContainsEffect(this IBotTask task) =>
            0 < task?.ApplicableEffects()?.Count();

        public static IEnumerable<IBotTask[]> TakeSubsequenceWhileUnwantedInferenceRuledOut(this IEnumerable<IBotTask[]> listTaskPath) =>
            listTaskPath
                ?.EnumerateSubsequencesStartingWithFirstElement()
                ?.OrderBy(subsequenceTaskPath => 1 == subsequenceTaskPath?.Count(BotExtension.LastContainsEffect))
                ?.LastOrDefault();

        public static IUIElementText TitleElementText(this IModuleButtonTooltip tooltip)
        {
            var tooltipHorizontalCenter = tooltip?.RegionCenter()?.A;

            var setLabelIntersectingHorizontalCenter =
                tooltip?.LabelText
                    ?.Where(label => label?.Region.Min0 < tooltipHorizontalCenter && tooltipHorizontalCenter < label?.Region.Max0);

            return
                setLabelIntersectingHorizontalCenter
                    ?.OrderByCenterVerticalDown()?.FirstOrDefault();
        }

        public static bool ShouldBeActivePermanent(this Sanderling.Accumulation.IShipUiModule module, Bot bot) =>
            new[]
                {
                    module?.TooltipLast?.Value?.IsHardener,
                    bot?.ConfigSerialAndStruct.Value?.ModuleActivePermanentSetTitlePattern
                        ?.Any(activePermanentTitlePattern => module?.TooltipLast?.Value?.TitleElementText()?.Text?.RegexMatchSuccessIgnoreCase(activePermanentTitlePattern) ?? false),
                }
                .Any(sufficientCondition => sufficientCondition ?? false);
    }


}
