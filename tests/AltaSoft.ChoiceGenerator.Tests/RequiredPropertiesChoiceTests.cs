using AltaSoft.Choice;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Choice type with required ordinary properties - demonstrates generator enhancement
/// </summary>
[Choice]
public sealed partial class OrderWithRequiredProperties
{
    /// <summary>
    /// Required ordinary property - Order ID
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Required ordinary property - Customer Name
    /// </summary>
    public required string CustomerName { get; set; }

    /// <summary>
    /// Choice property - Express delivery
    /// </summary>
    public partial ExpressDelivery? Express { get; set; }

    /// <summary>
    /// Choice property - Standard delivery
    /// </summary>
    public partial StandardDelivery? Standard { get; set; }
}

/// <summary>
/// Tests for Choice types with required ordinary properties
/// </summary>
public class RequiredPropertiesChoiceTests
{
    [Fact]
    public void RequiredProperties_CreateAsExpress_ShouldRequireOrdinaryProperties()
    {
        // This should work after the fix - note camelCase parameter names
        var order = OrderWithRequiredProperties.CreateAsExpress(
            "ORD-001",  // orderId
            "John Doe", // customerName
            new ExpressDelivery(new System.DateTime(2024, 12, 25), 25.00m) // express (value)
        );

        Assert.Equal("ORD-001", order.OrderId);
        Assert.Equal("John Doe", order.CustomerName);
        Assert.Equal(OrderWithRequiredProperties.ChoiceOf.Express, order.ChoiceType);
        Assert.NotNull(order.Express);
    }

    [Fact]
    public void RequiredProperties_CreateAsStandard_ShouldRequireOrdinaryProperties()
    {
        // This should work after the fix - note camelCase parameter names
        var order = OrderWithRequiredProperties.CreateAsStandard(
            "ORD-002",   // orderId
            "Jane Smith", // customerName
            new StandardDelivery(5, 10.00m) // standard (value)
        );

        Assert.Equal("ORD-002", order.OrderId);
        Assert.Equal("Jane Smith", order.CustomerName);
        Assert.Equal(OrderWithRequiredProperties.ChoiceOf.Standard, order.ChoiceType);
        Assert.NotNull(order.Standard);
    }

    [Fact]
    public void RequiredPropertyNamedValue_ShouldNotConflictWithChoiceParameter()
    {
        // This tests the edge case where required ordinary properties would collide
        // with both the default choice parameter name and its first fallback.
        var config = ConfigWithValueProperty.CreateAsOptionA(
            "CONFIG-123",  // value (required property)
            "CONFIG-ALT",  // choiceValue (required property)
            new OptionA("Option A Data") // choiceValue1 (choice parameter - uniquely renamed)
        );

        Assert.Equal("CONFIG-123", config.Value);
        Assert.Equal("CONFIG-ALT", config.ChoiceValue);
        Assert.Equal(ConfigWithValueProperty.ChoiceOf.OptionA, config.ChoiceType);
        Assert.NotNull(config.OptionA);
        Assert.Equal("Option A Data", config.OptionA.Data);
    }
}

/// <summary>
/// Choice type with required properties named "Value" and "ChoiceValue" to test parameter conflict resolution
/// </summary>
[Choice]
public sealed partial class ConfigWithValueProperty
{
    /// <summary>
    /// Required property named "Value" - this would conflict with the default choice parameter name
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Required property named "ChoiceValue" - this would conflict with the first fallback choice parameter name
    /// </summary>
    public required string ChoiceValue { get; set; }

    /// <summary>
    /// Choice property - Option A
    /// </summary>
    public partial OptionA? OptionA { get; set; }

    /// <summary>
    /// Choice property - Option B
    /// </summary>
    public partial OptionB? OptionB { get; set; }
}

public sealed class OptionA
{
    public string Data { get; set; }
    public OptionA(string data) => Data = data;
}

public sealed class OptionB
{
    public int Count { get; set; }
    public OptionB(int count) => Count = count;
}
