using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling;
using Sanderling.Parse;
using MemoryStruct=Sanderling.Interface.MemoryStruct;

namespace WPFScanHelper.Parse
{
    public static class ParseExtension
    {
        public static int? CountFromDroneGroupCaption(this string groupCaption) =>
            groupCaption?.RegexMatchIfSuccess(@"\((\d+)\)")?.Groups[1]?.Value?.TryParseInt();

        /// <summary>
        /// Hobgoblin I ( <color=0xFF00FF00>Idle</color> )
        /// </summary>
        const string StatusStringFromDroneEntryTextRegexPattern = @"\((.*)\)";

        public static string StatusStringFromDroneEntryText(this string droneEntryText) =>
            droneEntryText?.RegexMatchIfSuccess(StatusStringFromDroneEntryTextRegexPattern)?.Groups[1]?.Value?.RemoveXmlTag()?.Trim();

        public static bool ManeuverStartPossible(this Sanderling.Parse.IMemoryMeasurement memoryMeasurement) =>
            !(memoryMeasurement?.IsDocked ?? false) &&
            !new[] { ShipManeuverTypeEnum.Warp, ShipManeuverTypeEnum.Jump, ShipManeuverTypeEnum.Docked }.Contains(
                memoryMeasurement?.ShipUi?.Indication?.ManeuverType ?? ShipManeuverTypeEnum.None);

        public static Int64 Width(this RectInt rect) => rect.Side0Length();
        public static Int64 Height(this RectInt rect) => rect.Side1Length();

        public static bool IsScrollable(this MemoryStruct.IScroll scroll) =>
            scroll?.ScrollHandle?.Region.Height() < scroll?.ScrollHandleBound?.Region.Height() - 4;

        public static bool IsNeutralOrEnemy(this MemoryStruct.IChatParticipantEntry participantEntry) =>
            !(participantEntry?.FlagIcon?.Any(flagIcon =>
                  new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation|alliance)", }
                      .Any(goodStandingText => flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);
    }
}
}
