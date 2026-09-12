using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using GatherBuddy.Alarms;
using GatherBuddy.Config;
using GatherBuddy.Enums;
using GatherBuddy.FishTimer;
using OtterGui;
using OtterGui.Widgets;
using ImRaii = OtterGui.Raii.ImRaii;

namespace GatherBuddy.Gui;

public partial class Interface
{
    private static class ConfigFunctions
    {
        public static Interface _base = null!;

        public static void DrawSetInput(string jobName, string oldName, Action<string> setName)
        {
            var tmp = oldName;
            ImGui.SetNextItemWidth(SetInputWidth);
            if (ImGui.InputText(Localize.FormatLabel("{0} Set", jobName), ref tmp, 15) && tmp != oldName)
            {
                setName(tmp);
                GatherBuddy.Config.Save();
            }

            ImGuiUtil.HoverTooltip(Localize.Format("Set the name of your {0} set. Can also be the numerical id instead.", jobName.ToLowerInvariant()));
        }

        private static void DrawCheckbox(string label, string description, bool oldValue, Action<bool> setter)
        {
            if (ImGuiUtil.Checkbox(label, description, oldValue, setter))
                GatherBuddy.Config.Save();
        }

        private static void DrawChatTypeSelector(string label, string description, XivChatType currentValue, Action<XivChatType> setter)
        {
            ImGui.SetNextItemWidth(SetInputWidth);
            if (CnWidget.DrawChatTypeSelector(label, description, currentValue, setter))
                GatherBuddy.Config.Save();
        }


        // General Config
        public static void DrawOpenOnStartBox()
            => DrawCheckbox(Localize.Label("Open Config UI On Start"),
                Localize.Text("Toggle whether the GatherBuddy GUI should be visible after you start the game."),
                GatherBuddy.Config.OpenOnStart, b => GatherBuddy.Config.OpenOnStart = b);

        public static void DrawLockPositionBox()
            => DrawCheckbox(Localize.Label("Lock Config UI Movement"),
                Localize.Text("Toggle whether the GatherBuddy GUI movement should be locked."),
                GatherBuddy.Config.MainWindowLockPosition, b =>
                {
                    GatherBuddy.Config.MainWindowLockPosition = b;
                    _base.UpdateFlags();
                });

        public static void DrawLockResizeBox()
            => DrawCheckbox(Localize.Label("Lock Config UI Size"),
                Localize.Text("Toggle whether the GatherBuddy GUI size should be locked."),
                GatherBuddy.Config.MainWindowLockResize, b =>
                {
                    GatherBuddy.Config.MainWindowLockResize = b;
                    _base.UpdateFlags();
                });

        public static void DrawRespectEscapeBox()
            => DrawCheckbox(Localize.Label("Escape Closes Main Window"),
                Localize.Text("Toggle whether pressing escape while having the main window focused shall close it."),
                GatherBuddy.Config.CloseOnEscape, b =>
                {
                    GatherBuddy.Config.CloseOnEscape = b;
                    _base.UpdateFlags();
                });

        public static void DrawGearChangeBox()
            => DrawCheckbox(Localize.Label("Enable Gear Change"),
                Localize.Text("Toggle whether to automatically switch gear to the correct job gear for a node.\nUses Miner Set, Botanist Set and Fisher Set."),
                GatherBuddy.Config.UseGearChange, b => GatherBuddy.Config.UseGearChange = b);

        public static void DrawTeleportBox()
            => DrawCheckbox(Localize.Label("Enable Teleport"),
                Localize.Text("Toggle whether to automatically teleport to a chosen node."),
                GatherBuddy.Config.UseTeleport, b => GatherBuddy.Config.UseTeleport = b);

        public static void DrawMapOpenBox()
            => DrawCheckbox(Localize.Label("Open Map With Location"),
                Localize.Text("Toggle whether to automatically open the map of the territory of the chosen node with its gathering location highlighted."),
                GatherBuddy.Config.UseCoordinates, b => GatherBuddy.Config.UseCoordinates = b);

        public static void DrawPlaceMarkerBox()
            => DrawCheckbox(Localize.Label("Place Flag Marker on Map"),
                Localize.Text("Toggle whether to automatically set a red flag marker on the approximate location of the chosen node without opening the map."),
                GatherBuddy.Config.UseFlag, b => GatherBuddy.Config.UseFlag = b);

        public static void DrawMapMarkerPrintBox()
            => DrawCheckbox(Localize.Label("Print Map Location"),
                Localize.Text("Toggle whether to automatically write a map link to the approximate location of the chosen node to chat."),
                GatherBuddy.Config.WriteCoordinates, b => GatherBuddy.Config.WriteCoordinates = b);

        public static void DrawPlaceWaymarkBox()
            => DrawCheckbox(Localize.Label("Place Custom Waymarks"),
                Localize.Text("Toggle whether to place custom Waymarks you set manually set up for certain locations."),
                GatherBuddy.Config.PlaceCustomWaymarks, b => GatherBuddy.Config.PlaceCustomWaymarks = b);

