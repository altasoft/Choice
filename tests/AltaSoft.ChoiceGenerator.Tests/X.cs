using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests;

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
#pragma warning disable CS0628 // New protected member declared in sealed type

public sealed partial class Authorisation1Choice /*: IXmlSerializable*/
{
    /// <summary>
    /// <para>Choice enum </para>
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public ChoiceOf ChoiceType { get; private set; }

    private AltaSoft.ChoiceGenerator.Tests.Authorisation1Code? _code;

    /// <summary>
    /// Specifies the authorisation, in a coded form.
    /// </summary>
    [DisallowNull]
    [XmlElement("Cd")]
    public partial AltaSoft.ChoiceGenerator.Tests.Authorisation1Code? Code
    {
        get => _code;
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
    [XmlElement("Prtry")]
    public partial Proprietary? Proprietary
    {
        get => _proprietary;
        set
        {
            _proprietary = value ?? throw new InvalidOperationException("Choice value cannot be null");
            _code = null;
            ChoiceType = ChoiceOf.Proprietary;
        }
    }

    private static readonly XmlSerializer s_proprietarySerializer = new(typeof(Proprietary), new XmlRootAttribute("Prtry"));

    /// <summary>
    /// Creates a new <see cref="AltaSoft.ChoiceGenerator.Tests.Authorisation1Choice"/> instance and sets its value using the specified <see cref="AltaSoft.ChoiceGenerator.Tests.Authorisation1Code"/>.
    /// </summary>
    /// <param name="value">The value to assign to the created choice instance.</param>
    public static AltaSoft.ChoiceGenerator.Tests.Authorisation1Choice CreateAsCode(AltaSoft.ChoiceGenerator.Tests.Authorisation1Code value) => new() { Code = value };

    /// <summary>
    /// Creates a new <see cref="AltaSoft.ChoiceGenerator.Tests.Authorisation1Choice"/> instance and sets its value using the specified <see cref="Proprietary"/>.
    /// </summary>
    /// <param name="value">The value to assign to the created choice instance.</param>
    public static AltaSoft.ChoiceGenerator.Tests.Authorisation1Choice CreateAsProprietary(Proprietary value) => new() { Proprietary = value };

    /// <summary>
    /// <para>Applies the appropriate function based on the current choice type</para>
    /// </summary>
    /// <typeparam name="TResult">The return type of the provided match functions</typeparam>
    /// <param name="matchCode">Function to invoke if the choice is a <see cref="ChoiceOf.Code"/> value</param>
    /// <param name="matchProprietary">Function to invoke if the choice is a <see cref="ChoiceOf.Proprietary"/> value</param>
    public TResult Match<TResult>(
        Func<AltaSoft.ChoiceGenerator.Tests.Authorisation1Code, TResult> matchCode,
        Func<Proprietary, TResult> matchProprietary)
    {
        return ChoiceType switch
        {
            ChoiceOf.Code => matchCode(Code!.Value),
            ChoiceOf.Proprietary => matchProprietary(Proprietary!),
            _ => throw new InvalidOperationException($"Invalid ChoiceType. '{ChoiceType}'")
        };
    }

    /// <summary>
    /// <para>Applies the appropriate Action based on the current choice type</para>
    /// </summary>
    /// <param name="matchCode">Action to invoke if the choice is a <see cref="ChoiceOf.Code"/> value</param>
    /// <param name="matchProprietary">Action to invoke if the choice is a <see cref="ChoiceOf.Proprietary"/> value</param>
    public void Switch(
        Action<AltaSoft.ChoiceGenerator.Tests.Authorisation1Code> matchCode,
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
                throw new XmlException($"Invalid ChoiceType. '{ChoiceType}'");
        }
    }

    /// <summary>
    /// Implicitly converts an <see cref="AltaSoft.ChoiceGenerator.Tests.Authorisation1Code"/> to an <see cref="Authorisation1Choice"/>.
    /// </summary>
    /// <param name="value">The <see cref="AltaSoft.ChoiceGenerator.Tests.Authorisation1Code"/> to convert.</param>
    /// <returns>
    /// <see cref="Authorisation1Choice"/> instance representing the code.
    /// </returns>
    public static implicit operator Authorisation1Choice(AltaSoft.ChoiceGenerator.Tests.Authorisation1Code value) => CreateAsCode(value);

    /// <summary>
    /// Implicitly converts an <see cref="Proprietary"/> to an <see cref="Authorisation1Choice"/>.
    /// </summary>
    /// <param name="value">The <see cref="Proprietary"/> to convert.</param>
    /// <returns>
    /// <see cref="Authorisation1Choice"/> instance representing the code.
    /// </returns>
    public static implicit operator Authorisation1Choice(Proprietary value) => CreateAsProprietary(value);

    /// <summary>
    /// <para>Choice enumeration</para>
    /// </summary>
    [XmlType("ChoiceOf.Authorisation1Code")]
    public enum ChoiceOf
    {
        /// <summary>
        /// Specifies the authorisation, in a coded form.
        /// </summary>
        Code,
        /// <summary>
        /// Specifies the authorisation, in a free text form.
        /// </summary>
        Proprietary,
    }
}
