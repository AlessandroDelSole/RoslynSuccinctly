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

            if (node is IdentifierNameSyntax == false)
            {
                return;
            }

            context.RegisterCodeFix(
            CodeAction.Create(title:title,
                              createChangedDocument: c=> ReplaceDateTimeAsync(context.Document, node, c), 
                              equivalenceKey:title), diagnostic);
        }


        private async Task<Document> ReplaceDateTimeAsync(Document document, SyntaxNode node,
                         CancellationToken cancellationToken)
        {
            var semanticModel = await document.
                                GetSemanticModelAsync(cancellationToken);
            var root = await document.GetSyntaxRootAsync();

            var convertedNode = (IdentifierNameSyntax)node;

            var newNode = convertedNode.WithIdentifier(SyntaxFactory.
                          ParseToken("DateTimeOffset")).
                          WithLeadingTrivia(node.GetLeadingTrivia()).
                          WithTrailingTrivia(node.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(node, newNode);

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}