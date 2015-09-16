using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Workspaces_CS
{
    class Program
    {
        static void Main(string[] args)
        {
            // Path to an existing solution
            string solutionPath = "C:\\temp\\RoslynSolution\\RoslynSolution.sln";

            // Create a workspace
            var ws = Microsoft.CodeAnalysis.
                MSBuild.MSBuildWorkspace.
                Create();

            // Open a solution
            var solution = 
                ws.OpenSolutionAsync(solutionPath).Result;

            // Invoke code to iterate items
            // in the solution
            IterateSolution(solution, 
                solutionPath);
        }

        static void IterateSolution(Solution solution, 
                    string solutionPath)
        {
            // Print solution's pathname and version
            Console.WriteLine(
                $"Solution {System.IO.Path.GetFileName(solutionPath)}, version {solution.Version.ToString()}");

            // For each project...
            foreach (var prj in 
                     solution.Projects)
            {
                // Print the name and version
                Console.WriteLine(
                    $"Project name: {prj.Name}, version: {prj.Version.ToString()}");
                // Then print the number of code files
                Console.WriteLine(
                    $" {prj.Documents.Count()} code files:");

                // For each code file, print the file name
                foreach (var codeFile in 
                         prj.Documents)
                {
                    Console.
                    WriteLine($"     {codeFile.Name}");
                }

                Console.WriteLine(" References:");

                // For each reference in the project
                // Print the name
                foreach (var reference in 
                         prj.MetadataReferences)
                {
                    Console.WriteLine(
                        $"    {System.IO.Path.GetFileName(reference.Display)}");
                }
            }

            Console.ReadLine();
        }
    }
}
