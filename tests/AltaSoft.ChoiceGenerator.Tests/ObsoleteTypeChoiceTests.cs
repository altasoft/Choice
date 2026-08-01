using AltaSoft.Choice;
using System;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Choice type using obsolete types - verifies warning suppression
/// </summary>
[Choice]
public sealed partial class PaymentMethodChoice
{
    /// <summary>
    /// Legacy payment method - obsolete but still supported
    /// </summary>
    public partial LegacyPayment? Legacy { get; set; }

    /// <summary>
    /// Modern payment method
    /// </summary>
    public partial ModernPayment? Modern { get; set; }
}

/// <summary>
/// Obsolete payment type - still used in some legacy systems
/// </summary>
[Obsolete("Use ModernPayment instead", false)]
public sealed class LegacyPayment
{
    public string CardNumber { get; set; } = string.Empty;
    public LegacyPayment() { }
}

/// <summary>
/// Modern payment type
/// </summary>
public sealed class ModernPayment
{
    public string Token { get; set; } = string.Empty;
    public ModernPayment() { }
}

/// <summary>
/// Tests for Choice types using obsolete types - verifies no warnings in generated code
/// </summary>
public class ObsoleteTypeChoiceTests
{
    [Fact]
    public void ObsoleteType_CanBeUsedInChoice_WithoutWarnings()
    {
        // The generated code should have CS0618 suppressed, so using obsolete types works
        var payment = PaymentMethodChoice.CreateAsLegacy(new LegacyPayment { CardNumber = "1234" });

        Assert.Equal(PaymentMethodChoice.ChoiceOf.Legacy, payment.ChoiceType);
        Assert.NotNull(payment.Legacy);
        Assert.Equal("1234", payment.Legacy.CardNumber);
    }

    [Fact]
    public void ObsoleteType_Switch_ShouldWork()
    {
        var payment = PaymentMethodChoice.CreateAsLegacy(new LegacyPayment { CardNumber = "5678" });

        var result = string.Empty;
        payment.Switch(
            legacy => result = $"Legacy: {legacy.CardNumber}",
            modern => result = $"Modern: {modern.Token}"
        );

        Assert.Equal("Legacy: 5678", result);
    }

    [Fact]
    public void ObsoleteType_Match_ShouldWork()
    {
        var payment = PaymentMethodChoice.CreateAsLegacy(new LegacyPayment { CardNumber = "9012" });

        var result = payment.Match(
            legacy => $"Legacy: {legacy.CardNumber}",
            modern => $"Modern: {modern.Token}"
        );

        Assert.Equal("Legacy: 9012", result);
    }
}
