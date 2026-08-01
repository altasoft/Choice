using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
public sealed partial class XmlNamespaceChoice
{
    [XmlTag("Cd", Namespace = "urn:test:code")]
    public partial string? Code { get; set; }

    [XmlTag("Prtry", Namespace = "urn:test:proprietary")]
    public partial string? Proprietary { get; set; }
}
