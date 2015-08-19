using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace PublicFieldRefactoring
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(PublicFieldRefactoringCodeRefactoringProvider)), Shared]
    internal class PublicFieldRefactoringCodeRefactoringProvider : CodeRefactoringProvider
    {

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            var fieldDecl = node as FieldDeclarationSyntax;
            if (fieldDecl == null || fieldDecl.Modifiers.ToFullString().Contains("public") == false) { return; }

            bool mustRegisterAction;

            foreach (var declarator in fieldDecl.Declarators)
            {
                if (declarator.Names.Any(d=> char.IsUpper(d.Identifier.Value.ToString[0]))) {
                    mustRegisterAction = true;
                }
                else
                {
                    mustRegisterAction = false;
                }
            }

            
            if (mustRegisterAction==false)
            {
                return;
            }

            var action = CodeAction.Create("Make first char upper case",
                               c => RenameFieldAsync(context.
                               Document, fieldDecl, c));

            context.RegisterRefactoring(action);       
        }

        private async Task<Document> RenameFieldAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync();

            var oldDeclarators = fieldDeclaration.Modifiers;

            Dim listOfNewModifiedIdentifiers As _
            New SeparatedSyntaxList(Of ModifiedIdentifierSyntax)
        Dim listOfNewModifiedDeclarators As _
            New SeparatedSyntaxList(Of VariableDeclaratorSyntax)
        }

        private string ConvertName(string oldName)
        {
            return char.ToUpperInvariant(oldName[0]) + oldName.Substring(1);
        }
    }
}