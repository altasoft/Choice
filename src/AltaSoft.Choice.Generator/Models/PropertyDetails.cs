using AltaSoft.Choice.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace AltaSoft.Choice.Generator.Models;

internal sealed class PropertyDetails
{
    /// <summary>
    /// The name of the property.
    /// </summary>
    internal string Name { get; private set; }

    /// <summary>
    /// The type name of the property.
    /// </summary>
    internal string TypeName { get; private set; }

    /// <summary>
    /// The namespace of the property.
    /// </summary>
    internal string Namespace { get; private set; }

    /// <summary>
    /// The XML name value of the property.
    /// </summary>
    internal string XmlNameValue { get; private set; }

    /// <summary>
    /// The summary documentation for the property.
    /// </summary>
    internal string? Summary { get; private set; }

    /// <summary>
    /// The access modifiers for the property's getter.
    /// </summary>
    internal Accessibility GetterAccessibility { get; private set; }

    /// <summary>
    /// The access modifiers for the property's setter.
    /// </summary>
    internal Accessibility SetterAccessibility { get; private set; }

    /// <summary>
    /// The type symbol of the property.
    /// </summary>
    internal ITypeSymbol TypeSymbol { get; private set; }

    /// <summary>
    /// returns if the type is dateOnly
    /// </summary>
    internal bool IsDateOnly()
    {
        return TypeSymbol.IsValueType && TypeSymbol.GetFullName()?.Replace("?", "") == "System.DateOnly";
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDetails"/> class.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="typeName">The type name of the property.</param>
    /// <param name="namespace">The namespace of the property.</param>
    /// <param name="xmlNameValue">The XML name value of the property.</param>
    /// <param name="summary">The summary documentation for the property.</param>
    /// <param name="getterAccessibility">The access modifiers for the property's getter.</param>
    /// <param name="setterAccessibility">The access modifiers for the property's setter.</param>
    /// <param name="typeSymbol">The type symbol of the property.</param>
    public PropertyDetails(
        string name,
        string typeName,
        string @namespace,
        string xmlNameValue,
        string? summary,
        Accessibility getterAccessibility,
        Accessibility setterAccessibility,
        ITypeSymbol typeSymbol)
    {
        Name = name;
        TypeName = typeName;
        Namespace = @namespace;
        XmlNameValue = xmlNameValue;
        Summary = summary;
        GetterAccessibility = getterAccessibility;
        SetterAccessibility = setterAccessibility;
        TypeSymbol = typeSymbol;
    }

}
