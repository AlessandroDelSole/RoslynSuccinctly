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
            // Get the root syntax node for the current document
            var root = await context.Document.
                GetSyntaxRootAsync(context.CancellationToken).
                ConfigureAwait(false);

            // Get a reference to the diagnostic to fix
            var diagnostic = context.Diagnostics.First();
            // Get the location in the code editor for the diagnostic
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the syntax node on the span 
            // where there is a squiggle
            var node = root.FindNode(context.Span);

            // If the syntax node is not an IdentifierName
            // return
            if (node is IdentifierNameSyntax == false)
            {
                return;
            }

            // Register a code action that invokes the fix
            // on the current document
            context.RegisterCodeFix(
            CodeAction.Create(title:title,
                              createChangedDocument: 
                              c=> ReplaceDateTimeAsync(context.Document, 
                              node, c), 
                              equivalenceKey:title), diagnostic);
        }


        private async Task<Document> ReplaceDateTimeAsync(Document document, 
                         SyntaxNode node,
                         CancellationToken cancellationToken)
        {
            // Get the root syntax node for the current document
            var root = await document.GetSyntaxRootAsync();

            // Convert the syntax node into the specialized kind
            var convertedNode = (IdentifierNameSyntax)node;

            // Create a new syntax node
            var newNode = convertedNode?.WithIdentifier(SyntaxFactory.
                          ParseToken("DateTimeOffset")).
                          WithLeadingTrivia(node.GetLeadingTrivia()).
                          WithTrailingTrivia(node.GetTrailingTrivia());

            // Create a new root syntax node for the current document
            // replacing the syntax node that has diagnostic with
            // a new syntax node
            var newRoot = root.ReplaceNode(node, newNode);

            // Generate a new document
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}