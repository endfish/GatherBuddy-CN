using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GatherBuddy.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum HookSet : byte
{
    Unknown    = 0,
    Precise    = 1,
    Powerful   = 2,
    Hook       = 3,
    DoubleHook = 4,
    TripleHook = 5,
    Stellar    = 6,
    None       = 255,
}

public static class HookSetExtensions
{
    public static string ToName(this HookSet value)
        => value switch
        {
            HookSet.Unknown    => Localize.Text("Unknown"),
            HookSet.Precise    => Localize.Text("Precise"),
            HookSet.Powerful   => Localize.Text("Powerful"),
            HookSet.Hook       => Localize.Display(value),
            HookSet.DoubleHook => Localize.Text("Double"),
            HookSet.TripleHook => Localize.Text("Triple"),
            HookSet.Stellar    => Localize.Text("Stellar"),
            HookSet.None       => Localize.Text("None"),
            _                  => Localize.Text("Invalid"),
        };
}
