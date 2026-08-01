using AltaSoft.ChoiceGenerator.Tests.TestModels;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Tests for choice type creation using factory methods and implicit operators
/// </summary>
public class ChoiceTypeCreationTests
{
    #region Factory Method Tests

    [Fact]
    public void CreateAsCreditCard_WithValidData_ShouldCreateCorrectChoice()
    {
        // Arrange
        var cardPayment = new CreditCardPayment("4111111111111111", "John Doe", "12/25", "123");

        // Act
        var paymentMethod = PaymentMethod.CreateAsCreditCard(cardPayment);

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.CreditCard, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.CreditCard);
        Assert.Equal("4111111111111111", paymentMethod.CreditCard.CardNumber);
        Assert.Equal("John Doe", paymentMethod.CreditCard.CardHolderName);
        Assert.Null(paymentMethod.BankTransfer);
        Assert.Null(paymentMethod.PayPal);
    }

    [Fact]
    public void CreateAsBankTransfer_WithValidData_ShouldCreateCorrectChoice()
    {
        // Arrange
        var bankTransfer = new BankTransferPayment("123456789", "987654321", "Bank of America");

        // Act
        var paymentMethod = PaymentMethod.CreateAsBankTransfer(bankTransfer);

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.BankTransfer, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.BankTransfer);
        Assert.Equal("123456789", paymentMethod.BankTransfer.AccountNumber);
        Assert.Null(paymentMethod.CreditCard);
        Assert.Null(paymentMethod.PayPal);
    }

    [Fact]
    public void CreateAsPayPal_WithValidData_ShouldCreateCorrectChoice()
    {
        // Arrange
        var payPal = new PayPalPayment("john.doe@example.com", "TXN-12345");

        // Act
        var paymentMethod = PaymentMethod.CreateAsPayPal(payPal);

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.PayPal, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.PayPal);
        Assert.Equal("john.doe@example.com", paymentMethod.PayPal.Email);
        Assert.Null(paymentMethod.CreditCard);
        Assert.Null(paymentMethod.BankTransfer);
    }

    [Fact]
    public void CreateAsStandard_WithShippingDetails_ShouldCreateCorrectChoice()
    {
        // Arrange
        var shipping = new ShippingDetails(5.99m, 7, "USPS");

        // Act
        var shippingOption = ShippingOption.CreateAsStandard(shipping);

        // Assert
        Assert.NotNull(shippingOption);
        Assert.Equal(ShippingOption.ChoiceOf.Standard, shippingOption.ChoiceType);
        Assert.NotNull(shippingOption.Standard);
        Assert.Equal(5.99m, shippingOption.Standard.Cost);
        Assert.Equal(7, shippingOption.Standard.EstimatedDays);
        Assert.Null(shippingOption.Express);
        Assert.Null(shippingOption.Overnight);
    }

    [Fact]
    public void CreateAsKeyword_WithString_ShouldCreateCorrectChoice()
    {
        // Arrange
        const string keyword = "laptop";

        // Act
        var searchCriteria = SearchCriteria.CreateAsKeyword(keyword);

        // Assert
        Assert.NotNull(searchCriteria);
        Assert.Equal(SearchCriteria.ChoiceOf.Keyword, searchCriteria.ChoiceType);
        Assert.Equal("laptop", searchCriteria.Keyword);
        Assert.Null(searchCriteria.CategoryId);
        Assert.Null(searchCriteria.DateRange);
        Assert.Null(searchCriteria.PriceRange);
    }

    [Fact]
    public void CreateAsCategoryId_WithInt_ShouldCreateCorrectChoice()
    {
        // Arrange
        const int categoryId = 42;

        // Act
        var searchCriteria = SearchCriteria.CreateAsCategoryId(categoryId);

        // Assert
        Assert.NotNull(searchCriteria);
        Assert.Equal(SearchCriteria.ChoiceOf.CategoryId, searchCriteria.ChoiceType);
        Assert.Equal(42, searchCriteria.CategoryId);
        Assert.Null(searchCriteria.Keyword);
        Assert.Null(searchCriteria.DateRange);
        Assert.Null(searchCriteria.PriceRange);
    }

    #endregion

    #region Implicit Operator Tests

    [Fact]
    public void ImplicitOperator_FromCreditCardPayment_ShouldCreateChoice()
    {
        // Arrange
        var cardPayment = new CreditCardPayment("4111111111111111", "Jane Smith", "06/26", "456");

        // Act
        PaymentMethod paymentMethod = cardPayment;

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.CreditCard, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.CreditCard);
        Assert.Equal("Jane Smith", paymentMethod.CreditCard.CardHolderName);
    }

    [Fact]
    public void ImplicitOperator_FromBankTransferPayment_ShouldCreateChoice()
    {
        // Arrange
        var bankTransfer = new BankTransferPayment("987654321", "123456789", "Chase Bank");

        // Act
        PaymentMethod paymentMethod = bankTransfer;

        // Assert
        Assert.NotNull(paymentMethod);
        Assert.Equal(PaymentMethod.ChoiceOf.BankTransfer, paymentMethod.ChoiceType);
        Assert.NotNull(paymentMethod.BankTransfer);
        Assert.Equal("Chase Bank", paymentMethod.BankTransfer.BankName);
    }

    [Fact]
    public void ImplicitOperator_FromString_ShouldCreateSearchCriteria()
    {
        // Arrange
        const string keyword = "smartphone";

        // Act
        SearchCriteria criteria = keyword;

        // Assert
        Assert.NotNull(criteria);
        Assert.Equal(SearchCriteria.ChoiceOf.Keyword, criteria.ChoiceType);
        Assert.Equal("smartphone", criteria.Keyword);
    }

    [Fact]
    public void ImplicitOperator_FromInt_ShouldCreateSearchCriteria()
    {
        // Arrange
        const int categoryId = 100;

        // Act
        SearchCriteria criteria = categoryId;

        // Assert
        Assert.NotNull(criteria);
        Assert.Equal(SearchCriteria.ChoiceOf.CategoryId, criteria.ChoiceType);
        Assert.Equal(100, criteria.CategoryId);
    }

    #endregion

    #region Single Property Choice Tests

    [Fact]
    public void NotificationChannel_SingleProperty_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var channel = NotificationChannel.CreateAsChannel(NotificationChannelType.Email);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal(NotificationChannel.ChoiceOf.Channel, channel.ChoiceType);
        Assert.Equal(NotificationChannelType.Email, channel.Channel);
    }

    [Fact]
    public void NotificationChannel_ImplicitOperator_ShouldWork()
    {
        // Arrange
        const NotificationChannelType type = NotificationChannelType.SMS;

        // Act
        NotificationChannel channel = type;

        // Assert
        Assert.NotNull(channel);
        Assert.Equal(NotificationChannelType.SMS, channel.Channel);
    }

    #endregion
}
