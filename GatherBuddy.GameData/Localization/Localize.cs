using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GatherBuddy.Localization;

/// <summary>Source-text localization shared by the UI and game-data display helpers.</summary>
public static class Localize
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("zh-CN");
    private static readonly FrozenDictionary<string, string> Translations = Load();
    public static IReadOnlyDictionary<string, string> Entries => Translations;

    private static FrozenDictionary<string, string> Load()
    {
        using var stream = typeof(Localize).Assembly.GetManifestResourceStream("GatherBuddy.Localization.zh-CN.json")
            ?? throw new FileNotFoundException("Missing embedded GatherBuddy Chinese translation resource.");
        var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
            ?? throw new InvalidDataException("Invalid GatherBuddy Chinese translation resource.");
        return entries.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public static string Text(string source)
    {
        var id = source.IndexOf("##", StringComparison.Ordinal);
        var visible = id < 0 ? source : source[..id];
        if (visible.Length == 0 || !Translations.TryGetValue(visible, out var translated))
            return source;
        return id < 0 ? translated : translated + source[id..];
    }

    /// <summary>Keep explicit ### IDs; otherwise derive a stable ID from the untranslated source.</summary>
    public static string Label(string source)
    {
        var translated = Text(source);
        return source.StartsWith("##", StringComparison.Ordinal) || source.Contains("###", StringComparison.Ordinal)
            ? translated
            : translated + "###" + source;
    }

    public static string Format(string source, params object?[] arguments)
        => string.Format(Culture, Text(source), arguments);

    public static string Display(object? value)
    {
        if (value is null)
            return string.Empty;
        var source = value.ToString() ?? string.Empty;
        if (!value.GetType().IsEnum)
            return source;
        var type = value.GetType().Name;
        return string.Join("、", source.Split(", ", StringSplitOptions.None).Select(name
            => Translations.GetValueOrDefault($"enum.{type}.{name}", Text(name))));
    }

    public static string[] EnumNames<T>() where T : struct, Enum
        => Enum.GetValues<T>().Select(value => Display(value)).ToArray();

    public static string FormatLabel(string source, params object?[] arguments)
    {
        var translated = Format(source, arguments);
        var original = string.Format(Culture, source, arguments);
        return original.StartsWith("##", StringComparison.Ordinal) || original.Contains("###", StringComparison.Ordinal)
            ? translated
            : translated + "###" + original;
    }
}
