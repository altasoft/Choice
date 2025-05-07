using System;

namespace AltaSoft.Choice;

/// <summary>
/// An attribute used to associate a property with a specific XML Tag.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class XmlTagAttribute : Attribute
{
    /// <summary>
    /// Gets the XML tag value associated with the property.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the XML tag.
    /// </value>
    public string Tag { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTagAttribute"/> class with the specified XML tag value.
    /// </summary>
    /// <param name="tag">The XML tag value to associate with the property.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is <c>null</c> or empty.</exception>
    public XmlTagAttribute(string tag)
    {
        Tag = tag;
    }
}
