using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests
{
    [Choice]
    public sealed partial class TwoSameTypeChoice
    {
        public partial string? StringChoiceOne { get; set; }

        public partial string? StringChoiceTwo { get; set; }
    }
}
