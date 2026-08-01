using System;
using System.Text.Json.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests.TestModels;

/// <summary>
/// Represents shipping options for order delivery
/// Tests multiple choices of the same type (ShippingDetails)
/// </summary>
[Choice]
public sealed partial class ShippingOption
{
    /// <summary>
    /// Standard shipping (5-7 business days)
    /// </summary>
    [XmlTag("Standard")]
    [JsonPropertyName("standard")]
    public partial ShippingDetails? Standard { get; set; }

    /// <summary>
    /// Express shipping (2-3 business days)
    /// </summary>
    [XmlTag("Express")]
    [JsonPropertyName("express")]
    public partial ShippingDetails? Express { get; set; }

    /// <summary>
    /// Overnight shipping (next business day)
    /// </summary>
    [XmlTag("Overnight")]
    [JsonPropertyName("overnight")]
    public partial ShippingDetails? Overnight { get; set; }
}

/// <summary>
/// Shipping details including cost and estimated delivery
/// </summary>
public sealed class ShippingDetails
{
    public decimal Cost { get; set; }
    public int EstimatedDays { get; set; }
    public string Carrier { get; set; } = string.Empty;

    public ShippingDetails() { }

    public ShippingDetails(decimal cost, int estimatedDays, string carrier)
    {
        Cost = cost;
        EstimatedDays = estimatedDays;
        Carrier = carrier;
    }
}
