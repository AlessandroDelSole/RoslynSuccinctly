<ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name:=NameOf(PublicFieldRefactoring_VBCodeRefactoringProvider)), [Shared]>
Friend Class PublicFieldRefactoring_VBCodeRefactoringProvider
    Inherits CodeRefactoringProvider

    Public NotOverridable Overrides Async Function ComputeRefactoringsAsync(context As CodeRefactoringContext) As Task
        ' TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

        Dim root = Await context.Document.
            GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

        ' Find the node at the selection.
        Dim node = root.FindNode(context.Span)

        Dim fieldDecl = TryCast(node, FieldDeclarationSyntax)
        If fieldDecl Is Nothing Or fieldDecl.Modifiers.
            ToFullString.Contains("Public") = False Then
            Return
        End If

        Dim mustRegisterAction As Boolean

        For Each declarator In fieldDecl.Declarators
            'If at least one starting character is upper case, must register an action 
            If declarator.Names.Any(Function(d) _
                                        Char.IsLower(d.Identifier.Value.ToString(0))) Then
                mustRegisterAction = True
            Else
                mustRegisterAction = False
            End If
        Next

        If mustRegisterAction = False Then
            Return
        Else
            Dim action = CodeAction.Create("Make first char upper case",
                                           Function(c) RenameFieldAsync(context.
                                           Document, fieldDecl, c))

            ' Register this code action.
            context.RegisterRefactoring(action)
        End If
    End Function

    Private Async Function RenameFieldAsync(document As Document,
                                            fieldDeclaration As FieldDeclarationSyntax,
                                        cancellationToken As CancellationToken) _
                                        As Task(Of Document)

        Dim semanticModel = Await document.
            GetSemanticModelAsync(cancellationToken).
            ConfigureAwait(False)

        Dim root = Await document.GetSyntaxRootAsync

        Dim oldDeclarators = fieldDeclaration.Declarators
        Dim listOfNewModifiedIdentifiers As _
            New SeparatedSyntaxList(Of ModifiedIdentifierSyntax)
        Dim listOfNewModifiedDeclarators As _
            New SeparatedSyntaxList(Of VariableDeclaratorSyntax)

        'Iterate the declarators collection
        For Each declarator In oldDeclarators
            'For each variable name in the declarator...
            For Each modifiedIdentifier In declarator.Names
                'Get a new proper name
                Dim tempString = ConvertName(modifiedIdentifier.ToFullString)

                'Generate a new ModifiedIdentifierSyntax based on
                'the previous one's properties but with a new Identifier
                Dim newModifiedIdentifier As ModifiedIdentifierSyntax =
                    modifiedIdentifier.
                    WithIdentifier(SyntaxFactory.ParseToken(tempString)).
                    WithTrailingTrivia(modifiedIdentifier.GetTrailingTrivia)

                'Add the new element to the collection
                listOfNewModifiedIdentifiers =
                    listOfNewModifiedIdentifiers.Add(newModifiedIdentifier)
            Next
            'Store a new variable declarator with new
            'variable names
            listOfNewModifiedDeclarators =
                listOfNewModifiedDeclarators.Add(declarator.
                WithNames(listOfNewModifiedIdentifiers))

            'Clear the list before next iteration
            listOfNewModifiedIdentifiers = Nothing
            listOfNewModifiedIdentifiers = New _
                SeparatedSyntaxList(Of ModifiedIdentifierSyntax)
        Next


        Dim newField = fieldDeclaration.
            WithDeclarators(listOfNewModifiedDeclarators)

        Dim newRoot As SyntaxNode =
            root.ReplaceNode(fieldDeclaration, newField)

        Dim newDocument =
            document.WithSyntaxRoot(newRoot)

        Return newDocument
    End Function

    Private Function ConvertName(oldName As String) As String
        Return Char.ToUpperInvariant(oldName(0)) + oldName.Substring(1)
    End Function
End Class