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
    /// The type symbol of the property.
    /// </summary>
    internal ITypeSymbol TypeSymbol { get; private set; }

    public PropertyDetails(string name, string typeName, string @namespace, string xmlNameValue, string? summary, ITypeSymbol typeSymbol)
    {
        Name = name;
        TypeName = typeName;
        Namespace = @namespace;
        XmlNameValue = xmlNameValue;
        Summary = summary;
        TypeSymbol = typeSymbol;
    }
}
