Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.Rename

<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(DateTimeAnalyzer_VBCodeFixProvider)), [Shared]>
Public Class DateTimeAnalyzer_VBCodeFixProvider
    Inherits CodeFixProvider

    Private Const title As String = "Replace DateTime with DateTimeOffset"

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create(DateTimeAnalyzer_VBAnalyzer.DiagnosticId)
        End Get
    End Property

    Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public NotOverridable Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
        Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

        ' TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest

        Dim diagnostic = context.Diagnostics.First()
        Dim diagnosticSpan = diagnostic.Location.SourceSpan

        Dim node = root.FindNode(context.Span)

        If TypeOf (node) Is PredefinedTypeSyntax Then
            context.RegisterCodeFix(
            CodeAction.Create("Replace with DateTimeOffset",
                              Function(c) ReplaceDateAsync(context.Document, node, c), equivalenceKey:=title), diagnostic)
        Else
            context.RegisterCodeFix(
            CodeAction.Create("Replace with DateTimeOffset",
                              Function(c) ReplaceDateTimeAsync(context.Document, node, c), equivalenceKey:=title), diagnostic)
        End If
    End Function

    Private Async Function ReplaceDateAsync(document As Document, node As PredefinedTypeSyntax,
                                         cancellationToken As CancellationToken) As Task(Of Document)

        'Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken)
        Dim root = Await document.GetSyntaxRootAsync
        Dim generator = SyntaxGenerator.GetGenerator(document)

        Dim newIdentifierSyntax = generator.IdentifierName("DateTimeOffset").WithTrailingTrivia(node.GetTrailingTrivia)

        Dim newRoot = root.ReplaceNode(node, newIdentifierSyntax)
        Dim newDocument = document.WithSyntaxRoot(newRoot)
        Return newDocument
    End Function

    Private Async Function ReplaceDateTimeAsync(document As Document, node As IdentifierNameSyntax,
                                     cancellationToken As CancellationToken) As Task(Of Document)

        Dim semanticModel = Await document.GetSemanticModelAsync(cancellationToken)
        Dim root = Await document.GetSyntaxRootAsync

        'DateTime
        Dim node2 = node.WithIdentifier(SyntaxFactory.ParseToken("DateTimeOffset")).WithTrailingTrivia(node.GetTrailingTrivia)
        Dim newRoot = root.ReplaceNode(node, node2)

        Dim newDocument = document.WithSyntaxRoot(newRoot)
        Return newDocument

    End Function
End Class