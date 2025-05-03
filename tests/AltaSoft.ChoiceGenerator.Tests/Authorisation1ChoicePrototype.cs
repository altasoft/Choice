using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TestNamespace;

//#pragma warning disable CS8774 // Member must have a non-null value when exiting.

public sealed partial class Authorisation1ChoicePrototype : IXmlSerializable
{
    /// <summary>
    /// <para>Specifies the authorisation, in a coded form.</para>
    /// </summary>
    [XmlValue("Cd")]
    [JsonPropertyName("cd")]
    public partial Authorisation1Code? Code { get; set; }

    /// <summary>
    /// <para>Specifies the authorisation, in a free text form.</para>
    /// </summary>
    [XmlValue("Prtry")]
    public partial Proprietary? Proprietary { get; set; }

    //----------------------------

    protected Authorisation1ChoicePrototype()
    {
    }

    private static Authorisation1Code StringToCode(string value) => Enum.Parse<Authorisation1Code>(value);
    private static string CodeToString(Authorisation1Code value) => value.ToString();

    // Conditional if not methods != 2
    private static readonly XmlSerializer s_codeSerializer = new XmlSerializer(typeof(Authorisation1Code));
    private static readonly XmlSerializer s_properietarySerializer = new XmlSerializer(typeof(Proprietary));

    /// <summary>
    /// <para>Choice enum </para>
    /// </summary>
    [JsonIgnore]
    public ChoiceOf ChoiceType { get; private set; }

    private Authorisation1Code? _code;

    /// <summary>
    /// Specifies the authorisation, in a coded form.
    /// </summary>
    [DisallowNull]
    public partial Authorisation1Code? Code
    {
        get => _code;
        [MemberNotNull(nameof(Code))]
        set
        {
            _code = value ?? throw new InvalidOperationException("Choice value cannot be null");
            _proprietary = null;
            ChoiceType = ChoiceOf.Code;
        }
    }

    private Proprietary? _proprietary;

    /// <summary>
    /// Specifies the authorisation, in a free text form.
    /// </summary>
    [DisallowNull]
    [NotNullIfNotNull(nameof(_proprietary))]
    public partial Proprietary? Proprietary
    {
        get => _proprietary;
        [MemberNotNull(nameof(_proprietary))]
        set
        {
            _proprietary = value ?? throw new InvalidOperationException("Choice value cannot be null");
            _code = null;
            ChoiceType = ChoiceOf.Proprietary;
        }
    }

    /// <summary>
    /// Creates a new <see cref="Authorisation1ChoicePrototype"/> instance and sets its value using the specified <see cref="Authorisation1Code"/>.
    /// </summary>
    /// <param name="value">The value to assign to the created choice instance.</param>
    public static Authorisation1ChoicePrototype CreateAsCode(Authorisation1Code value) => new() { Code = value };

    /// <summary>
    /// Creates a new <see cref="Authorisation1ChoicePrototype"/> instance and sets its value using the specified <see cref="Proprietary"/>.
    /// </summary>
    /// <param name="value">The value to assign to the created choice instance.</param>
    public static Authorisation1ChoicePrototype CreateAsProprietary(Proprietary value) => new() { Proprietary = value };

    /// <summary>
    /// <para>Applies the appropriate function based on the current choice type</para>
    /// </summary>
    /// <typeparam name="TResult">The return type of the provided match functions</typeparam>
    /// <param name="matchCode">Function to invoke if the choice is a <see cref="ChoiceOf.Code"/> value</param>
    /// <param name="matchProprietary">Function to invoke if the choice is a <see cref="ChoiceOf.Proprietary"/> value</param>
    public TResult Match<TResult>(
        Func<Authorisation1Code, TResult> matchCode,
        Func<Proprietary, TResult> matchProprietary)
    {
        return ChoiceType switch
        {
            ChoiceOf.Code => matchCode(Code!.Value),
            ChoiceOf.Proprietary => matchProprietary(Proprietary!),
            _ => throw new InvalidOperationException($"Invalid ChoiceType. '{ChoiceType}'"),
        };
    }

