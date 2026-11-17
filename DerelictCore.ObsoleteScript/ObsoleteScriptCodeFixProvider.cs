using DerelictCore.ObsoleteScript.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DerelictCore.ObsoleteScript;

/// <summary>
/// The code fix provider for member calls marked with <c>[Obsolete]</c> (i.e. compiler warnings
/// CS0612 and CS0618) when the original member is also decorated with <c>[ObsoleteScript]</c>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObsoleteScriptCodeFixProvider))]
public class ObsoleteScriptCodeFixProvider : CodeFixProvider
{
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ["CS0612", "CS0618"];

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.FirstOrDefault() is { Location.SourceSpan: { } span } diagnostic &&
            await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is { } root &&
            root.FindNode(span) is InvocationExpressionSyntax node &&
            await GetNameAndScriptAsync(context, node).ConfigureAwait(false) is ({ Length: >0 } name, { Length: >0 } script))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Execute ObsoleteScript to resolve",
                    token => CreateChangedDocumentAsync(context, root, diagnostic, node, name, script, token),
                    equivalenceKey: nameof(ObsoleteScriptCodeFixProvider)),
                diagnostic);
        }
    }

    private async Task<(string? Name, string? Script)> GetNameAndScriptAsync(CodeFixContext context, SyntaxNode node)
    {
        var token = context.CancellationToken;
        if (await context.Document.GetSemanticModelAsync(token).ConfigureAwait(false) is not { } semanticModel)
        {
            return (null, null);
        }

        var symbol = semanticModel.GetSymbolInfo(node, token).Symbol;
        var script = symbol?
            .GetAttributes()
            .First(attribute => attribute.AttributeClass?.Name == nameof(ObsoleteScriptAttribute))
            .ConstructorArguments
            .FirstOrDefault()
            .Value?
            .ToString();
        return (symbol?.Name, script);
    }

    private async Task<Document> CreateChangedDocumentAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        InvocationExpressionSyntax node,
        string name,
        string script,
        CancellationToken token)
    {
        Document ReplaceNode(SyntaxNode from, SyntaxNode to)
        {
            var newRoot = root.ReplaceNode(from, to);
            return context.Document.WithSyntaxRoot(newRoot);
        }

        var identifier = GetIdentifier(node, name);
        var allNodes = node.DescendantNodes().ToList();

        if (script.Contains(ScriptElements.SubstitutionSeparator))
        {
            var parts = script.Split([ScriptElements.SubstitutionSeparator], 2, StringSplitOptions.None);
            var from = parts[0].Trim();
            var to = parts[1].Trim();

            if (from.Length == 0) from = name;

            identifier = GetIdentifier(node, from);
            var newIdentifier = SyntaxFactory.IdentifierName(to)
                .WithTriviaFrom(identifier); // Preserves spaces and comments
            return ReplaceNode(identifier, newIdentifier);
        }

        if (script.StartsWith("overloads:", StringComparison.OrdinalIgnoreCase))
        {
            var json = script.Substring("overloads:".Length).Replace('`', '"');
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            var argumentsList = node.DescendantNodes().OfType<ArgumentListSyntax>().First().ChildNodes().ToList();

            if (dictionary.TryGetValue(argumentsList.Count.ToString(CultureInfo.InvariantCulture), out var template))
            {
                var prefix = node.ToString().Split([name], StringSplitOptions.None)[0];
                object[] formatArgs = [prefix, .. argumentsList];
                var newCode = string.Format(CultureInfo.InvariantCulture, template, formatArgs);
                return ReplaceNode(node, SyntaxFactory.ParseExpression(newCode).WithTriviaFrom(node));
            }
        }

        return context.Document;
    }

    private IdentifierNameSyntax GetIdentifier(InvocationExpressionSyntax invocation, string name) =>
        invocation
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .FirstOrDefault(node => node.Identifier.ValueText == name);
}
