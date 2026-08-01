using System;
using System.Linq;
using AltaSoft.ChoiceGenerator.Tests.TestModels;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Tests for Switch and Match pattern matching on choice types
/// </summary>
public class ChoiceTypeSwitchMatchTests
{
    #region Match Pattern Tests

    [Fact]
    public void Match_WithCreditCardPayment_ShouldExecuteCorrectBranch()
    {
        // Arrange
        var cardPayment = new CreditCardPayment("4111111111111111", "Alice", "12/25", "123");
        var paymentMethod = PaymentMethod.CreateAsCreditCard(cardPayment);

        // Act
        var result = paymentMethod.Match(
            creditCard => $"Card: {creditCard.CardHolderName}",
            bankTransfer => $"Bank: {bankTransfer.BankName}",
            payPal => $"PayPal: {payPal.Email}"
        );

        // Assert
        Assert.Equal("Card: Alice", result);
    }

    [Fact]
    public void Match_WithBankTransferPayment_ShouldExecuteCorrectBranch()
    {
        // Arrange
        var bankTransfer = new BankTransferPayment("123456", "654321", "Wells Fargo");
        var paymentMethod = PaymentMethod.CreateAsBankTransfer(bankTransfer);

        // Act
        var result = paymentMethod.Match(
            creditCard => $"Card: {creditCard.CardHolderName}",
            bankTransfer => $"Bank: {bankTransfer.BankName}",
            payPal => $"PayPal: {payPal.Email}"
        );

        // Assert
        Assert.Equal("Bank: Wells Fargo", result);
    }

    [Fact]
    public void Match_WithPayPalPayment_ShouldExecuteCorrectBranch()
    {
        // Arrange
        var payPal = new PayPalPayment("bob@example.com", "TXN-789");
        var paymentMethod = PaymentMethod.CreateAsPayPal(payPal);

        // Act
        var result = paymentMethod.Match(
            creditCard => $"Card: {creditCard.CardHolderName}",
            bankTransfer => $"Bank: {bankTransfer.BankName}",
            payPal => $"PayPal: {payPal.Email}"
        );

        // Assert
        Assert.Equal("PayPal: bob@example.com", result);
    }

    [Fact]
    public void Match_WithShippingOptions_ShouldCalculateTotalCost()
    {
        // Arrange
        var expressShipping = new ShippingDetails(15.99m, 2, "FedEx");
        var shippingOption = ShippingOption.CreateAsExpress(expressShipping);

        // Act
        var totalCost = shippingOption.Match(
            standard => standard.Cost,
            express => express.Cost * 1.1m, // 10% processing fee for express
            overnight => overnight.Cost * 1.2m // 20% processing fee for overnight
        );

        // Assert
        Assert.Equal(17.589m, totalCost);
    }

    [Fact]
    public void Match_WithSearchCriteria_ShouldFormatQuery()
    {
        // Arrange
        var searchCriteria = SearchCriteria.CreateAsKeyword("laptop");

        // Act
        var query = searchCriteria.Match(
            keyword => $"keyword={keyword}",
            categoryId => $"category={categoryId}",
            dateRange => $"from={dateRange.StartDate:yyyy-MM-dd}&to={dateRange.EndDate:yyyy-MM-dd}",
            priceRange => $"min={priceRange.MinPrice}&max={priceRange.MaxPrice}"
        );

        // Assert
        Assert.Equal("keyword=laptop", query);
    }

    #endregion

    #region Switch Pattern Tests

    [Fact]
    public void Switch_WithCreditCardPayment_ShouldExecuteCorrectAction()
    {
        // Arrange
        var cardPayment = new CreditCardPayment("4111111111111111", "Charlie", "12/25", "123");
        var paymentMethod = PaymentMethod.CreateAsCreditCard(cardPayment);
        var processedType = string.Empty;

        // Act
        paymentMethod.Switch(
            creditCard => processedType = "credit_card",
            bankTransfer => processedType = "bank_transfer",
            payPal => processedType = "paypal"
        );

        // Assert
        Assert.Equal("credit_card", processedType);
    }

