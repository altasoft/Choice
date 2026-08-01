using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests.AdvancedXml;

/// <summary>
/// Customer data document
/// </summary>
[XmlRoot(ElementName = "CstmrData", Namespace = "urn:test:customer")]
[Serializable]
public sealed record CustomerData
{
    [XmlElement("CstmrId")]
    [JsonPropertyName("cstmr_id")]
    public string? CustomerId { get; set; }

    [XmlElement("Nm")]
    [JsonPropertyName("nm")]
    public string? Name { get; set; }

    [XmlElement("Email")]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [XmlElement("PhoneNb")]
    [JsonPropertyName("phone_nb")]
    public string? PhoneNumber { get; set; }

    [XmlElement("Ctry")]
    [JsonPropertyName("ctry")]
    public string? Country { get; set; }
}
