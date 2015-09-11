Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.Rename

<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(DateTimeAnalyzer_VBCodeFixProvider)), [Shared]>
Public Class DateTimeAnalyzer_VBCodeFixProvider
    Inherits CodeFixProvider

    Private Const title As String = "Replace DateTime with DateTimeOffset"

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.
                   Create(DateTimeAnalyzer_VBAnalyzer.DiagnosticId)
        End Get
    End Property

    Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public NotOverridable Overrides _
        Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task

        'Get the root syntax node for the current document
        Dim root = Await context.Document.
            GetSyntaxRootAsync(context.CancellationToken).
            ConfigureAwait(False)

        'Get a reference to the diagnostic to fix
        Dim diagnostic = context.Diagnostics.First()

        'Get the location in the code 
        'editor for the diagnostic
        Dim diagnosticSpan =
            diagnostic.Location.SourceSpan
        'Find the syntax node on the span 
        'where there Is a squiggle
        Dim node = root.FindNode(context.Span)

        'Register a code action that invokes the fix
        'on the current document
        context.RegisterCodeFix(
        CodeAction.
        Create("Replace with DateTimeOffset",
               Function(c) ReplaceDateTimeAsync(context.Document,
                          node, c),
                          equivalenceKey:=title),
                          diagnostic)
    End Function

    Private Async Function ReplaceDateTimeAsync _
            (document As Document,
             node As SyntaxNode,
             cancellationToken As CancellationToken) _
             As Task(Of Document)

        'Get the root syntax node for the current document
        Dim root = Await document.GetSyntaxRootAsync

        Dim newRoot As SyntaxNode = Nothing

        'Convert the syntax node into the specialized kind
        If TypeOf (node) Is IdentifierNameSyntax Then
            Dim convertedNode = DirectCast(node, IdentifierNameSyntax)

            'Create a new syntax node
            Dim newNode = convertedNode.
                          WithIdentifier(SyntaxFactory.
                          ParseToken("DateTimeOffset")).
                          WithTrailingTrivia(node.
                          GetTrailingTrivia)

            'Create a New root syntax node for the current document
            'replacing the syntax node that has diagnostic with
            'a new syntax node
            newRoot = root.ReplaceNode(node, newNode)
        ElseIf TypeOf (node) Is PredefinedTypeSyntax
            Dim convertedNode = DirectCast(node, PredefinedTypeSyntax)

            Dim newIdentifierName = SyntaxFactory.
                          IdentifierName(SyntaxFactory.
                          ParseToken("DateTimeOffset")).
                          WithTrailingTrivia(node.
                          GetTrailingTrivia)
            newRoot = root.ReplaceNode(node, newIdentifierName)
        End If

        'Generate a new document
        Dim newDocument = document.WithSyntaxRoot(newRoot)
        Return newDocument

    End Function
End Class