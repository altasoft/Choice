using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
public sealed partial class SinglePropertyChoice
{
    public required partial string Value { get; set; }
}
