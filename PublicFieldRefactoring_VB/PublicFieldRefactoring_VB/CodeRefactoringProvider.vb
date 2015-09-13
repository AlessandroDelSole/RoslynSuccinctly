<ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name:=NameOf(PublicFieldRefactoring_VBCodeRefactoringProvider)), [Shared]>
Friend Class PublicFieldRefactoring_VBCodeRefactoringProvider
    Inherits CodeRefactoringProvider

    Public NotOverridable Overrides Async _
        Function ComputeRefactoringsAsync(
                 context As CodeRefactoringContext) As Task

        'Get the root syntax node
        Dim root = Await context.Document.
            GetSyntaxRootAsync(context.
            CancellationToken).ConfigureAwait(False)

        ' Find the node at the selection.
        Dim node = root.FindNode(context.Span)

        'Convert the node to a field declaration
        Dim fieldDecl = TryCast(node,
            FieldDeclarationSyntax)

        'If it's not Public, return
        If fieldDecl Is Nothing Or fieldDecl.Modifiers.
            ToFullString.Contains("Public") = False Then
            Return
        End If

        'Used to determine if an action
        'must be registered
        Dim mustRegisterAction As Boolean

        'Iterate the variable declarators
        For Each declarator
            In fieldDecl.Declarators

            'If at least one starting character 
            'is lower case, must register an action 
            If declarator.Names.Any(Function(d) _
                          Char.IsLower(d.Identifier.
                          Value.ToString(0))) Then
                mustRegisterAction = True
            Else
                mustRegisterAction = False
            End If
        Next

        If mustRegisterAction = False Then
            Return
        Else
            'Create an action invoking a delegate passing
            'the document instance, the syntax node, and
            'a cancellation token
            Dim action =
                CodeAction.Create("Make first char upper case",
                           Function(c) RenameFieldAsync(context.
                           Document, fieldDecl, c))

            ' Register this code action.
            context.RegisterRefactoring(action)
        End If
    End Function

    Private Async Function _
        RenameFieldAsync(document As Document,
                         fieldDeclaration As FieldDeclarationSyntax,
                         cancellationToken As CancellationToken) _
                         As Task(Of Document)

        'Get the semantic model for the code file
        Dim semanticModel = Await document.
            GetSemanticModelAsync(cancellationToken).
            ConfigureAwait(False)

        'Get the root syntax node
        Dim root = Await document.GetSyntaxRootAsync

        'Get a list of old declarators
        Dim oldDeclarators =
            fieldDeclaration.Declarators

        'Store the collection of identifiers
        Dim listOfNewModifiedIdentifiers As _
            New SeparatedSyntaxList(
            Of ModifiedIdentifierSyntax)
        'Store the collection of declarators
        Dim listOfNewModifiedDeclarators As _
            New SeparatedSyntaxList(
            Of VariableDeclaratorSyntax)

        'Iterate the declarators collection
        For Each declarator In oldDeclarators
            'For each variable name in the declarator...
            For Each modifiedIdentifier In declarator.Names
                'Get a new name
                Dim tempString =
                    ConvertName(modifiedIdentifier.
                    ToFullString())

                'Generate a new ModifiedIdentifierSyntax based on
                'the previous one's properties but with a new Identifier
                Dim newModifiedIdentifier As _
                    ModifiedIdentifierSyntax =
                    modifiedIdentifier.
                    WithIdentifier(SyntaxFactory.
                    ParseToken(tempString)).
                    WithTrailingTrivia(modifiedIdentifier.
                    GetTrailingTrivia)

                'Add the new element to the collection
                listOfNewModifiedIdentifiers =
                    listOfNewModifiedIdentifiers.
                    Add(newModifiedIdentifier)
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

        'Generate a new FieldDeclarationSyntax
        Dim newField = fieldDeclaration.
            WithDeclarators(listOfNewModifiedDeclarators)

        'Replace the old declaration with the new one
        Dim newRoot As SyntaxNode =
            root.ReplaceNode(fieldDeclaration,
                             newField)

        'Generate a new document
        Dim newDocument =
            document.WithSyntaxRoot(newRoot)

        'Return the new document
        Return newDocument
    End Function

    'Return a new identifier with an
    'uppercase letter
    Private Function _
        ConvertName(oldName As String) As String
        Return Char.
            ToUpperInvariant(oldName(0)) &
            oldName.Substring(1)
    End Function
End Class