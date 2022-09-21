// See https://aka.ms/new-console-template for more information
using GameJSON.Testing.Functionality;

Console.WriteLine(SimpleTester.TestRunner.RunAllTests(typeof(Tests).Assembly));
Console.WriteLine("\nManual Performance:\n");
ManualPerformance.Start();
Console.WriteLine("\nReflection Performance:\n");
ReflectionPerformance.Start();