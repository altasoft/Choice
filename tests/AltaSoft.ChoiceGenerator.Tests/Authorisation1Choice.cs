using System;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

//[Choice]

public sealed partial class Authorisation1Choice
{
    /// <summary>
    /// <para>Specifies the authorisation, in a coded form.</para>
    /// </summary>
    [XmlTag("Cd")]
    [JsonPropertyName("cd")]
    public partial Authorisation1Code? Code { get; set; }

    /// <summary>
    /// <para>Specifies the authorisation, in a free text form.</para>
    /// </summary>
    [XmlTag("Prtry")]
    public partial Proprietary? Proprietary { get; set; }

    /// <summary>
    /// Custom method to Deserialize enums from XML 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static Authorisation1Code StringToCode(string value) => Enum.Parse<Authorisation1Code>(value);
    /// <summary>
    /// Custom method to serialize enum to XML
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string CodeToString(Authorisation1Code value) => value.ToString();

}

public class Proprietary : IXmlSerializable
{
    public string Other { get; set; }

    public XmlSchema? GetSchema() => throw new NotImplementedException();

    public void ReadXml(XmlReader reader) => throw new NotImplementedException();

    public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
}

public enum Authorisation1Code
{
    One,
    Two
}
