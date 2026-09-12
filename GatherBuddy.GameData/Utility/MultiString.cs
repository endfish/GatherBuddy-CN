using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace GatherBuddy.Utility;

public readonly struct MultiString(string en, string de, string fr, string jp, string zh = "")
{
    public static string ParseSeStringLumina(ReadOnlySeString? luminaString)
        => luminaString?.ExtractText() ?? string.Empty;

    public readonly string English  = en;
    public readonly string German   = de;
    public readonly string French   = fr;
    public readonly string Japanese = jp;
    public readonly string Chinese  = zh;

    // The CN Dalamud fork extends ClientLanguage with Simplified Chinese (4).
    public const ClientLanguage ChineseLanguage = (ClientLanguage)4;

    public string this[ClientLanguage lang]
        => Name(lang);

    public override string ToString()
        => Name(ClientLanguage.English);

    public string ToWholeString()
        => $"{English}|{German}|{French}|{Japanese}";


    public static MultiString FromPlaceName(IDataManager gameData, uint id)
    {
        if (gameData.Language == ChineseLanguage)
        {
            var name = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>().GetRowOrDefault(id)?.Name);
            return new MultiString(name, name, name, name, name);
        }
        var en = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.English).GetRowOrDefault(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.German).GetRowOrDefault(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.French).GetRowOrDefault(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.Japanese).GetRowOrDefault(id)?.Name);
        return new MultiString(en, de, fr, jp);
    }

    public static MultiString FromItem(IDataManager gameData, uint id)
    {
        if (gameData.Language == ChineseLanguage)
        {
            var name = ParseSeStringLumina(gameData.GetExcelSheet<Item>().GetRowOrDefault(id)?.Name);
            return new MultiString(name, name, name, name, name);
        }
        var en = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.English).GetRowOrDefault(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.German).GetRowOrDefault(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.French).GetRowOrDefault(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.Japanese).GetRowOrDefault(id)?.Name);
        return new MultiString(en, de, fr, jp);
    }

    private string Name(ClientLanguage lang)
    {
        var name = lang switch
        {
            ClientLanguage.English  => English,
            ClientLanguage.German   => German,
            ClientLanguage.Japanese => Japanese,
            ClientLanguage.French   => French,
            ChineseLanguage         => Chinese,
            _                       => English,
        };
        if (!string.IsNullOrEmpty(name))
            return name;
        foreach (var fallback in new[] { English, Chinese, Japanese, German, French })
            if (!string.IsNullOrEmpty(fallback))
                return fallback;
        return string.Empty;
    }

    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);
}
