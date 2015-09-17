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

        //Generate a syntax tree
        //from source text
        private static void GenerateCode()
        {
            SyntaxTree syntaxTree = 
                CSharpSyntaxTree.ParseText(@"
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
            //Get a random file name for
            //the output assembly
            string outputAssemblyName = 
                Path.GetRandomFileName();

            //Add a list of references from assemblies
            //By a type name, get the assembly ref
            MetadataReference[] referenceList = 
                new MetadataReference[]
                {
                    MetadataReference.
                    CreateFromFile(typeof(object).
                    Assembly.Location),
                    MetadataReference.
                    CreateFromFile(typeof(File).
                    Assembly.Location)
                };

            //Single invocation to the compiler
            //Create an assembly with the specified
            //syntax trees, references, and options
            CSharpCompilation compilation = 
                CSharpCompilation.Create(
                outputAssemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: referenceList,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));

            //Create a stream
            using (var ms = new MemoryStream())
            {
                //Emit the IL code into the stream
                EmitResult result = compilation.Emit(ms);

                //If emit fails,
                if (!result.Success)
                {
                    //Query the list of diagnostics
                    //in the source code
                    IEnumerable<Diagnostic> diagnostics = 
                        result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == 
                        DiagnosticSeverity.Error);

                    //Write id and message for each diagnostic
                    foreach (Diagnostic diagnostic in 
                        diagnostics)
                    {
                        Console.Error.
                            WriteLine("{0}: {1}", 
                            diagnostic.Id, 
                            diagnostic.
                            GetMessage());
                    }
                }
                else
                {
                    //If emit succeeds, move to
                    //the beginning of the assembly
                    ms.Seek(0, 
                        SeekOrigin.Begin);
                    //Load the generated assembly
                    //into memory
                    Assembly inputAssembly = 
                        Assembly.Load(ms.ToArray());

                    //Get a reference to the type
                    //defined in the syntax tree
                    Type typeInstance = 
                        inputAssembly.
                        GetType("RoslynSuccinctly.Helper");

                    //Create an instance of the type
                    object obj = 
                        Activator.CreateInstance(typeInstance);

                    //Invoke the method
                    typeInstance.
                        InvokeMember("PrintTextFromFile",
                        BindingFlags.Default | 
                        BindingFlags.InvokeMethod,
                        null,
                        obj,
                        new object[] 
                        { "C:\\temp\\MIT_License.txt" });
                }
            }
        }
    }
}
