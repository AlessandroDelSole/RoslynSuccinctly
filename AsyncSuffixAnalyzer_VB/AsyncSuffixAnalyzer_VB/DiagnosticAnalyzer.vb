<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class AsyncSuffixAnalyzer_VBAnalyzer
    Inherits DiagnosticAnalyzer

    Public Const DiagnosticId = "SUC001"

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
        context.RegisterSymbolAction(AddressOf AnalyzeSymbol, SymbolKind.Method)
    End Sub

    Private Sub AnalyzeSymbol(context As SymbolAnalysisContext)
        ' TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find

        Dim methodSymbol = CType(context.Symbol, IMethodSymbol)

        If methodSymbol.IsAsync = False Then
            Return
        End If

        If methodSymbol.HandledEvents.Any Then
            Return
        End If

        If Not methodSymbol.Name.ToLowerInvariant.EndsWith("async") Then
            Dim diag = Diagnostic.Create(Rule, methodSymbol.Locations(0), methodSymbol.Name)

            context.ReportDiagnostic(diag)
        End If
    End Sub
End Class