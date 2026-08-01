using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AltaSoft.Choice;
using AltaSoft.ChoiceGenerator.Tests.AdvancedXml;
using Xunit;
using Xunit.Abstractions;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Comprehensive tests for XML serialization with namespaces and Choice types in envelope structures.
/// Tests verify that xsi:nil attributes are NOT generated and only the active choice is serialized.
/// </summary>
public class AdvancedXmlNamespaceTests
{
    private readonly ITestOutputHelper _output;

    private static readonly XmlWriterSettings s_xmlWriterSettings = new()
    {
        OmitXmlDeclaration = true,
        Indent = true
    };

    public AdvancedXmlNamespaceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Payment Document Tests

    [Fact]
    public void XmlSerialization_Envelope_WithPaymentDocument_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="urn:test:envelope">
                                     <AppHdr xmlns="urn:test:head">
                                       <BizMsgIdr>MSG-12345</BizMsgIdr>
                                       <CreDtTm>2024-01-15T10:30:00</CreDtTm>
                                       <MsgDefIdr>pacs.008.001.08</MsgDefIdr>
                                     </AppHdr>
                                     <PmtInstr xmlns="urn:test:payment">
                                       <InstrId>PMT-001</InstrId>
                                       <Amt>1500.50</Amt>
                                       <Ccy>USD</Ccy>
                                       <DbtrNm>John Doe</DbtrNm>
                                       <CdtrNm>Jane Smith</CdtrNm>
                                     </PmtInstr>
                                   </Envelope>
                                   """;

        var serializer = new XmlSerializer(typeof(MessageEnvelope));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var envelope = (MessageEnvelope)serializer.Deserialize(reader)!;

        // Validate Header
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Header);
        Assert.Equal("MSG-12345", envelope.Header.BusinessMessageIdentifier);
        Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0), envelope.Header.CreationDateTime);
        Assert.Equal("pacs.008.001.08", envelope.Header.MessageDefinitionIdentifier);

        // Validate Document Choice
        Assert.Equal(MessageEnvelope.ChoiceOf.Payment, envelope.ChoiceType);
        Assert.NotNull(envelope.Payment);
        Assert.Null(envelope.Account);
        Assert.Null(envelope.Customer);

        // Validate Payment Details
        var payment = envelope.Payment;
        Assert.Equal("PMT-001", payment.InstructionId);
        Assert.Equal(1500.50m, payment.Amount);
        Assert.Equal("USD", payment.Currency);
        Assert.Equal("John Doe", payment.DebtorName);
        Assert.Equal("Jane Smith", payment.CreditorName);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_Envelope_CreatePaymentDocument_ShouldSerializeCorrectly()
    {
        var payment = new PaymentInstruction
        {
            InstructionId = "PMT-999",
            Amount = 5000.00m,
            Currency = "EUR",
            DebtorName = "ACME Corp",
            CreditorName = "Supplier Ltd"
        };

        var envelope = MessageEnvelope.CreateAsPayment(payment);
        envelope.Header = new ApplicationHeader
        {
            BusinessMessageIdentifier = "MSG-999",
            CreationDateTime = new DateTime(2024, 12, 25, 14, 0, 0),
            MessageDefinitionIdentifier = "pacs.008.001.10"
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        _output.WriteLine("Generated Payment XML:");
        _output.WriteLine(xml);

        // Validate key elements presence
        Assert.Contains("<Envelope", xml);
        Assert.Contains("xmlns=\"urn:test:envelope\"", xml);
        Assert.Contains("<AppHdr xmlns=\"urn:test:head\">", xml);
        Assert.Contains("<BizMsgIdr>MSG-999</BizMsgIdr>", xml);
        Assert.Contains("<PmtInstr xmlns=\"urn:test:payment\">", xml);
        Assert.Contains("<InstrId>PMT-999</InstrId>", xml);
        Assert.Contains("<Amt>5000.00</Amt>", xml);
        Assert.Contains("<Ccy>EUR</Ccy>", xml);

        // KEY ASSERTIONS: No xsi:nil and no other document types
        Assert.DoesNotContain("xsi:nil=\"true\"", xml);
        Assert.DoesNotContain("<AcctRpt", xml);
        Assert.DoesNotContain("<CstmrData", xml);
    }

    [Fact]
    public void XmlSerialization_Envelope_PaymentOnly_NoNilElements()
    {
        var payment = new PaymentInstruction
        {
            InstructionId = "PMT-VERIFY",
            Amount = 1000.00m,
            Currency = "GBP"
        };

        var envelope = new MessageEnvelope
        {
            Header = new ApplicationHeader
            {
                BusinessMessageIdentifier = "MSG-VERIFY",
                CreationDateTime = new DateTime(2024, 6, 15, 12, 0, 0),
                MessageDefinitionIdentifier = "test.verify"
            },
            Payment = payment
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        // Verify Payment is present with correct namespace
        Assert.Contains("<PmtInstr xmlns=\"urn:test:payment\">", xml);
        Assert.Contains("<InstrId>PMT-VERIFY</InstrId>", xml);

        // Verify Account and Customer are NOT present (no xsi:nil)
        Assert.DoesNotContain("AcctRpt", xml);
        Assert.DoesNotContain("CstmrData", xml);
        Assert.DoesNotContain("xsi:nil", xml);
    }

    #endregion

    #region Account Document Tests

    [Fact]
    public void XmlSerialization_Envelope_WithAccountDocument_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="urn:test:envelope">
                                     <AppHdr xmlns="urn:test:head">
                                       <BizMsgIdr>MSG-ACCT-001</BizMsgIdr>
                                       <CreDtTm>2024-06-15T09:00:00</CreDtTm>
                                       <MsgDefIdr>camt.053.001.08</MsgDefIdr>
                                     </AppHdr>
                                     <AcctRpt xmlns="urn:test:account">
                                       <RptId>RPT-2024-001</RptId>
                                       <AcctId>ACC-123456</AcctId>
                                       <Bal>25000.75</Bal>
                                       <BalDt>2024-06-15T00:00:00</BalDt>
                                     </AcctRpt>
                                   </Envelope>
                                   """;

