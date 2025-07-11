using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AltaSoft.Choice.Generator.Extensions;

/// <summary>
/// Extension methods for working with Roslyn's Compilation and related types.
/// </summary>
internal static class CompilationExt
{
    internal static string ToFieldName(this string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        if (s.Length == 1)
            return "_" + s.ToLower(CultureInfo.InvariantCulture);

        return "_" + char.ToLower(s[0], CultureInfo.InvariantCulture) + s.Substring(1);
    }

    /// <summary>
    /// Returns the C# keyword representation of a property's <see cref="Accessibility"/> level,
    /// optionally omitting the keyword for <c>public</c> if <paramref name="emptyOnPublic"/> is true.
    /// </summary>
    /// <param name="accessibility">The <see cref="Accessibility"/> enum value representing the access level.</param>
    /// <param name="emptyOnPublic">
    /// If true, returns an empty string when <paramref name="accessibility"/> is <c>Public</c>;
    /// otherwise, returns the string <c>"public"</c>.
    /// </param>
    /// <returns>
    /// A string representing the C# access modifier, such as <c>"private"</c>, <c>"protected internal"</c>,
    /// or an empty string if <paramref name="accessibility"/> is <c>NotApplicable</c> or <c>Public</c> with <paramref name="emptyOnPublic"/> set to true.
    /// </returns>
    internal static string GetPropertyAccessibilityString(this Accessibility accessibility, bool emptyOnPublic = true)
    {
        return accessibility switch
        {
            Accessibility.NotApplicable => "",
            Accessibility.Public => emptyOnPublic ? "" : "public ",
            Accessibility.Private => "private ",
            Accessibility.Protected => "protected ",
            Accessibility.Internal => "internal ",
            Accessibility.ProtectedAndInternal => "private protected ",
            Accessibility.ProtectedOrInternal => "protected internal ",
            _ => ""
        };
    }

    /// <summary>
    /// Reads Summary of a symbol from its XML documentation comment.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    internal static string? GetSummaryText(this ISymbol symbol)
    {
        // Try to get the XML documentation comment
        var xmlText = symbol.GetDocumentationCommentXml();

        if (string.IsNullOrWhiteSpace(xmlText))
            return null;

        try
        {
            var xmlDoc = System.Xml.Linq.XDocument.Parse(xmlText);
            var summary = xmlDoc.Root?.Element("summary")?.Value.Trim();
            return summary;
        }
        catch
        {
            return null; // Invalid XML format
        }
    }

    /// <summary>
    /// Gets members of a specific type for a given ITypeSymbol.
    /// </summary>
    /// <typeparam name="TMember">The type of members to retrieve.</typeparam>
    /// <param name="self">The ITypeSymbol to retrieve members from.</param>
    /// <returns>An IEnumerable of members of the specified type.</returns>
    public static IEnumerable<TMember> GetMembersOfType<TMember>(this ITypeSymbol? self) where TMember : ISymbol
    {
        return self?.GetMembers().OfType<TMember>() ?? [];
    }

    #region Accessibility

    /// <summary>
    /// Gets the modifiers for the named type symbol.
    /// </summary>
    /// <param name="self">The named type symbol to retrieve modifiers from.</param>
    /// <returns>The modifiers as a string, or null if the type is null or has no modifiers.</returns>
    public static string? GetModifiers(this INamedTypeSymbol? self)
    {
        var declaringSyntax = self?.DeclaringSyntaxReferences;
        if (self is null || declaringSyntax is null or { Length: 0 })
            return null;

        foreach (var syntax in declaringSyntax)
        {
            if (syntax.GetSyntax() is TypeDeclarationSyntax typeDeclaration && string.Equals(typeDeclaration.GetClassName(), self.GetClassNameWithArguments(), StringComparison.Ordinal))
            {
                var modifiers = typeDeclaration.Modifiers.ToString();

                return modifiers;
            }
        }

        return null;
    }

    #endregion Accessibility
    /// <summary>
    /// Gets the class name including generic arguments as a string.
    /// </summary>
    /// <param name="type">The named type symbol to get the class name from.</param>
    /// <returns>The class name including generic arguments as a string.</returns>
    public static string GetClassNameWithArguments(this INamedTypeSymbol? type)
    {
        if (type is null)
            return string.Empty;

        var builder = new StringBuilder(type.Name);

        if (type.TypeArguments.Length == 0)
            return builder.ToString();

        builder.Append('<');
        for (var index = 0; index < type.TypeArguments.Length; index++)
        {
            var arg = type.TypeArguments[index];
            builder.Append(arg.Name);

            if (index != type.TypeArguments.Length - 1)
                builder.Append(", ");
        }

        builder.Append('>');

        return builder.ToString();
    }
#pragma warning disable S1643
    public static string GetFullName(this ITypeSymbol type)
    {
        var ns = type.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining))!;

        if (s_typeAliases.TryGetValue(ns + "." + type.MetadataName, out var result))
        {
            return result;
        }

        var friendlyName = type.MetadataName;
        if (friendlyName.Length == 0)
            friendlyName = type.OriginalDefinition.ToDisplayString();

        if (type is not INamedTypeSymbol namedType)
            return friendlyName;

        if (!namedType.IsGenericType)
            return ns + '.' + friendlyName;

        if (namedType.IsNullableValueType(out var underlyingType))
            return underlyingType!.GetFullName();

        var iBacktick = friendlyName.IndexOf('`');
        if (iBacktick > 0)
            friendlyName = friendlyName.Remove(iBacktick);
        friendlyName += "<";

        var typeParameters = namedType.TypeArguments;
        for (var i = 0; i < typeParameters.Length; ++i)
        {
            var typeParamName = typeParameters[i].ToString();

            friendlyName += i == 0 ? typeParamName : "," + typeParamName;

        }
        friendlyName += ">";
        return ns + '.' + friendlyName;
    }
#pragma warning restore S1643
    /// <summary>
    /// Determines whether the specified type symbol is a Nullable&lt;T&gt; value type and, if so, provides the underlying value type symbol.
    /// </summary>
    /// <param name="type">The type symbol to check.</param>
    /// <param name="underlyingTypeSymbol">When this method returns, contains the type symbol of the underlying value type if <paramref name="type"/> is a nullable value type; otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if <paramref name="type"/> is a nullable value type; otherwise, <c>false</c>.</returns>
    public static bool IsNullableValueType(this INamedTypeSymbol type, out ITypeSymbol? underlyingTypeSymbol)
    {
        if (type is { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T })
        {
            underlyingTypeSymbol = type.TypeArguments[0];
            return true;
        }

        underlyingTypeSymbol = null;
        return false;
    }

    /// <summary>
    /// A dictionary that provides aliases for common .NET framework types, mapping their full names to shorter aliases.
    /// </summary>
    private static readonly Dictionary<string, string> s_typeAliases = new(StringComparer.Ordinal)
    {
        { typeof(byte).FullName, "byte" },
        { typeof(sbyte).FullName, "sbyte" },
        { typeof(short).FullName, "short" },
        { typeof(ushort).FullName, "ushort" },
        { typeof(int).FullName, "int" },
        { typeof(uint).FullName, "uint" },
        { typeof(long).FullName, "long" },
        { typeof(ulong).FullName, "ulong" },
        { typeof(float).FullName, "float" },
        { typeof(double).FullName, "double" },
        { typeof(decimal).FullName, "decimal" },
        { typeof(object).FullName, "object" },
        { typeof(bool).FullName, "bool" },
        { typeof(char).FullName, "char" },
        { typeof(string).FullName, "string" },
        { typeof(void).FullName, "void" },
    };
}
