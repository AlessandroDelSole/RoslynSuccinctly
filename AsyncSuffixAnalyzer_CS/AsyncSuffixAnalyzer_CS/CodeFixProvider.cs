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

namespace AsyncSuffixAnalyzer_CS
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncSuffixAnalyzer_CSCodeFixProvider)), Shared]
    public class AsyncSuffixAnalyzer_CSCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add Async suffix to asynchronous methods";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AsyncSuffixAnalyzer_CSAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(
             CodeAction.Create(
                 title: title,
                 createChangedSolution: c => MakeMethodNameAsync(context.Document, declaration, c),
                 equivalenceKey: title),
             diagnostic);
        }

        private async Task<Solution> MakeMethodNameAsync(Document document, MethodDeclarationSyntax methodStmt, CancellationToken cancellationToken)
        {
            var identifierToken = methodStmt.Identifier;
            var newName = identifierToken.Text + "Async";
            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodStmt, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, methodSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;

        }
    }
}