using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
public sealed partial class SinglePropertyDateOnlyChoice
{
    public required partial string Value { get; set; }
}