        public static void DrawPrintUptimesBox()
            => DrawCheckbox(Localize.Label("Print Node Uptimes On Gather"),
                Localize.Text("Print the uptimes of nodes you try to /gather in the chat if they are not always up."),
                GatherBuddy.Config.PrintUptime, b => GatherBuddy.Config.PrintUptime = b);

        public static void DrawSkipTeleportBox()
            => DrawCheckbox(Localize.Label("Skip Nearby Teleports"),
                Localize.Text("Skips teleports if you are in the same map and closer to the target than the selected aetheryte already."),
                GatherBuddy.Config.SkipTeleportIfClose, b => GatherBuddy.Config.SkipTeleportIfClose = b);

        public static void DrawShowStatusLineBox()
            => DrawCheckbox(Localize.Label("Show Status Line"),
                Localize.Text("Show a status line below the gatherables and fish tables."),
                GatherBuddy.Config.ShowStatusLine, v => GatherBuddy.Config.ShowStatusLine = v);

        public static void DrawHideClippyBox()
            => DrawCheckbox(Localize.Label("Hide GatherClippy Button"),
                Localize.Text("Permanently hide the GatherClippy Button in the Gatherables and Fish tabs."),
                GatherBuddy.Config.HideClippy, v => GatherBuddy.Config.HideClippy = v);

        private static readonly string ChatInformationString =
            Localize.Text("Note that the message only gets printed to your chat log, regardless of the selected channel - other people will not see your 'Say' message.");

        public static void DrawPrintTypeSelector()
            => DrawChatTypeSelector(Localize.Label("Chat Type for Messages"),
                Localize.Text("The chat type used to print regular messages issued by GatherBuddy.\n")
              + ChatInformationString,
                GatherBuddy.Config.ChatTypeMessage, t => GatherBuddy.Config.ChatTypeMessage = t);

        public static void DrawErrorTypeSelector()
            => DrawChatTypeSelector(Localize.Label("Chat Type for Errors"),
                Localize.Text("The chat type used to print error messages issued by GatherBuddy.\n")
              + ChatInformationString,
                GatherBuddy.Config.ChatTypeError, t => GatherBuddy.Config.ChatTypeError = t);

        public static void DrawContextMenuBox()
            => DrawCheckbox(Localize.Label("Add In-Game Context Menus"),
                Localize.Text("Add a 'Gather' entry to in-game right-click context menus for gatherable items."),
                GatherBuddy.Config.AddIngameContextMenus, b =>
                {
                    GatherBuddy.Config.AddIngameContextMenus = b;
                    if (b)
                        _plugin.ContextMenu.Enable();
                    else
                        _plugin.ContextMenu.Disable();
                });

        public static void DrawPreferredJobSelect()
        {
            var v       = GatherBuddy.Config.PreferredGatheringType;
            var current = v == GatheringType.Multiple ? Localize.Text("No Preference") : Localize.Display(v);
            ImGui.SetNextItemWidth(SetInputWidth);
            using var combo = ImRaii.Combo(Localize.Label("Preferred Job"), current);
            ImGuiUtil.HoverTooltip(
                Localize.Text("Choose your job preference when gathering items that can be gathered by miners as well as botanists.\nThis effectively turns the regular gather command to /gathermin or /gatherbtn when an item can be gathered by both, ignoring the other options even on successive tries."));
            if (!combo)
                return;

            if (ImGui.Selectable(Localize.Label("No Preference"), v == GatheringType.Multiple) && v != GatheringType.Multiple)
            {
                GatherBuddy.Config.PreferredGatheringType = GatheringType.Multiple;
                GatherBuddy.Config.Save();
            }

            if (ImGui.Selectable(Localize.Display(GatheringType.Miner), v == GatheringType.Miner) && v != GatheringType.Miner)
            {
                GatherBuddy.Config.PreferredGatheringType = GatheringType.Miner;
                GatherBuddy.Config.Save();
            }

            if (ImGui.Selectable(Localize.Display(GatheringType.Botanist), v == GatheringType.Botanist) && v != GatheringType.Botanist)
            {
                GatherBuddy.Config.PreferredGatheringType = GatheringType.Botanist;
                GatherBuddy.Config.Save();
            }
        }

        public static void DrawPrintClipboardBox()
            => DrawCheckbox(Localize.Label("Print Clipboard Information"),
                Localize.Text("Print to the chat whenever you save an object to the clipboard. Failures will be printed regardless."),
                GatherBuddy.Config.PrintClipboardMessages, b => GatherBuddy.Config.PrintClipboardMessages = b);

        // Weather Tab
        public static void DrawWeatherTabNamesBox()
            => DrawCheckbox(Localize.Label("Show Names in Weather Tab"),
                Localize.Text("Toggle whether to write the names in the table for the weather tab, or just the icons with names on hover."),
                GatherBuddy.Config.ShowWeatherNames, b => GatherBuddy.Config.ShowWeatherNames = b);

