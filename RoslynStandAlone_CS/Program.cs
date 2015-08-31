using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

namespace RoslynStandAlone_CS
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateCode();
            Console.ReadLine();
        }

        private static void GenerateCode()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(@"
    using System;
    using System.IO;

    namespace RoslynSuccinctly
    {
        public class Helper
        {
            public void PrintTextFromFile(string fileName)
            {
                if (File.Exists(fileName) == false)
                {
                    Console.WriteLine(""File does not exist"");
                    return;
            }

                using (StreamReader str = new StreamReader(fileName))
                {
                    Console.WriteLine(str.ReadToEnd());
                }
            }
        }
    }");

            string outputAssemblyName = Path.GetRandomFileName();
            MetadataReference[] referenceList = new MetadataReference[]
            {
    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(File).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                outputAssemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: referenceList,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> diagnostics = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in diagnostics)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly inputAssembly = Assembly.Load(ms.ToArray());
                    Type typeInstance = inputAssembly.GetType("RoslynSuccinctly.Helper");
                    object obj = Activator.CreateInstance(typeInstance);
                    typeInstance.InvokeMember("PrintTextFromFile",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        obj,
                        new object[] { "C:\\temp\\MIT_License.txt" });
                }
            }
        }
    }
}
