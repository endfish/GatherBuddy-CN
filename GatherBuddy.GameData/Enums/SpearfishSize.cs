using System;

namespace GatherBuddy.Enums;

public enum SpearfishSize : byte
{
    Unknown = 0,
    Small   = 1,
    Average = 2,
    Large   = 3,
    None    = 255,
}

public static class SpearFishSizeExtensions
{
    public static string ToName(this SpearfishSize size)
        => size switch
        {
            SpearfishSize.Unknown => Localize.Text("Unknown Size"),
            SpearfishSize.Small   => Localize.Text("Small"),
            SpearfishSize.Average => Localize.Display(size),
            SpearfishSize.Large   => Localize.Text("Large"),
            SpearfishSize.None    => Localize.Text("No Size"),
            _                     => throw new ArgumentOutOfRangeException(nameof(size), size, null),
        };
}