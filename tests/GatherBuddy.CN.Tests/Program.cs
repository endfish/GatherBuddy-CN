using System.Reflection;
using System.Text.RegularExpressions;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using GatherBuddy.FishTimer.Parser;
using GatherBuddy.Utility;
using Lumina;
using Lumina.Data;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"FAIL: {name}");
    ++checks;
}

var names = new MultiString("Fire Shard", "", "", "", "火之碎晶");
Check(names[MultiString.ChineseLanguage] == "火之碎晶", "Chinese name");
Check(names[ClientLanguage.German] == "Fire Shard", "Missing name fallback");
Check(names[(ClientLanguage)99] == "Fire Shard", "Unknown language fallback");
Check(new MultiString("", "", "", "", "火之碎晶")[ClientLanguage.English] == "火之碎晶", "CN-only data");
Check(MultiString.Empty[MultiString.ChineseLanguage] == "", "Empty names");
var options = RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture;
var cast = new Regex(ChineseFishingMessages.Cast, options);
var discovered = new Regex(ChineseFishingMessages.AreaDiscovered, options);
var mooch = new Regex(ChineseFishingMessages.Mooch, options);
Check(cast.Match("测试角色在白银乡甩出了鱼线开始钓鱼。").Groups["FishingSpot"].Value == "白银乡", "CN cast");
Check(discovered.Match("将新钓场“白银乡”记录到了钓鱼笔记中！").Groups["FishingSpot"].Value == "白银乡", "CN discovery");
Check(mooch.IsMatch("测试角色开始利用上钩的沙蚕尝试以小钓大。"), "CN mooch");
Check(!cast.IsMatch("无法开始钓鱼。"), "Ignore failed cast");
Check(cast.Match("测试角色在未知钓场甩出了鱼线开始钓鱼。").Groups["FishingSpot"].Value == ChineseFishingMessages.Undiscovered, "Unknown fishing spot");

if (args.Length > 0)
{
    using var client = new GameData(args[0], new LuminaOptions { DefaultExcelLanguage = Language.ChineseSimplified });
    IDataManager proxy = new ClientDataManager(client);
    var sink = new CaptureSink();
    Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(sink).CreateLogger();
    var data = new GatherBuddy.GameData(proxy, new OtterGui.Log.Logger(), "");
    foreach (var error in sink.Events.Where(e => e.Level >= LogEventLevel.Error))
        Console.WriteLine(error.RenderMessage());
    Check(!sink.Events.Any(e => e.RenderMessage().Contains("Error while setting up data")), "Complete CN database initialization");
    Check(data.Gatherables.Count > 1000 && data.Fishes.Count > 1000, "Gatherable and fish counts");
    Check(data.OceanRoutes.Count > 0 && data.FishingSpots.Count > 0, "Ocean and fishing data");
    Check(MultiString.FromItem(proxy, 2)[MultiString.ChineseLanguage] == "火之碎晶", "Names from CN client");
    Check(data.GatherablesTrie.FuzzyFind("火之碎晶", 1, out var shard) == 0 && shard?.ItemId == 2, "CN search trie");
    Console.WriteLine($"CN data: {data.Gatherables.Count} gatherables, {data.Fishes.Count} fish, {data.FishingSpots.Count} spots, {data.OceanRoutes.Count} ocean routes.");
}
Console.WriteLine($"PASS: {checks} checks.");

internal sealed class ClientDataManager(GameData client) : IDataManager
{
    public ClientLanguage Language => MultiString.ChineseLanguage;
    public GameData GameData => client;
    public Lumina.Excel.ExcelModule Excel => client.Excel;
    public bool HasModifiedGameDataFiles => false;
    private static Lumina.Data.Language ToLanguage(ClientLanguage? language) => language is {} l ? (Lumina.Data.Language)((int)l + 1) : Lumina.Data.Language.ChineseSimplified;
    public Lumina.Excel.ExcelSheet<T> GetExcelSheet<T>(ClientLanguage? language = null, string? name = null) where T : struct, Lumina.Excel.IExcelRow<T>
        => client.GetExcelSheet<T>(ToLanguage(language), name) ?? throw new InvalidOperationException("Missing sheet: " + typeof(T).Name);
    public Lumina.Excel.SubrowExcelSheet<T> GetSubrowExcelSheet<T>(ClientLanguage? language = null, string? name = null) where T : struct, Lumina.Excel.IExcelSubrow<T>
        => client.GetSubrowExcelSheet<T>(ToLanguage(language), name) ?? throw new InvalidOperationException("Missing subrow sheet: " + typeof(T).Name);
    public FileResource? GetFile(string path) => client.GetFile(path);
    public T? GetFile<T>(string path) where T : FileResource => client.GetFile<T>(path);
    public Task<T> GetFileAsync<T>(string path, CancellationToken cancellationToken) where T : FileResource => Task.FromResult(client.GetFile<T>(path)!);
    public bool FileExists(string path) => client.FileExists(path);
}
internal sealed class CaptureSink : ILogEventSink
{
    public readonly List<LogEvent> Events = [];
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
