using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests.AdvancedXml;

/// <summary>
/// Application header with custom XML namespace
/// </summary>
[XmlRoot(ElementName = "AppHdr", Namespace = "urn:test:head")]
[Serializable]
public sealed record ApplicationHeader
{
    [XmlElement("BizMsgIdr")]
    [JsonPropertyName("biz_msg_idr")]
    public string? BusinessMessageIdentifier { get; set; }

    [XmlElement("CreDtTm")]
    [JsonPropertyName("cre_dt_tm")]
    public DateTime CreationDateTime { get; set; }

    [XmlElement("MsgDefIdr")]
    [JsonPropertyName("msg_def_idr")]
    public string? MessageDefinitionIdentifier { get; set; }
}
