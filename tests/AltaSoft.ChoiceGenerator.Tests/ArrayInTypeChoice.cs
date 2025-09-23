using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests;

[Choice]
public sealed partial class ArrayInTypeChoice
{
    public partial string? StringChoice { get; set; }

    public partial AccountId[]? Accounts { get; set; }
}

public sealed record AccountId(int Id);
