using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AltaSoft.Choice;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

#region Test Model Definitions

/// <summary>
/// Choice type with one ordinary property and two choice properties
/// </summary>
[Choice]
public sealed partial class OrderChoice
{
    /// <summary>
    /// Ordinary property - Order ID (not part of the choice)
    /// </summary>
    [XmlElement("OrderId")]
    public string? OrderId { get; set; }

    /// <summary>
    /// Choice property - Express delivery
    /// </summary>
    [XmlTag("Express")]
    public partial ExpressDelivery? Express { get; set; }

    /// <summary>
    /// Choice property - Standard delivery
    /// </summary>
    [XmlTag("Standard")]
    public partial StandardDelivery? Standard { get; set; }
}

public sealed class ExpressDelivery
{
    public DateTime DeliveryDate { get; set; }
    public decimal SurchargeAmount { get; set; }

    public ExpressDelivery() { }
    public ExpressDelivery(DateTime deliveryDate, decimal surchargeAmount)
    {
        DeliveryDate = deliveryDate;
        SurchargeAmount = surchargeAmount;
    }
}

public sealed class StandardDelivery
{
    public int DeliveryDays { get; set; }
    public decimal ShippingCost { get; set; }

    public StandardDelivery() { }
    public StandardDelivery(int deliveryDays, decimal shippingCost)
    {
        DeliveryDays = deliveryDays;
        ShippingCost = shippingCost;
    }
}

/// <summary>
/// Choice type with multiple ordinary properties and choice properties
/// </summary>
[Choice]
public sealed partial class PaymentRequest
{
    /// <summary>
    /// Ordinary property - Transaction ID
    /// </summary>
    [XmlElement("TxnId")]
    public string? TransactionId { get; set; }

    /// <summary>
    /// Ordinary property - Amount
    /// </summary>
    [XmlElement("Amt")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Ordinary property - Currency
    /// </summary>
    [XmlElement("Ccy")]
    public string? Currency { get; set; }

    /// <summary>
    /// Choice property - Card payment
    /// </summary>
    [XmlTag("Card")]
    public partial CardPayment? Card { get; set; }

    /// <summary>
    /// Choice property - Bank transfer
    /// </summary>
    [XmlTag("BankTrf")]
    public partial BankTransfer? BankTransfer { get; set; }

    /// <summary>
    /// Choice property - Cash payment
    /// </summary>
    [XmlTag("Cash")]
    public partial CashPayment? Cash { get; set; }
}

public sealed class CardPayment
{
    public string? CardNumber { get; set; }
    public string? ExpiryDate { get; set; }

    public CardPayment() { }
    public CardPayment(string cardNumber, string expiryDate)
    {
        CardNumber = cardNumber;
        ExpiryDate = expiryDate;
    }
}

public sealed class BankTransfer
{
    public string? IBAN { get; set; }
    public string? BIC { get; set; }

    public BankTransfer() { }
    public BankTransfer(string iban, string bic)
    {
        IBAN = iban;
        BIC = bic;
    }
}

public sealed class CashPayment
{
    public string? ReceiptNumber { get; set; }
    public DateTime ReceivedDate { get; set; }

    public CashPayment() { }
    public CashPayment(string receiptNumber, DateTime receivedDate)
    {
        ReceiptNumber = receiptNumber;
        ReceivedDate = receivedDate;
    }
}

#endregion

/// <summary>
/// Tests for Choice types that contain both Choice properties (partial) and ordinary properties
/// </summary>
public class MixedPropertiesChoiceTests
{
    private static readonly XmlWriterSettings s_xmlWriterSettings = new()
    {
        OmitXmlDeclaration = true,
        Indent = true
    };

    [Fact]
    public void MixedProperties_OrdinaryPropertyShouldNotAffectChoice()
    {
        var order = new OrderChoice
        {
            OrderId = "ORD-12345",
            Express = new ExpressDelivery(new DateTime(2024, 12, 25), 25.00m)
        };

        Assert.Equal("ORD-12345", order.OrderId);
        Assert.Equal(OrderChoice.ChoiceOf.Express, order.ChoiceType);
        Assert.NotNull(order.Express);
        Assert.Null(order.Standard);
    }

