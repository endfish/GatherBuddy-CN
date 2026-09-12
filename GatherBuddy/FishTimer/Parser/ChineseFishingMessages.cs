namespace GatherBuddy.FishTimer.Parser;

/// <summary>Patterns verified against the Simplified Chinese client's LogMessage sheet.</summary>
internal static class ChineseFishingMessages
{
    // LogMessage 1110, 1115 and 1121. These match game text, not localized UI text.
    internal const string Cast = @"^.*?在(?<FishingSpot>.+)甩出了鱼线开始钓鱼。$";
    internal const string AreaDiscovered = @"^.*?将新钓场[“「](?<FishingSpot>.+)[”」]记录到了钓鱼笔记中！$";
    internal const string Mooch = @"开始利用上钩的.+尝试以小钓大。$";
    // PlaceName 950.
    internal const string Undiscovered = "未知钓场";
}
