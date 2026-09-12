using System.Numerics;
using Dalamud.Bindings.ImGui;
using GatherBuddy.Gui.CnWidgets;
using OtterGui.Table;

internal static class NativeUiChecks
{
    // Optional: run in a separate console process with matching cimgui.dll beside this executable.
    // Does not connect to the game or modify the game's ImGui context.
    public static unsafe void Run(Action<bool, string> check)
    {
        var context = ImGui.CreateContext();
        try
        {
            var io = ImGui.GetIO();
            io.IniFilename = null;
            io.DisplaySize = new Vector2(1280, 720);
            io.DeltaTime = 1f / 60;
            io.Fonts.AddFontDefault();
            byte* pixels;
            int width, height, bytes;
            io.Fonts.GetTexDataAsRGBA32(0, &pixels, &width, &height, &bytes);
            var menu = new TestTable { Flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable };
            for (var frame = 0; frame < 3; ++frame)
            {
                ImGui.NewFrame();
                ImGui.SetNextWindowSize(new Vector2(900, 500));
                ImGui.Begin("CN table test");
                // ImGui hashes the ### marker itself; labels without an explicit ### get a source-derived ID.
                check(ImGui.GetID("###Name##pane") == ImGui.GetID(GatherBuddy.Localization.Localize.Label("Name##pane")), "Native source-derived label identity");
                check(ImGui.GetID("Name###pane") == ImGui.GetID(GatherBuddy.Localization.Localize.Label("Name###pane")), "Native ### label identity");
                check(ImGui.BeginTable("table", 2, menu.Flags), "Native table opens");
                if (frame == 1)
                {
                    ImGuiP.TableOpenContextMenu(0);
                    check(ImGuiP.GetCurrentTable().IsContextPopupOpen, "Native column menu opens");
                }
                menu.DrawMenu();
                check(!ImGuiP.GetCurrentTable().IsContextPopupOpen, "Native English menu suppressed");
                ImGui.TableSetupColumn("名称");
                ImGui.TableSetupColumn("备注");
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted("测试物品");
                ImGui.EndTable();
                ImGui.End();
                ImGui.Render();
                check(ImGui.GetDrawData().Valid, "Native UI frame renders");
            }
            Console.WriteLine("Native ImGui menu smoke test: passed.");
        }
        finally { ImGui.DestroyContext(context); }
    }
    private sealed class TestTable() : CnTable<int>("table", [], new Column<int> { Label = "名称" }, new Column<int> { Label = "备注" })
    {
        public void DrawMenu() => base.PreDraw();
    }
}
