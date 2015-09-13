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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, 
        Name = nameof(PublicFieldRefactoringCodeRefactoringProvider)), Shared]
    internal class PublicFieldRefactoringCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task 
            ComputeRefactoringsAsync(
            CodeRefactoringContext context)
        {
            // Get the root syntax node
            var root = await context.Document.
                GetSyntaxRootAsync(context.CancellationToken).
                ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Convert the node into a field declaration
            var fieldDecl = node as FieldDeclarationSyntax;
            // If it's not public, return
            if (fieldDecl == null || 
                fieldDecl.Modifiers.ToFullString().
                Contains("public") == false) { return; }

            // Used to determine if an action must
            // be registered
            bool mustRegisterAction = false;

            // If at least one starting character is
            // lower case, must register an action
            if (fieldDecl.Declaration.
                Variables.Any(i => 
                char.IsLower(i.Identifier.
                ValueText.ToCharArray().First())))
            {
                mustRegisterAction = true;
            }
            else
            {
                mustRegisterAction = false;
            }

            
            if (mustRegisterAction==false)
            {
                return;
            }

            // Create an action invoking a delegate passing
            // the document instance, the syntax node, and
            // a cancellation token
            var action = 
                CodeAction.Create("Make first char upper case",
                           c => RenameFieldAsync(context.
                           Document, fieldDecl, c));

            // Register this code action
            context.RegisterRefactoring(action);       
        }

        private async Task<Document> 
            RenameFieldAsync(Document document, 
            FieldDeclarationSyntax fieldDeclaration, 
            CancellationToken cancellationToken)
        {
            // Get the semantic model for the code file
            var semanticModel = 
                await document.
                      GetSemanticModelAsync(cancellationToken).
                      ConfigureAwait(false);

            // Get the root syntax node
            var root = await document.GetSyntaxRootAsync();

            // Get a list of old variable declarations
            var oldDeclarators = 
                fieldDeclaration.Declaration.
                Variables;

            // Store the collection of variables
            var listOfNewModifiedDeclarators = 
                new SeparatedSyntaxList<
                    VariableDeclaratorSyntax>();

            // Iterate the declarators collection
            foreach (var declarator in oldDeclarators) {
                // Get a new name
                var tempString = 
                    ConvertName(declarator.Identifier.
                    ToFullString());

                // Generate a new modified declarator
                // based on the previous one but with
                // a new identifier
                listOfNewModifiedDeclarators = 
                    listOfNewModifiedDeclarators.
                    Add(declarator.WithIdentifier(
                        SyntaxFactory.ParseToken(tempString)));
            }

            // Generate a new field declaration
            // with updated variable names
            var newDeclaration = 
                fieldDeclaration.Declaration.
                WithVariables(listOfNewModifiedDeclarators);

            // Generate a new FieldDeclarationSyntax
            var newField = fieldDeclaration.
                WithDeclaration(newDeclaration);

            // Replace the old syntax node with the new one
            SyntaxNode newRoot = root.
                       ReplaceNode(fieldDeclaration, 
                       newField);

            // Generate a new document
            var newDocument = 
                document.WithSyntaxRoot(newRoot);

            // Return the document
            return newDocument;
       }

        // Return a new identifier with an
        // uppercase letter
        private string ConvertName(string oldName)
        {
            return char.
                   ToUpperInvariant(oldName[0]) + 
                   oldName.Substring(1);
        }
    }
}