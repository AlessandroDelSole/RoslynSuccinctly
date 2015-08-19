<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class DateTimeAnalyzer_VBAnalyzer
    Inherits DiagnosticAnalyzer

    Public Const DiagnosticId = "DTA001"

    ' You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    Private Shared ReadOnly Title As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.AnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly MessageFormat As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.AnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly Description As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.AnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Const Category = "Naming"

    Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault:=True, description:=Description)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        ' TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeDate, SyntaxKind.PredefinedType)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeDate, SyntaxKind.IdentifierName)
    End Sub

    Private Sub AnalyzeDate(context As SyntaxNodeAnalysisContext)
        If context.SemanticModel.Compilation.GetTypeByMetadataName("Windows.Storage.StorageFile") Is Nothing Then
            If context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.OData.Core.ODataAction") Is Nothing Then
                Return
            End If
        End If

        Dim root = context.Node

        If TypeOf (root) Is PredefinedTypeSyntax Then
            root = CType(context.Node, PredefinedTypeSyntax)
        ElseIf TypeOf (root) Is IdentifierNameSyntax
            root = CType(context.Node, IdentifierNameSyntax)
        Else
            Return
        End If

        Dim dateSymbol = TryCast(context.SemanticModel.GetSymbolInfo(root).Symbol, INamedTypeSymbol)

        If dateSymbol Is Nothing Then
            Return
        End If

        If Not dateSymbol.MetadataName = "DateTime" Then
            Return
        End If

        Dim diagn = Diagnostic.Create(Rule, root.GetLocation,
                              "Consider replacing with DateTimeOffset")
        context.ReportDiagnostic(diagn)

    End Sub
End Class