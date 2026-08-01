using System;
using AltaSoft.ChoiceGenerator.Tests.TestHelpers;
using AltaSoft.ChoiceGenerator.Tests.TestModels;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Tests for XML serialization and deserialization of choice types
/// </summary>
public class ChoiceSerializationXmlTests
{
    #region Serialization Tests

    [Fact]
    public void XmlSerialization_PaymentMethodWithCreditCard_ShouldProduceCorrectXml()
    {
        // Arrange
        var cardPayment = new CreditCardPayment("4111111111111111", "John Doe", "12/25", "123");
        var paymentMethod = PaymentMethod.CreateAsCreditCard(cardPayment);

        // Act
        var xml = XmlSerializationHelper.SerializeToXml(paymentMethod);

        // Assert
        Assert.Contains("<CreditCard>", xml);
        Assert.Contains("<CardNumber>4111111111111111</CardNumber>", xml);
        Assert.Contains("<CardHolderName>John Doe</CardHolderName>", xml);
        Assert.DoesNotContain("<BankTransfer", xml);
        Assert.DoesNotContain("<PayPal", xml);
    }

    [Fact]
    public void XmlSerialization_PaymentMethodWithBankTransfer_ShouldProduceCorrectXml()
    {
        // Arrange
        var bankTransfer = new BankTransferPayment("123456789", "987654321", "Bank of America");
        var paymentMethod = PaymentMethod.CreateAsBankTransfer(bankTransfer);

        // Act
        var xml = XmlSerializationHelper.SerializeToXml(paymentMethod);

        // Assert
        Assert.Contains("<BankTransfer>", xml);
        Assert.Contains("<AccountNumber>123456789</AccountNumber>", xml);
        Assert.Contains("<BankName>Bank of America</BankName>", xml);
        Assert.DoesNotContain("<CreditCard", xml);
    }

    [Fact]
    public void XmlSerialization_ShippingOption_ShouldSerializeCorrectly()
    {
        // Arrange
        var expressShipping = new ShippingDetails(15.99m, 2, "FedEx");
        var shippingOption = ShippingOption.CreateAsExpress(expressShipping);

        // Act
        var xml = XmlSerializationHelper.SerializeToXml(shippingOption);

        // Assert
        Assert.Contains("<Express>", xml);
        Assert.Contains("<Cost>15.99</Cost>", xml);
        Assert.Contains("<EstimatedDays>2</EstimatedDays>", xml);
        Assert.Contains("<Carrier>FedEx</Carrier>", xml);
        Assert.DoesNotContain("<Standard", xml);
        Assert.DoesNotContain("<Overnight", xml);
    }

    [Fact]
    public void XmlSerialization_SearchCriteriaWithKeyword_ShouldSerializeCorrectly()
    {
        // Arrange
        var searchCriteria = SearchCriteria.CreateAsKeyword("laptop");

        // Act
        var xml = XmlSerializationHelper.SerializeToXml(searchCriteria);

        // Assert
        Assert.Contains("<Keyword>laptop</Keyword>", xml);
        Assert.DoesNotContain("<CategoryId", xml);
        Assert.DoesNotContain("<DateRange", xml);
    }

    [Fact]
    public void XmlSerialization_NotificationChannel_ShouldSerializeEnum()
    {
        // Arrange
        var channel = NotificationChannel.CreateAsChannel(NotificationChannelType.Push);

        // Act
        var xml = XmlSerializationHelper.SerializeToXml(channel);

        // Assert
        Assert.Contains("<Channel>Push</Channel>", xml);
    }

    [Fact]
    public void XmlSerialization_ShouldNotIncludeInactiveChoices()
    {
        // Arrange
        var payPal = new PayPalPayment("test@example.com", "TXN-999");
        var paymentMethod = PaymentMethod.CreateAsPayPal(payPal);

        // Act
        var xml = XmlSerializationHelper.SerializeToXml(paymentMethod);

        // Assert
        Assert.Contains("<PayPal>", xml);
        // Inactive choices should not be serialized (no xsi:nil elements)
        Assert.DoesNotContain("nil=", xml);
        Assert.DoesNotContain("<CreditCard", xml);
        Assert.DoesNotContain("<BankTransfer", xml);
    }

    #endregion

    #region Deserialization Tests

    [Fact]
    public void XmlDeserialization_CreditCardPayment_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string xml = """
            <PaymentMethod>
              <CreditCard>
                <CardNumber>4111111111111111</CardNumber>
                <CardHolderName>Jane Smith</CardHolderName>
                <ExpiryDate>06/26</ExpiryDate>
                <Cvv>456</Cvv>
              </CreditCard>
            </PaymentMethod>
            """;

        // Act
        var paymentMethod = XmlSerializationHelper.DeserializeFromXml<PaymentMethod>(xml);

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
    public void XmlDeserialization_PayPalPayment_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string xml = """
            <PaymentMethod>
              <PayPal>
                <Email>user@paypal.com</Email>
                <TransactionId>TXN-ABC123</TransactionId>
              </PayPal>
            </PaymentMethod>
            """;

