// Adapted from Ottermandias/OtterGui (Widgets/ChatTypeSelector.cs).
// Apache-2.0; baseline 79771ee5f3d463f02c63bebbedaa0aff49e59718.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using OtterGui;
using OtterGui.Table;
using OtterGui.Widgets;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;

namespace GatherBuddy.Gui.CnWidgets;

public static partial class CnWidget
{
    // Regular combo to select a Dalamud chat type.
    // Can have a tooltip on hover.
    // Returns true if a different chat type was selected and calls the setter.
    public static bool DrawChatTypeSelector(string label, string description, XivChatType currentValue, Action<XivChatType> setter)
    {
        using var id    = ImRaii.PushId(label);
        using var combo = ImRaii.Combo(label, currentValue.ToString());
        ImGuiUtil.HoverTooltip(description);
        if (!combo)
            return false;

        var ret = false;
        // Draw the actual combo values.
        foreach (var type in Enum.GetValues<XivChatType>())
        {
            if (!ImGui.Selectable(type.ToString(), currentValue == type) || type == currentValue)
                continue;

            setter(type);
            ret = true;
        }

        return ret;
    }
}
