namespace GatherBuddy.Localization;

/// <summary>Only replace untouched upstream defaults; all user-authored templates stay intact.</summary>
public static class MessageTemplates
{
    public const string Identified = "Identified {Item} for \"{Input}\".";
    public const string Alarm = "{Alarm} {Item} {DelayString} at {Location}.";

    public static string MigrateDefault(string current, string upstreamDefault)
        => current == upstreamDefault ? Localize.Text(upstreamDefault) : current;
}
