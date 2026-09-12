using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GatherBuddy.Localization;
using GatherBuddy.Enums;
using GatherBuddy.GatherGroup;
using GatherBuddy.Interfaces;
using GatherBuddy.Plugin;
using Newtonsoft.Json;

internal static class LocalizationChecks
{
    public static void Run(Action<bool, string> check)
    {
        using var stream = typeof(Localize).Assembly.GetManifestResourceStream("GatherBuddy.Localization.zh-CN.json")!;
        using var json = JsonDocument.Parse(stream);
        var entries = json.RootElement.EnumerateObject().ToArray();
        check(entries.Select(e => e.Name).Distinct(StringComparer.Ordinal).Count() == entries.Length, "No duplicate resource keys");
        var tokens = new Regex(@"(?<!\{)\{(?:[A-Za-z]+|\d+)(?:,-?\d+)?(?::[^{}]+)?\}(?!\})");
        foreach (var entry in entries)
        {
            var value = entry.Value.GetString()!;
            check(!string.IsNullOrWhiteSpace(value), "Nonempty translation: " + entry.Name);
            check(!value.Contains('\uFFFD'), "Valid UTF-8 translation: " + entry.Name);
            check(tokens.Matches(entry.Name).Select(m => m.Value).Order().SequenceEqual(tokens.Matches(value).Select(m => m.Value).Order()),
                "Preserved placeholders: " + entry.Name);
            check(!entry.Name.Contains("##") && !value.Contains("##"), "Labels keep IDs in code: " + entry.Name);
            if (Regex.IsMatch(entry.Name, @"\{\d"))
                check(CompositeFormat.Parse(entry.Name).MinimumArgumentCount == CompositeFormat.Parse(value).MinimumArgumentCount,
                    "Valid composite format: " + entry.Name);
        }
        check(Localize.Text("Missing English fallback") == "Missing English fallback", "English fallback");
        check(Localize.Text("##hidden") == "##hidden", "Hidden ImGui ID");
        check(Localize.Label("Name##pane") == Localize.Text("Name") + "##pane###Name##pane", "Stable ## identity");
        check(Localize.Label("Name###pane") == Localize.Text("Name") + "###pane", "Stable ### identity");
        check(Localize.Format("Double Hook for {0}-{1} fish", 2, 4) == "双重提钩可获得 2～4 条鱼", "Format complete sentence");
        check(Localize.FormatLabel("Double Hook for {0}-{1} fish", 2, 4).EndsWith("###Double Hook for 2-4 fish"), "Stable dynamic label identity");
        check(Localize.Display("Name") == "Name", "User-authored string stays untouched");
        check(Localize.Display(SpearfishSize.Average) == "中型" && Localize.Display(SpearfishSpeed.Average) == "普通", "Contextual enum translation");
        check(JsonConvert.SerializeObject(HookSet.DoubleHook) == "\"DoubleHook\"", "Serialized hook name unchanged");
        check(Localize.Display(OceanTime.Day | OceanTime.Night) == "夜晚、白天", "Flag display");
        foreach (var type in new[] { typeof(HookSet), typeof(BiteType), typeof(NodeType), typeof(GatheringType), typeof(FishType), typeof(Snagging), typeof(Lure),
                     typeof(SpearfishSize), typeof(SpearfishSpeed), typeof(OceanSpecies), typeof(OceanTime), typeof(FishRestrictions),
                     typeof(GatherBuddy.Config.ItemFilter), typeof(GatherBuddy.Config.FishFilter), typeof(GatherBuddy.Config.JobFlags),
                     typeof(GatherBuddy.FishTimer.FishRecord.Effects), typeof(GatherBuddy.Classes.OceanArea), typeof(Dalamud.Game.Text.XivChatType) })
            foreach (var name in Enum.GetNames(type))
                check(Localize.Entries.ContainsKey($"enum.{type.Name}.{name}"), "Complete enum: " + type.Name + "." + name);
        foreach (var source in new[] { MessageTemplates.Alarm, MessageTemplates.Identified })
        {
            var migrated = MessageTemplates.MigrateDefault(source, source);
            check(migrated != source, "Migrate upstream default");
            check(MessageTemplates.MigrateDefault(migrated, source) == migrated, "Idempotent migration");
            check(MessageTemplates.MigrateDefault("", source) == "", "Protect muted template");
            check(MessageTemplates.MigrateDefault("Custom: " + source, source) == "Custom: " + source, "Protect custom template");
            check(MessageTemplates.MigrateDefault(source + " ", source) == source + " ", "Exact-only migration");
        }
        var config = new TimedGroup.Config
        {
            Name = "自定义 Settings", Description = "保留我的 {Item} 提示",
            Nodes = [new TimedGroupNode.Config { Annotation = "我的备注", ItemId = 2, Type = ObjectType.Gatherable, StartMinute = 120, EndMinute = 240 }],
        };
        var exported = config.ToBase64();
        check(TimedGroup.Config.FromBase64(exported, out var imported), "Import exported preset");
        check(imported.Name == config.Name && imported.Description == config.Description && imported.Nodes[0].Annotation == "我的备注", "Preset custom text preserved");
        var wire = Functions.DecompressedBase64(exported);
        check(wire[0] == 1 && Encoding.UTF8.GetString(wire.AsSpan(1)).Contains("\"Type\":\"Gatherable\""), "Preset wire version and enum preserved");
        check(!TimedGroup.Config.FromBase64("invalid", out _), "Reject invalid preset");
        check(GatherBuddy.GatherBuddy.InternalName == "GatherBuddy", "Plugin configuration identity");
        check(new[] { GatherBuddyIpc.InitializedName, GatherBuddyIpc.DisposedName, GatherBuddyIpc.VersionName, GatherBuddyIpc.VersionNameV2,
                GatherBuddyIpc.IdentifyName, GatherBuddyIpc.DrawFishTooltipName, GatherBuddyIpc.RecordCreatedName, GatherBuddyIpc.QueryUptimesName }
            .SequenceEqual(new[] { "GatherBuddy.Initialized", "GatherBuddy.Disposed", "GatherBuddy.Version", "GatherBuddy.Version.V2",
                "GatherBuddy.Identify", "GatherBuddy.DrawFishToolitp", "GatherBuddy.RecordCreated", "GatherBuddy.QueryUptimes" }), "IPC channel identity including upstream spelling");
        Console.WriteLine($"Resources: {entries.Length} Chinese entries.");
    }
}
