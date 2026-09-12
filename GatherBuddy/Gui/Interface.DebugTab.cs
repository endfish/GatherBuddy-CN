using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game;
using GatherBuddy.Classes;
using GatherBuddy.Enums;
using GatherBuddy.FishTimer;
using GatherBuddy.Levenshtein;
using GatherBuddy.Plugin;
using GatherBuddy.Structs;
using GatherBuddy.Time;
using Lumina.Excel.Sheets;
using OtterGui;
using OtterGui.Log;
using OtterGui.Text;
using OtterGui.Widgets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static GatherBuddy.FishTimer.FishRecord;
using static System.Net.Mime.MediaTypeNames;
using Aetheryte = GatherBuddy.Classes.Aetheryte;
using FishingSpot = GatherBuddy.Classes.FishingSpot;
using ImGuiTable = OtterGui.ImGuiTable;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    [GeneratedRegex(@"(?<Name>.*) \((?<Id>\d{5})\)$", RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking)]
    private static partial Regex CosmicMissionRegex();

    private static uint _startId = 10031;
    private static uint _endId   = 10096;

    private static void DrawDebugAetheryte(Aetheryte a)
    {
        ImGuiUtil.DrawTableColumn(Localize.Display(a.Id));
        ImGuiUtil.DrawTableColumn(a.Name);
        ImGuiUtil.DrawTableColumn(a.Territory.Name);
        ImGuiUtil.DrawTableColumn($"{a.XCoord}-{a.YCoord}");
        ImGuiUtil.DrawTableColumn($"{a.XStream}-{a.YStream}-{a.Plane}");
    }

    private static void DrawDebugTerritory(Territory t)
    {
        ImGuiUtil.DrawTableColumn(Localize.Display(t.Id));
        ImGuiUtil.DrawTableColumn(t.Name);
        ImGuiUtil.DrawTableColumn(t.SizeFactor.ToString(CultureInfo.InvariantCulture));
        ImGuiUtil.DrawTableColumn(Localize.Display(t.WeatherRates.Rates.Length));
        ImGuiUtil.DrawTableColumn(string.Join(", ", t.WeatherRates.Rates.Select(r => $"{r.Weather.Name} ({r.Weather.Id})")));
    }

    private static void DrawDebugBait(Bait b)
    {
        ImGuiUtil.DrawTableColumn(Localize.Display(b.Id));
        ImGuiUtil.DrawTableColumn(b.Name);
    }

    private static void DrawGatherableDebug(Gatherable g)
    {
        ImGuiUtil.DrawTableColumn(Localize.Display(g.ItemId));
        ImGuiUtil.DrawTableColumn(Localize.Display(g.GatheringId));
        ImGuiUtil.DrawTableColumn(g.Name.English);
        ImGuiUtil.DrawTableColumn(g.LevelString());
        ImGuiUtil.DrawTableColumn(Localize.Display(g.NodeList.Count));
    }

    private static void DrawGatheringNodeDebug(GatheringNode n)
    {
        ImGuiUtil.DrawTableColumn(Localize.Display(n.Id));
        ImGuiUtil.DrawTableColumn(n.Name);
        ImGuiUtil.DrawTableColumn(Localize.Display(n.GatheringType));
        ImGuiUtil.DrawTableColumn(Localize.Display(n.Level));
        ImGuiUtil.DrawTableColumn(Localize.Display(n.NodeType));
        ImGuiUtil.DrawTableColumn($"{n.Territory.Name} ({n.Territory.Id})");
        ImGuiUtil.DrawTableColumn($"{n.IntegralXCoord}-{n.IntegralYCoord}");
        ImGuiUtil.DrawTableColumn(n.ClosestAetheryte?.Name ?? Localize.Text("Unknown"));
        ImGuiUtil.DrawTableColumn(n.Folklore);
        ImGuiUtil.DrawTableColumn(n.Times.PrintHours(true));
        ImGuiUtil.DrawTableColumn(n.PrintItems());
    }

    private static void DrawFishDebug(Fish f)
    {
        ImGuiUtil.DrawTableColumn(Localize.Display(f.ItemId));
        ImGuiUtil.DrawTableColumn($"{f.FishId}{(f.IsSpearFish ? Localize.Text(" (sf)") : "")}");
        ImGuiUtil.DrawTableColumn(f.Name.English);
        ImGuiUtil.DrawTableColumn(Localize.Display(f.FishRestrictions));
        ImGuiUtil.DrawTableColumn(f.Folklore);
        ImGuiUtil.DrawTableColumn(Localize.Display(f.InLog));
        ImGuiUtil.DrawTableColumn(Localize.Display(f.IsBigFish));
        ImGuiUtil.DrawTableColumn(string.Join('|', f.FishingSpots.Select(s => s.Name)));
    }

    private static void DrawFishingSpotDebug(FishingSpot s)
    {
        ImGuiUtil.DrawTableColumn($"{s.Id}{(s.Spearfishing ? Localize.Text(" (sf)") : "")}");
        ImGuiUtil.DrawTableColumn(s.Name);
        ImGuiUtil.DrawTableColumn($"{s.Territory.Name} ({s.Territory.Id})");
        ImGuiUtil.DrawTableColumn(s.ClosestAetheryte?.Name ?? Localize.Text("Unknown"));
        ImGuiUtil.DrawTableColumn($"{s.IntegralXCoord / 100f:00.00}-{s.IntegralYCoord / 100f:00.00}");
        ImGuiUtil.DrawTableColumn($"{s.SpearfishingSpotData?.IsShadowNode ?? false}");
        ImGuiUtil.DrawTableColumn(string.Join('|', s.Items.Select(fish => fish.Name)));
    }

    private static void PrintNode<T>(PatriciaTrie<T>.Node node)
    {
        var name = Localize.Display(node.TotalWord);
        if (name.Length == 0)
            name = Localize.Text("Root");
        if (node.Children.Count == 0)
        {
            ImGui.Text(name);
        }
        else
        {
            if (!ImGui.TreeNodeEx(name))
                return;

            foreach (var child in node.Children)
                PrintNode(child);
            ImGui.TreePop();
        }
    }

    private void DrawDebugButtons()
    {
        if (ImGui.CollapsingHeader(Localize.Label("Debug")))
        {
            if (ImGui.Button(Localize.Label("Set Weather Dirty")))
                _weatherTable.SetDirty();
            if (ImGui.Button(Localize.Label("Set Locations Dirty")))
                GatherBuddy.UptimeManager.ResetLocations();
            if(ImGui.Button(Localize.Label("Set All Fish Unlocked")))
                GatherBuddy.FishLog.SetAllUnlocked();

            if (FishTimerWindow.CollectableIcon.TryGetWrap(out var wrapCollectable, out _))
                ImGui.Image(wrapCollectable.Handle, wrapCollectable.Size);

            ImGui.SameLine();
            if (FishTimerWindow.DoubleHookIcon.TryGetWrap(out var wrapDoubleHook, out _))
                ImGui.Image(wrapDoubleHook.Handle, wrapDoubleHook.Size);

            ImGui.SameLine();
            if (FishTimerWindow.TripleHookIcon.TryGetWrap(out var wrapTripleHook, out _))
                ImGui.Image(wrapTripleHook.Handle, wrapTripleHook.Size);

            ImGui.SameLine();
            if (FishTimerWindow.QuadHookIcon.TryGetWrap(out var wrapQuadHook, out _))
                ImGui.Image(wrapQuadHook.Handle, wrapQuadHook.Size);

            ImGui.SameLine();
            if (FishTimerWindow.OctopusIcon.TryGetWrap(out var wrapOctopus, out _))
                ImGui.Image(wrapOctopus.Handle, wrapOctopus.Size);

            ImGui.SameLine();
            if (FishTimerWindow.SharkIcon.TryGetWrap(out var wrapShark, out _))
                ImGui.Image(wrapShark.Handle, wrapShark.Size);

            ImGui.SameLine();
            if (FishTimerWindow.JellyfishIcon.TryGetWrap(out var wrapJellyfish, out _))
                ImGui.Image(wrapJellyfish.Handle, wrapJellyfish.Size);

            ImGui.SameLine();
            if (FishTimerWindow.SeadragonIcon.TryGetWrap(out var wrapSeadragon, out _))
                ImGui.Image(wrapSeadragon.Handle, wrapSeadragon.Size);

            ImGui.SameLine();
            if (FishTimerWindow.FuguIcon.TryGetWrap(out var wrapFugu, out _))
                ImGui.Image(wrapFugu.Handle, wrapFugu.Size);

            ImGui.SameLine();
            if (FishTimerWindow.CrabIcon.TryGetWrap(out var wrapCrab, out _))
                ImGui.Image(wrapCrab.Handle, wrapCrab.Size);

            ImGui.SameLine();
            if (FishTimerWindow.MantaIcon.TryGetWrap(out var wrapManta, out _))
                ImGui.Image(wrapManta.Handle, wrapManta.Size);

            ImGui.SameLine();
            if (FishTimerWindow.ShellfishIcon.TryGetWrap(out var wrapShellfish, out _))
                ImGui.Image(wrapShellfish.Handle, wrapShellfish.Size);

            ImGui.SameLine();
            if (FishTimerWindow.SquidIcon.TryGetWrap(out var wrapSquid, out _))
                ImGui.Image(wrapSquid.Handle, wrapSquid.Size);

            ImGui.SameLine();
            if (FishTimerWindow.ShrimpIcon.TryGetWrap(out var wrapShrimp, out _))
                ImGui.Image(wrapShrimp.Handle, wrapShrimp.Size);
        }
    }

    private static unsafe void DrawDebugTime()
    {
        if (!ImGui.CollapsingHeader(Localize.Label("Time")))
            return;

        using var table = ImRaii.Table("##Times", 2);
        if (!table)
            return;

        var fw = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        ImGuiUtil.DrawTableColumn(Localize.Text("Framework Timestamp"));
        ImGuiUtil.DrawTableColumn(fw == null ? Localize.Text("NULL") : Localize.Display(fw->UtcTime.Timestamp));
        ImGuiUtil.DrawTableColumn(Localize.Text("Framework Eorzea"));
        ImGuiUtil.DrawTableColumn(fw == null ? Localize.Text("NULL") : Localize.Display(fw->ClientTime.EorzeaTime));
        ImGuiUtil.DrawTableColumn(Localize.Text("Framework Func"));
        ImGuiUtil.DrawTableColumn(Localize.Display(FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.GetServerTime()));
        ImGuiUtil.DrawTableColumn(Localize.Text("DateTimeOffset"));
        ImGuiUtil.DrawTableColumn(Localize.Display(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        ImGuiUtil.DrawTableColumn(Localize.Text("GatherBuddy TimeStamp"));
        ImGuiUtil.DrawTableColumn(Localize.Display(GatherBuddy.Time.ServerTime.Time));
        ImGuiUtil.DrawTableColumn(Localize.Text("GatherBuddy EorzeaTime"));
        ImGuiUtil.DrawTableColumn(Localize.Display(GatherBuddy.Time.EorzeaTime.Time));
        ImGuiUtil.DrawTableColumn(Localize.Text("Current Computed Weather"));
        ImGuiUtil.DrawTableColumn(Dalamud.ClientState.TerritoryType != 0
            ? GatherBuddy.WeatherManager.FindLastCurrentNextWeather(Dalamud.ClientState.TerritoryType).Current.Name
            : Localize.Text("None"));
        ImGuiUtil.DrawTableColumn(Localize.Text("Current True Weather"));
        ImGuiUtil.DrawTableColumn(Dalamud.ClientState.TerritoryType != 0
         && GatherBuddy.GameData.Weathers.TryGetValue(WeatherManager.Instance()->GetCurrentWeather(), out var w)
                ? w.Name
                : Localize.Text("None"));
    }

    private static unsafe void DrawDebugFishingState()
    {
        if (!ImGui.CollapsingHeader(Localize.Label("Fishing State")))
            return;

        using var table = ImRaii.Table("##Framework", 2);
        if (!table)
            return;

        ImGuiUtil.DrawTableColumn(Localize.Text("Current Save Changes"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.FishRecorder.Changes));
        ImGuiUtil.DrawTableColumn(Localize.Text("Next Timed Save"));
        ImGuiUtil.DrawTableColumn(_plugin.FishRecorder.SaveTime == TimeStamp.MaxValue
            ? Localize.Text("Never")
            : TimeInterval.DurationString(_plugin.FishRecorder.SaveTime, TimeStamp.UtcNow, false));
        ImGuiUtil.DrawTableColumn(Localize.Text("UiState Address"));
        ImGui.TableNextColumn();
        GatherBuddy.Dynamis.DrawPointer(FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance());
        ImGuiUtil.DrawTableColumn(Localize.Text("FishingEventHandler Address"));
        ImGui.TableNextColumn();
        GatherBuddy.Dynamis.DrawPointer(GatherBuddy.EventFramework.FishingEventHandler);
        ImGuiUtil.DrawTableColumn(Localize.Text("Fishing State"));
        ImGuiUtil.DrawTableColumn(Localize.Display(GatherBuddy.EventFramework.FishingState));
        ImGuiUtil.DrawTableColumn(Localize.Text("Num SwimBait"));
        ImGuiUtil.DrawTableColumn(Localize.Display(GatherBuddy.EventFramework.NumSwimBait));
        ImGuiUtil.DrawTableColumn(Localize.Text("Selected SwimBait"));
        ImGuiUtil.DrawTableColumn(GatherBuddy.EventFramework.CurrentSwimBait?.ToString() ?? Localize.Text("NULL"));
        ImGuiUtil.DrawTableColumn(Localize.Text("SwimBait 1"));
        ImGuiUtil.DrawTableColumn(GatherBuddy.EventFramework.SwimBait(0)?.ToString() ?? Localize.Text("NULL"));
        ImGuiUtil.DrawTableColumn(Localize.Text("SwimBait 2"));
        ImGuiUtil.DrawTableColumn(GatherBuddy.EventFramework.SwimBait(1)?.ToString() ?? Localize.Text("NULL"));
        ImGuiUtil.DrawTableColumn(Localize.Text("SwimBait 3"));
        ImGuiUtil.DrawTableColumn(GatherBuddy.EventFramework.SwimBait(2)?.ToString() ?? Localize.Text("NULL"));
        ImGuiUtil.DrawTableColumn(Localize.Text("Bite Type Address"));
        ImGuiUtil.DrawTableColumn(GatherBuddy.TugType.Address.ToString("X"));
        ImGuiUtil.DrawTableColumn(Localize.Text("Bite Type"));
        ImGuiUtil.DrawTableColumn(Localize.Display(GatherBuddy.TugType.Bite));

        var record = _plugin.FishRecorder.Record;
        ImGuiUtil.DrawTableColumn(Localize.Text("Last Fishing State"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.FishRecorder.LastState));
        ImGuiUtil.DrawTableColumn(Localize.Text("Current Step"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.FishRecorder.Step));
        ImGuiUtil.DrawTableColumn(Localize.Text("ContentIdHash"));
        ImGuiUtil.DrawTableColumn(Localize.Display(record.ContentIdHash));
        ImGuiUtil.DrawTableColumn(Localize.Text("Gathering"));
        ImGuiUtil.DrawTableColumn(Localize.Display(record.Gathering));
        ImGuiUtil.DrawTableColumn(Localize.Text("Perception"));
        ImGuiUtil.DrawTableColumn(Localize.Display(record.Perception));
        ImGuiUtil.DrawTableColumn(Localize.Text("Start Time"));
        ImGuiUtil.DrawTableColumn(Localize.Display((record.TimeStamp / 1000)));
        ImGuiUtil.DrawTableColumn(Localize.Text("Current Spot"));
        ImGuiUtil.DrawTableColumn($"{record.FishingSpot?.Name ?? Localize.Text("Unknown")} ({record.FishingSpot?.Id ?? 0})");
        if (CosmicMissionRegex().Match(record.FishingSpot?.Name ?? string.Empty).Groups[Localize.Text("Id")] is { Success: true, Value: { } mission })
        {
            var id = uint.Parse(mission);
            if (Dalamud.GameData.GetExcelSheet<WKSMissionUnit>().TryGetRow(id, out var row))
            {
                ImGuiUtil.DrawTableColumn(Localize.Text("Current Mission"));
                ImGuiUtil.DrawTableColumn($"{row.Name.ExtractText()} ({id})");
            }
        }

        ImGuiUtil.DrawTableColumn(Localize.Text("Selected Bait"));
        var baitId = GatherBuddy.CurrentBait.Current;
        ImGuiUtil.DrawTableColumn($"{GatherBuddy.GameData.Bait.GetValueOrDefault(baitId, Bait.Unknown).Name} ({baitId})");
        ImGuiUtil.DrawTableColumn(Localize.Text("Current Bait"));
        ImGuiUtil.DrawTableColumn($"{record.Bait.Name} ({record.Bait.Id})");
        ImGuiUtil.DrawTableColumn(Localize.Text("Duration"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.FishRecorder.Timer.ElapsedMilliseconds));
        ImGuiUtil.DrawTableColumn(Localize.Text("BiteType"));
        ImGuiUtil.DrawTableColumn(Localize.Display(record.Tug));
        ImGuiUtil.DrawTableColumn(Localize.Text("HookSet"));
        ImGuiUtil.DrawTableColumn(Localize.Display(record.Hook));
        ImGuiUtil.DrawTableColumn(Localize.Text("Last Catch"));
        ImGuiUtil.DrawTableColumn(
            $"{_plugin.FishRecorder.LastCatch?.Name[ClientLanguage.English] ?? Localize.Text("None")} ({_plugin.FishRecorder.LastCatch?.ItemId ?? 0} - {_plugin.FishRecorder.LastCatch?.FishId ?? 0})");
        ImGuiUtil.DrawTableColumn(Localize.Text("Current Catch"));
        ImGuiUtil.DrawTableColumn(
            Localize.Format("{0} ({1} - {2}) - of size {3} times {4}", record.Catch?.Name[ClientLanguage.English] ?? Localize.Text("None"), record.Catch?.ItemId ?? 0, record.Catch?.FishId ?? 0, record.Size / 10f, record.Amount));
        foreach (var flag in Enum.GetValues<Effects>())
        {
            ImGuiUtil.DrawTableColumn(Localize.Display(flag));
            ImGuiUtil.DrawTableColumn(Localize.Display(record.Flags.HasFlag(flag)));
        }
    }

    private unsafe void DrawDebugFishingTimes()
    {
        if (!ImGui.CollapsingHeader(Localize.Label("Fishing Times")))
            return;

        using var table = ImRaii.Table("##Fishing Times", 6);
        if (!table)
            return;

        foreach (var (fishId, data) in _plugin.FishRecorder.Times)
        {
            ImGuiUtil.DrawTableColumn(GatherBuddy.GameData.Fishes[fishId].Name[ClientLanguage.English]);
            ImGuiUtil.DrawTableColumn(Localize.Text("Overall"));
            ImGuiUtil.DrawTableColumn(Localize.Display(data.All.Min));
            ImGuiUtil.DrawTableColumn(Localize.Display(data.All.Max));
            ImGuiUtil.DrawTableColumn(Localize.Display(data.All.MinChum));
            ImGuiUtil.DrawTableColumn(Localize.Display(data.All.MaxChum));
            foreach (var (baitId, times) in data.Data)
            {
                var bait = GatherBuddy.GameData.Bait.TryGetValue(baitId, out var b)
                    ? b
                    : new Bait(GatherBuddy.GameData.Fishes[fishId].ItemData);
                ImGui.TableNextColumn();
                ImGuiUtil.DrawTableColumn(bait.Name);
                ImGuiUtil.DrawTableColumn(Localize.Display(times.Min));
                ImGuiUtil.DrawTableColumn(Localize.Display(times.Max));
                ImGuiUtil.DrawTableColumn(Localize.Display(times.MinChum));
                ImGuiUtil.DrawTableColumn(Localize.Display(times.MaxChum));
            }
        }
    }

    private static void DrawUptimeManagerTable()
    {
        if (!ImGui.CollapsingHeader(Localize.FormatLabel("Uptimes ({0})", GatherBuddy.GameData.TimedGatherables)))
            return;

        using var table = ImRaii.Table("##Uptimes", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table)
            return;

        foreach (var item in GatherBuddy.GameData.Gatherables.Values)
        {
            if (item.InternalLocationId == 0)
                continue;

            ImGuiUtil.DrawTableColumn(Math.Abs(item.InternalLocationId).ToString("0000"));
            ImGuiUtil.DrawTableColumn(item.Name[ClientLanguage.English]);
            ImGuiUtil.DrawTableColumn(Localize.Display(item.NodeList.Count));
            var (loc, time) = GatherBuddy.UptimeManager.BestLocation(item);
            ImGuiUtil.DrawTableColumn(loc.Name);
            if (item.InternalLocationId > 0)
            {
                if (time == TimeInterval.Invalid)
                {
                    ImGuiUtil.DrawTableColumn(Localize.Text("Invalid"));
                    ImGuiUtil.DrawTableColumn(string.Empty);
                }
                else if (time == TimeInterval.Never)
                {
                    ImGuiUtil.DrawTableColumn(Localize.Text("Never"));
                    ImGuiUtil.DrawTableColumn(string.Empty);
                }
                else
                {
                    ImGuiUtil.DrawTableColumn(Localize.Display(time.Start));
                    ImGuiUtil.DrawTableColumn(Localize.Display(time.End));
                }
            }
            else
            {
                ImGuiUtil.DrawTableColumn(Localize.Text("Always"));
                ImGui.TableNextColumn();
            }
        }

        foreach (var fish in GatherBuddy.GameData.Fishes.Values)
        {
            if (fish.InternalLocationId == 0)
                continue;

            ImGuiUtil.DrawTableColumn(Math.Abs(fish.InternalLocationId).ToString("0000"));
            ImGuiUtil.DrawTableColumn(fish.Name[ClientLanguage.English]);
            ImGuiUtil.DrawTableColumn(Localize.Display(fish.FishingSpots.Count));
            var (loc, time) = GatherBuddy.UptimeManager.BestLocation(fish);
            ImGuiUtil.DrawTableColumn(loc.Name);
            if (fish.InternalLocationId > 0)
            {
                if (time == TimeInterval.Invalid)
                {
                    ImGuiUtil.DrawTableColumn(Localize.Text("Invalid"));
                    ImGuiUtil.DrawTableColumn(string.Empty);
                }
                else if (time == TimeInterval.Never)
                {
                    ImGuiUtil.DrawTableColumn(Localize.Text("Never"));
                    ImGuiUtil.DrawTableColumn(string.Empty);
                }
                else
                {
                    ImGuiUtil.DrawTableColumn(Localize.Display(time.Start));
                    ImGuiUtil.DrawTableColumn(Localize.Display(time.End));
                }
            }
            else
            {
                ImGuiUtil.DrawTableColumn(Localize.Text("Always"));
                ImGui.TableNextColumn();
            }
        }
    }

    private void DrawAlarmDebug()
    {
        if (!ImGui.CollapsingHeader(Localize.Label("Alarms##AlarmDebug")))
            return;

        using var table = ImRaii.Table("##Alarms", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table)
            return;

        var nextAlarm = _plugin.AlarmManager.ActiveAlarms.Count > 0 ? _plugin.AlarmManager.ActiveAlarms[0].Item2 : TimeStamp.Epoch;
        var (abs, rel) = nextAlarm != TimeStamp.Epoch
            ? (nextAlarm.LocalTime.ToString(CultureInfo.InvariantCulture),
                TimeInterval.DurationString(nextAlarm, GatherBuddy.Time.ServerTime, false))
            : (Localize.Text("Never"), Localize.Text("Never"));

        ImGuiUtil.DrawTableColumn(Localize.Text("Enabled"));
        ImGuiUtil.DrawTableColumn(Localize.Display(GatherBuddy.Config.AlarmsEnabled));
        ImGuiUtil.DrawTableColumn(Localize.Text("Dirty"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.AlarmManager.Dirty));
        ImGuiUtil.DrawTableColumn(Localize.Text("Next Change (Absolute)"));
        ImGuiUtil.DrawTableColumn(abs);
        ImGuiUtil.DrawTableColumn(Localize.Text("Next Change (Relative)"));
        ImGuiUtil.DrawTableColumn(rel);
        ImGuiUtil.DrawTableColumn(Localize.Text("#Alarm Groups"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.AlarmManager.Alarms.Count));
        ImGuiUtil.DrawTableColumn(Localize.Text("#Enabled Alarms"));
        ImGuiUtil.DrawTableColumn(Localize.Display(_plugin.AlarmManager.ActiveAlarms.Count));
        foreach (var (alarm, state) in _plugin.AlarmManager.ActiveAlarms)
        {
            ImGuiUtil.DrawTableColumn(alarm.Name.Any() ? alarm.Name : alarm.Item.Name[ClientLanguage.English]);
            ImGuiUtil.DrawTableColumn($"{state} ({TimeInterval.DurationString(state, GatherBuddy.Time.ServerTime, false)})");
        }
    }

    private string _identifyTest       = string.Empty;
    private uint   _lastItemIdentified = 0;

    private void DrawWaymarkTab()
    {
        if (!ImGui.CollapsingHeader(Localize.Label("Waymarks##WaymarkDebug")))
            return;

        ImGui.TextUnformatted(Localize.Format("Waymark Manager: 0x{0:X}", GatherBuddy.WaymarkManager.Address));
        ImGui.TextUnformatted(
            Localize.Format("Waymark Manager Offset: +0x{0:X}", (ulong)GatherBuddy.WaymarkManager.Address - (ulong)Dalamud.SigScanner.Module.BaseAddress));
        using var table = ImRaii.Table("##Waymarks", 9, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table)
            return;

        for (var i = 0; i < GatherBuddy.WaymarkManager.Count; ++i)
        {
            using var id      = ImRaii.PushId(i);
            var       waymark = GatherBuddy.WaymarkManager[i];
            ImGui.TableNextColumn();
            if (ImGui.Button(Localize.Label("Clear")))
                GatherBuddy.WaymarkManager.ClearWaymark(i);
            ImGui.TableNextColumn();
            if (ImGui.Button(Localize.Label("Set")))
                GatherBuddy.WaymarkManager.SetWaymark(i);
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.Active));
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.Position.X));
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.Position.Y));
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.Position.Z));
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.X));
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.Y));
            ImGuiUtil.DrawTableColumn(Localize.Display(waymark.Z));
        }
    }

    private static void DrawOceanTab()
    {
        if (!ImGui.CollapsingHeader(Localize.Label("Ocean Routes##OceanDebug")))
            return;

        using (var table = ImRaii.Table("##Ocean", 9, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersOuter))
        {
            if (table)
            {
                ImGui.TableSetupColumn(Localize.Text("Route"));
                ImGui.TableSetupColumn(Localize.Text("Time"));
                ImGui.TableSetupColumn(Localize.Text("Area"));
                ImGui.TableSetupColumn(Localize.Text("Spot 1 Normal"));
                ImGui.TableSetupColumn(Localize.Text("Spot 1 Spectral"));
                ImGui.TableSetupColumn(Localize.Text("Spot 2 Normal"));
                ImGui.TableSetupColumn(Localize.Text("Spot 2 Spectral"));
                ImGui.TableSetupColumn(Localize.Text("Spot 3 Normal"));
                ImGui.TableSetupColumn(Localize.Text("Spot 3 Spectral"));
                ImGui.TableHeadersRow();
                foreach (var route in GatherBuddy.GameData.OceanRoutes)
                {
                    ImGuiUtil.DrawTableColumn(Localize.Display(route));
                    ImGuiUtil.DrawTableColumn(Localize.Display(route.StartTime));
                    ImGuiUtil.DrawTableColumn(Localize.Display(route.Area));
                    ImGuiUtil.DrawTableColumn(route.GetSpots(0).Normal.Name);
                    ImGuiUtil.DrawTableColumn(route.GetSpots(0).Spectral.Name);
                    ImGuiUtil.DrawTableColumn(route.GetSpots(1).Normal.Name);
                    ImGuiUtil.DrawTableColumn(route.GetSpots(1).Spectral.Name);
                    ImGuiUtil.DrawTableColumn(route.GetSpots(2).Normal.Name);
                    ImGuiUtil.DrawTableColumn(route.GetSpots(2).Spectral.Name);
                }
            }
        }

        using (var table = ImRaii.Table("##OceanTimeline", 9,
                   ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersOuter))
        {
            if (table)
            {
                ImGui.TableSetupColumn("#"u8);
                ImGui.TableSetupColumn(Localize.Text("Aldenard"));
                ImGui.TableSetupColumn(Localize.Text("Spot 1###A"));
                ImGui.TableSetupColumn(Localize.Text("Spot 2###A"));
                ImGui.TableSetupColumn(Localize.Text("Spot 3###A"));
                ImGui.TableSetupColumn(Localize.Text("Othard"));
                ImGui.TableSetupColumn(Localize.Text("Spot 1###O"));
                ImGui.TableSetupColumn(Localize.Text("Spot 2###O"));
                ImGui.TableSetupColumn(Localize.Text("Spot 3###O"));
                ImGui.TableHeadersRow();
                for (var idx = 0; idx < GatherBuddy.GameData.OceanTimeline.Count; ++idx)
                {
                    var routeAldenard = GatherBuddy.GameData.OceanTimeline[OceanArea.Aldenard][idx];
                    var routeOthard   = GatherBuddy.GameData.OceanTimeline[OceanArea.Othard][idx];
                    ImGuiUtil.DrawTableColumn(Localize.Display(idx));
                    ImGuiUtil.DrawTableColumn(Localize.Display(routeAldenard));
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(ImGuiCol.TableHeaderBg));
                    ImGuiUtil.DrawTableColumn(routeAldenard.GetSpots(0).Normal.Name);
                    ImGuiUtil.DrawTableColumn(routeAldenard.GetSpots(1).Normal.Name);
                    ImGuiUtil.DrawTableColumn(routeAldenard.GetSpots(2).Normal.Name);
                    ImGuiUtil.DrawTableColumn(Localize.Display(routeOthard));
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(ImGuiCol.TableHeaderBg));
                    ImGuiUtil.DrawTableColumn(routeOthard.GetSpots(0).Normal.Name);
                    ImGuiUtil.DrawTableColumn(routeOthard.GetSpots(1).Normal.Name);
                    ImGuiUtil.DrawTableColumn(routeOthard.GetSpots(2).Normal.Name);
                }
            }
        }
    }

    private static void DrawCosmicTab()
    {
        if (!ImUtf8.CollapsingHeader(Localize.Label("Cosmic Exploration Fishing Missions##CosmicDebug")))
            return;

        using (var table = ImUtf8.Table("##Cosmic", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
        {
            if (table)
                foreach (var mission in GatherBuddy.GameData.CosmicFishingMissions.Values.OrderBy(m => m.Id))
                {
                    ImUtf8.DrawTableColumn($"{mission.Id}");
                    ImUtf8.DrawTableColumn(mission.Name);
                }
        }
    }

    private class TerritoryFilterCombo()
        : CnFilterComboCache<Territory>(() => GatherBuddy.GameData.Territories.Values.ToList(), MouseWheelType.Control, GatherBuddy.Log)
    {
        protected override string ToString(Territory obj)
            => $"{obj.Name} ({obj.Id})";
    }

    private class WeatherFilterCombo()
        : CnFilterComboCache<string>(() => GatherBuddy.GameData.Weathers.Values.Select(w => w.Name).Distinct().ToList(), MouseWheelType.Control,
            GatherBuddy.Log)
    {
        protected override string ToString(string obj)
            => obj;
    }

    private class FishBaitCombo()
        : CnFilterComboCache<FishBaitCombo.StringId>(
            () => GatherBuddy.GameData.Fishes.Values.Select(f => new StringId(f.Name.English, f.ItemId, true))
                .Concat(GatherBuddy.GameData.Bait.Values.Select(b => new StringId(b.Name, b.Id, false))).ToList(), MouseWheelType.Control,
            GatherBuddy.Log)
    {
        public record StringId(string Name, uint Id, bool Mooch);

        protected override string ToString(StringId obj)
            => obj.Name;
    }

    private readonly TerritoryFilterCombo _territoryCombo = new();
    private readonly WeatherFilterCombo   _weatherCombo   = new();
    private readonly FishBaitCombo        _fishBaitCombo  = new();

    private void DrawDebugFishHelper()
    {
        _territoryCombo.Draw("##Territory", _territoryCombo.CurrentSelection?.Name ?? Localize.Text("Choose Territory"), string.Empty,
            300 * ImUtf8.GlobalScale,
            ImUtf8.TextHeightSpacing);
        ImGui.SameLine();
        _weatherCombo.Draw("##Weather", _weatherCombo.CurrentSelection ?? Localize.Text("Choose Weather"), string.Empty, 150 * ImUtf8.GlobalScale,
            ImUtf8.TextHeightSpacing);
        if (_territoryCombo.CurrentSelection is { } territory && _weatherCombo.CurrentSelection is { Length: > 0 } weather)
        {
            ImGui.SameLine();
            var weathers = territory.WeatherRates.Rates.Where(w => w.Weather.Name == _weatherCombo.CurrentSelection).Select(w => w.Weather.Id).ToList();
            if (weather.Length > 0)
            {
                var text = string.Join(", ", weathers);
                ImUtf8.Text(text);
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    ImGui.SetClipboardText(text);
            }
            else
            {
                ImUtf8.Text(Localize.Text("Territory does not support this weather."));
            }
        }

        _fishBaitCombo.Draw("##fish", _fishBaitCombo.CurrentSelection?.Name ?? Localize.Text("Choose Fish or Bait"), string.Empty, 300 * ImUtf8.GlobalScale,
            ImUtf8.TextHeightSpacing);
        if (_fishBaitCombo.CurrentSelection is { } fish)
        {
            ImGui.SameLine();
            var text = $"{fish.Id}";
            ImUtf8.Text(text);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                ImGui.SetClipboardText(text);
            if (fish.Mooch)
            {
                ImGui.SameLine();
                ImUtf8.Text(Localize.Text("(Mooch)"));
            }
        }
    }

    private void DrawDebugTab()
    {
        if (!GatherBuddy.DebugMode)
            return;

        using var id  = ImRaii.PushId("Debug");
        using var tab = ImRaii.TabItem(Localize.Label("Debug"));
        ImGuiUtil.HoverTooltip(Localize.Text("I really hope there is a good reason for you seeing this."));

        if (!tab)
            return;

        DrawDebugFishHelper();

        using var child = ImRaii.Child(string.Empty);
        if (!child)
            return;

        const ImGuiTableFlags flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit;

        DrawDebugButtons();
        DrawDebugTime();
        DrawDebugFishingState();
        DrawDebugFishingTimes();
        DrawAlarmDebug();
        ImGuiTable.DrawTabbedTable(Localize.Format("Aetherytes ({0})", GatherBuddy.GameData.Aetherytes.Count), GatherBuddy.GameData.Aetherytes.Values,
            DrawDebugAetheryte, flags, Localize.Text("Id"), Localize.Text("Name"), Localize.Text("Territory"), Localize.Text("Coords"), Localize.Text("Aetherstream"));
        ImGuiTable.DrawTabbedTable(Localize.Format("Territories ({0})", GatherBuddy.GameData.WeatherTerritories.Length), GatherBuddy.GameData.WeatherTerritories,
            DrawDebugTerritory, flags, Localize.Text("Id"), Localize.Text("Name"), Localize.Text("SizeFactor"), Localize.Text("#Weathers"), Localize.Text("Weathers"));
        ImGuiTable.DrawTabbedTable(Localize.Format("Bait ({0})", GatherBuddy.GameData.Bait.Count), GatherBuddy.GameData.Bait.Values,
            DrawDebugBait, flags, Localize.Text("Id"), Localize.Text("Name"));
        ImGuiTable.DrawTabbedTable(Localize.Format("Gatherables ({0})", GatherBuddy.GameData.Gatherables.Count),
            GatherBuddy.GameData.Gatherables.Values.OrderBy(g => g.ItemId),
            DrawGatherableDebug, flags, Localize.Text("ItemId"), Localize.Text("GatheringId"), Localize.Text("Name"), Localize.Text("Level"), Localize.Text("#Nodes"));
        ImGuiTable.DrawTabbedTable(Localize.Format("Gathering Nodes ({0})", GatherBuddy.GameData.GatheringNodes.Count), GatherBuddy.GameData.GatheringNodes.Values,
            DrawGatheringNodeDebug, flags, Localize.Text("Id"), Localize.Text("Name"), Localize.Text("Job"), Localize.Text("Level"), Localize.Text("Type"), Localize.Text("Territory"), Localize.Text("Coords"), Localize.Text("Aetheryte"), Localize.Text("Folklore"), Localize.Text("Times"),
            Localize.Text("Items"));
        ImGuiTable.DrawTabbedTable(Localize.Format("Fish ({0})", GatherBuddy.GameData.Fishes.Count), GatherBuddy.GameData.Fishes.Values,
            DrawFishDebug, flags, Localize.Text("ItemId"), Localize.Text("FishId"), Localize.Text("Name"), Localize.Text("Restrictions"), Localize.Text("Folklore"), Localize.Text("InLog"), Localize.Text("Big"), Localize.Text("Fishing Spots"));
        ImGuiTable.DrawTabbedTable(Localize.Format("Fishing Spots ({0})", GatherBuddy.GameData.FishingSpots.Count), GatherBuddy.GameData.FishingSpots.Values,
            DrawFishingSpotDebug, flags, Localize.Text("Id"), Localize.Text("Name"), Localize.Text("Territory"), Localize.Text("Aetheryte"), Localize.Text("Coords"), Localize.Text("Shadow"), Localize.Text("Fishes"));
        DrawUptimeManagerTable();
        DrawOceanTab();
        DrawCosmicTab();
        DrawWaymarkTab();
        if (ImGui.CollapsingHeader(Localize.Label("GatheringTree")))
        {
            id.Push(Localize.Text("GatheringTree"));
            PrintNode(GatherBuddy.GameData.GatherablesTrie.Root);
            id.Pop();
        }

        if (ImGui.CollapsingHeader(Localize.Label("FishingTree")))
        {
            id.Push(Localize.Text("FishingTree"));
            PrintNode(GatherBuddy.GameData.FishTrie.Root);
            id.Pop();
        }

        if (ImGui.CollapsingHeader("IPC"))
        {
            using (var group1 = ImRaii.Group())
            {
                ImGui.Text(Localize.Text("Version"));
                ImGui.Text(GatherBuddyIpc.VersionName);
                ImGui.Text(GatherBuddyIpc.IdentifyName);
                if (_plugin.Ipc._identifyProvider != null && ImGui.InputTextWithHint("##IPCIdentifyTest", Localize.Text("Identify..."), ref _identifyTest, 64))
                    _lastItemIdentified = Dalamud.PluginInterface.GetIpcSubscriber<string, uint>(GatherBuddyIpc.IdentifyName)
                        .InvokeFunc(_identifyTest);
            }

            ImGui.SameLine();
            using var group2 = ImRaii.Group();
            ImGui.Text(Localize.Display(GatherBuddyIpc.IpcVersion));
            ImGui.Text(_plugin.Ipc._versionProvider != null ? Localize.Text("Available") : Localize.Text("Unavailable"));
            ImGui.Text(_plugin.Ipc._identifyProvider != null ? Localize.Text("Available") : Localize.Text("Unavailable"));
            ImGui.Text(Localize.Display(_lastItemIdentified));
        }

        DrawCosmicFishDataButton();
    }

    private static void DrawCosmicFishDataButton()
    {
        ImGui.PushItemWidth(100);
        ImUtf8.InputScalar(Localize.Format("Start ID: {0}###startid", GatherBuddy.GameData.FishingSpots.GetValueOrDefault(_startId)?.Name), ref _startId);
        ImUtf8.InputScalar(Localize.Format("End ID: {0}###endid", GatherBuddy.GameData.FishingSpots.GetValueOrDefault(_endId)?.Name),       ref _endId);
        ImGui.PopItemWidth();

        if (!ImUtf8.Button(Localize.Label("Copy Most Recent Unknown Fish Data")))
            return;

        var patch = $"{nameof(Patch)}.{Enum.GetValues<Patch>().Last()}";
        var text  = "";
        foreach (var spot in GatherBuddy.GameData.FishingSpots.Values)
        {
            if (spot.Id < _startId || spot.Id > _endId)
                continue;

            if (spot.Items.Length is 0)
                continue;

            var  match     = CosmicMissionRegex().Match(spot.Name);
            uint missionId = 0;
            var  name      = spot.Name;
            if (match.Success)
            {
                var spotName = match.Groups[1].Value;
                missionId = uint.Parse(match.Groups[2].Value);
                name = spotName
                  + " "
                  + (Dalamud.GameData.GetExcelSheet<WKSMissionUnit>().GetRowOrDefault(missionId)?.Name.ExtractText() ?? Localize.Text("Unknown"));
            }

            text += $"\n        // {name}\n";
            foreach (var fish in spot.Items)
            {
                text += $"        data.Apply({fish.ItemId}, {patch}) // {fish.Name}\n";
                text += "            .Bait(data)\n";
                if (missionId is not 0)
                    text += $"            .Mission(data, {missionId})\n";
                text += "            .Bite(data, HookSet.Unknown, BiteType.Unknown);\n";
            }
        }

        ImGui.SetClipboardText(text);
    }
}
