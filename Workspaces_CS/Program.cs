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
            string solutionPath = "C:\\temp\\RoslynSolution\\RoslynSolution.sln";

            var ws = Microsoft.CodeAnalysis.
                MSBuild.MSBuildWorkspace.
                Create();

            var solution = 
                ws.OpenSolutionAsync(solutionPath).Result;

            IterateSolution(solution, 
                solutionPath);
        }

        static void IterateSolution(Solution solution, string solutionPath)
        {
            Console.WriteLine(
                $"Solution {System.IO.Path.GetFileName(solutionPath)}, version {solution.Version.ToString()}");

            foreach (var prj in 
                     solution.Projects)
            {
                Console.WriteLine(
                    $"Project name: {prj.Name}, version: {prj.Version.ToString()}");
                Console.WriteLine(
                    $" {prj.Documents.Count()} code files:");

                foreach (var codeFile in 
                         prj.Documents)
                {
                    Console.
                    WriteLine($"     {codeFile.Name}");
                }

                Console.WriteLine(" References:");

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