        // Act
        var paymentMethod = XmlSerializationHelper.DeserializeFromXml<PaymentMethod>(xml);

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.PayPal, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.PayPal);
        Assert.Equal("user@paypal.com", paymentMethod.PayPal.Email);
        Assert.Equal("TXN-ABC123", paymentMethod.PayPal.TransactionId);
    }

    [Fact]
    public void XmlDeserialization_ShippingOption_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string xml = """
            <ShippingOption>
              <Overnight>
                <Cost>29.99</Cost>
                <EstimatedDays>1</EstimatedDays>
                <Carrier>DHL</Carrier>
              </Overnight>
            </ShippingOption>
            """;

        // Act
        var shippingOption = XmlSerializationHelper.DeserializeFromXml<ShippingOption>(xml);

        // Assert
        Assert.NotNull(shippingOption);
        Assert.Equal(ShippingOption.ChoiceOf.Overnight, shippingOption.ChoiceType);
        Assert.NotNull(shippingOption.Overnight);
        Assert.Equal(29.99m, shippingOption.Overnight.Cost);
        Assert.Equal(1, shippingOption.Overnight.EstimatedDays);
        Assert.Equal("DHL", shippingOption.Overnight.Carrier);
    }

    [Fact]
    public void XmlDeserialization_SearchCriteriaWithCategoryId_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string xml = """
            <SearchCriteria>
              <CategoryId>42</CategoryId>
            </SearchCriteria>
            """;

        // Act
        var searchCriteria = XmlSerializationHelper.DeserializeFromXml<SearchCriteria>(xml);

        // Assert
        Assert.NotNull(searchCriteria);
        Assert.Equal(SearchCriteria.ChoiceOf.CategoryId, searchCriteria.ChoiceType);
        Assert.Equal(42, searchCriteria.CategoryId);
    }

    [Fact]
    public void XmlDeserialization_NotificationChannel_ShouldDeserializeEnum()
    {
        // Arrange
        const string xml = """
            <NotificationChannel>
              <Channel>Email</Channel>
            </NotificationChannel>
            """;

        // Act
        var channel = XmlSerializationHelper.DeserializeFromXml<NotificationChannel>(xml);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal(NotificationChannelType.Email, channel.Channel);
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void XmlRoundTrip_PaymentMethod_ShouldPreserveData()
    {
        // Arrange
        var original = PaymentMethod.CreateAsCreditCard(
            new CreditCardPayment("4111111111111111", "Alice Johnson", "12/25", "123")
        );

        // Act
        var restored = XmlSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(PaymentMethod.ChoiceOf.CreditCard, restored.ChoiceType);
        Assert.Equal("Alice Johnson", restored.CreditCard?.CardHolderName);
        Assert.Equal("4111111111111111", restored.CreditCard?.CardNumber);
    }

    [Fact]
    public void XmlRoundTrip_ShippingOption_ShouldPreserveData()
    {
        // Arrange
        var original = ShippingOption.CreateAsStandard(
            new ShippingDetails(5.99m, 7, "USPS")
        );

        // Act
        var restored = XmlSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(ShippingOption.ChoiceOf.Standard, restored.ChoiceType);
        Assert.Equal(5.99m, restored.Standard?.Cost);
        Assert.Equal(7, restored.Standard?.EstimatedDays);
        Assert.Equal("USPS", restored.Standard?.Carrier);
    }

    [Fact]
    public void XmlRoundTrip_SearchCriteria_WithPriceRange_ShouldPreserveStruct()
    {
        // Arrange
        var original = SearchCriteria.CreateAsPriceRange(new PriceRange(100m, 500m));

        // Act
        var restored = XmlSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(SearchCriteria.ChoiceOf.PriceRange, restored.ChoiceType);
        Assert.NotNull(restored.PriceRange);
        Assert.Equal(100m, restored.PriceRange.Value.MinPrice);
        Assert.Equal(500m, restored.PriceRange.Value.MaxPrice);
    }

    [Fact]
    public void XmlRoundTrip_SearchCriteria_WithDateRange_ShouldPreserveDates()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var original = SearchCriteria.CreateAsDateRange(new DateRange(startDate, endDate));

        // Act
        var restored = XmlSerializationHelper.RoundTrip(original);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(SearchCriteria.ChoiceOf.DateRange, restored.ChoiceType);
        Assert.NotNull(restored.DateRange);
        Assert.Equal(startDate, restored.DateRange.StartDate.Date);
        Assert.Equal(endDate, restored.DateRange.EndDate.Date);
    }

    [Fact]
    public void XmlRoundTrip_MultipleDifferentChoices_ShouldAllWork()
    {
        // Arrange
        var payments = new[]
        {
            PaymentMethod.CreateAsCreditCard(new CreditCardPayment("4111", "Alice", "12/25", "123")),
            PaymentMethod.CreateAsPayPal(new PayPalPayment("bob@test.com")),
            PaymentMethod.CreateAsBankTransfer(new BankTransferPayment("123", "456", "Chase"))
        };

        // Act & Assert
        foreach (var original in payments)
        {
            var restored = XmlSerializationHelper.RoundTrip(original);
            Assert.NotNull(restored);
            Assert.Equal(original.ChoiceType, restored.ChoiceType);
        }
    }

    #endregion
}
