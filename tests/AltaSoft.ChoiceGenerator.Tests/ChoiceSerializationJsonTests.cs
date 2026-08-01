using System;
using AltaSoft.ChoiceGenerator.Tests.TestHelpers;
using AltaSoft.ChoiceGenerator.Tests.TestModels;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Tests for JSON serialization and deserialization of choice types
/// </summary>
public class ChoiceSerializationJsonTests
{
    #region Serialization Tests

    [Fact]
    public void JsonSerialization_PaymentMethodWithCreditCard_ShouldProduceCorrectJson()
    {
        // Arrange
        var cardPayment = new CreditCardPayment("4111111111111111", "John Doe", "12/25", "123");
        var paymentMethod = PaymentMethod.CreateAsCreditCard(cardPayment);

        // Act
        var json = JsonSerializationHelper.SerializeToJson(paymentMethod);

        // Assert
        Assert.Contains("\"creditCard\"", json);
        Assert.Contains("\"CardNumber\": \"4111111111111111\"", json);
        Assert.Contains("\"CardHolderName\": \"John Doe\"", json);
        Assert.DoesNotContain("bankTransfer", json);
        Assert.DoesNotContain("payPal", json);
    }

    [Fact]
    public void JsonSerialization_PaymentMethodWithBankTransfer_ShouldProduceCorrectJson()
    {
        // Arrange
        var bankTransfer = new BankTransferPayment("123456789", "987654321", "Bank of America");
        var paymentMethod = PaymentMethod.CreateAsBankTransfer(bankTransfer);

        // Act
        var json = JsonSerializationHelper.SerializeToJson(paymentMethod);

        // Assert
        Assert.Contains("\"bankTransfer\"", json);
        Assert.Contains("\"AccountNumber\": \"123456789\"", json);
        Assert.Contains("\"BankName\": \"Bank of America\"", json);
        Assert.DoesNotContain("creditCard", json);
        Assert.DoesNotContain("payPal", json);
    }

    [Fact]
    public void JsonSerialization_ShippingOption_ShouldSerializeCorrectly()
    {
        // Arrange
        var expressShipping = new ShippingDetails(15.99m, 2, "FedEx");
        var shippingOption = ShippingOption.CreateAsExpress(expressShipping);

        // Act
        var json = JsonSerializationHelper.SerializeToJson(shippingOption);

        // Assert
        Assert.Contains("\"express\"", json);
        Assert.Contains("\"Cost\": 15.99", json);
        Assert.Contains("\"EstimatedDays\": 2", json);
        Assert.Contains("\"Carrier\": \"FedEx\"", json);
    }

    [Fact]
    public void JsonSerialization_SearchCriteriaWithKeyword_ShouldSerializeCorrectly()
    {
        // Arrange
        var searchCriteria = SearchCriteria.CreateAsKeyword("laptop");

        // Act
        var json = JsonSerializationHelper.SerializeToJson(searchCriteria);

        // Assert
        Assert.Contains("\"keyword\": \"laptop\"", json);
        Assert.DoesNotContain("categoryId", json);
        Assert.DoesNotContain("dateRange", json);
        Assert.DoesNotContain("priceRange", json);
    }

    [Fact]
    public void JsonSerialization_NotificationChannel_ShouldSerializeEnum()
    {
        // Arrange
        var channel = NotificationChannel.CreateAsChannel(NotificationChannelType.Email);

        // Act
        var json = JsonSerializationHelper.SerializeToJson(channel);

        // Assert
        Assert.Contains("\"channel\": \"Email\"", json);
    }

    #endregion

    #region Deserialization Tests

    [Fact]
    public void JsonDeserialization_CreditCardPayment_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = """
            {
              "creditCard": {
                "CardNumber": "4111111111111111",
                "CardHolderName": "Jane Smith",
                "ExpiryDate": "06/26",
                "Cvv": "456"
              }
            }
            """;

