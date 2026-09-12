using System.Text.Json;
using System.Text.RegularExpressions;
using GatherBuddy.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Deliberately scans declarations as well as draw calls: many UI labels are cached in fields.
// Keep machine identifiers in the explicit allowlist; review new findings after every upstream sync.
internal static class SourceAudit
{
    public static void Run(string repo, Action<bool, string> check)
    {
        var allowlist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
            File.ReadAllText(Path.Combine(repo, "tests", "GatherBuddy.CN.Tests", "localization-allowlist.json")))!;
        foreach (var folder in new[] { "GatherBuddy", "GatherBuddy.GameData" })
        foreach (var file in Directory.GetFiles(Path.Combine(repo, folder), "*.cs", SearchOption.AllDirectories))
        {
            var path = Path.GetRelativePath(repo, file).Replace('\\', '/');
            if (path.Contains("/bin/") || path.Contains("/obj/") || path.Contains("/Localization/")
                || path.Contains("/Parser/") || path.Contains("/OldRecords/") || path.EndsWith("/WotsitIpc.cs")) continue;
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file), new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
            foreach (var assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>().Where(a => a.Left.ToString() == "Namespace"))
                check(Source(assignment.Right) is not null, "Window namespace stays a literal identity: " + path);
            foreach (var call in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (call.Expression.ToString() is not ("Localize.Text" or "Localize.Label" or "Localize.Format" or "Localize.FormatLabel")) continue;
                if (call.ArgumentList.Arguments.Count == 0 || Source(call.ArgumentList.Arguments[0].Expression) is not { } key) continue;
                check(Localize.Entries.ContainsKey(key.Split("##")[0]), $"Missing translation: {path}: {key}");
            }
            foreach (var node in root.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (Source(node) is not { } source || !source.Any(char.IsAsciiLetter)) continue;
                if (node.Parent is BinaryExpressionSyntax parent && Source(parent) is not null) continue;
                if (source.StartsWith("##") || !Regex.Replace(source, @"\{\d+(?:,-?\d+)?(?::[^{}]+)?\}", "").Any(char.IsAsciiLetter)) continue;
                if (Protected(node)) continue;
                // Built-in fish comments are localized at the assignment point; override files stay user-authored.
                if (path.Contains("/Data/Fish/"))
                {
                    if (node.Parent?.Parent?.Parent is InvocationExpressionSyntax comment && comment.Expression.ToString().EndsWith(".Comment"))
                        check(Localize.Entries.ContainsKey(source), "Missing built-in fish guide: " + source);
                    continue;
                }
                check(allowlist.TryGetValue(path, out var allowed) && allowed.TryGetValue(source, out var reason) && !string.IsNullOrWhiteSpace(reason),
                    $"Unreviewed English text: {path}:{root.SyntaxTree.GetLineSpan(node.Span).StartLinePosition.Line + 1}: {source}");
            }
        }
        Console.WriteLine("Source text audit: passed (documented machine identifiers excluded).");
    }

    private static bool Protected(SyntaxNode node)
    {
        if (node.Ancestors().OfType<MethodDeclarationSyntax>().Any(m => m.Identifier.ValueText is "CreateTsv" or "DrawCosmicFishDataButton")) return true;
        if (node.Ancestors().Any(n => n is AttributeSyntax or ConstantPatternSyntax or CaseSwitchLabelSyntax)) return true;
        foreach (var call in node.Ancestors().OfType<InvocationExpressionSyntax>())
        {
            var name = call.Expression.ToString();
            if (call.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "Localize" } }
                || name.Contains(".Log.") || name.StartsWith("Log.") || name.Contains("Logger.")) return true;
            if (name.EndsWith(".PushId") || name.EndsWith(".PushID") || name.EndsWith(".GetID") || name.EndsWith(".OpenPopup")
                || name.EndsWith(".Child") || name.EndsWith(".Table") || name.EndsWith(".BeginTable") || name.EndsWith(".TabBar")
                || name.EndsWith(".GetExcelSheet") || name.Contains(".Register") || name.Contains(".GetManifestResourceStream")) return true;
        }
        return node.Parent is BinaryExpressionSyntax b && b.Kind() is SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression;
    }

    private static string? Source(ExpressionSyntax node) => node switch
    {
        LiteralExpressionSyntax lit when lit.IsKind(SyntaxKind.StringLiteralExpression) || lit.IsKind(SyntaxKind.Utf8StringLiteralExpression) => lit.Token.ValueText,
        BinaryExpressionSyntax bin when bin.IsKind(SyntaxKind.AddExpression) && Constant(bin.Left) is { } l && Constant(bin.Right) is { } r => l + r,
        InterpolatedStringExpressionSyntax str => Interpolation(str),
        _ => null,
    };
    private static string? Constant(ExpressionSyntax node) => node is InterpolatedStringExpressionSyntax ? null : Source(node);
    private static string Interpolation(InterpolatedStringExpressionSyntax str)
    {
        var index = 0;
        return string.Concat(str.Contents.Select(c => c switch
        {
            InterpolatedStringTextSyntax text => text.TextToken.ValueText,
            InterpolationSyntax i => "{" + index++ + i.AlignmentClause?.ToString() + i.FormatClause?.ToString() + "}",
            _ => "",
        }));
    }
}
