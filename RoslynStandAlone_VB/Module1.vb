Imports System.IO
Imports System.Reflection
Imports Microsoft.CodeAnalysis.Emit

Module Module1

    Sub Main()
        GenerateCode()
        Console.ReadLine()
    End Sub

    'Generate a syntax tree
    'from source text
    Private Sub GenerateCode()
        Dim tree = VisualBasicSyntaxTree.ParseText("
Imports System
Imports System.IO

Namespace RoslynSuccinctly
    Public Class Helper

        Public Sub PrintTextFromFile(fileName As String)
            If File.Exists(fileName) = False Then
                Console.WriteLine(""File does not exist"")
                Exit Sub
            End If

            Using str As New StreamReader(fileName)
                Console.WriteLine(str.ReadToEnd())
            End Using
        End Sub
    End Class
End Namespace")

        'Get a random file name for
        'the output assembly
        Dim outputAssemblyName As String =
            Path.GetRandomFileName()

        'Add a list of references from assemblies
        'By a type name, get the assembly ref
        Dim referenceList As MetadataReference() =
            New MetadataReference() _
            {MetadataReference.
            CreateFromFile(GetType(Object).
            Assembly.Location),
            MetadataReference.
            CreateFromFile(GetType(File).
            Assembly.Location)}

        'Single invocation to the compiler
        'Create an assembly with the specified
        'syntax trees, references, and options
        Dim compilation As VisualBasicCompilation =
            VisualBasicCompilation.
            Create(outputAssemblyName,
                   syntaxTrees:=New SyntaxTree() {tree},
                   references:=referenceList,
                   options:=New VisualBasicCompilationOptions(
                   OutputKind.DynamicallyLinkedLibrary))

        'Crete a stream
        Using ms As New MemoryStream()
            'Emit the IL code into the 
            'stream
            Dim result As EmitResult =
                compilation.Emit(ms)

            'If emit fails, 
            If Not result.Success Then
                'Query the list of diagnostics in the source code
                Dim diagnostics As _
                    IEnumerable(Of Diagnostic) =
                    result.Diagnostics.Where(Function(diagnostic) _
                    diagnostic.IsWarningAsError _
                    OrElse diagnostic.Severity =
                    DiagnosticSeverity.[Error])

                'Write id and message for each diagnostic
                For Each diagnostic As _
                    Diagnostic In diagnostics

                    Console.Error.WriteLine("{0}: {1}",
                                              diagnostic.Id,
                                              diagnostic.GetMessage())
                Next
            Else
                'If emit succeeds, move to
                'the beginning of the assembly
                ms.Seek(0, SeekOrigin.Begin)

                'Load the generated assembly
                'into memory
                Dim inputAssembly As Assembly =
                    Assembly.Load(ms.ToArray())

                'Get a reference to the type 
                'defined in the syntax tree
                Dim typeInstance As Type =
                    inputAssembly.
                    GetType("RoslynSuccinctly.Helper")

                'Create an instance of the type
                Dim obj As Object =
                    Activator.
                    CreateInstance(typeInstance)

                'Invoke the method
                typeInstance.
                    InvokeMember("PrintTextFromFile",
                                 BindingFlags.Default Or
                                 BindingFlags.InvokeMethod,
                                 Nothing, obj,
                                 New Object() _
                                 {"C:\Temp\MIT_License.txt"})
            End If
        End Using
    End Sub
End Module
