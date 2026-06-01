using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
public sealed partial class SinglePropertyIntChoice
{
    public required partial int Value { get; set; }
}