        // Alarms
        public static void DrawAlarmToggle()
            => DrawCheckbox(Localize.Label("Enable Alarms"), Localize.Text("Toggle all alarms on or off."), GatherBuddy.Config.AlarmsEnabled,
                b =>
                {
                    if (b)
                        _plugin.AlarmManager.Enable();
                    else
                        _plugin.AlarmManager.Disable();
                });

        public static void DrawAlarmsInDutyToggle()
            => DrawCheckbox(Localize.Label("Enable Alarms in Duty"), Localize.Text("Set whether alarms should trigger while you are bound by a duty."),
                GatherBuddy.Config.AlarmsInDuty,     b => GatherBuddy.Config.AlarmsInDuty = b);

        public static void DrawAlarmsOnlyWhenLoggedInToggle()
            => DrawCheckbox(Localize.Label("Enable Alarms Only In-Game"),  Localize.Text("Set whether alarms should trigger while you are not logged into any character."),
                GatherBuddy.Config.AlarmsOnlyWhenLoggedIn, b => GatherBuddy.Config.AlarmsOnlyWhenLoggedIn = b);

        private static void DrawAlarmPicker(string label, string description, Sounds current, Action<Sounds> setter)
        {
            var cur = (int)current;
            ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
            if (ImGui.Combo(new ImU8String(label), ref cur, AlarmCache.SoundIdNames))
                setter((Sounds)cur);
            ImGuiUtil.HoverTooltip(description);
        }

        public static void DrawWeatherAlarmPicker()
            => DrawAlarmPicker(Localize.Text("Weather Change Alarm"), Localize.Text("Choose a sound that is played every 8 Eorzea hours on regular weather changes."),
                GatherBuddy.Config.WeatherAlarm,       _plugin.AlarmManager.SetWeatherAlarm);

        public static void DrawHourAlarmPicker()
            => DrawAlarmPicker(Localize.Text("Eorzea Hour Change Alarm"), Localize.Text("Choose a sound that is played every time the current Eorzea hour changes."),
                GatherBuddy.Config.HourAlarm,              _plugin.AlarmManager.SetHourAlarm);

        // Fish Timer
        public static void DrawFishTimerBox()
            => DrawCheckbox(Localize.Label("Show Fish Timer"),
                Localize.Text("Toggle whether to show the fish timer window while fishing."),
                GatherBuddy.Config.ShowFishTimer, b => GatherBuddy.Config.ShowFishTimer = b);

        public static void DrawFishTimerEditBox()
            => DrawCheckbox(Localize.Label("Edit Fish Timer"),
                Localize.Text("Enable editing the fish timer window."),
                GatherBuddy.Config.FishTimerEdit, b => GatherBuddy.Config.FishTimerEdit = b);

        public static void DrawFishTimerClickthroughBox()
            => DrawCheckbox(Localize.Label("Enable Fish Timer Clickthrough"),
                Localize.Text("Allow clicking through the fish timer and disabling the context menus instead."),
                GatherBuddy.Config.FishTimerClickthrough, b => GatherBuddy.Config.FishTimerClickthrough = b);

        public static void DrawFishTimerHideBox()
            => DrawCheckbox(Localize.Label("Hide Uncaught Fish in Fish Timer"),
                Localize.Text("Hide all fish from the fish timer window that have not been recorded with the given combination of snagging and bait."),
                GatherBuddy.Config.HideUncaughtFish, b => GatherBuddy.Config.HideUncaughtFish = b);

        public static void DrawFishTimerHideBox2()
            => DrawCheckbox(Localize.Label("Hide Unavailable Fish in Fish Timer"),
                Localize.Text("Hide all fish from the fish timer window that have have known requirements that are unfulfilled, like Fisher's Intuition or Snagging."),
                GatherBuddy.Config.HideUnavailableFish, b => GatherBuddy.Config.HideUnavailableFish = b);

        public static void DrawFishTimerUptimesBox()
            => DrawCheckbox(Localize.Label("Show Uptimes in Fish Timer"),
                Localize.Text("Show the uptimes for restricted fish in the fish timer window."),
                GatherBuddy.Config.ShowFishTimerUptimes, b => GatherBuddy.Config.ShowFishTimerUptimes = b);

        public static void DrawKeepRecordsBox()
            => DrawCheckbox(Localize.Label("Keep Fish Records"),
                Localize.Text("Store Fish Records on your computer. This is necessary for bite timings for the fish timer window."),
                GatherBuddy.Config.StoreFishRecords, b => GatherBuddy.Config.StoreFishRecords = b);

        public static void DrawShowLocalTimeInRecordsBox()
            => DrawCheckbox(Localize.Label("Use Local Time in Records"),
                Localize.Text("When displaying timestamps in the Fish Records Tab, use local time instead of Unix time."),
                GatherBuddy.Config.UseUnixTimeFishRecords, b => GatherBuddy.Config.UseUnixTimeFishRecords = b);

