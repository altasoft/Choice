using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests.TestHelpers;

/// <summary>
/// Helper methods for XML serialization in tests
/// </summary>
public static class XmlSerializationHelper
{
    private static readonly XmlWriterSettings s_writerSettings = new()
    {
        OmitXmlDeclaration = true,
        Indent = true,
        Encoding = Encoding.UTF8
    };

    /// <summary>
    /// Serializes an object to XML string
    /// </summary>
    public static string SerializeToXml<T>(T obj) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_writerSettings);

        serializer.Serialize(writer, obj, XmlNamespaceHelper.EmptyNamespace);

        return sw.ToString();
    }

    /// <summary>
    /// Deserializes an object from XML string
    /// </summary>
    public static T DeserializeFromXml<T>(string xml) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);

        var result = serializer.Deserialize(reader) as T;
        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize XML to type {typeof(T).Name}");

        return result;
    }

    /// <summary>
    /// Performs a round-trip serialization test: object -> XML -> object
    /// </summary>
    public static T RoundTrip<T>(T original) where T : class
    {
        var xml = SerializeToXml(original);
        return DeserializeFromXml<T>(xml);
    }

    /// <summary>
    /// Normalizes XML string for comparison (removes formatting differences)
    /// </summary>
    public static string NormalizeXml(string xml)
    {
        return xml.Trim().Replace("\r\n", "\n");
    }
}
