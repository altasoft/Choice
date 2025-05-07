using System.Text.Json.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
public sealed partial record Authorisation1Choice
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
