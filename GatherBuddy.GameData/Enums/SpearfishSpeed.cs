namespace GatherBuddy.Enums;

public enum SpearfishSpeed : ushort
{
    Unknown       = 0,
    SuperSlow     = 100,
    ExtremelySlow = 150,
    VerySlow      = 200,
    Slow          = 250,
    Average       = 300,
    Fast          = 350,
    VeryFast      = 400,
    ExtremelyFast = 450,
    SuperFast     = 500,
    HyperFast     = 550,
    LynFast       = 600,

    None = ushort.MaxValue,
}

public static class SpearFishSpeedExtensions
{
    public static string ToName(this SpearfishSpeed speed)
        => speed switch
        {
            SpearfishSpeed.Unknown       => Localize.Text("Unknown Speed"),
            SpearfishSpeed.SuperSlow     => Localize.Text("Super Slow"),
            SpearfishSpeed.ExtremelySlow => Localize.Text("Extremely Slow"),
            SpearfishSpeed.VerySlow      => Localize.Text("Very Slow"),
            SpearfishSpeed.Slow          => Localize.Text("Slow"),
            SpearfishSpeed.Average       => Localize.Display(speed),
            SpearfishSpeed.Fast          => Localize.Text("Fast"),
            SpearfishSpeed.VeryFast      => Localize.Text("Very Fast"),
            SpearfishSpeed.ExtremelyFast => Localize.Text("Extremely Fast"),
            SpearfishSpeed.SuperFast     => Localize.Text("Super Fast"),
            SpearfishSpeed.HyperFast     => Localize.Text("Hyper Fast"),
            SpearfishSpeed.LynFast       => Localize.Text("Mega Fast"),
            SpearfishSpeed.None          => Localize.Text("No Speed"),
            _                            => $"{(ushort)speed}",
        };
}