        public static void DrawFishTimerScale()
        {
            var value = GatherBuddy.Config.FishTimerScale / 1000f;
            ImGui.SetNextItemWidth(SetInputWidth);
            var ret = ImGui.DragFloat(Localize.Label("Fish Timer Bite Time Scale"), ref value, 0.1f, FishRecord.MinBiteTime / 500f,
                FishRecord.MaxBiteTime / 1000f,
                Localize.Text("%2.3f Seconds"));

            ImGuiUtil.HoverTooltip(Localize.Text("The fishing timer window bite times are scaled to this value.\nIf your bite time exceeds the value, the progress bar and bite windows will not be displayed.\nYou should probably keep this as high as your highest bite window and as low as possible. About 40 seconds is usually enough."));

            if (!ret)
                return;

            var newValue = (ushort)Math.Clamp((int)(value * 1000f + 0.9), FishRecord.MinBiteTime * 2, FishRecord.MaxBiteTime);
            if (newValue == GatherBuddy.Config.FishTimerScale)
                return;

            GatherBuddy.Config.FishTimerScale = newValue;
            GatherBuddy.Config.Save();
        }

        public static void DrawFishTimerIntervals()
        {
            int value = GatherBuddy.Config.ShowSecondIntervals;
            ImGui.SetNextItemWidth(SetInputWidth);
            var ret = ImGui.DragInt(Localize.Label("Fish Timer Interval Separators"), ref value, 0.01f, 0, 16);
            ImGuiUtil.HoverTooltip(Localize.Text("The fishing timer window can show a number of interval lines and corresponding seconds between 0 and 16.\nSet to 0 to turn this feature off."));
            if (!ret)
                return;

            var newValue = (byte)Math.Clamp(value, 0, 16);
            if (newValue == GatherBuddy.Config.ShowSecondIntervals)
                return;

            GatherBuddy.Config.ShowSecondIntervals = newValue;
            GatherBuddy.Config.Save();
        }

        public static void DrawFishTimerIntervalsRounding()
        {
            var value = GatherBuddy.Config.SecondIntervalsRounding;
            ImGui.SetNextItemWidth(SetInputWidth);
            var ret = ImGui.DragInt(Localize.Label("Fish Timer Interval Rounding"), ref value, 0.01f, 0, 3);
            ImGuiUtil.HoverTooltip(Localize.Text("Round the displayed second value to this number of digits past the decimal. \nSet to 0 to display only whole numbers."));
            if (!ret)
                return;

            var newValue = (byte)Math.Clamp(value, 0, 3);
            if (newValue == GatherBuddy.Config.SecondIntervalsRounding)
                return;

            GatherBuddy.Config.SecondIntervalsRounding = newValue;
            GatherBuddy.Config.Save();
        }

        public static void DrawUpcomingUptimesCount()
        {
            var value = GatherBuddy.Config.UpcomingUptimesCount;
            ImGui.SetNextItemWidth(SetInputWidth);
            var ret = ImGui.DragUShort(Localize.Text("Upcoming Windows Uptime Count"), ref value, 0.1f, 1, 20);
            ImGuiUtil.HoverTooltip(Localize.Text("The number of gathering window in the upcoming windows uptime tooltip/pop-up."));
            if (!ret)
                return;

            var newValue = Math.Clamp(value, (ushort)1, (ushort)100);
            if (newValue == GatherBuddy.Config.UpcomingUptimesCount)
                return;

            GatherBuddy.Config.UpcomingUptimesCount = newValue;
            GatherBuddy.Config.Save();
        }

        public static void DrawHideFishPopupBox()
            => DrawCheckbox(Localize.Label("Hide Catch Popup"),
                Localize.Text("Prevents the popup window that shows you your caught fish and its size, amount and quality from being shown."),
                GatherBuddy.Config.HideFishSizePopup, b => GatherBuddy.Config.HideFishSizePopup = b);

        public static void DrawCollectableHintPopupBox()
            => DrawCheckbox(Localize.Label("Show Collectable Hints"),
                Localize.Text("Show if a fish is collectable in the fish timer window."),
                GatherBuddy.Config.ShowCollectableHints, b => GatherBuddy.Config.ShowCollectableHints = b);

        public static void DrawDoubleHookHintPopupBox()
            => DrawCheckbox(Localize.Label("Show Multi Hook Hints"),
                Localize.Text("Show if a fish can be double or triple hooked in Cosmic Exploration and Ocean Fishing"),
                GatherBuddy.Config.ShowMultiHookHints, b => GatherBuddy.Config.ShowMultiHookHints = b);

        public static void DrawOceanTypeHintPopupBox()
            => DrawCheckbox(Localize.Label("Show Ocean Type Hints"),
                Localize.Text("Show what type of fish in Ocean Fishing"),
                GatherBuddy.Config.ShowOceanTypeHints, b => GatherBuddy.Config.ShowOceanTypeHints = b);

        // Fish Stats Window
        public static void DrawEnableFishStats()
            => DrawCheckbox(Localize.Label("Enable Fish Stats"),
                Localize.Text("New tab for aggregating and reporting fish stats based on local records. Currently in testing."),
                GatherBuddy.Config.EnableFishStats, b => GatherBuddy.Config.EnableFishStats = b);

