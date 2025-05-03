using System;

namespace AltaSoft.Choice;

/// <summary>
/// An attribute used to mark a class or struct as a "choice".
/// This attribute can only be applied to classes or structs.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ChoiceAttribute : Attribute;
