using Xunit;

namespace AltaSoft.ChoiceGenerator.Tests;

public class ChoiceGeneratorTests
{
    [Fact]
    public void GenerateTwoDifferentTypeChoice()
    {
        var choice = TwoDifferentTypeChoice.CreateAsIntChoice(1);

        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.IntChoice, choice.ChoiceType);
        Assert.Equal(1, choice.IntChoice);
        Assert.Null(choice.StringChoice);
        Assert.True(choice.Item is 1);

        var value = choice.Match(_ => "str", _ => "int");
        Assert.Equal("int", value);

        choice = TwoDifferentTypeChoice.CreateAsStringChoice("value");

        Assert.Equal(TwoDifferentTypeChoice.ChoiceOf.StringChoice, choice.ChoiceType);
        Assert.Null(choice.IntChoice);
        Assert.Equal("value", choice.StringChoice);
        Assert.True(choice.Item is "value");

        value = choice.Match(_ => "str", _ => "int");
        Assert.Equal("str", value);
    }

    [Fact]
    public void GenerateTwoSameTypeChoice()
    {
        var choice = TwoSameTypeChoice.CreateAsStringChoiceOne("one");

        Assert.Equal(TwoSameTypeChoice.ChoiceOf.StringChoiceOne, choice.ChoiceType);
        Assert.Equal("one", choice.StringChoiceOne);
        Assert.Null(choice.StringChoiceTwo);
        Assert.True(choice.Item is "one");

        var value = choice.Match(_ => "strOne", _ => "strTwo");
        Assert.Equal("strOne", value);

        choice = TwoSameTypeChoice.CreateAsStringChoiceTwo("two");

        Assert.Equal(TwoSameTypeChoice.ChoiceOf.StringChoiceTwo, choice.ChoiceType);
        Assert.Null(choice.StringChoiceOne);
        Assert.Equal("two", choice.StringChoiceTwo);
        Assert.True(choice.Item is "two");

        value = choice.Match(_ => "strOne", _ => "strTwo");
        Assert.Equal("strTwo", value);
    }
}
