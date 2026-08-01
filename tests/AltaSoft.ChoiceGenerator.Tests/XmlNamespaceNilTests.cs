using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AltaSoft.ChoiceGenerator.Tests.AdvancedXml;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Tests to verify that xsi:nil attributes are NOT generated for null Choice properties
/// </summary>
public class XmlNamespaceNilTests
{
    private static readonly XmlWriterSettings s_xmlWriterSettings = new()
    {
        OmitXmlDeclaration = true,
        Indent = true
    };

    [Fact]
    public void MessageEnvelope_WithPayment_ShouldNotIncludeNilElements()
    {
        var payment = new PaymentInstruction
        {
            InstructionId = "PMT-999",
            Amount = 5000.00m,
            Currency = "EUR",
            DebtorName = "ACME Corp",
            CreditorName = "Supplier Ltd"
        };

        var envelope = new MessageEnvelope
        {
            Header = new ApplicationHeader
            {
                BusinessMessageIdentifier = "MSG-999",
                CreationDateTime = new DateTime(2024, 12, 25, 14, 0, 0),
                MessageDefinitionIdentifier = "pacs.008.001.10"
            },
            Payment = payment
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        // Verify Payment is present
        Assert.Contains("<PmtInstr xmlns=\"urn:test:payment\">", xml);
        Assert.Contains("<InstrId>PMT-999</InstrId>", xml);

        // Verify Account and Customer are NOT present (no xsi:nil)
        Assert.DoesNotContain("AcctRpt", xml);
        Assert.DoesNotContain("CstmrData", xml);
        Assert.DoesNotContain("xsi:nil", xml);
    }

    [Fact]
    public void MessageEnvelope_WithAccount_ShouldNotIncludeNilElements()
    {
        var account = new AccountReport
        {
            ReportId = "RPT-123",
            AccountId = "ACC-456",
            Balance = 10000.00m,
            BalanceDate = new DateTime(2024, 12, 25)
        };

        var envelope = new MessageEnvelope
        {
            Header = new ApplicationHeader
            {
                BusinessMessageIdentifier = "MSG-ACCT",
                CreationDateTime = new DateTime(2024, 12, 25, 10, 0, 0),
                MessageDefinitionIdentifier = "camt.053"
            },
            Account = account
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        // Verify Account is present
        Assert.Contains("<AcctRpt xmlns=\"urn:test:account\">", xml);
        Assert.Contains("<RptId>RPT-123</RptId>", xml);

        // Verify Payment and Customer are NOT present (no xsi:nil)
        Assert.DoesNotContain("PmtInstr", xml);
        Assert.DoesNotContain("CstmrData", xml);
        Assert.DoesNotContain("xsi:nil", xml);
    }

    [Fact]
    public void MessageEnvelope_WithCustomer_ShouldNotIncludeNilElements()
    {
        var customer = new CustomerData
        {
            CustomerId = "CUST-789",
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "+1-555-0100",
            Country = "USA"
        };

        var envelope = new MessageEnvelope
        {
            Header = new ApplicationHeader
            {
                BusinessMessageIdentifier = "MSG-CUST",
                CreationDateTime = new DateTime(2024, 12, 25, 15, 0, 0),
                MessageDefinitionIdentifier = "acmt.001"
            },
            Customer = customer
        };

        var serializer = new XmlSerializer(typeof(MessageEnvelope));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, envelope);

        var xml = sw.ToString();

        // Verify Customer is present
        Assert.Contains("<CstmrData xmlns=\"urn:test:customer\">", xml);
        Assert.Contains("<CstmrId>CUST-789</CstmrId>", xml);

        // Verify Payment and Account are NOT present (no xsi:nil)
        Assert.DoesNotContain("PmtInstr", xml);
        Assert.DoesNotContain("AcctRpt", xml);
        Assert.DoesNotContain("xsi:nil", xml);
    }
}