    /// <summary>
    /// <para>Applies the appropriate Action based on the current choice type</para>
    /// </summary>
    /// <param name="matchCode">Action to invoke if the choice is a <see cref="ChoiceOf.Code"/> value</param>
    /// <param name="matchProprietary">Action to invoke if the choice is a <see cref="ChoiceOf.Proprietary"/> value</param>
    public void Switch(
        Action<Authorisation1Code> matchCode,
        Action<Proprietary> matchProprietary)
    {
        switch (ChoiceType)
        {
            case ChoiceOf.Code:
                matchCode(Code!.Value);
                return;
            case ChoiceOf.Proprietary:
                matchProprietary(Proprietary!);
                return;
            default:
                throw new InvalidOperationException($"Invalid ChoiceType. '{ChoiceType}'");
        }
    }

    /// <inheritdoc/>>
    public XmlSchema? GetSchema() => null;

    /// <inheritdoc/>>
    public void ReadXml(XmlReader reader)
    {
        if (reader is null) throw new ArgumentNullException(nameof(reader));

        reader.MoveToContent();

        // Empty <Authorisation1Choice/> → error (we need exactly one child)
        if (reader.IsEmptyElement)
            throw new XmlException("Authorisation1Choice element must contain exactly one of <Cd> or <Prtry>.");

        reader.ReadStartElement();

        var sawChoice = false;

        while (reader.MoveToContent() == XmlNodeType.Element)
        {
            if (sawChoice)
                throw new XmlException("Authorisation1Choice must contain at most one of <Cd> or <Prtry>.");

            switch (reader.LocalName)
            {
                case "Cd":
                    Code = StringToCode(reader.ReadElementContentAsString());
                    //Code = (Authorisation1Code)(s_codeSerializer.Deserialize(reader) ?? throw new XmlException("dfsdfsdfdsfsd"));
                    sawChoice = true;
                    break;

                case "Prtry":
                    Proprietary = (Proprietary)(s_properietarySerializer.Deserialize(reader) ?? throw new XmlException("aaaaaaaaaaaaaaaa"));
                    sawChoice = true;
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        reader.ReadEndElement();

        if (!sawChoice)
            throw new XmlException("Authorisation1Choice must contain exactly one of <Cd> or <Prtry>.");
    }

    /// <inheritdoc/>>
    public void WriteXml(XmlWriter writer)
    {
        switch (ChoiceType)
        {
            case ChoiceOf.Code:
                writer.WriteStartElement("Cd");
                writer.WriteString(CodeToString(Code!.Value));
                //s_codeSerializer.Serialize(writer, Code!);
                writer.WriteEndElement();
                break;

            case ChoiceOf.Proprietary:
                writer.WriteStartElement("Prtry");
                var serializer = new XmlSerializer(typeof(Proprietary));
                serializer.Serialize(writer, Proprietary!);
                writer.WriteEndElement();
                break;

            default:
                throw new InvalidOperationException("Authorisation1Choice must have ChoiceType set to Code or Proprietary before serializing.");
        }
    }

    /// <summary>
    /// <para>Choice enumeration</para>
    /// </summary>
    [Serializable]
    [XmlType("Authorisation1Choice__ChoiceOf")]
    public enum ChoiceOf
    {
        /// <summary>
        /// Specifies the authorisation, in a coded form.
        /// </summary>
        [XmlEnum("Cd")]
        Code,
        /// <summary>
        /// Specifies the authorisation, in a free text form.
        /// </summary>
        [XmlEnum("Prtry")]
        Proprietary,
    }

}
public class Proprietary
{
    public string Other { get; set; }
}

public enum Authorisation1Code
{
    One,
    Two
}

internal class XmlValueAttribute : Attribute
{
    private string _v;

    public XmlValueAttribute(string v) => _v = v;
}


public static class Test
{
    public static void Method()
    {
        var ch = Authorisation1ChoicePrototype.CreateAsCode(Authorisation1Code.One);
        ch.Code = Authorisation1Code.One;
        if (ch.Code is null)
        {
            Console.WriteLine();
        }

        ch.Proprietary = new Proprietary { Other = "Test" };
        ch.Proprietary = null;
        if (ch.Proprietary is null)
        {
            Console.WriteLine();
        }
    }
}
