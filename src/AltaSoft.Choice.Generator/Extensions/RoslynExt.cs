using Microsoft.CodeAnalysis.CSharp.Syntax;

// https://www.meziantou.net/working-with-types-in-a-roslyn-analyzer.htm

namespace AltaSoft.Choice.Generator.Extensions;

/// <summary>
/// A collection of extension methods for working with Roslyn syntax and symbols.
/// </summary>
internal static class RoslynExt
{
    /// <summary>
    /// Gets the namespace of the specified type declaration syntax.
    /// </summary>
    /// <param name="self">The type declaration syntax to retrieve the namespace from.</param>
    /// <returns>The namespace of the type declaration or null if not found.</returns>
    public static string? GetNamespace(this TypeDeclarationSyntax self)
    {
        return self.Parent is not BaseNamespaceDeclarationSyntax ns ? null : ns.GetNamespace();
    }

    /// <summary>
    /// Gets the namespace from the specified base namespace declaration syntax.
    /// </summary>
    /// <param name="self">The base namespace declaration syntax to retrieve the namespace from.</param>
    /// <returns>The namespace from the base namespace declaration.</returns>
    public static string GetNamespace(this BaseNamespaceDeclarationSyntax self) => self.Name.ToString();

    /// <summary>
    /// Gets the fully qualified name of the specified type declaration syntax, including its namespace.
    /// </summary>
    /// <param name="self">The type declaration syntax to retrieve the fully qualified name from.</param>
    /// <returns>The fully qualified name of the type declaration.</returns>
    public static string GetClassFullName(this TypeDeclarationSyntax self)
    {
        return self.GetNamespace() + "." + self.GetClassName();
    }

    /// <summary>
    /// Gets the name of the class specified in the type declaration syntax.
    /// </summary>
    /// <param name="proxy">The type declaration syntax to retrieve the class name from.</param>
    /// <returns>The name of the class.</returns>
    public static string GetClassName(this TypeDeclarationSyntax proxy)
    {
        return proxy.Identifier.Text + proxy.TypeParameterList?.ToFullString();
    }
}
