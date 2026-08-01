using System.Text.Json.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests.TestModels;

/// <summary>
/// Represents a payment method choice for e-commerce scenarios
/// </summary>
[Choice]
public sealed partial class PaymentMethod
{
    /// <summary>
    /// Payment via credit or debit card
    /// </summary>
    [XmlTag("CreditCard")]
    [JsonPropertyName("creditCard")]
    public partial CreditCardPayment? CreditCard { get; set; }

    /// <summary>
    /// Payment via bank transfer
    /// </summary>
    [XmlTag("BankTransfer")]
    [JsonPropertyName("bankTransfer")]
    public partial BankTransferPayment? BankTransfer { get; set; }

    /// <summary>
    /// Payment via PayPal
    /// </summary>
    [XmlTag("PayPal")]
    [JsonPropertyName("payPal")]
    public partial PayPalPayment? PayPal { get; set; }
}

/// <summary>
/// Credit card payment details
/// </summary>
public sealed class CreditCardPayment
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;

    public CreditCardPayment() { }

    public CreditCardPayment(string cardNumber, string cardHolderName, string expiryDate, string cvv)
    {
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        ExpiryDate = expiryDate;
        Cvv = cvv;
    }
}

/// <summary>
/// Bank transfer payment details
/// </summary>
public sealed class BankTransferPayment
{
    public string AccountNumber { get; set; } = string.Empty;
    public string RoutingNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;

    public BankTransferPayment() { }

    public BankTransferPayment(string accountNumber, string routingNumber, string bankName)
    {
        AccountNumber = accountNumber;
        RoutingNumber = routingNumber;
        BankName = bankName;
    }
}

/// <summary>
/// PayPal payment details
/// </summary>
public sealed class PayPalPayment
{
    public string Email { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;

    public PayPalPayment() { }

    public PayPalPayment(string email, string transactionId = "")
    {
        Email = email;
        TransactionId = transactionId;
    }
}
