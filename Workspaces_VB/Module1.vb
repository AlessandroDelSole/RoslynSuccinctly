Module Module1

    Sub Main()
        'Path to an existing solution
        Dim solutionPath =
            "C:\temp\RoslynSolution\RoslynSolution.sln"

        'Create a workspace
        Dim ws =
            MSBuild.
            MSBuildWorkspace.Create()

        'Open a solution
        Dim solution =
            ws.OpenSolutionAsync(
            solutionPath).Result

        'Invoke code to iterate
        'items in the solution
        IterateSolution(
            solution, solutionPath)
    End Sub

    Private Sub IterateSolution(
                solution As Solution,
                solutionPath As String)

        'Print solution's pathname and version
        Console.WriteLine(
            $"Solution {IO.Path.
            GetFileName(solutionPath)}, version {solution.Version.ToString}")

        'For each project...
        For Each prj In solution.Projects

            'Print the name and version
            Console.
            WriteLine(
            $"Project name: {prj.Name}, version: {prj.Version.ToString}")
            'Then print the number of code files
            Console.
            WriteLine($" {prj.Documents.Count} code files:")

            'For each code file, print the file name
            For Each codeFile In
                prj.Documents
                Console.
                WriteLine($"     {codeFile.Name}")
            Next

            Console.
            WriteLine(" References:")

            'For each reference in the project
            'Print the name
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
