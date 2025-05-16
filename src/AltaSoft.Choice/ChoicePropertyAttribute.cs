using System;

namespace AltaSoft.Choice;

/// <summary>
/// Represents the attribute used to mark a property as a choice property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ChoicePropertyAttribute : Attribute;
