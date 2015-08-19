using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;

namespace DateTimeAnalyzer_CS
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DateTimeAnalyzer_CSCodeFixProvider)), Shared]
    public class DateTimeAnalyzer_CSCodeFixProvider : CodeFixProvider
    {
        private const string title = "Replace with DateTimeOffset";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DateTimeAnalyzer_CSAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(context.Span);

            if (node is PredefinedTypeSyntax) {
                context.RegisterCodeFix(
                CodeAction.Create("Replace with DateTimeOffset",
                                  c => ReplaceDateAsync(context.Document, node, c), title), diagnostic);
            }
            else {
                context.RegisterCodeFix(
                CodeAction.Create("Replace with DateTimeOffset",
                                  c=> ReplaceDateTimeAsync(context.Document, node, c), title), diagnostic);
            }
        }


        private async Task<Document> ReplaceDateAsync(Document document, SyntaxNode node,
                                 CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync();
            var generator = SyntaxGenerator.GetGenerator(document);

            var newIdentifierSyntax = generator.IdentifierName("DateTimeOffset").WithTrailingTrivia(node.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(node, newIdentifierSyntax);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;

        }

        private async Task<Document> ReplaceDateTimeAsync(Document document, SyntaxNode node,
                         CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var root = await document.GetSyntaxRootAsync();

            var convertedNode = (PredefinedTypeSyntax)node;

            var node2 = convertedNode.WithKeyword(SyntaxFactory.ParseToken("DateTimeOffset")).WithTrailingTrivia(node.GetTrailingTrivia());
            var newRoot = root.ReplaceNode(node, node2);

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}