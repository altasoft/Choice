using System;

namespace AltaSoft.Choice
{
    /// <summary>
    /// Represents the attribute used to mark a property as a choice type property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ChoiceTypePropertyAttribute : Attribute;
}
