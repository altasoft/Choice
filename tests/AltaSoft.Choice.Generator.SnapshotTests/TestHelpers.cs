using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// ReSharper disable ConvertToPrimaryConstructor

#pragma warning disable IDE0290

namespace AltaSoft.Choice.Generator.SnapshotTests;

internal static class TestHelpers
{
    internal static (ImmutableArray<Diagnostic> Diagnostics, List<string> Output, GeneratorDriver driver) GetGeneratedOutput<T>
        (string source, IEnumerable<Assembly> assembliesToImport)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat([
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ChoiceAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location)
            ])
            .Concat(assembliesToImport.Select(a => MetadataReference.CreateFromFile(a.Location)));

        var compilation = CSharpCompilation.Create(
            "generator_Test",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();
        var driver = CSharpGeneratorDriver.Create(generator);

        var newDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var syntaxDiagnostics);
        var trees = outputCompilation.SyntaxTrees.ToList();

        var compilationDiagnostics = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error) // Only capture errors
            .ToImmutableArray();

        var diagnostics = ImmutableArray.Create(syntaxDiagnostics.Concat(compilationDiagnostics).ToArray());

        return (diagnostics, trees.Count != originalTreeCount ? trees[1..].ConvertAll(x => x.ToString()) : [], newDriver);
    }

}
