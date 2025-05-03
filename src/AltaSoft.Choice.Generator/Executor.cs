using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AltaSoft.Choice.Generator.Extensions;
using AltaSoft.Choice.Generator.Helpers;
using AltaSoft.Choice.Generator.Models;
using Microsoft.CodeAnalysis;

namespace AltaSoft.Choice.Generator;

/// <summary>
/// A static class responsible for executing the generation of code for domain primitive types.
/// </summary>
internal static class Executor
{
    /// <summary>
    /// Executes the generation of domain primitives based on the provided parameters.
    /// </summary>
    /// <param name="typesToGenerate">The list of domain primitives to generate.</param>
    /// /// <param name="_"> compilation unit </param>
    /// <param name="context">The source production context.</param>
    internal static void Execute(in ImmutableArray<INamedTypeSymbol?> typesToGenerate, in Compilation _, in SourceProductionContext context)
    {
        if (typesToGenerate.IsDefaultOrEmpty)
            return;

        try
        {
            foreach (var typeSymbol in typesToGenerate)
            {
                if (typeSymbol is null) // Will never happen
                    continue;

                if (!(typeSymbol.GetModifiers() ?? "").Contains("partial"))
                {
                    context.ReportDiagnostic(DiagnosticHelper.ClassMustBePartial(typeSymbol.Locations.FirstOrDefault()));
                }

                var partialProperties = typeSymbol.GetMembersOfType<IPropertySymbol>().Where(x
                    => x is
                    {
                        IsStatic: false, IsWriteOnly: false, CanBeReferencedByName: true, IsPartialDefinition: true,
                        DeclaredAccessibility: Accessibility.Public
                    }).ToList();

                var sb = Process(typeSymbol, partialProperties);
                context.AddSource($"{typeSymbol.Name}.g.cs", sb.ToString());
            }

        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(DiagnosticHelper.GeneralError(Location.None, ex));
        }
    }

    private static PropertyDetails ProcessProperty(IPropertySymbol propertySymbol)
    {
        var xmlTagAttribute = propertySymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == Constants.XmlTagAttributeFullName);
        var xmlElementName = (string?)xmlTagAttribute?.ConstructorArguments[0].Value ?? propertySymbol.Name;

        var typeFullName = propertySymbol.Type.GetFriendlyName();
        var propertyName = propertySymbol.Name;