        // Act
        var paymentMethod = JsonSerializationHelper.DeserializeFromJson<PaymentMethod>(json);

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.CreditCard, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.CreditCard);
        Assert.Equal("4111111111111111", paymentMethod.CreditCard.CardNumber);
        Assert.Equal("Jane Smith", paymentMethod.CreditCard.CardHolderName);
        Assert.Null(paymentMethod.BankTransfer);
        Assert.Null(paymentMethod.PayPal);
    }

    [Fact]
    public void JsonDeserialization_PayPalPayment_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = """
            {
              "payPal": {
                "Email": "test@example.com",
                "TransactionId": "TXN-12345"
              }
            }
            """;

        // Act
        var paymentMethod = JsonSerializationHelper.DeserializeFromJson<PaymentMethod>(json);

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.PayPal, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.PayPal);
        Assert.Equal("test@example.com", paymentMethod.PayPal.Email);
        Assert.Equal("TXN-12345", paymentMethod.PayPal.TransactionId);
    }

    [Fact]
    public void JsonDeserialization_ShippingOption_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = """
            {
              "overnight": {
                "Cost": 29.99,
                "EstimatedDays": 1,
                "Carrier": "DHL"
              }
            }
            """;

        // Act
        var shippingOption = JsonSerializationHelper.DeserializeFromJson<ShippingOption>(json);

        // Assert
        Assert.NotNull(shippingOption);
        Assert.Equal(ShippingOption.ChoiceOf.Overnight, shippingOption.ChoiceType);
        Assert.NotNull(shippingOption.Overnight);
        Assert.Equal(29.99m, shippingOption.Overnight.Cost);
        Assert.Equal(1, shippingOption.Overnight.EstimatedDays);
    }

    [Fact]
    public void JsonDeserialization_SearchCriteriaWithCategoryId_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = """
            {
              "categoryId": 42
            }
            """;

        // Act
        var searchCriteria = JsonSerializationHelper.DeserializeFromJson<SearchCriteria>(json);

        // Assert
        Assert.NotNull(searchCriteria);
        Assert.Equal(SearchCriteria.ChoiceOf.CategoryId, searchCriteria.ChoiceType);
        Assert.Equal(42, searchCriteria.CategoryId);
    }

    [Fact]
    public void JsonDeserialization_NotificationChannel_ShouldDeserializeEnum()
    {
        // Arrange
        const string json = """
            {
              "channel": "SMS"
            }
            """;

        // Act
        var channel = JsonSerializationHelper.DeserializeFromJson<NotificationChannel>(json);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal(NotificationChannelType.SMS, channel.Channel);
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void JsonRoundTrip_PaymentMethod_ShouldPreserveData()
    {
        // Arrange
        var original = PaymentMethod.CreateAsCreditCard(
            new CreditCardPayment("4111111111111111", "Alice Johnson", "12/25", "123")
        );

        // Act
        var restored = JsonSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(PaymentMethod.ChoiceOf.CreditCard, restored.ChoiceType);
        Assert.Equal("Alice Johnson", restored.CreditCard?.CardHolderName);
        Assert.Equal("4111111111111111", restored.CreditCard?.CardNumber);
    }

    [Fact]
    public void JsonRoundTrip_ShippingOption_ShouldPreserveData()
    {
        // Arrange
        var original = ShippingOption.CreateAsStandard(
            new ShippingDetails(5.99m, 7, "USPS")
        );

        // Act
        var restored = JsonSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(ShippingOption.ChoiceOf.Standard, restored.ChoiceType);
        Assert.Equal(5.99m, restored.Standard?.Cost);
        Assert.Equal(7, restored.Standard?.EstimatedDays);
    }

    [Fact]
    public void JsonRoundTrip_SearchCriteria_WithPriceRange_ShouldPreserveStruct()
    {
        // Arrange
        var original = SearchCriteria.CreateAsPriceRange(new PriceRange(100m, 500m));

        // Act
        var restored = JsonSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(SearchCriteria.ChoiceOf.PriceRange, restored.ChoiceType);
        Assert.NotNull(restored.PriceRange);
        Assert.Equal(100m, restored.PriceRange.Value.MinPrice);
        Assert.Equal(500m, restored.PriceRange.Value.MaxPrice);
    }

    [Fact]
    public void JsonRoundTrip_SearchCriteria_WithDateRange_ShouldPreserveDates()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var original = SearchCriteria.CreateAsDateRange(new DateRange(startDate, endDate));

        // Act
        var restored = JsonSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(SearchCriteria.ChoiceOf.DateRange, restored.ChoiceType);
        Assert.NotNull(restored.DateRange);
        Assert.Equal(startDate, restored.DateRange.StartDate);
        Assert.Equal(endDate, restored.DateRange.EndDate);
    }

    #endregion
}
