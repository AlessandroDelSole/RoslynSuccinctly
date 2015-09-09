using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncSuffixAnalyzer_CS
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncSuffixAnalyzer_CSAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SUC001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, 
                Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find

            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (methodSymbol.IsAsync == false)
            {
                return;
            }

            if (!methodSymbol.Name.ToLowerInvariant().EndsWith("async"))
            {
                var diag = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);

                context.ReportDiagnostic(diag);
            }
        }
    }
}