    [Fact]
    public void MixedProperties_ChangingChoiceShouldNotAffectOrdinaryProperty()
    {
        var order = new OrderChoice
        {
            OrderId = "ORD-99999",
            Express = new ExpressDelivery(new DateTime(2024, 12, 25), 25.00m)
        };

        // Change the choice
        order.Standard = new StandardDelivery(5, 10.00m);

        // Ordinary property should remain unchanged
        Assert.Equal("ORD-99999", order.OrderId);
        Assert.Equal(OrderChoice.ChoiceOf.Standard, order.ChoiceType);
        Assert.Null(order.Express);
        Assert.NotNull(order.Standard);
    }

    [Fact]
    public void MixedProperties_MultipleOrdinaryProperties_ShouldNotAffectChoice()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-001",
            Amount = 150.50m,
            Currency = "USD",
            Card = new CardPayment("4111111111111111", "12/25")
        };

        Assert.Equal("TXN-001", payment.TransactionId);
        Assert.Equal(150.50m, payment.Amount);
        Assert.Equal("USD", payment.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.Card, payment.ChoiceType);
        Assert.NotNull(payment.Card);
        Assert.Null(payment.BankTransfer);
        Assert.Null(payment.Cash);
    }

    [Fact]
    public void MixedProperties_SwitchChoice_OrdinaryPropertiesRemainIntact()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-002",
            Amount = 500.00m,
            Currency = "EUR",
            Card = new CardPayment("5500000000000004", "06/26")
        };

        // Switch to BankTransfer
        payment.BankTransfer = new BankTransfer("GB82WEST12345698765432", "WESTGB22");

        // Ordinary properties should remain unchanged
        Assert.Equal("TXN-002", payment.TransactionId);
        Assert.Equal(500.00m, payment.Amount);
        Assert.Equal("EUR", payment.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.BankTransfer, payment.ChoiceType);
        Assert.Null(payment.Card);
        Assert.NotNull(payment.BankTransfer);
        Assert.Null(payment.Cash);

        // Switch to Cash
        payment.Cash = new CashPayment("RCPT-999", new DateTime(2024, 12, 20));

        // Ordinary properties should still remain unchanged
        Assert.Equal("TXN-002", payment.TransactionId);
        Assert.Equal(500.00m, payment.Amount);
        Assert.Equal("EUR", payment.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.Cash, payment.ChoiceType);
        Assert.Null(payment.Card);
        Assert.Null(payment.BankTransfer);
        Assert.NotNull(payment.Cash);
    }

    #region Factory Method Tests

    [Fact]
    public void MixedProperties_CreateAsFactory_ShouldSetChoiceOnly()
    {
        var order = OrderChoice.CreateAsExpress(
            new ExpressDelivery(new DateTime(2024, 12, 31), 30.00m)
        );

        Assert.Null(order.OrderId); // Ordinary property not set by factory
        Assert.Equal(OrderChoice.ChoiceOf.Express, order.ChoiceType);
        Assert.NotNull(order.Express);
        Assert.Equal(30.00m, order.Express.SurchargeAmount);
    }

    [Fact]
    public void MixedProperties_CreateAsFactory_ThenSetOrdinaryProperties()
    {
        var payment = PaymentRequest.CreateAsCard(
            new CardPayment("4012888888881881", "03/27")
        );

        // Set ordinary properties after creation
        payment.TransactionId = "TXN-FACTORY";
        payment.Amount = 299.99m;
        payment.Currency = "GBP";

        Assert.Equal("TXN-FACTORY", payment.TransactionId);
        Assert.Equal(299.99m, payment.Amount);
        Assert.Equal("GBP", payment.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.Card, payment.ChoiceType);
        Assert.NotNull(payment.Card);
    }

    #endregion

    #region Match and Switch Tests

    [Fact]
    public void MixedProperties_Match_ShouldAccessChoiceAndOrdinaryProperties()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-MATCH",
            Amount = 100.00m,
            Currency = "USD",
            Card = new CardPayment("4111111111111111", "01/26")
        };

        var result = payment.Match(
            matchCard: card => $"Card payment: {card.CardNumber} for {payment.Amount} {payment.Currency}",
            matchBankTransfer: bank => $"Bank transfer to {bank.IBAN}",
            matchCash: cash => $"Cash payment receipt {cash.ReceiptNumber}"
        );

        Assert.Equal("Card payment: 4111111111111111 for 100.00 USD", result);
    }

    [Fact]
    public void MixedProperties_Switch_ShouldAccessChoiceAndOrdinaryProperties()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-SWITCH",
            Amount = 250.00m,
            Currency = "EUR",
            BankTransfer = new BankTransfer("DE89370400440532013000", "COBADEFFXXX")
        };

        string output = string.Empty;

        payment.Switch(
            matchCard: card => output = $"Processing card {payment.TransactionId}",
            matchBankTransfer: bank => output = $"Processing bank transfer {payment.TransactionId} to {bank.IBAN}",
            matchCash: cash => output = $"Processing cash {payment.TransactionId}"
        );

        Assert.Equal("Processing bank transfer TXN-SWITCH to DE89370400440532013000", output);
    }

    #endregion

    #region XML Serialization Tests

    [Fact]
    public void MixedProperties_XmlSerialization_OrderChoice_ShouldRoundTrip()
    {
        var order = new OrderChoice
        {
            OrderId = "ORD-XML-001",
            Express = new ExpressDelivery(new DateTime(2024, 12, 25), 25.00m)
        };

        var serializer = new XmlSerializer(typeof(OrderChoice));

        // Serialize
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, order, XmlNamespaceHelper.EmptyNamespace);

        var xml = sw.ToString();

        // Verify structure
        Assert.Contains("<OrderId>ORD-XML-001</OrderId>", xml);
        Assert.Contains("<Express>", xml);
        Assert.DoesNotContain("<Standard", xml);
        Assert.DoesNotContain("xsi:nil", xml);

        // Deserialize
        using var sr = new StringReader(xml);
        var deserialized = (OrderChoice)serializer.Deserialize(sr)!;

        Assert.Equal("ORD-XML-001", deserialized.OrderId);
        Assert.Equal(OrderChoice.ChoiceOf.Express, deserialized.ChoiceType);
        Assert.NotNull(deserialized.Express);
        Assert.Equal(25.00m, deserialized.Express.SurchargeAmount);
        Assert.Null(deserialized.Standard);
    }

    [Fact]
    public void MixedProperties_XmlSerialization_PaymentRequest_CardChoice_ShouldRoundTrip()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-XML-001",
            Amount = 150.00m,
            Currency = "USD",
            Card = new CardPayment("4111111111111111", "12/25")
        };

        var serializer = new XmlSerializer(typeof(PaymentRequest));

        // Serialize
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, payment, XmlNamespaceHelper.EmptyNamespace);

        var xml = sw.ToString();

        // Verify structure
        Assert.Contains("<TxnId>TXN-XML-001</TxnId>", xml);
        Assert.Contains("<Amt>150.00</Amt>", xml);
        Assert.Contains("<Ccy>USD</Ccy>", xml);
        Assert.Contains("<Card>", xml);
        Assert.DoesNotContain("<BankTrf", xml);
        Assert.DoesNotContain("<Cash", xml);
        Assert.DoesNotContain("xsi:nil", xml);

        // Deserialize
        using var sr = new StringReader(xml);
        var deserialized = (PaymentRequest)serializer.Deserialize(sr)!;

        Assert.Equal("TXN-XML-001", deserialized.TransactionId);
        Assert.Equal(150.00m, deserialized.Amount);
        Assert.Equal("USD", deserialized.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.Card, deserialized.ChoiceType);
        Assert.NotNull(deserialized.Card);
        Assert.Equal("4111111111111111", deserialized.Card.CardNumber);
        Assert.Null(deserialized.BankTransfer);
        Assert.Null(deserialized.Cash);
    }

    [Fact]
    public void MixedProperties_XmlSerialization_PaymentRequest_BankTransferChoice_ShouldRoundTrip()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-XML-002",
            Amount = 500.00m,
            Currency = "EUR",
            BankTransfer = new BankTransfer("GB82WEST12345698765432", "WESTGB22")
        };

        var serializer = new XmlSerializer(typeof(PaymentRequest));

        // Serialize
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, payment, XmlNamespaceHelper.EmptyNamespace);

        var xml = sw.ToString();

        // Verify structure
        Assert.Contains("<TxnId>TXN-XML-002</TxnId>", xml);
        Assert.Contains("<Amt>500.00</Amt>", xml);
        Assert.Contains("<Ccy>EUR</Ccy>", xml);
        Assert.Contains("<BankTrf>", xml);
        Assert.DoesNotContain("<Card>", xml);
        Assert.DoesNotContain("<Cash", xml);
        Assert.DoesNotContain("xsi:nil", xml);

        // Deserialize
        using var sr = new StringReader(xml);
        var deserialized = (PaymentRequest)serializer.Deserialize(sr)!;

        Assert.Equal("TXN-XML-002", deserialized.TransactionId);
        Assert.Equal(500.00m, deserialized.Amount);
        Assert.Equal("EUR", deserialized.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.BankTransfer, deserialized.ChoiceType);
        Assert.NotNull(deserialized.BankTransfer);
        Assert.Equal("GB82WEST12345698765432", deserialized.BankTransfer.IBAN);
        Assert.Null(deserialized.Card);
        Assert.Null(deserialized.Cash);
    }

    [Fact]
    public void MixedProperties_XmlSerialization_OnlyOrdinaryPropertiesSet_ShouldNotSerializeChoices()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "TXN-NO-CHOICE",
            Amount = 99.99m,
            Currency = "GBP"
            // No choice property set
        };

        var serializer = new XmlSerializer(typeof(PaymentRequest));

        // Serialize
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, payment, XmlNamespaceHelper.EmptyNamespace);

        var xml = sw.ToString();

        Assert.Contains("<TxnId>TXN-NO-CHOICE</TxnId>", xml);
        Assert.Contains("<Amt>99.99</Amt>", xml);
        Assert.Contains("<Ccy>GBP</Ccy>", xml);
        Assert.DoesNotContain("<Card>", xml);
        Assert.DoesNotContain("<BankTrf>", xml);
        Assert.DoesNotContain("<Cash>", xml);
        Assert.DoesNotContain("xsi:nil", xml);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MixedProperties_ModifyOrdinaryPropertyMultipleTimes_ChoiceShouldRemainStable()
    {
        var order = new OrderChoice
        {
            OrderId = "ORD-001",
            Express = new ExpressDelivery(new DateTime(2024, 12, 25), 25.00m)
        };

        // Modify ordinary property multiple times
        order.OrderId = "ORD-002";
        order.OrderId = "ORD-003";
        order.OrderId = "ORD-004";

        Assert.Equal("ORD-004", order.OrderId);
        Assert.Equal(OrderChoice.ChoiceOf.Express, order.ChoiceType);
        Assert.NotNull(order.Express);
    }

    [Fact]
    public void MixedProperties_SetOrdinaryPropertyToNull_ChoiceShouldRemainUnaffected()
    {
        var order = new OrderChoice
        {
            OrderId = "ORD-NULL",
            Standard = new StandardDelivery(3, 5.00m)
        };

        order.OrderId = null;

        Assert.Null(order.OrderId);
        Assert.Equal(OrderChoice.ChoiceOf.Standard, order.ChoiceType);
        Assert.NotNull(order.Standard);
    }

    [Fact]
    public void MixedProperties_ComplexScenario_AllPropertiesModified()
    {
        var payment = new PaymentRequest
        {
            TransactionId = "INIT",
            Amount = 100m,
            Currency = "USD",
            Card = new CardPayment("1234", "01/25")
        };

        // Modify ordinary properties
        payment.TransactionId = "UPDATED-1";
        payment.Amount = 200m;

        // Switch choice
        payment.BankTransfer = new BankTransfer("IBAN123", "BIC456");

        // Modify ordinary properties again
        payment.Currency = "EUR";
        payment.TransactionId = "UPDATED-2";

        // Switch choice again
        payment.Cash = new CashPayment("RCPT-001", DateTime.Now);

        // Modify ordinary properties one more time
        payment.Amount = 300m;

        // Verify final state
        Assert.Equal("UPDATED-2", payment.TransactionId);
        Assert.Equal(300m, payment.Amount);
        Assert.Equal("EUR", payment.Currency);
        Assert.Equal(PaymentRequest.ChoiceOf.Cash, payment.ChoiceType);
        Assert.Null(payment.Card);
        Assert.Null(payment.BankTransfer);
        Assert.NotNull(payment.Cash);
    }

    #endregion
}
