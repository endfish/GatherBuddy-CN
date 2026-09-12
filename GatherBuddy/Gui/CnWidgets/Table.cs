using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using OtterGui.Raii;
using OtterGui.Table;

namespace GatherBuddy.Gui.CnWidgets;

/// <summary>Keep OtterGui table behavior while drawing its native context menu in Chinese.</summary>
public class CnTable<T>(string label, IReadOnlyCollection<T> items, params Column<T>[] headers)
    : Table<T>(label, items, headers)
{
    // OtterGui calls this before the first row triggers ImGui's table layout and native menu.
    protected override unsafe void PreDraw()
    {
        var table = ImGuiP.GetCurrentTable();
        table.IsContextPopupOpen = false;
        ImGuiP.PushOverrideID(table.ID);
        try
        {
            using var popup = ImRaii.Popup("##ContextMenu");
            if (!popup)
                return;

            var columnIndex = table.ContextPopupColumn;
            if (Flags.HasFlag(ImGuiTableFlags.Resizable))
            {
                if (columnIndex >= 0 && columnIndex < Headers.Length)
                {
                    var column = new ImGuiTableColumnPtr(table.Columns.Data + columnIndex);
                    if (ImGui.MenuItem(Localize.Label("Size column to fit###SizeOne"), string.Empty, false,
                            column.IsEnabled && !column.Flags.HasFlag(ImGuiTableColumnFlags.NoResize)))
                        ImGuiP.TableSetColumnWidthAutoSingle(table, columnIndex);
                }
                if (ImGui.MenuItem(Localize.Label("Size all columns to fit###SizeAll")))
                    ImGuiP.TableSetColumnWidthAutoAll(table);
            }
            if (Flags.HasFlag(ImGuiTableFlags.Reorderable)
                && ImGui.MenuItem(Localize.Label("Reset order"), string.Empty, false, !table.IsDefaultDisplayOrder))
                table.IsResetDisplayOrderRequest = true;
            if (!Flags.HasFlag(ImGuiTableFlags.Hideable))
                return;

            ImGui.Separator();
            for (var i = 0; i < Headers.Length; ++i)
            {
                var column = new ImGuiTableColumnPtr(table.Columns.Data + i);
                if (column.Flags.HasFlag(ImGuiTableColumnFlags.Disabled))
                    continue;
                var enabled = !column.Flags.HasFlag(ImGuiTableColumnFlags.NoHide)
                    && (!column.IsUserEnabled || table.ColumnsEnabledCount > 1);
                using var id = ImRaii.PushId(i);
                if (ImGui.MenuItem(Headers[i].Label, string.Empty, column.IsUserEnabled, enabled))
                    column.IsUserEnabledNextFrame = !column.IsUserEnabled;
            }
        }
        finally
        {
            ImGui.PopID();
        }
    }
}
