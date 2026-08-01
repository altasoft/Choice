using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests.AdvancedXml;

/// <summary>
/// Payment instruction document
/// </summary>
[XmlRoot(ElementName = "PmtInstr", Namespace = "urn:test:payment")]
[Serializable]
public sealed record PaymentInstruction
{
    [XmlElement("InstrId")]
    [JsonPropertyName("instr_id")]
    public string? InstructionId { get; set; }

    [XmlElement("Amt")]
    [JsonPropertyName("amt")]
    public decimal Amount { get; set; }

    [XmlElement("Ccy")]
    [JsonPropertyName("ccy")]
    public string? Currency { get; set; }

    [XmlElement("DbtrNm")]
    [JsonPropertyName("dbtr_nm")]
    public string? DebtorName { get; set; }

    [XmlElement("CdtrNm")]
    [JsonPropertyName("cdtr_nm")]
    public string? CreditorName { get; set; }
}
