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
                await IsApplicableAsync(context, node).ConfigureAwait(false))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Execute ObsoleteScript to resolve",
                        token => CreateChangedDocument(context, root, diagnostic, node, token),
                        equivalenceKey: nameof(ObsoleteScriptCodeFixProvider)),
                    diagnostic);
            }
        }

        private async Task<bool> IsApplicableAsync(CodeFixContext context, SyntaxNode node)
        {
            if (await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false) is not { } semanticModel)
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(node, context.CancellationToken).Symbol;
            return symbol?.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(ObsoleteScriptAttribute)) == true;
        }

        private async Task<Document> CreateChangedDocument(
            CodeFixContext context,
            SyntaxNode root,
            Diagnostic diagnostic,
            InvocationExpressionSyntax node,
            CancellationToken token)
        {
            // if (node is IdentifierNameSyntax identifier)
            // {
            //     var newIdentifier = SyntaxFactory.IdentifierName("NewReplacementMethod")
            //         .WithTriviaFrom(identifier); // Preserves spaces and comments
            // 
            //     var newRoot = root.ReplaceNode(identifier, newIdentifier);
            //     return context.Document.WithSyntaxRoot(newRoot);
            // }

            return context.Document;
        }
    }
}
