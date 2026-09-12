// Adapted from Ottermandias/OtterGui Util.cs; Apache-2.0.
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using OtterGui.Raii;

namespace GatherBuddy.Gui.CnWidgets;

public static partial class CnWidget
{
    public static bool OpenNameField(string popupName, ref string newName)
    {
        using var popup = ImRaii.Popup(popupName);
        if (!popup)
            return false;
        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            ImGui.CloseCurrentPopup();
        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        if (!ImGui.InputTextWithHint("##newName", Localize.Text("Enter New Name..."), ref newName, 512, ImGuiInputTextFlags.EnterReturnsTrue))
            return false;
        ImGui.CloseCurrentPopup();
        return true;
    }
}
