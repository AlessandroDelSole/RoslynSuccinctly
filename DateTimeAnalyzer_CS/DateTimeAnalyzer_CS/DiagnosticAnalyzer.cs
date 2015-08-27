using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DateTimeAnalyzer_CS
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DateTimeAnalyzer_CSAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DTA002";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.RegisterSyntaxNodeAction(AnalyzeDate, SyntaxKind.PredefinedType);
            context.RegisterSyntaxNodeAction(AnalyzeDate, SyntaxKind.IdentifierName);
        }

        private void AnalyzeDate(SyntaxNodeAnalysisContext context)
        {
            if (context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.OData.Core.ODataAction") == null)
            {
                if (context.SemanticModel.Compilation.GetTypeByMetadataName("Windows.Storage.StorageFile") == null)
                {
                    return;
                }
            }

            var root = context.Node;

            if ((root) is IdentifierNameSyntax)
            {
                root = (IdentifierNameSyntax)context.Node;
            }
            else
            {
                return;
            }

            var dateSymbol = context.SemanticModel.GetSymbolInfo(root).Symbol as INamedTypeSymbol;

            if (dateSymbol == null)
            {
                return;
            }

            if (!(dateSymbol.MetadataName == "DateTime"))
            {
                return;
            }

            var diagn = Diagnostic.Create(Rule, root.GetLocation(), "Consider replacing with DateTimeOffset");
            context.ReportDiagnostic(diagn);

        }
    }
}
