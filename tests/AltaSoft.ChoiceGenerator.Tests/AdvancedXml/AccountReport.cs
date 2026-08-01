using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests.AdvancedXml;

/// <summary>
/// Account report document
/// </summary>
[XmlRoot(ElementName = "AcctRpt", Namespace = "urn:test:account")]
[Serializable]
public sealed record AccountReport
{
    [XmlElement("RptId")]
    [JsonPropertyName("rpt_id")]
    public string? ReportId { get; set; }

    [XmlElement("AcctId")]
    [JsonPropertyName("acct_id")]
    public string? AccountId { get; set; }

    [XmlElement("Bal")]
    [JsonPropertyName("bal")]
    public decimal Balance { get; set; }

    [XmlElement("BalDt")]
    [JsonPropertyName("bal_dt")]
    public DateTime BalanceDate { get; set; }
}
