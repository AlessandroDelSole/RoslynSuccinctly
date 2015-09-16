Module Module1

    Sub Main()
        Dim solutionPath =
            "C:\temp\ConsoleApplication1\ConsoleApplication1.sln"

        Dim ws =
            MSBuild.
            MSBuildWorkspace.Create()

        Dim solution =
            ws.OpenSolutionAsync(
            solutionPath).Result

        IterateSolution(
            solution, solutionPath)
    End Sub

    Private Sub IterateSolution(
                solution As Solution,
                solutionPath As String)

        Console.WriteLine(
            $"Solution {IO.Path.
            GetFileName(solutionPath)}, version {solution.Version.ToString}")
        For Each prj In solution.Projects
            Console.
            WriteLine(
            $"Project name: {prj.Name}, version: {prj.Version.ToString}")
            Console.
            WriteLine($" {prj.Documents.Count} code files:")

            For Each codeFile In
                prj.Documents
                Console.
                WriteLine($"     {codeFile.Name}")
            Next

            Console.
            WriteLine(" References:")

            For Each ref In
                prj.MetadataReferences
                Console.
                WriteLine($"    {IO.Path.GetFileName(ref.Display)}")
            Next
            Console.WriteLine("")
        Next

        Console.ReadLine()
    End Sub
End Module