        public static void DrawEnableReportTime()
            => DrawCheckbox(Localize.Label("Copy Time Stats when reporting."),
                Localize.Text("When copying the report, add min and max times to the report."),
                GatherBuddy.Config.EnableReportTime, b => GatherBuddy.Config.EnableReportTime = b);

        public static void DrawEnableReportSize()
            => DrawCheckbox(Localize.Label("Copy Sizes Stats when reporting."),
                Localize.Text("When copying the report, add min and max sizes to the report."),
                GatherBuddy.Config.EnableReportSize, b => GatherBuddy.Config.EnableReportSize = b);

        public static void DrawEnableReportMulti()
            => DrawCheckbox(Localize.Label("Copy Multi Hook Stats when reporting."),
                Localize.Text("When copying the report, add stats about multi-hook yields to the report."),
                GatherBuddy.Config.EnableReportMulti, b => GatherBuddy.Config.EnableReportMulti = b);

        public static void DrawEnableGraphs()
            => DrawCheckbox(Localize.Label("Enable Graphs."),
                Localize.Text("When viewing a fishing spot, enable visualization of fish report data. Extreme Testing!"),
                GatherBuddy.Config.EnableFishStatsGraphs, b => GatherBuddy.Config.EnableFishStatsGraphs = b);

        // Spearfishing Helper
        public static void DrawSpearfishHelperBox()
            => DrawCheckbox(Localize.Label("Show Spearfishing Helper"),
                Localize.Text("Toggle whether to show the Spearfishing Helper while spearfishing."),
                GatherBuddy.Config.ShowSpearfishHelper, b => GatherBuddy.Config.ShowSpearfishHelper = b);

        public static void DrawSpearfishNamesBox()
            => DrawCheckbox(Localize.Label("Show Fish Name Overlay"),
                Localize.Text("Toggle whether to show the identified names of fish in the spearfishing window."),
                GatherBuddy.Config.ShowSpearfishNames, b => GatherBuddy.Config.ShowSpearfishNames = b);

        public static void DrawAvailableSpearfishBox()
            => DrawCheckbox(Localize.Label("Show List of Available Fish"),
                Localize.Text("Toggle whether to show the list of fish available in your current spearfishing spot on the side of the spearfishing window."),
                GatherBuddy.Config.ShowAvailableSpearfish, b => GatherBuddy.Config.ShowAvailableSpearfish = b);

        public static void DrawSpearfishSpeedBox()
            => DrawCheckbox(Localize.Label("Show Speed of Fish in Overlay"),
                Localize.Text("Toggle whether to show the speed of fish in the spearfishing window in addition to their names."),
                GatherBuddy.Config.ShowSpearfishSpeed, b => GatherBuddy.Config.ShowSpearfishSpeed = b);

        public static void DrawSpearfishCenterLineBox()
            => DrawCheckbox(Localize.Label("Show Center Line"),
                Localize.Text("Toggle whether to show a straight line up from the center of the spearfishing gig in the spearfishing window."),
                GatherBuddy.Config.ShowSpearfishCenterLine, b => GatherBuddy.Config.ShowSpearfishCenterLine = b);

        public static void DrawSpearfishIconsAsTextBox()
            => DrawCheckbox(Localize.Label("Show Speed and Size as Text"),
                Localize.Text("Toggle whether to show the speed and size of available fish as text instead of icons."),
                GatherBuddy.Config.ShowSpearfishListIconsAsText, b => GatherBuddy.Config.ShowSpearfishListIconsAsText = b);

        public static void DrawSpearfishFishNameFixed()
            => DrawCheckbox(Localize.Label("Show Fish Names in Fixed Position"),
                Localize.Text("Toggle whether to show the identified names of fish on the moving fish themselves or in a fixed position."),
                GatherBuddy.Config.FixNamesOnPosition, b => GatherBuddy.Config.FixNamesOnPosition = b);

        public static void DrawSpearfishFishNamePercentage()
        {
            if (!GatherBuddy.Config.FixNamesOnPosition)
                return;

            var tmp = (int)GatherBuddy.Config.FixNamesPercentage;
            ImGui.SetNextItemWidth(SetInputWidth);
            if (!ImGui.DragInt(Localize.Label("Fish Name Position Percentage"), ref tmp, 0.1f, 0, 100, "%i%%"))
                return;

            tmp = Math.Clamp(tmp, 0, 100);
            if (tmp == GatherBuddy.Config.FixNamesPercentage)
                return;

            GatherBuddy.Config.FixNamesPercentage = (byte)tmp;
            GatherBuddy.Config.Save();
        }

        // Gather Window
        public static void DrawShowGatherWindowBox()
            => DrawCheckbox(Localize.Label("Show Gather Window"),
                Localize.Text("Show a small window with pinned Gatherables and their uptimes."),
                GatherBuddy.Config.ShowGatherWindow, b => GatherBuddy.Config.ShowGatherWindow = b);

        public static void DrawGatherWindowAnchorBox()
            => DrawCheckbox(Localize.Label("Anchor Gather Window to Bottom Left"),
                Localize.Text("Lets the Gather Window grow to the top and shrink from the top instead of the bottom."),
                GatherBuddy.Config.GatherWindowBottomAnchor, b => GatherBuddy.Config.GatherWindowBottomAnchor = b);

