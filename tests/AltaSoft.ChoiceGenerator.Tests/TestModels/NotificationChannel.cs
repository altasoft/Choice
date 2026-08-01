using System.Text.Json.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests.TestModels;

/// <summary>
/// Represents a notification delivery channel (single property choice for testing)
/// </summary>
[Choice]
public sealed partial class NotificationChannel
{
    /// <summary>
    /// The notification channel identifier
    /// </summary>
    [XmlTag("Channel")]
    [JsonPropertyName("channel")]
    public required partial NotificationChannelType Channel { get; set; }
}

/// <summary>
/// Types of notification channels
/// </summary>
public enum NotificationChannelType
{
    Email,
    SMS,
    Push,
    InApp
}
