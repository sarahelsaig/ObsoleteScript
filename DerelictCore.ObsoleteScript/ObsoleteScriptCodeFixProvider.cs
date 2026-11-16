using DerelictCore.ObsoleteScript.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DerelictCore.ObsoleteScript
{
    /// <summary>
    /// The code fix provider for member calls marked with <see cref="ObsoleteAttribute"/> (i.e. compiler warnings
    /// CS0612 and CS0618) when the original member is also decorated with <see cref="ObsoleteScriptAttribute"/>.
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
                await GetScript(context, node).ConfigureAwait(false) is { Length: >0 } script)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Execute ObsoleteScript to resolve",
                        token => CreateChangedDocument(context, root, diagnostic, node, script, token),
                        equivalenceKey: nameof(ObsoleteScriptCodeFixProvider)),
                    diagnostic);
            }
        }

        private async Task<string?> GetScript(CodeFixContext context, SyntaxNode node)
        {
            var token = context.CancellationToken;
            if (await context.Document.GetSemanticModelAsync(token).ConfigureAwait(false) is not { } semanticModel)
            {
                return null;
            }

            var symbol = semanticModel.GetSymbolInfo(node, token).Symbol;
            return symbol?
                .GetAttributes()
                .First(attribute => attribute.AttributeClass?.Name == nameof(ObsoleteScriptAttribute))
                .ConstructorArguments
                .FirstOrDefault()
                .Value?
                .ToString();
        }

        private async Task<Document> CreateChangedDocument(
            CodeFixContext context,
            SyntaxNode root,
            Diagnostic diagnostic,
            InvocationExpressionSyntax node,
            string script,
            CancellationToken token)
        {
            if (script.Contains(ScriptElements.SubstitutionSeparator))
            {
                var parts = script.Split([ScriptElements.SubstitutionSeparator], 2, StringSplitOptions.None);
                var from = parts[0].Trim();
                var to = parts[1].Trim();

                var identifier = (IdentifierNameSyntax)node.DescendantNodes()
                    .FirstOrDefault(node => (node as IdentifierNameSyntax)?.Identifier.ValueText == from);
                var newIdentifier = SyntaxFactory.IdentifierName(to)
                    .WithTriviaFrom(identifier); // Preserves spaces and comments

                var newRoot = root.ReplaceNode(identifier, newIdentifier);
                return context.Document.WithSyntaxRoot(newRoot);
            }

            return context.Document;
        }
    }
}
