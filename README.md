# AltaSoft.ChoiceGenerator

[![NuGet - AltaSoft.Choice](https://img.shields.io/nuget/v/AltaSoft.Choice?label=AltaSoft.Choice)](https://www.nuget.org/packages/AltaSoft.Choice)
[![NuGet - AltaSoft.Choice.Generator](https://img.shields.io/nuget/v/AltaSoft.Choice.Generator?label=AltaSoft.Choice.Generator)](https://www.nuget.org/packages/AltaSoft.Choice.Generator)
[![Dot NET 8+](https://img.shields.io/static/v1?label=DOTNET&message=8%2B&color=0c3c60&style=for-the-badge)](https://dotnet.microsoft.com)

**AltaSoft.ChoiceGenerator** is a lightweight C# source generator that allows you to define *choice types* (discriminated unions) with minimal syntax.

---

## ✨ Features

- Simple `[Choice]` attribute for defining alternatives
- Generates type-safe properties
- Supports XML and System.Text.Json serialization
- Includes `CreateAsXxx`, `Match`, and `Switch` methods
- Auto-generates enum for valid choice types

---

## 🛠️ Installation

Add the following NuGet packages to your project:

```xml
<ItemGroup>
  <PackageReference Include="AltaSoft.Choice" Version="x.x.x" />
  <PackageReference Include="AltaSoft.Choice.Generator" Version="x.x.x" PrivateAssets="all" />
</ItemGroup>
```

---

## ✅ Define Your Choice Type

Mark your class with `[Choice]` and define **partial nullable properties** :

```csharp
using AltaSoft.Choice;

[Choice]
public sealed partial class TwoDifferentTypeChoice
{
    public partial string? StringChoice { get; set; }

    public partial int? IntChoice { get; set; }
}
```

---

## ⚙️ Generated Code

Below is the generated code for the example above:

```csharp
#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

public sealed partial class TwoDifferentTypeChoice
{
    [XmlElement("StringChoice", typeof(string))]
    [XmlElement("IntChoice", typeof(int))]
    [XmlChoiceIdentifier(nameof(ChoiceType))]
    [JsonIgnore]
    public object Value { get; set; } = default!;

    [XmlIgnore]
    [JsonIgnore]
    public ChoiceOf ChoiceType { get; set; }

    [XmlIgnore]
    [JsonInclude]
    public partial string? StringChoice
    {
        get => ChoiceType == ChoiceOf.StringChoice ? GetAsStringChoice() : (string?)null;
        set => SetAsStringChoice(value ?? throw new JsonException("Choice value cannot be null"));
    }

    [XmlIgnore]
    [JsonInclude]
    public partial int? IntChoice
    {
        get => ChoiceType == ChoiceOf.IntChoice ? GetAsIntChoice() : (int?)null;
        set => SetAsIntChoice(value ?? throw new JsonException("Choice value cannot be null"));
    }

    private string GetAsStringChoice() => (string)Value;

    private void SetAsStringChoice(string value)
    {
        Value = value;
        ChoiceType = ChoiceOf.StringChoice;
    }

    public static TwoDifferentTypeChoice CreateAsStringChoice(string value)
    {
        var instance = new TwoDifferentTypeChoice();
        instance.SetAsStringChoice(value);
        return instance;
    }

    private int GetAsIntChoice() => (int)Value;

    private void SetAsIntChoice(int value)
    {
        Value = value;
        ChoiceType = ChoiceOf.IntChoice;
    }

    public static TwoDifferentTypeChoice CreateAsIntChoice(int value)
    {
        var instance = new TwoDifferentTypeChoice();
        instance.SetAsIntChoice(value);
        return instance;
    }

    public TResult Match<TResult>(
        Func<string, TResult> matchStringChoice,
        Func<int, TResult> matchIntChoice)
    {
        if (ChoiceType == ChoiceOf.StringChoice)
            return matchStringChoice(StringChoice!);

        if (ChoiceType == ChoiceOf.IntChoice)
            return matchIntChoice(IntChoice!.Value);

        throw new InvalidOperationException($"Invalid ChoiceType: {ChoiceType}");
    }

    public void Switch(
        Action<string> matchStringChoice,
        Action<int> matchIntChoice)
    {
        if (ChoiceType == ChoiceOf.StringChoice)
        {
            matchStringChoice(StringChoice!);
            return;
        }

        if (ChoiceType == ChoiceOf.IntChoice)
        {
            matchIntChoice(IntChoice!.Value);
            return;
        }

        throw new InvalidOperationException($"Invalid ChoiceType: {ChoiceType}");
    }

    [Serializable]
    [XmlType("TwoDifferentTypeChoice__ChoiceOf")]
    public enum ChoiceOf
    {
        [XmlEnum("StringChoice")]
        StringChoice,
        [XmlEnum("IntChoice")]
        IntChoice,
    }
}
```

---

## 💡 Example Usage

```csharp
var choice = TwoDifferentTypeChoice.CreateAsIntChoice(123);

var result = choice.Match(
    str => $"It's a string: {str}",
    num => $"It's a number: {num}"
);

choice.Switch(
    str => Console.WriteLine($"String: {str}"),
    num => Console.WriteLine($"Number: {num}")
);
```

---

## 📦 Projects

- `AltaSoft.Choice`  
  Contains the `[Choice]` marker attribute

- `AltaSoft.Choice.Generator`  
  Implements the source generator that produces boilerplate code

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