        public static void DrawGatherWindowTimersBox()
            => DrawCheckbox(Localize.Label("Show Gather Window Timers"),
                Localize.Text("Show the uptimes for gatherables in the gather window."),
                GatherBuddy.Config.ShowGatherWindowTimers, b => GatherBuddy.Config.ShowGatherWindowTimers = b);

        public static void DrawGatherWindowAlarmsBox()
            => DrawCheckbox(Localize.Label("Show Active Alarms in Gather Window"),
                Localize.Text("Additionally show active alarms as a last gather window preset, obeying the regular rules for the window."),
                GatherBuddy.Config.ShowGatherWindowAlarms, b =>
                {
                    GatherBuddy.Config.ShowGatherWindowAlarms = b;
                    _plugin.GatherWindowManager.SetShowGatherWindowAlarms(b);
                });

        public static void DrawSortGatherWindowBox()
            => DrawCheckbox(Localize.Label("Sort Gather Window by Uptime"),
                Localize.Text("Sort the items selected for the gather window by their uptimes."),
                GatherBuddy.Config.SortGatherWindowByUptime, b => GatherBuddy.Config.SortGatherWindowByUptime = b);

        public static void DrawGatherWindowShowOnlyAvailableBox()
            => DrawCheckbox(Localize.Label("Show Only Available Items"),
                Localize.Text("Show only those items from your gather window setup that are currently available."),
                GatherBuddy.Config.ShowGatherWindowOnlyAvailable, b => GatherBuddy.Config.ShowGatherWindowOnlyAvailable = b);

        public static void DrawHideGatherWindowInDutyBox()
            => DrawCheckbox(Localize.Label("Hide Gather Window in Duty"),
                Localize.Text("Hide the gather window when bound by any duty."),
                GatherBuddy.Config.HideGatherWindowInDuty, b => GatherBuddy.Config.HideGatherWindowInDuty = b);

        public static void DrawGatherWindowHoldKey()
        {
            DrawCheckbox(Localize.Label("Only Show Gather Window if Holding Key"),
                Localize.Text("Only show the gather window if you are holding your selected key."),
                GatherBuddy.Config.OnlyShowGatherWindowHoldingKey, b => GatherBuddy.Config.OnlyShowGatherWindowHoldingKey = b);

            if (!GatherBuddy.Config.OnlyShowGatherWindowHoldingKey)
                return;

            ImGui.SetNextItemWidth(SetInputWidth);
            CnWidget.KeySelector(Localize.Text("Hotkey to Hold"), Localize.Text("Set the hotkey to hold to keep the window visible."),
                GatherBuddy.Config.GatherWindowHoldKey,
                k => GatherBuddy.Config.GatherWindowHoldKey = k, Configuration.ValidKeys);
        }

        public static void DrawGatherWindowLockBox()
            => DrawCheckbox(Localize.Label("Lock Gather Window Position"),
                Localize.Text("Prevent moving the gather window by dragging it around."),
                GatherBuddy.Config.LockGatherWindow, b => GatherBuddy.Config.LockGatherWindow = b);


        public static void DrawGatherWindowHotkeyInput()
        {
            if (CnWidget.ModifiableKeySelector(Localize.Text("Hotkey to Open Gather Window"), Localize.Text("Set a hotkey to open the Gather Window."), SetInputWidth,
                    GatherBuddy.Config.GatherWindowHotkey, k => GatherBuddy.Config.GatherWindowHotkey = k, Configuration.ValidKeys))
                GatherBuddy.Config.Save();
        }

        public static void DrawMainInterfaceHotkeyInput()
        {
            if (CnWidget.ModifiableKeySelector(Localize.Text("Hotkey to Open Main Interface"), Localize.Text("Set a hotkey to open the main GatherBuddy interface."),
                    SetInputWidth,
                    GatherBuddy.Config.MainInterfaceHotkey, k => GatherBuddy.Config.MainInterfaceHotkey = k, Configuration.ValidKeys))
                GatherBuddy.Config.Save();
        }


        public static void DrawGatherWindowDeleteModifierInput()
        {
            ImGui.SetNextItemWidth(SetInputWidth);
            if (CnWidget.ModifierSelector(Localize.Text("Modifier to Delete Items on Right-Click"),
                    Localize.Text("Set the modifier key to be used while right-clicking items in the gather window to delete them."),
                    GatherBuddy.Config.GatherWindowDeleteModifier, k => GatherBuddy.Config.GatherWindowDeleteModifier = k))
                GatherBuddy.Config.Save();
        }