        var serializer = new XmlSerializer(typeof(MessageEnvelope));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var envelope = (MessageEnvelope)serializer.Deserialize(reader)!;

        // Validate Header
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Header);
        Assert.Equal("MSG-ACCT-001", envelope.Header.BusinessMessageIdentifier);
        Assert.Equal("camt.053.001.08", envelope.Header.MessageDefinitionIdentifier);

        // Validate Document Choice
        Assert.Equal(MessageEnvelope.ChoiceOf.Account, envelope.ChoiceType);
        Assert.Null(envelope.Payment);
        Assert.NotNull(envelope.Account);
        Assert.Null(envelope.Customer);

        // Validate Account Details
        var account = envelope.Account;
        Assert.Equal("RPT-2024-001", account.ReportId);
        Assert.Equal("ACC-123456", account.AccountId);
        Assert.Equal(25000.75m, account.Balance);
        Assert.Equal(new DateTime(2024, 6, 15), account.BalanceDate);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_Envelope_CreateAccountDocument_ShouldSerializeCorrectly()
    {
        var account = new AccountReport
        {
            ReportId = "RPT-TEST",
            AccountId = "ACC-999",
            Balance = 100000.00m,
            BalanceDate = new DateTime(2024, 12, 31)
        };

        var envelope = MessageEnvelope.CreateAsAccount(account);
        envelope.Header = new ApplicationHeader
        {
            BusinessMessageIdentifier = "MSG-TEST",
            CreationDateTime = new DateTime(2024, 11, 1, 10, 30, 0),
            MessageDefinitionIdentifier = "camt.053.001.10"
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        _output.WriteLine("Generated Account XML:");
        _output.WriteLine(xml);

        // Verify Account is present with correct namespace
        Assert.Contains("<AcctRpt xmlns=\"urn:test:account\">", xml);
        Assert.Contains("<RptId>RPT-TEST</RptId>", xml);
        Assert.Contains("<AcctId>ACC-999</AcctId>", xml);
        Assert.Contains("<Bal>100000.00</Bal>", xml);

        // KEY ASSERTIONS: No xsi:nil and no other document types
        Assert.DoesNotContain("xsi:nil=\"true\"", xml);
        Assert.DoesNotContain("<PmtInstr", xml);
        Assert.DoesNotContain("<CstmrData", xml);
    }

    #endregion

    #region Customer Document Tests

    [Fact]
    public void XmlSerialization_Envelope_WithCustomerDocument_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="urn:test:envelope">
                                     <AppHdr xmlns="urn:test:head">
                                       <BizMsgIdr>MSG-CUST-001</BizMsgIdr>
                                       <CreDtTm>2024-03-20T14:30:00</CreDtTm>
                                       <MsgDefIdr>acmt.001.001.06</MsgDefIdr>
                                     </AppHdr>
                                     <CstmrData xmlns="urn:test:customer">
                                       <CstmrId>CUST-789</CstmrId>
                                       <Nm>Alice Johnson</Nm>
                                       <Email>alice.johnson@example.com</Email>
                                       <PhoneNb>+1-555-0123</PhoneNb>
                                       <Ctry>USA</Ctry>
                                     </CstmrData>
                                   </Envelope>
                                   """;

        var serializer = new XmlSerializer(typeof(MessageEnvelope));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var envelope = (MessageEnvelope)serializer.Deserialize(reader)!;

        // Validate Header
        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Header);
        Assert.Equal("MSG-CUST-001", envelope.Header.BusinessMessageIdentifier);
        Assert.Equal("acmt.001.001.06", envelope.Header.MessageDefinitionIdentifier);

        // Validate Document Choice
        Assert.Equal(MessageEnvelope.ChoiceOf.Customer, envelope.ChoiceType);
        Assert.Null(envelope.Payment);
        Assert.Null(envelope.Account);
        Assert.NotNull(envelope.Customer);

        // Validate Customer Details
        var customer = envelope.Customer;
        Assert.Equal("CUST-789", customer.CustomerId);
        Assert.Equal("Alice Johnson", customer.Name);
        Assert.Equal("alice.johnson@example.com", customer.Email);
        Assert.Equal("+1-555-0123", customer.PhoneNumber);
        Assert.Equal("USA", customer.Country);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_Envelope_CreateCustomerDocument_ShouldSerializeCorrectly()
    {
        var customer = new CustomerData
        {
            CustomerId = "CUST-XYZ",
            Name = "Bob Williams",
            Email = "bob@company.com",
            PhoneNumber = "+44-20-1234-5678",
            Country = "UK"
        };

        var envelope = MessageEnvelope.CreateAsCustomer(customer);
        envelope.Header = new ApplicationHeader
        {
            BusinessMessageIdentifier = "MSG-BOB",
            CreationDateTime = new DateTime(2024, 11, 1, 8, 0, 0),
            MessageDefinitionIdentifier = "acmt.002.001.06"
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        _output.WriteLine("Generated Customer XML:");
        _output.WriteLine(xml);

        // Verify Customer is present with correct namespace
        Assert.Contains("<CstmrData xmlns=\"urn:test:customer\">", xml);
        Assert.Contains("<CstmrId>CUST-XYZ</CstmrId>", xml);
        Assert.Contains("<Nm>Bob Williams</Nm>", xml);
        Assert.Contains("<Email>bob@company.com</Email>", xml);

        // KEY ASSERTIONS: No xsi:nil and no other document types
        Assert.DoesNotContain("xsi:nil=\"true\"", xml);
        Assert.DoesNotContain("<PmtInstr", xml);
        Assert.DoesNotContain("<AcctRpt", xml);
    }

    #endregion

    #region Choice Switching and Match Pattern Tests

    [Fact]
    public void XmlSerialization_Envelope_SwitchDocumentType_ShouldWork()
    {
        // Start with Payment
        var envelope = new MessageEnvelope
        {
            Header = new ApplicationHeader
            {
                BusinessMessageIdentifier = "MSG-SWITCH",
                CreationDateTime = DateTime.Now,
                MessageDefinitionIdentifier = "switch.test"
            },
            Payment = new PaymentInstruction
            {
                InstructionId = "PMT-INITIAL",
                Amount = 100m,
                Currency = "USD"
            }
        };

        Assert.Equal(MessageEnvelope.ChoiceOf.Payment, envelope.ChoiceType);
        Assert.NotNull(envelope.Payment);
        Assert.Null(envelope.Account);
        Assert.Null(envelope.Customer);

        // Switch to Account
        envelope.Account = new AccountReport
        {
            ReportId = "RPT-SWITCHED",
            AccountId = "ACC-001",
            Balance = 5000m,
            BalanceDate = DateTime.Now
        };

        Assert.Equal(MessageEnvelope.ChoiceOf.Account, envelope.ChoiceType);
        Assert.Null(envelope.Payment);
        Assert.NotNull(envelope.Account);
        Assert.Null(envelope.Customer);

        // Switch to Customer
        envelope.Customer = new CustomerData
        {
            CustomerId = "CUST-FINAL",
            Name = "Final Customer",
            Email = "final@test.com"
        };

        Assert.Equal(MessageEnvelope.ChoiceOf.Customer, envelope.ChoiceType);
        Assert.Null(envelope.Payment);
        Assert.Null(envelope.Account);
        Assert.NotNull(envelope.Customer);
    }

    [Fact]
    public void XmlSerialization_Envelope_MatchPattern_ShouldWork()
    {
        var paymentEnvelope = MessageEnvelope.CreateAsPayment(new PaymentInstruction
        {
            InstructionId = "PMT-1",
            Amount = 100m
        });

        var result = paymentEnvelope.Match(
            payment => $"Payment: {payment.InstructionId}",
            account => $"Account: {account.AccountId}",
            customer => $"Customer: {customer.Name}"
        );

        Assert.Equal("Payment: PMT-1", result);

        var accountEnvelope = MessageEnvelope.CreateAsAccount(new AccountReport
        {
            AccountId = "ACC-123",
            Balance = 1000m
        });

        result = accountEnvelope.Match(
            payment => $"Payment: {payment.InstructionId}",
            account => $"Account: {account.AccountId}",
            customer => $"Customer: {customer.Name}"
        );

        Assert.Equal("Account: ACC-123", result);

        var customerEnvelope = MessageEnvelope.CreateAsCustomer(new CustomerData
        {
            Name = "Test User"
        });

        result = customerEnvelope.Match(
            payment => $"Payment: {payment.InstructionId}",
            account => $"Account: {account.AccountId}",
            customer => $"Customer: {customer.Name}"
        );

        Assert.Equal("Customer: Test User", result);
    }

    #endregion

    #region Namespace Validation Tests

    [Fact]
    public void XmlSerialization_Envelope_AllNamespacesShouldBeCorrect()
    {
        var envelope = MessageEnvelope.CreateAsPayment(new PaymentInstruction
        {
            InstructionId = "PMT-NS",
            Amount = 50m,
            Currency = "GBP"
        });
        envelope.Header = new ApplicationHeader
        {
            BusinessMessageIdentifier = "NS-TEST",
            CreationDateTime = new DateTime(2024, 1, 1),
            MessageDefinitionIdentifier = "ns.test"
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        _output.WriteLine("Namespace validation XML:");
        _output.WriteLine(xml);

        // Validate all namespaces are present and correct
        Assert.Contains("xmlns=\"urn:test:envelope\"", xml);
        Assert.Contains("xmlns=\"urn:test:head\"", xml);
        Assert.Contains("xmlns=\"urn:test:payment\"", xml);

        // Validate namespace prefixes are not mixed
        Assert.Contains("<AppHdr xmlns=\"urn:test:head\">", xml);
        Assert.Contains("<PmtInstr xmlns=\"urn:test:payment\">", xml);

        // Ensure only Payment namespace is present, not others
        Assert.DoesNotContain("urn:test:account", xml);
        Assert.DoesNotContain("urn:test:customer", xml);
    }

    [Fact]
    public void XmlSerialization_Envelope_EachDocumentType_HasCorrectNamespace()
    {
        // Test Payment namespace
        var paymentEnv = MessageEnvelope.CreateAsPayment(new PaymentInstruction { InstructionId = "P1" });
        var xml1 = SerializeToString(paymentEnv);
        _output.WriteLine("Payment namespace check:");
        _output.WriteLine(xml1);
        Assert.Contains("xmlns=\"urn:test:payment\"", xml1);
        Assert.DoesNotContain("urn:test:account", xml1);
        Assert.DoesNotContain("urn:test:customer", xml1);

        // Test Account namespace
        var accountEnv = MessageEnvelope.CreateAsAccount(new AccountReport { AccountId = "A1" });
        var xml2 = SerializeToString(accountEnv);
        _output.WriteLine("\nAccount namespace check:");
        _output.WriteLine(xml2);
        Assert.Contains("xmlns=\"urn:test:account\"", xml2);
        Assert.DoesNotContain("urn:test:payment", xml2);
        Assert.DoesNotContain("urn:test:customer", xml2);

        // Test Customer namespace
        var customerEnv = MessageEnvelope.CreateAsCustomer(new CustomerData { CustomerId = "C1" });
        var xml3 = SerializeToString(customerEnv);
        _output.WriteLine("\nCustomer namespace check:");
        _output.WriteLine(xml3);
        Assert.Contains("xmlns=\"urn:test:customer\"", xml3);
        Assert.DoesNotContain("urn:test:payment", xml3);
        Assert.DoesNotContain("urn:test:account", xml3);
    }

    #endregion

    #region Helper Methods

    private static string SerializeToString(MessageEnvelope envelope)
    {
        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);
        return sw.ToString();
    }

    #endregion
}

