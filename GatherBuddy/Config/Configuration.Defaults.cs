using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using OtterGui;
using Lumina.Excel.Sheets;

namespace GatherBuddy.Config;

public partial class Configuration
{
    public static readonly VirtualKey[] ValidKeys = Dalamud.Keys.GetValidVirtualKeys().Prepend(VirtualKey.NO_KEY).ToArray();

    public const string DefaultIdentifiedGatherableFormat = global::GatherBuddy.Localization.MessageTemplates.Identified;
    public const string DefaultAlarmFormat                = global::GatherBuddy.Localization.MessageTemplates.Alarm;

    public static readonly Dictionary<int, uint> ForegroundColors = Dalamud.GameData.GetExcelSheet<UIColor>()!
        .Where(c => (c.Dark & 0xFF) > 0)
        .ToDictionary(c => (int)c.RowId, c => Functions.ReorderColor(c.Dark));

    public const int DefaultSeColorNames     = 504;
    public const int DefaultSeColorCommands  = 31;
    public const int DefaultSeColorArguments = 546;
    public const int DefaultSeColorAlarm     = 518;
}
