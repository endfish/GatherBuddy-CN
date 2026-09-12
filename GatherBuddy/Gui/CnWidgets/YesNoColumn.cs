// Adapted from Ottermandias/OtterGui (Table/YesNoColumn.cs).
// Apache-2.0; baseline 79771ee5f3d463f02c63bebbedaa0aff49e59718.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using OtterGui;
using OtterGui.Table;
using OtterGui.Widgets;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;

namespace GatherBuddy.Gui.CnWidgets;

[Flags]
public enum YesNoFlag
{
    Yes = 0x01,
    No  = 0x02,
};

public class CnYesNoColumn<TItem> : CnColumnFlags<YesNoFlag, TItem>
{
    public string Tooltip = string.Empty;

    private YesNoFlag _filterValue;

    public override YesNoFlag FilterValue
        => _filterValue;

    public CnYesNoColumn()
    {
        Flags        &= ~ImGuiTableColumnFlags.NoResize;
        AllFlags     =  YesNoFlag.Yes | YesNoFlag.No;
        _filterValue =  AllFlags;
    }

    protected override void SetValue(YesNoFlag value, bool enable)
        => _filterValue = enable ? _filterValue | value : _filterValue & ~value;

    protected virtual bool GetValue(TItem _)
        => false;

    public override float Width
        => ImGui.GetFrameHeight() * 2;

    public override bool FilterFunc(TItem item)
        => GetValue(item)
            ? FilterValue.HasFlag(YesNoFlag.Yes)
            : FilterValue.HasFlag(YesNoFlag.No);

    public override int Compare(TItem lhs, TItem rhs)
        => GetValue(lhs).CompareTo(GetValue(rhs));

    public override void DrawColumn(TItem item, int idx)
    {
        using (var font = ImRaii.PushFont(UiBuilder.IconFont))
        {
            ImGuiUtil.Center(GetValue(item) ? FontAwesomeIcon.Check.ToIconString() : FontAwesomeIcon.Times.ToIconString());
        }

        ImGuiUtil.HoverTooltip(Tooltip);
    }
}
