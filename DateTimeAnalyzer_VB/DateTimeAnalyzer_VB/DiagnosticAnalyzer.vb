<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class DateTimeAnalyzer_VBAnalyzer
    Inherits DiagnosticAnalyzer

    Public Const DiagnosticId = "DTA001"

    ' You can change these strings in the Resources.resx file. 
    'If you Do Not want your analyzer To be localize-able, 
    'you can use regular strings For Title And MessageFormat.
    Private Shared ReadOnly Title As LocalizableString =
        New LocalizableResourceString(NameOf(My.Resources.AnalyzerTitle),
        My.Resources.ResourceManager,
        GetType(My.Resources.Resources))
    Private Shared ReadOnly MessageFormat As LocalizableString =
        New LocalizableResourceString(NameOf(My.Resources.
        AnalyzerMessageFormat),
        My.Resources.ResourceManager,
        GetType(My.Resources.Resources))
    Private Shared ReadOnly Description As LocalizableString =
        New LocalizableResourceString(NameOf(My.Resources.
        AnalyzerDescription), My.Resources.ResourceManager,
        GetType(My.Resources.Resources))
    Private Const Category = "Naming"

    Private Shared Rule As New DiagnosticDescriptor(DiagnosticId,
            Title, MessageFormat, Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault:=True,
            description:=Description,
            helpLinkUri:="https://github.com/AlessandroDelSole/RoslynSuccinctly/wiki/DTA001")

    Public Overrides ReadOnly Property SupportedDiagnostics As _
        ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeDateTime,
                                         SyntaxKind.PredefinedType)
        context.RegisterSyntaxNodeAction(AddressOf AnalyzeDateTime,
                                         SyntaxKind.IdentifierName)
    End Sub

    Private Sub AnalyzeDateTime(context As SyntaxNodeAnalysisContext)
        If context.SemanticModel.Compilation.GetTypeByMetadataName("Windows.Storage.StorageFile") Is Nothing Then
            If context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.OData.Core.ODataAction") Is Nothing Then
                Return
            End If
        End If

        'Get the syntax node to analyze
        Dim root = context.Node

        'If it's not an IdentifierName syntax node,
        'return
        If TypeOf (root) Is PredefinedTypeSyntax Then
            root = CType(context.Node, PredefinedTypeSyntax)
        ElseIf TypeOf (root) Is IdentifierNameSyntax
            'Conver to IdentifierNameSyntax
            root = CType(context.Node, IdentifierNameSyntax)
        Else
            Return
        End If

        'Get the symbol info for
        'the DateTime type declaration
        Dim dateSymbol =
            TryCast(context.SemanticModel.
                    GetSymbolInfo(root).Symbol,
                    INamedTypeSymbol)

        'If no symbol info, return
        If dateSymbol Is Nothing Then
            Return
        End If

        'If the name  of the symbol is not 
        'DateTime, return
        If Not dateSymbol.MetadataName = "DateTime" Then
            Return
        End If

        'Create a diagnostic at the node location
        'with the specified message And rule info
        Dim diagn =
            Diagnostic.Create(Rule, root.GetLocation,
                       "Consider replacing with DateTimeOffset")

        'Report the diagnostic
        context.ReportDiagnostic(diagn)
    End Sub
End Class