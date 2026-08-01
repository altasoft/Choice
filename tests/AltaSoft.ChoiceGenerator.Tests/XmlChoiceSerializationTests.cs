using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AltaSoft.Choice;
using AltaSoft.ChoiceGenerator.Tests.OtherNamespace;
using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

/// <summary>
/// Comprehensive tests for XML serialization and deserialization of Choice types.
/// </summary>
public class XmlChoiceSerializationTests
{
    private static readonly XmlWriterSettings s_xmlWriterSettings = new()
    {
        OmitXmlDeclaration = true,
        Indent = true
    };

    #region TwoDifferentTypeChoice Tests

    [Fact]
    public void XmlSerialization_TwoDifferentTypeChoice_StringChoice_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoDifferentTypeChoice>
                                     <StringChoice>test value</StringChoice>
                                   </TwoDifferentTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoDifferentTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoDifferentTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.StringChoice, choice.ChoiceType);
        Assert.Equal("test value", choice.StringChoice);
        Assert.Null(choice.IntChoice);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_TwoDifferentTypeChoice_IntChoice_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoDifferentTypeChoice>
                                     <IntChoice>42</IntChoice>
                                   </TwoDifferentTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoDifferentTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoDifferentTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.IntChoice, choice.ChoiceType);
        Assert.Equal(42, choice.IntChoice);
        Assert.Null(choice.StringChoice);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_TwoDifferentTypeChoice_CreateAsStringChoice_ShouldSerializeCorrectly()
    {
        var choice = TwoDifferentTypeChoice.CreateAsStringChoice("xml test");

        var serializer = new XmlSerializer(typeof(TwoDifferentTypeChoice));
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var xml = sw.ToString();
        Assert.Contains("<StringChoice>xml test</StringChoice>", xml);
        Assert.DoesNotContain("<IntChoice>", xml);
    }

    #endregion

    #region TwoSameTypeChoice Tests

    [Fact]
    public void XmlSerialization_TwoSameTypeChoice_StringChoiceOne_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoSameTypeChoice>
                                     <StringChoiceOne>first</StringChoiceOne>
                                   </TwoSameTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoSameTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoSameTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoSameTypeChoice.ChoiceOf.StringChoiceOne, choice.ChoiceType);
        Assert.Equal("first", choice.StringChoiceOne);
        Assert.Null(choice.StringChoiceTwo);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_TwoSameTypeChoice_StringChoiceTwo_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoSameTypeChoice>
                                     <StringChoiceTwo>second</StringChoiceTwo>
                                   </TwoSameTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoSameTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoSameTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoSameTypeChoice.ChoiceOf.StringChoiceTwo, choice.ChoiceType);
        Assert.Null(choice.StringChoiceOne);
        Assert.Equal("second", choice.StringChoiceTwo);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    #endregion

    #region TwoValueTypeChoice Tests

    [Fact]
    public void XmlSerialization_TwoValueTypeChoice_Integer_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoValueTypeChoice>
                                     <Integer>999</Integer>
                                   </TwoValueTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoValueTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoValueTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoValueTypeChoice.ChoiceOf.Integer, choice.ChoiceType);
        Assert.Equal(999, choice.Integer);
        Assert.Null(choice.Code);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_TwoValueTypeChoice_EnumCode_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoValueTypeChoice>
                                     <Code>One</Code>
                                   </TwoValueTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoValueTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoValueTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoValueTypeChoice.ChoiceOf.Code, choice.ChoiceType);
        Assert.Equal(Authorisation1Code.One, choice.Code);
        Assert.Null(choice.Integer);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    #endregion

    #region Authorisation1Choice with XmlTag Tests

    [Fact]
    public void XmlSerialization_Authorisation1Choice_WithCustomXmlTag_Code_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <Authorisation1Choice>
                                     <Cd>One</Cd>
                                   </Authorisation1Choice>
                                   """;

        var serializer = new XmlSerializer(typeof(Authorisation1Choice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (Authorisation1Choice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Code, choice.ChoiceType);
        Assert.Equal(Authorisation1Code.One, choice.Code);
        Assert.Null(choice.Proprietary);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_Authorisation1Choice_WithCustomXmlTag_Proprietary_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <Authorisation1Choice>
                                     <Prtry>
                                       <Other>custom data</Other>
                                     </Prtry>
                                   </Authorisation1Choice>
                                   """;

        var serializer = new XmlSerializer(typeof(Authorisation1Choice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (Authorisation1Choice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(Authorisation1Choice.ChoiceOf.Proprietary, choice.ChoiceType);
        Assert.Null(choice.Code);
        Assert.NotNull(choice.Proprietary);
        Assert.Equal("custom data", choice.Proprietary.Other);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    #endregion

    #region DateTypeChoice with DateOnly Tests

    [Fact]
    public void XmlSerialization_DateTypeChoice_OnlyDate_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <DateTypeChoice>
                                     <OnlyDate>2024-12-25</OnlyDate>
                                   </DateTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(DateTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (DateTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(DateTypeChoice.ChoiceOf.OnlyDate, choice.ChoiceType);
        Assert.Equal(new DateOnly(2024, 12, 25), choice.OnlyDate);
        Assert.Null(choice.DateTimeChoice);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_DateTypeChoice_DateTime_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <DateTypeChoice>
                                     <DateTimeChoice>2024-12-25T15:30:45</DateTimeChoice>
                                   </DateTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(DateTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (DateTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(DateTypeChoice.ChoiceOf.DateTimeChoice, choice.ChoiceType);
        Assert.Null(choice.OnlyDate);
        Assert.Equal(new DateTime(2024, 12, 25, 15, 30, 45), choice.DateTimeChoice);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    #endregion

    #region Edge Cases and Validation

    [Fact]
    public void XmlSerialization_EmptyStringValue_ShouldSerializeAndDeserialize()
    {
        const string expectedXml = """
                                   <TwoDifferentTypeChoice>
                                     <StringChoice />
                                   </TwoDifferentTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoDifferentTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoDifferentTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.StringChoice, choice.ChoiceType);
        Assert.Equal(string.Empty, choice.StringChoice);
    }

    [Fact]
    public void XmlSerialization_SpecialCharacters_ShouldBeEscaped()
    {
        const string expectedXml = """
                                   <TwoDifferentTypeChoice>
                                     <StringChoice>&lt;test&gt; &amp; "quotes"</StringChoice>
                                   </TwoDifferentTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoDifferentTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoDifferentTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal("<test> & \"quotes\"", choice.StringChoice);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    [Fact]
    public void XmlSerialization_NegativeInteger_ShouldRoundTrip()
    {
        const string expectedXml = """
                                   <TwoDifferentTypeChoice>
                                     <IntChoice>-42</IntChoice>
                                   </TwoDifferentTypeChoice>
                                   """;

        var serializer = new XmlSerializer(typeof(TwoDifferentTypeChoice));

        // Deserialize
        using var reader = new StringReader(expectedXml);
        var choice = (TwoDifferentTypeChoice)serializer.Deserialize(reader)!;

        Assert.NotNull(choice);
        Assert.Equal(-42, choice.IntChoice);

        // Serialize back
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, s_xmlWriterSettings);
        serializer.Serialize(writer, choice, XmlNamespaceHelper.EmptyNamespace);

        var actualXml = sw.ToString();
        Assert.Equal(expectedXml, actualXml);
    }

    #endregion
}