        public static void DrawAetherytePreference()
        {
            var tmp     = GatherBuddy.Config.AetherytePreference == AetherytePreference.Cost;
            var oldPref = GatherBuddy.Config.AetherytePreference;
            if (ImGui.RadioButton(Localize.Text("Prefer Cheaper Aetherytes"), tmp))
                GatherBuddy.Config.AetherytePreference = AetherytePreference.Cost;
            var hovered = ImGui.IsItemHovered();
            ImGui.SameLine();
            if (ImGui.RadioButton(Localize.Text("Prefer Less Travel Time"), !tmp))
                GatherBuddy.Config.AetherytePreference = AetherytePreference.Distance;
            hovered |= ImGui.IsItemHovered();
            if (hovered)
                ImGui.SetTooltip(
                    Localize.Text("Specify whether you prefer aetherytes that are closer to your target (less travel time) or aetherytes that are cheaper to teleport to when scanning through all available nodes for an item. Only matters if the item is not timed and has multiple sources."));

            if (oldPref != GatherBuddy.Config.AetherytePreference)
            {
                GatherBuddy.UptimeManager.ResetLocations();
                GatherBuddy.Config.Save();
            }
        }

        public static void DrawAlarmFormatInput()
            => DrawFormatInput(Localize.Text("Alarm Chat Format"),
                Localize.Text("Keep empty to have no chat output.\nCan replace:\n- {Alarm} with the alarm name in brackets.\n- {Item} with the item link.\n- {Offset} with the alarm offset in seconds.\n- {DelayString} with 'will be up for the next ...' or 'is currently up for ...'.\n- {Location} with the map flag link and location name."),
                GatherBuddy.Config.AlarmFormat, Localize.Text(Configuration.DefaultAlarmFormat), s => GatherBuddy.Config.AlarmFormat = s);

        public static void DrawIdentifiedGatherableFormatInput()
            => DrawFormatInput(Localize.Text("Identified Gatherable Chat Format"),
                Localize.Text("Keep empty to have no chat output.\nCan replace:\n- {Input} with the entered search text.\n- {Item} with the item link."),
                GatherBuddy.Config.IdentifiedGatherableFormat, Localize.Text(Configuration.DefaultIdentifiedGatherableFormat),
                s => GatherBuddy.Config.IdentifiedGatherableFormat = s);
    }

