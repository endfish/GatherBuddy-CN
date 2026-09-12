// Adapted from Ottermandias/OtterGui (Widgets/FilterComboCache.cs).
// Apache-2.0; baseline 79771ee5f3d463f02c63bebbedaa0aff49e59718.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using OtterGui;
using OtterGui.Table;
using OtterGui.Widgets;
using Dalamud.Bindings.ImGui;
using OtterGui.Classes;
using OtterGui.Extensions;
using OtterGui.Log;

namespace GatherBuddy.Gui.CnWidgets;

/// <summary>
/// A wrapper around filterable combos that makes them work with temporary lists without taking permanent additional memory.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CnFilterComboCache<T> : CnFilterComboBase<T>
{
    public T? CurrentSelection { get; protected set; }

    private readonly ICachingList<T> _items;
    protected        int             CurrentSelectionIdx = -1;

    protected bool IsInitialized
        => _items.IsInitialized;

    protected CnFilterComboCache(IEnumerable<T> items, MouseWheelType allowMouseWheel, Logger log)
        : base(new TemporaryList<T>(items), false, log)
    {
        AllowMouseWheel  = allowMouseWheel;
        CurrentSelection = default;
        _items           = (ICachingList<T>)Items;
    }

    protected CnFilterComboCache(Func<IReadOnlyList<T>> generator, MouseWheelType allowMouseWheel, Logger log)
        : base(new LazyList<T>(generator), false, log)
    {
        AllowMouseWheel  = allowMouseWheel;
        CurrentSelection = default;
        _items           = (ICachingList<T>)Items;
    }

    protected override void Cleanup()
        => _items.ClearList();


    protected override void DrawList(float width, float itemHeight)
    {
        base.DrawList(width, itemHeight);
        if (NewSelection != null && Items.Count > NewSelection.Value)
            UpdateSelection(Items[NewSelection.Value]);
    }

    protected virtual void UpdateSelection(T? newSelection)
    {
        if (!ReferenceEquals(CurrentSelection, newSelection))
            SelectionChanged?.Invoke(CurrentSelection, newSelection);
        CurrentSelection = newSelection;
    }

    protected override void OnMouseWheel(string preview, ref int _2, int steps)
    {
        if (Items.Count <= 1)
            return;

        if (CurrentSelectionIdx < 0)
            CurrentSelectionIdx = Items.IndexOf(i => ToString(i) == preview);

        var mouseWheel = -steps % Items.Count;
        NewSelection = mouseWheel switch
        {
            < 0 when CurrentSelectionIdx < 0 => Items.Count - 1 + mouseWheel,
            < 0                              => (CurrentSelectionIdx + Items.Count + mouseWheel) % Items.Count,
            > 0 when CurrentSelectionIdx < 0 => mouseWheel,
            > 0                              => (CurrentSelectionIdx + mouseWheel) % Items.Count,
            _                                => null,
        };
        if (NewSelection != null && Items.Count > NewSelection.Value)
        {
            CurrentSelectionIdx = NewSelection.Value;
            UpdateSelection(Items[NewSelection.Value]);
        }

        Cleanup();
    }

    public bool Draw(string label, string preview, string tooltip, float previewWidth, float itemHeight,
        ImGuiComboFlags flags = ImGuiComboFlags.None)
        => Draw(label, preview, tooltip, ref CurrentSelectionIdx, previewWidth, itemHeight, flags);

    public event Action<T?, T?>? SelectionChanged;
}