        return new PropertyDetails(
            name: propertyName,
            typeName: typeFullName.Replace("?", ""),
            @namespace: propertySymbol.ContainingNamespace.ToDisplayString(),
            xmlNameValue: xmlElementName,
            summary: propertySymbol.GetSummaryText(),
            typeSymbol: propertySymbol.Type);
    }

    private static SourceCodeBuilder Process(INamedTypeSymbol typeSymbol, List<IPropertySymbol> properties)
    {
        var processedProperties = properties.ConvertAll(ProcessProperty);
        var usingStatements = processedProperties.Select(x => x.Namespace).Concat(s_baseNamespaces);
        var sb = new SourceCodeBuilder();

        sb.AppendSourceHeader("AltaSoft Choice.Generator");
        sb.AppendUsings(usingStatements);
        sb.NewLine();
        sb.AppendNamespace(typeSymbol.ContainingNamespace.ToDisplayString());

        sb.AppendClass(typeSymbol.IsRecord, typeSymbol.GetModifiers() ?? "public partial", typeSymbol.Name);

        sb.NewLine();

        var summary = $"<para> Choice element. One of: <list type=\"bullet\"/> {string.Join("", processedProperties.Select(Func))} </para>";

        sb.AppendSummary(summary);
        foreach (var propertySymbol in processedProperties)
        {

            sb.Append("[XmlElement(\"").Append(propertySymbol.XmlNameValue).Append("\", typeof(").Append(propertySymbol.TypeName).AppendLine("))]");
        }

        sb.AppendLine("[XmlChoiceIdentifier(nameof(ChoiceType))]");
        sb.AppendLine("[JsonIgnore]");
        sb.AppendLine("[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]");
        sb.AppendLine("public object Item { get; set; } = default!;");
        sb.NewLine();

        sb.AppendSummary("<para>Choice enum </para>");
        sb.AppendLine("[XmlIgnore]");
        sb.AppendLine("[JsonIgnore]");
        sb.AppendLine("public ChoiceOf ChoiceType { get; set; }");
        sb.NewLine();

        foreach (var propertySymbol in processedProperties)
        {
            if (propertySymbol.Summary is not null)
                sb.AppendSummary(propertySymbol.Summary);

            sb.AppendLine("[XmlIgnore]");
            sb.AppendLine("[JsonInclude]");

            sb.Append("public partial ").Append(propertySymbol.TypeName).Append("? ").Append(propertySymbol.Name)
                .Append(" { get => ChoiceType == ChoiceOf.").Append(propertySymbol.Name).Append(" ? GetAs")
                .Append(propertySymbol.Name).Append("() : (").Append(propertySymbol.TypeName)
                .Append("?)null; set => SetAs").Append(propertySymbol.Name)
                .AppendLine("(value ?? throw new JsonException(\"Choice value cannot be null\")); }");

            sb.NewLine();
        }

        sb.NewLine();

        foreach (var propertySymbol in processedProperties)
        {
            sb.Append("private ").Append(propertySymbol.TypeName).Append(" GetAs").Append(propertySymbol.Name).Append("() => (")
                 .Append(propertySymbol.TypeName).AppendLine(")Item;");

            sb.NewLine();

            sb.Append("private void SetAs").Append(propertySymbol.Name).Append("(")
                .Append(propertySymbol.TypeName).AppendLine(" value)")
                .OpenBracket()
                .AppendLine("Item = value;")
                .Append("ChoiceType = ChoiceOf.").Append(propertySymbol.Name).AppendLine(";")
                .CloseBracket();

            sb.NewLine();

            var typeFullName = typeSymbol.GetFullName();
            sb.AppendSummary($"Creates a new <see cref=\"{typeFullName}\"/> instance " +
                             $"and sets its value using the specified <see cref=\"{propertySymbol.TypeName}\"/>.");
            sb.AppendParamDescription("value", "The value to assign to the created choice instance.");

            sb.Append($"public static {typeFullName} CreateAs").Append(propertySymbol.Name).Append("(")
                .Append(propertySymbol.TypeName).AppendLine(" value)").OpenBracket()
                .Append("var instance = new ").Append(typeSymbol.GetFullName()).AppendLine("();")
                .Append("instance.SetAs").Append(propertySymbol.Name).AppendLine("(value);")
                .Append("return instance;")
                .CloseBracket();

            sb.NewLine();
        }

        ProcessMatch(sb, processedProperties);

        sb.NewLine();

        ProcessSwitch(sb, processedProperties);

        sb.NewLine();
        sb.AppendSummary("<para>Choice enumeration</para>");

        sb.AppendLine("[Serializable]")
            .Append("[XmlType(\"").Append(typeSymbol.Name).AppendLine("__ChoiceOf\")]");

        sb.AppendLine("public enum ChoiceOf")
            .OpenBracket();

        foreach (var propertySymbol in processedProperties)
        {
            if (propertySymbol.Summary is not null)
                sb.AppendSummary(propertySymbol.Summary);

            sb.AppendLine($"[XmlEnum(\"{propertySymbol.XmlNameValue}\")]")
                .Append(propertySymbol.Name).AppendLine(", ");
        }

        sb.CloseBracket();
        sb.CloseBracket();

        return sb;

        static string Func(PropertyDetails p) => $"<para><item><term>{p.XmlNameValue}</term></item><description>{p.Name} <see cref = \"{p.TypeName}\"/></description> - {p.Summary}  </para>";
    }

    private static void ProcessMatch(SourceCodeBuilder sb, List<PropertyDetails> processedProperties)
    {

        sb.AppendSummary("<para>Applies the appropriate function based on the current choice type</para>");
        sb.AppendTypeParamDescription("TResult", "The return type of the provided match functions");
        processedProperties.ForEach(x =>
        {
            sb.AppendParamDescription($"match{x.Name}", $"Function to invoke if the choice is a <see cref=\"ChoiceOf.{x.Name}\"/> value");
        });

        sb.AppendLine("public TResult Match<TResult>(");
        var idx = 0;
        foreach (var prop in processedProperties)
        {
            sb.Append($"\tFunc<{prop.TypeName}, TResult> match{prop.Name}");
            if (idx++ != processedProperties.Count - 1)
            {
                sb.AppendLine(", ");
            }
        }

        sb.AppendLine(")")
            .OpenBracket();

        foreach (var prop in processedProperties)
        {
            sb.AppendLine($"if(ChoiceType == ChoiceOf.{prop.Name})");
            sb.Append($"\treturn match{prop.Name}({prop.Name}!")
                .AppendIf(prop.TypeSymbol.IsValueType, ".Value")
                .AppendLine(");")
                .NewLine();

        }

        sb.AppendLine("throw new InvalidOperationException($\"Invalid ChoiceType. '{ChoiceType}'\");");
        sb.CloseBracket();

    }

    private static void ProcessSwitch(SourceCodeBuilder sb, List<PropertyDetails> processedProperties)
    {

        sb.AppendSummary("<para>Applies the appropriate Action based on the current choice type</para>");
        processedProperties.ForEach(x =>
        {
            sb.AppendParamDescription($"match{x.Name}", $"Action to invoke if the choice is a <see cref=\"ChoiceOf.{x.Name}\"/> value");
        });

        sb.AppendLine("public void Switch(");
        var idx = 0;
        foreach (var prop in processedProperties)
        {
            sb.Append($"\tAction<{prop.TypeName}> match{prop.Name}");
            if (idx++ != processedProperties.Count - 1)
            {
                sb.AppendLine(", ");
            }
        }

        sb.AppendLine(")")
            .OpenBracket();

        foreach (var prop in processedProperties)
        {
            sb.AppendLine($"if(ChoiceType == ChoiceOf.{prop.Name}!)")
                .OpenBracket()
                .Append($"match{prop.Name}({prop.Name}!")
                .AppendIf(prop.TypeSymbol.IsValueType, ".Value")
                .AppendLine(");")
                .AppendLine("return;")
                .CloseBracket()
                .NewLine();

        }

        sb.AppendLine("throw new InvalidOperationException($\"Invalid ChoiceType. '{ChoiceType}'\");");
        sb.CloseBracket();
    }

    private static readonly List<string> s_baseNamespaces =
    [
        "System",
        "System.ComponentModel",
        "System.Text.Json",
        "System.Text.Json.Serialization",
        "System.Xml.Serialization"
    ];
}
