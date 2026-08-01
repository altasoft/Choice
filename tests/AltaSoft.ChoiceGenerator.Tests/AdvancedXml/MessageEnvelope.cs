using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests.AdvancedXml;

/// <summary>
/// Envelope containing header and business document choice
/// </summary>
[XmlRoot(ElementName = "Envelope", Namespace = "urn:test:envelope")]
[Serializable]
[Choice.Choice]
public sealed partial record MessageEnvelope
{
    /// <summary>
    /// Application header
    /// </summary>
    [XmlElement("AppHdr", Namespace = "urn:test:head")]
    [JsonPropertyName("app_hdr")]
    public ApplicationHeader? Header { get; set; }

    /// <summary>
    /// Payment instruction document
    /// </summary>
    [Choice.XmlTag("PmtInstr", Namespace = "urn:test:payment")]
    public partial PaymentInstruction? Payment { get; set; }

    /// <summary>
    /// Account report document
    /// </summary>
    [Choice.XmlTag("AcctRpt", Namespace = "urn:test:account")]
    public partial AccountReport? Account { get; set; }

    /// <summary>
    /// Customer data document
    /// </summary>
    [Choice.XmlTag("CstmrData", Namespace = "urn:test:customer")]
    public partial CustomerData? Customer { get; set; }
}