    private void DrawConfigTab()
    {
        using var id  = ImRaii.PushId("Config");
        using var tab = ImRaii.TabItem(Localize.Label("Config"));
        ImGuiUtil.HoverTooltip(Localize.Text("Set up your very own GatherBuddy to your meticulous specifications.\nIf you treat him well, he might even become a real boy."));

        if (!tab)
            return;

        using var child = ImRaii.Child("ConfigTab");
        if (!child)
            return;

        if (ImGui.CollapsingHeader(Localize.Label("General")))
        {
            if (ImGui.TreeNodeEx(Localize.Label("Gather Command")))
            {
                ConfigFunctions.DrawPreferredJobSelect();
                ConfigFunctions.DrawGearChangeBox();
                ConfigFunctions.DrawTeleportBox();
                ConfigFunctions.DrawMapOpenBox();
                ConfigFunctions.DrawPlaceMarkerBox();
                ConfigFunctions.DrawPlaceWaymarkBox();
                ConfigFunctions.DrawAetherytePreference();
                ConfigFunctions.DrawSkipTeleportBox();
                ConfigFunctions.DrawContextMenuBox();
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Set Names")))
            {
                ConfigFunctions.DrawSetInput(Localize.Text("Miner"),    GatherBuddy.Config.MinerSetName,    s => GatherBuddy.Config.MinerSetName    = s);
                ConfigFunctions.DrawSetInput(Localize.Text("Botanist"), GatherBuddy.Config.BotanistSetName, s => GatherBuddy.Config.BotanistSetName = s);
                ConfigFunctions.DrawSetInput(Localize.Text("Fisher"),   GatherBuddy.Config.FisherSetName,   s => GatherBuddy.Config.FisherSetName   = s);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Alarms")))
            {
                ConfigFunctions.DrawAlarmToggle();
                ConfigFunctions.DrawAlarmsInDutyToggle();
                ConfigFunctions.DrawAlarmsOnlyWhenLoggedInToggle();
                ConfigFunctions.DrawWeatherAlarmPicker();
                ConfigFunctions.DrawHourAlarmPicker();
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Messages")))
            {
                ConfigFunctions.DrawPrintTypeSelector();
                ConfigFunctions.DrawErrorTypeSelector();
                ConfigFunctions.DrawMapMarkerPrintBox();
                ConfigFunctions.DrawPrintUptimesBox();
                ConfigFunctions.DrawPrintClipboardBox();
                ConfigFunctions.DrawAlarmFormatInput();
                ConfigFunctions.DrawIdentifiedGatherableFormatInput();
                ImGui.TreePop();
            }

            ImGui.NewLine();
        }

        if (ImGui.CollapsingHeader(Localize.Label("Interface")))
        {
            if (ImGui.TreeNodeEx(Localize.Label("Config Window")))
            {
                ConfigFunctions._base = this;
                ConfigFunctions.DrawOpenOnStartBox();
                ConfigFunctions.DrawRespectEscapeBox();
                ConfigFunctions.DrawLockPositionBox();
                ConfigFunctions.DrawLockResizeBox();
                ConfigFunctions.DrawWeatherTabNamesBox();
                ConfigFunctions.DrawShowStatusLineBox();
                ConfigFunctions.DrawHideClippyBox();
                ConfigFunctions.DrawMainInterfaceHotkeyInput();
                ConfigFunctions.DrawUpcomingUptimesCount();
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Fish Timer")))
            {
                ConfigFunctions.DrawKeepRecordsBox();
                ConfigFunctions.DrawShowLocalTimeInRecordsBox();
                ConfigFunctions.DrawFishTimerBox();
                ConfigFunctions.DrawFishTimerEditBox();
                ConfigFunctions.DrawFishTimerClickthroughBox();
                ConfigFunctions.DrawFishTimerHideBox();
                ConfigFunctions.DrawFishTimerHideBox2();
                ConfigFunctions.DrawFishTimerUptimesBox();
                ConfigFunctions.DrawFishTimerScale();
                ConfigFunctions.DrawFishTimerIntervals();
                ConfigFunctions.DrawFishTimerIntervalsRounding();
                ConfigFunctions.DrawHideFishPopupBox();
                ConfigFunctions.DrawCollectableHintPopupBox();
                ConfigFunctions.DrawDoubleHookHintPopupBox();
                ConfigFunctions.DrawOceanTypeHintPopupBox();
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Fish Stats [Testing]")))
            {
                ConfigFunctions.DrawEnableFishStats();
                ConfigFunctions.DrawEnableReportTime();
                ConfigFunctions.DrawEnableReportSize();
                ConfigFunctions.DrawEnableReportMulti();
                ConfigFunctions.DrawEnableGraphs();
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Gather Window")))
            {
                ConfigFunctions.DrawShowGatherWindowBox();
                ConfigFunctions.DrawGatherWindowAnchorBox();
                ConfigFunctions.DrawGatherWindowTimersBox();
                ConfigFunctions.DrawGatherWindowAlarmsBox();
                ConfigFunctions.DrawSortGatherWindowBox();
                ConfigFunctions.DrawGatherWindowShowOnlyAvailableBox();
                ConfigFunctions.DrawHideGatherWindowInDutyBox();
                ConfigFunctions.DrawGatherWindowHoldKey();
                ConfigFunctions.DrawGatherWindowLockBox();
                ConfigFunctions.DrawGatherWindowHotkeyInput();
                ConfigFunctions.DrawGatherWindowDeleteModifierInput();
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx(Localize.Label("Spearfishing Helper")))
            {
                ConfigFunctions.DrawSpearfishHelperBox();
                ConfigFunctions.DrawSpearfishNamesBox();
                ConfigFunctions.DrawSpearfishSpeedBox();
                ConfigFunctions.DrawAvailableSpearfishBox();
                ConfigFunctions.DrawSpearfishIconsAsTextBox();
                ConfigFunctions.DrawSpearfishCenterLineBox();
                ConfigFunctions.DrawSpearfishFishNameFixed();
                ConfigFunctions.DrawSpearfishFishNamePercentage();
                ImGui.TreePop();
            }

            ImGui.NewLine();
        }

        if (ImGui.CollapsingHeader(Localize.Label("Colors")))
        {
            foreach (var color in Enum.GetValues<ColorId>())
            {
                var (defaultColor, name, description) = color.Data();
                var currentColor = GatherBuddy.Config.Colors.TryGetValue(color, out var current) ? current : defaultColor;
                if (Widget.ColorPicker(name, description, currentColor, c => GatherBuddy.Config.Colors[color] = c, defaultColor))
                    GatherBuddy.Config.Save();
            }

            ImGui.NewLine();

            if (Widget.PaletteColorPicker(Localize.Text("Names in Chat"), Vector2.One * ImGui.GetFrameHeight(), GatherBuddy.Config.SeColorNames,
                    Configuration.DefaultSeColorNames, Configuration.ForegroundColors, out var idx))
                GatherBuddy.Config.SeColorNames = idx;
            if (Widget.PaletteColorPicker(Localize.Text("Commands in Chat"), Vector2.One * ImGui.GetFrameHeight(), GatherBuddy.Config.SeColorCommands,
                    Configuration.DefaultSeColorCommands, Configuration.ForegroundColors, out idx))
                GatherBuddy.Config.SeColorCommands = idx;
            if (Widget.PaletteColorPicker(Localize.Text("Arguments in Chat"), Vector2.One * ImGui.GetFrameHeight(), GatherBuddy.Config.SeColorArguments,
                    Configuration.DefaultSeColorArguments, Configuration.ForegroundColors, out idx))
                GatherBuddy.Config.SeColorArguments = idx;
            if (Widget.PaletteColorPicker(Localize.Text("Alarm Message in Chat"), Vector2.One * ImGui.GetFrameHeight(), GatherBuddy.Config.SeColorAlarm,
                    Configuration.DefaultSeColorAlarm, Configuration.ForegroundColors, out idx))
                GatherBuddy.Config.SeColorAlarm = idx;

            ImGui.NewLine();
        }
    }
}
