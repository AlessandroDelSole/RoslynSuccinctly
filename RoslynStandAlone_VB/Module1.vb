Imports System.IO
Imports System.Reflection
Imports Microsoft.CodeAnalysis.Emit

Module Module1

    Sub Main()
        GenerateCode()
        Console.ReadLine()
    End Sub

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

        Dim outputAssemblyName As String = Path.GetRandomFileName()
        Dim referenceList As MetadataReference() = New MetadataReference() _
        {MetadataReference.CreateFromFile(GetType(Object).Assembly.Location),
        MetadataReference.CreateFromFile(GetType(File).Assembly.Location)}

        Dim compilation As VisualBasicCompilation =
            VisualBasicCompilation.Create(outputAssemblyName, syntaxTrees:=New SyntaxTree() {tree},
                                          references:=referenceList,
                                          options:=New VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))


        Using ms As New MemoryStream()
            Dim result As EmitResult = compilation.Emit(ms)

            If Not result.Success Then
                Dim diagnostics As IEnumerable(Of Diagnostic) = result.Diagnostics.Where(Function(diagnostic) _
                    diagnostic.IsWarningAsError _
                    OrElse diagnostic.Severity = DiagnosticSeverity.[Error])

                For Each diagnostic As Diagnostic In diagnostics
                    Console.Error.WriteLine("{0}: {1}",
                                              diagnostic.Id, diagnostic.GetMessage())
                Next
            Else
                ms.Seek(0, SeekOrigin.Begin)
                Dim inputAssembly As Assembly = Assembly.Load(ms.ToArray())

                Dim typeInstance As Type = inputAssembly.GetType("RoslynSuccinctly.Helper")
                Dim obj As Object = Activator.CreateInstance(typeInstance)
                typeInstance.InvokeMember("PrintTextFromFile",
                                          BindingFlags.Default Or
                                          BindingFlags.InvokeMethod,
                                          Nothing, obj,
                                          New Object() {"C:\Temp\MIT_License.txt"})
            End If
        End Using
    End Sub

End Module
