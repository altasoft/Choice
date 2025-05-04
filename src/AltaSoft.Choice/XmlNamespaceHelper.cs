using System.Xml.Serialization;

namespace AltaSoft.Choice;

/// <summary>
/// Provides helper methods and properties for working with XML namespaces in serialization.
/// </summary>
public static class XmlNamespaceHelper
{
    /// <summary>
    /// A predefined instance of <see cref="XmlSerializerNamespaces"/> with no namespaces.
    /// </summary>
    public static readonly XmlSerializerNamespaces EmptyNamespace = GetEmptyNamespace();

    /// <summary>
    /// Creates an instance of <see cref="XmlSerializerNamespaces"/> with no namespaces.
    /// </summary>
    /// <returns>An <see cref="XmlSerializerNamespaces"/> object with no namespaces added.</returns>
    private static XmlSerializerNamespaces GetEmptyNamespace()
    {
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        return ns;
    }
}