    [Fact]
    public void Switch_WithBankTransferPayment_ShouldExecuteCorrectAction()
    {
        // Arrange
        var bankTransfer = new BankTransferPayment("999888", "777666", "Bank of America");
        var paymentMethod = PaymentMethod.CreateAsBankTransfer(bankTransfer);
        var processedType = string.Empty;

        // Act
        paymentMethod.Switch(
            creditCard => processedType = "credit_card",
            bankTransfer => processedType = "bank_transfer",
            payPal => processedType = "paypal"
        );

        // Assert
        Assert.Equal("bank_transfer", processedType);
    }

    [Fact]
    public void Switch_WithPayPalPayment_ShouldExecuteCorrectAction()
    {
        // Arrange
        var payPal = new PayPalPayment("dana@test.com");
        var paymentMethod = PaymentMethod.CreateAsPayPal(payPal);
        var processedType = string.Empty;

        // Act
        paymentMethod.Switch(
            creditCard => processedType = "credit_card",
            bankTransfer => processedType = "bank_transfer",
            payPal => processedType = "paypal"
        );

        // Assert
        Assert.Equal("paypal", processedType);
    }

    [Fact]
    public void Switch_WithShippingOptions_ShouldAccumulateData()
    {
        // Arrange
        var overnightShipping = new ShippingDetails(29.99m, 1, "DHL");
        var shippingOption = ShippingOption.CreateAsOvernight(overnightShipping);
        var deliveryInfo = new { Days = 0, Cost = 0m, Type = "" };

        // Act
        shippingOption.Switch(
            standard => deliveryInfo = new { Days = standard.EstimatedDays, Cost = standard.Cost, Type = "Standard" },
            express => deliveryInfo = new { Days = express.EstimatedDays, Cost = express.Cost, Type = "Express" },
            overnight => deliveryInfo = new { Days = overnight.EstimatedDays, Cost = overnight.Cost, Type = "Overnight" }
        );

        // Assert
        Assert.Equal(1, deliveryInfo.Days);
        Assert.Equal(29.99m, deliveryInfo.Cost);
        Assert.Equal("Overnight", deliveryInfo.Type);
    }

    [Fact]
    public void Switch_WithSearchCriteria_ShouldModifyExternalState()
    {
        // Arrange
        var priceRange = new PriceRange(100m, 500m);
        var searchCriteria = SearchCriteria.CreateAsPriceRange(priceRange);
        var filterApplied = false;
        var filterType = "";

        // Act
        searchCriteria.Switch(
            keyword => { filterApplied = true; filterType = "text"; },
            categoryId => { filterApplied = true; filterType = "category"; },
            dateRange => { filterApplied = true; filterType = "date"; },
            priceRange => { filterApplied = true; filterType = "price"; }
        );

        // Assert
        Assert.True(filterApplied);
        Assert.Equal("price", filterType);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Match_ChainedWithSwitch_ShouldWorkCorrectly()
    {
        // Arrange
        var standardShipping = new ShippingDetails(5.99m, 7, "USPS");
        var shippingOption = ShippingOption.CreateAsStandard(standardShipping);

        // Act - Use Match to calculate discount
        var discount = shippingOption.Match(
            standard => 0m,
            express => 2m,
            overnight => 5m
        );

        // Switch to apply discount
        var finalCost = 0m;
        shippingOption.Switch(
            standard => finalCost = standard.Cost - discount,
            express => finalCost = express.Cost - discount,
            overnight => finalCost = overnight.Cost - discount
        );

        // Assert
        Assert.Equal(5.99m, finalCost);
    }

    [Fact]
    public void Match_UsedInLinqQuery_ShouldWork()
    {
        // Arrange
        var payments = new[]
        {
            PaymentMethod.CreateAsCreditCard(new CreditCardPayment("4111", "Alice", "12/25", "123")),
            PaymentMethod.CreateAsPayPal(new PayPalPayment("bob@test.com")),
            PaymentMethod.CreateAsBankTransfer(new BankTransferPayment("123", "456", "Chase"))
        };

        // Act
        var paymentTypes = payments.Select(p => p.Match(
            cc => "Card",
            bt => "Transfer",
            pp => "PayPal"
        )).ToArray();

        // Assert
        Assert.Equal(new[] { "Card", "PayPal", "Transfer" }, paymentTypes);
    }

    #endregion
}
