using System;
using System.Text.Json.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
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
    /// Custom method to serialzie enum to XML
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string CodeToString(Authorisation1Code value) => value.ToString();

}

public class Proprietary
{
    public string Other { get; set; }

}

public enum Authorisation1Code
{
    One,
    Two
}
