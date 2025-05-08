using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;
using AltaSoft.Choice;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

public class ChoiceGeneratorTests
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void DateOnlyTypeXmlSerialization()
    {
        const string codeXml = """
                               <DateTypeChoice>
                                 <OnlyDate>2020-01-01</OnlyDate>
                               </DateTypeChoice>
                               """;

        var serializer = new XmlSerializer(typeof(DateTypeChoice));
        using var reader = new StringReader(codeXml);

        var value = (DateTypeChoice)serializer.Deserialize(reader)!;

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true, // 👈 removes the XML declaration
            Indent = true              // optional: nicely formats the output
        };
        // Serialize back to XML
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);

        serializer.Serialize(writer, value, XmlNamespaceHelper.EmptyNamespace);

        var serializedXml = sw.ToString();
        Assert.Equal(codeXml, serializedXml);

    }

    [Fact]
    public void DateTimeTypeXmlSerialization()
    {
        const string codeXml = """
                               <DateTypeChoice>
                                 <DateTimeChoice>2020-01-01T00:00:00</DateTimeChoice>
                               </DateTypeChoice>
                               """;

        var serializer = new XmlSerializer(typeof(DateTypeChoice));
        using var reader = new StringReader(codeXml);

        var value = (DateTypeChoice)serializer.Deserialize(reader)!;

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true, // 👈 removes the XML declaration
            Indent = true              // optional: nicely formats the output
        };
        // Serialize back to XML
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);

        serializer.Serialize(writer, value, XmlNamespaceHelper.EmptyNamespace);

        var serializedXml = sw.ToString();
        Assert.Equal(codeXml, serializedXml);

    }

    [Fact]
    public void TwoValueTypeXmlSerialization()
    {
        const string codeXml = """
                               <TwoValueTypeChoice>
                                 <Code>Two</Code>
                               </TwoValueTypeChoice>
                               """;

        var serializer = new XmlSerializer(typeof(TwoValueTypeChoice));
        using var reader = new StringReader(codeXml);

        var value = (TwoValueTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(value);
        Assert.Equal(Authorisation1Code.Two, value.Code);
        Assert.NotNull(value.Code);
        Assert.Null(value.Integer);
        Assert.Equal(TwoValueTypeChoice.ChoiceOf.Code, value.ChoiceType);

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true, // 👈 removes the XML declaration
            Indent = true              // optional: nicely formats the output
        };
        // Serialize back to XML
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);

        serializer.Serialize(writer, value, XmlNamespaceHelper.EmptyNamespace);

        var serializedXml = sw.ToString();
        Assert.Equal(codeXml, serializedXml);

    }

    [Fact]
    public void XmlSerializationDeserializationOfEnumValue_ShouldSucceed()
    {
        const string codeXml = """
                               <Authorisation1Choice>
                                 <Cd>Two</Cd>
                               </Authorisation1Choice>
                               """;

        var serializer = new XmlSerializer(typeof(Authorisation1Choice));
        using var reader = new StringReader(codeXml);

        var value = (Authorisation1Choice)serializer.Deserialize(reader)!;

        Assert.NotNull(value);
        Assert.Equal(Authorisation1Code.Two, value.Code);
        Assert.Null(value.Proprietary);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Code, value.ChoiceType);

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true, // 👈 removes the XML declaration
            Indent = true              // optional: nicely formats the output
        };
        // Serialize back to XML
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);

        serializer.Serialize(writer, value, XmlNamespaceHelper.EmptyNamespace);

        var serializedXml = sw.ToString();
        Assert.Equal(codeXml, serializedXml);

    }

    [Fact]
    public void XmlSerializationDeserializationOfProprietary_ShouldSucceed()
    {
        const string codeXml = """
                               <Authorisation1Choice>
                                 <Prtry>
                                   <Other>value</Other>
                                 </Prtry>
                               </Authorisation1Choice>
                               """;

        // Deserialize from XML
        var serializer = new XmlSerializer(typeof(Authorisation1Choice));
        using var reader = new StringReader(codeXml);
        var value = (Authorisation1Choice)serializer.Deserialize(reader)!;

        // Assertions on deserialized object
        Assert.NotNull(value);
        Assert.Null(value.Code);
        Assert.NotNull(value.Proprietary);
        Assert.Equal("value", value.Proprietary.Other);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Proprietary, value.ChoiceType);

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true, // 👈 removes the XML declaration
            Indent = true              // optional: nicely formats the output
        };
        // Serialize back to XML
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);

        serializer.Serialize(writer, value, XmlNamespaceHelper.EmptyNamespace);

        var serializedXml = sw.ToString();

        // Assertions on serialized XML
        Assert.Equal(codeXml, serializedXml);
    }

    [Fact]
    public void JsonSerializationDeserializationOfEnumValue_ShouldSucceed()
    {
        const string codeJson = """
                                {
                                  "cd": "Two"
                                }
                                """;

        var value = JsonSerializer.Deserialize<Authorisation1Choice>(codeJson, s_options);
        Assert.NotNull(value);
        Assert.Equal(Authorisation1Code.Two, value.Code);
        Assert.Null(value.Proprietary);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Code, value.ChoiceType);

        var convertedToJson = JsonSerializer.Serialize(value, s_options);
        Assert.Equal(codeJson, convertedToJson);
    }
    [Fact]
    public void JsonSerializationDeserializationOfComplexValue_ShouldSucceed()
    {
        const string codeJson = """
                                {
                                  "Proprietary": {
                                    "Other": "value"
                                  }
                                }
                                """;

        var value = JsonSerializer.Deserialize<Authorisation1Choice>(codeJson);
        Assert.NotNull(value);
        Assert.Null(value.Code);
        Assert.NotNull(value.Proprietary);
        Assert.Equal("value", value.Proprietary.Other);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Proprietary, value.ChoiceType);

        var convertedToJson = JsonSerializer.Serialize(value, s_options);
        Assert.Equal(codeJson, convertedToJson);
    }

    [Fact]
    public void GenerateTwoDifferentTypeChoice()
    {
        var choice = TwoDifferentTypeChoice.CreateAsIntChoice(1);

        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.IntChoice, choice.ChoiceType);
        Assert.Equal(1, choice.IntChoice);
        Assert.Null(choice.StringChoice);

        var value = choice.Match(_ => "str", _ => "int");
        Assert.Equal("int", value);

        choice = TwoDifferentTypeChoice.CreateAsStringChoice("value");

        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.StringChoice, choice.ChoiceType);
        Assert.Null(choice.IntChoice);
        Assert.Equal("value", choice.StringChoice);

        value = choice.Match(_ => "str", _ => "int");
        Assert.Equal("str", value);
    }

    [Fact]
    public void GenerateTwoSameTypeChoice()
    {
        var choice = TwoSameTypeChoice.CreateAsStringChoiceOne("one");

        Assert.Equal(TwoSameTypeChoice.ChoiceOf.StringChoiceOne, choice.ChoiceType);
        Assert.Equal("one", choice.StringChoiceOne);
        Assert.Null(choice.StringChoiceTwo);

        var value = choice.Match(_ => "strOne", _ => "strTwo");
        Assert.Equal("strOne", value);

        choice = TwoSameTypeChoice.CreateAsStringChoiceTwo("two");

        Assert.Equal(TwoSameTypeChoice.ChoiceOf.StringChoiceTwo, choice.ChoiceType);
        Assert.Null(choice.StringChoiceOne);
        Assert.Equal("two", choice.StringChoiceTwo);

        value = choice.Match(_ => "strOne", _ => "strTwo");
        Assert.Equal("strTwo", value);
    }
    [Fact]
    public void MultipleAssignments_ShouldAssignCorrectly()
    {
        Authorisation1Choice choice = Authorisation1Code.Two;
        Assert.Equal(Authorisation1Code.Two, choice.Code);
        Assert.Null(choice.Proprietary);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Code, choice.ChoiceType);

        choice.Proprietary = new Proprietary { Other = "1234" };
        Assert.Null(choice.Code);
        Assert.Equal("1234", choice.Proprietary?.Other);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Proprietary, choice.ChoiceType);

    }

    [Fact]
    public void ImplicitConversions_ShouldConvertCorrectly()
    {
        Authorisation1Choice choice = Authorisation1Code.Two;
        Assert.Equal(Authorisation1Code.Two, choice.Code);
        Assert.Null(choice.Proprietary);

        choice = new Proprietary { Other = "1234" };
        Assert.Null(choice.Code);
        Assert.Equal("1234", choice.Proprietary?.Other);

    }
}
