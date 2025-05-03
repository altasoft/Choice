using AltaSoft.Choice.Generator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AltaSoft.Choice.Generator;

/// <summary>
/// A custom source code generator responsible for generating code for domain primitive types based on their declarations in the source code.
/// </summary>
[Generator]
public sealed class ChoiceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the DomainPrimitiveGenerator and registers it as a source code generator.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //#if DEBUG
        //        System.Diagnostics.Debugger.Launch();
        //#endif

        var choicesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(Constants.ChoiceAttributeFullName,
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(x => x is not null);

        var assemblyNames = context.CompilationProvider.Select((c, _) => c);

        //var globalOptions = context.AnalyzerConfigOptionsProvider.Select((c, _) => GetGlobalOptions(c));

        var allData = choicesToGenerate.Collect().Combine(assemblyNames);

        context.RegisterSourceOutput(allData, static (spc, pair) => Executor.Execute(in pair.Left, in pair.Right, in spc));
    }

    /// <summary>
    /// Determines if a given syntax node represents a semantic target for code generation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>
    /// The <see cref="TypeDeclarationSyntax"/> if the syntax node is a semantic target; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method analyzes a <see cref="TypeDeclarationSyntax"/> node to determine if it represents a semantic target
    /// for code generation. A semantic target is typically a class, struct, or record declaration that is not abstract
    /// and implements one or more interfaces marked as domain value types.
    /// </remarks>
    /// <seealso cref="ChoiceGenerator"/>
    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        var symbol = context.TargetSymbol;

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        return !typeSymbol.IsAbstract ? typeSymbol : null;
    }

}
