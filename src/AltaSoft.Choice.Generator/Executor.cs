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

                // Get ordinary (non-partial) required properties
                var ordinaryRequiredProperties = typeSymbol.GetMembersOfType<IPropertySymbol>().Where(x
                    => x is
                    {
                        IsStatic: false, IsWriteOnly: false, CanBeReferencedByName: true, IsPartialDefinition: false,
                        DeclaredAccessibility: Accessibility.Public,
                        IsRequired: true
                    }).ToList();

                var sb = Process(typeSymbol, partialProperties, ordinaryRequiredProperties);
                context.AddSource($"{typeSymbol.Name}.g.cs", sb.ToString());
            }

        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(DiagnosticHelper.GeneralError(Location.None, ex));
        }
    }

    private static SourceCodeBuilder Process(INamedTypeSymbol typeSymbol, List<IPropertySymbol> properties, List<IPropertySymbol> ordinaryRequiredProperties)
    {
        var processedProperties = properties.ConvertAll(ProcessProperty);
        var usingStatements = processedProperties.Select(x => x.Namespace).Concat(s_baseNamespaces);
        var sb = new SourceCodeBuilder();

        sb.AppendSourceHeader("AltaSoft Choice.Generator");
        sb.AppendUsings(usingStatements);
        sb.AppendNamespace(typeSymbol.ContainingNamespace.ToDisplayString());

        sb.AppendLine("#pragma warning disable CS8774 // Member must have a non-null value when exiting.")
            .AppendLine("#pragma warning disable CS0628 // New protected member declared in sealed type")
            .AppendLine("#pragma warning disable CS0618 // Type or member is obsolete")
            .AppendLine("#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor")
            .AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member")
            .AppendLine("#pragma warning disable IDE0290 // Use primary constructor")
            .NewLine();

        sb.AppendClass(typeSymbol.IsRecord, typeSymbol.GetModifiers() ?? "public partial", typeSymbol.Name);

        var hasDefaultCtor = typeSymbol.Constructors.Any(x => x.Parameters.Length == 0 && !x.IsImplicitlyDeclared);
        if (!hasDefaultCtor)
        {
            var isSealedType = typeSymbol.IsSealed;
            sb.AppendSummary("Constructor for Serialization/Deserialization");

            if (isSealedType)
            {
                sb.AppendLine("[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]");
            }

            sb.Append(typeSymbol.IsAbstract ? "protected " : "public ").Append(typeSymbol.Name).AppendLine("()")
                .OpenBracket()
                .CloseBracket()
                .NewLine();
        }

        sb.AppendSummary("<para>Choice enum </para>");
        sb.AppendLine("[JsonIgnore]");
        sb.AppendLine("[XmlIgnore]");
        sb.AppendLine("[ChoiceTypeProperty]");
        sb.AppendLine("public ChoiceOf ChoiceType { get; private set; }");
        sb.NewLine();

        var isOnly1Property = processedProperties.Count == 1;

        foreach (var p in processedProperties)
        {
            var fieldName = p.Name.ToFieldName();

            sb.Append("private ").Append(p.TypeName);
            if (isOnly1Property)
                sb.Append(" ");
            else
                sb.Append("? ");
            sb.Append(fieldName).AppendLine(";").NewLine();

            if (p.Summary is not null)
                sb.AppendSummary(p.Summary);

            var isDateOnly = p.IsDateOnly();
            if (isDateOnly)
            {
                sb.AppendLine("[XmlIgnore]");
            }
            else
            {
                sb.AppendLine("[DisallowNull]");

                // Generate XmlElement attribute with optional namespace
                sb.Append("[XmlElement(\"").Append(p.XmlNameValue).Append("\"");
                if (p.XmlNamespace is not null)
                {
                    sb.Append(", Namespace = \"").Append(p.XmlNamespace).Append("\"");
                }
                sb.AppendLine(")]");
            }

            sb.AppendLine("[ChoiceProperty]");
            sb.Append(p.Modifiers).Append(" ");
            sb.Append(p.TypeName);
            sb.Append(isOnly1Property ? " " : "? ");
            sb.Append(p.Name).OpenBracket();

            sb.AppendIfNotEmpty(p.GetterAccessibility.GetPropertyAccessibilityString()).Append("get => ")
                .Append(fieldName).AppendLine(";");

            sb.AppendIfNotEmpty(p.SetterAccessibility.GetPropertyAccessibilityString()).AppendLine("set")
            .OpenBracket()
            .Append(fieldName);
            if (isOnly1Property)
                sb.AppendLine(" = value;");
            else
                sb.AppendLine(" = value ?? throw new InvalidOperationException(\"Choice value cannot be null\");");
            sb.AppendLines(processedProperties.Where(x => x.Name != p.Name).Select(v => $"{v.Name.ToFieldName()} = null;"))
                .Append("ChoiceType = ChoiceOf.").Append(p.Name).AppendLine(";")
                .CloseBracket()
                .CloseBracket();

            sb.NewLine();

            if (isDateOnly)
            {
                sb.Append("[XmlElement(\"").Append(p.XmlNameValue).Append("\"");
                if (p.XmlNamespace is not null)
                {
                    sb.Append(", Namespace = \"").Append(p.XmlNamespace).Append("\"");
                }
                sb.AppendLine(")]");
                sb.AppendLine("[JsonIgnore]");

                sb.Append($"public string? {p.Name}Surrogate")
                    .OpenBracket()
                    .Append("get => ").Append(p.Name).AppendLine("?.ToString(\"yyyy-MM-dd\", CultureInfo.InvariantCulture);")
                    .Append("set => ").Append(p.Name).AppendLine(" =value is null? null : DateOnly.Parse(value, CultureInfo.InvariantCulture);")
                    .CloseBracket();
                sb.NewLine();
            }
        }

        sb.NewLine();

        foreach (var prop in processedProperties)
        {
            var typeFullName = typeSymbol.GetFullName();
            sb.AppendSummary($"Creates a new <see cref=\"{typeFullName}\"/> instance and sets its value using the specified {prop.TypeSymbol.GetCrefForType()}.");

            // Check if any required property would conflict with default "value" parameter name
            var choiceParamName = "value";
            foreach (var reqProp in ordinaryRequiredProperties)
            {
                var paramName = reqProp.Name.ToCamelCase();
                if (paramName == "value")
                {
                    choiceParamName = "choiceValue";
                    break;
                }
            }

            // Add parameter descriptions for required ordinary properties
            foreach (var reqProp in ordinaryRequiredProperties)
            {
                var paramName = reqProp.Name.ToCamelCase();
                sb.AppendParamDescription(paramName, $"The value for the required property {reqProp.Name}.");
            }

            sb.AppendParamDescription(choiceParamName, "The value to assign to the created choice instance.");

            // Build method signature with required property parameters
            sb.Append($"public static {typeFullName} CreateAs").Append(prop.Name).Append("(");

            // Add required ordinary properties as parameters first
            for (var i = 0; i < ordinaryRequiredProperties.Count; i++)
            {
                var reqProp = ordinaryRequiredProperties[i];
                var paramName = reqProp.Name.ToCamelCase();
                var propType = reqProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.Append(propType).Append(" ").Append(paramName).Append(", ");
            }

            // Add the choice property parameter
            sb.Append(prop.TypeName).Append(" ").Append(choiceParamName).Append(") => new () { ");

            // Initialize required ordinary properties
            foreach (var reqProp in ordinaryRequiredProperties)
            {
                var paramName = reqProp.Name.ToCamelCase();
                sb.Append(reqProp.Name).Append(" = ").Append(paramName).Append(", ");
            }

            // Initialize the choice property
            sb.Append(prop.Name).Append(" = ").Append(choiceParamName).AppendLine(" };");

            sb.NewLine();
        }

        ProcessMatch(sb, processedProperties);

        sb.NewLine();
        ProcessSwitch(sb, processedProperties);

        sb.NewLine();

        // Skip implicit operators if there are required ordinary properties
        // because implicit operators cannot initialize required properties
        if (ordinaryRequiredProperties.Count == 0)
        {
            if (!ProcessImplicitOperators(sb, typeSymbol.Name, processedProperties))
                sb.NewLine();
        }
        else
        {
            sb.NewLine();
        }

        // Generate ShouldSerialize methods for all properties to prevent xsi:nil in XML
        // This ensures that only the active choice property is serialized
        foreach (var p in processedProperties)
        {
            sb.AppendSummary($"Determines whether the <see cref=\"{p.Name}\"/> property should be serialized.")
                .AppendBlock("returns", $"<c>true</c> if <see cref=\"{p.Name}\"/> is the active choice; otherwise, <c>false</c>.");

            sb.AppendLine("[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]");
            sb.Append("public bool ShouldSerialize").Append(p.Name).Append("() => ");

            if (isOnly1Property)
                sb.AppendLine("true;");
            else
                sb.Append("ChoiceType == ChoiceOf.").Append(p.Name).AppendLine(";");

            sb.NewLine();
        }

        sb.AppendSummary("<para>Choice enumeration</para>");

        sb.Append("[XmlType(\"ChoiceOf.").Append(typeSymbol.Name).AppendLine("\")]");
        sb.AppendLine("public enum ChoiceOf")
            .OpenBracket();

        foreach (var propertySymbol in processedProperties)
        {
            if (propertySymbol.Summary is not null)
                sb.AppendSummary(propertySymbol.Summary);

            sb.Append(propertySymbol.Name).AppendLine(", ");
        }

        sb.CloseBracket();
        sb.CloseBracket();

        return sb;

    }

    private static PropertyDetails ProcessProperty(IPropertySymbol propertySymbol)
    {
        var xmlTagAttribute = propertySymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == Constants.XmlTagAttributeFullName);
        var xmlElementName = (string?)xmlTagAttribute?.ConstructorArguments[0].Value ?? propertySymbol.Name;

        // Read the Namespace property from XmlTagAttribute if present
        string? xmlNamespace = null;
        var namespaceProperty = xmlTagAttribute?.NamedArguments.FirstOrDefault(x => x.Key == "Namespace");
        if (namespaceProperty?.Value.Value is string ns)
        {
            xmlNamespace = ns;
        }

        var typeFullName = propertySymbol.Type.GetFullName();
        var propertyName = propertySymbol.Name;
        var modifiers = propertySymbol.GetModifiers();

        return new PropertyDetails(
            name: propertyName,
            typeName: typeFullName.Replace("?", ""),
            @namespace: propertySymbol.ContainingNamespace.ToDisplayString(),
            xmlNameValue: xmlElementName,
            xmlNamespace: xmlNamespace,
            modifiers: modifiers,
            summary: propertySymbol.GetSummaryText(),
            getterAccessibility: propertySymbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
            setterAccessibility: propertySymbol.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
            typeSymbol: propertySymbol.Type);
    }

    private static bool ProcessImplicitOperators(SourceCodeBuilder sb, string typeName, List<PropertyDetails> processedProperties)
    {
        if (processedProperties.Count == 0 || processedProperties.Select(x => x.TypeName).Distinct().Count() != processedProperties.Count)
            return false;

        foreach (var property in processedProperties)
        {
            sb.AppendSummary($"Implicitly converts an {property.TypeSymbol.GetCrefForType()} to an <see cref=\"{typeName}\"/>.");
            sb.AppendParamDescription("value", $"The {property.TypeSymbol.GetCrefForType()} to convert.");
            sb.AppendBlock("returns", $"<see cref=\"{typeName}\"/> instance representing the code.").NewLine();
            sb.AppendLine("[return: NotNullIfNotNull(parameterName: nameof(value))]");

            sb.Append("public static implicit operator ").Append(typeName).Append("? (")
                .Append(property.TypeName).AppendLine("? value) ")
                .OpenBracket()
                .Append("return value is null ? null : CreateAs").Append(property.Name).Append("(value").AppendIf(property.TypeSymbol.IsValueType, ".Value").AppendLine(");")
                .CloseBracket();

            sb.NewLine();
        }

        return true;
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

        sb.AppendLine("return ChoiceType switch")
            .OpenBracket();

        var isOnlyProperty = processedProperties.Count == 1;

        foreach (var prop in processedProperties)
        {
            sb.Append($"ChoiceOf.{prop.Name} => match").Append($"{prop.Name}({prop.Name}!")
                .AppendIf(!isOnlyProperty && prop.TypeSymbol.IsValueType, ".Value")
                .AppendLine("),");
        }

        sb.AppendLine("_ => throw new InvalidOperationException($\"Invalid ChoiceType. '{ChoiceType}'\")");
        sb.CloseBracketWithSemiColon();
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

        sb.AppendLine("switch (ChoiceType)")
            .OpenBracket();

        var isOnlyProperty = processedProperties.Count == 1;

        foreach (var prop in processedProperties)
        {
            sb.AppendSwitchCase($"ChoiceOf.{prop.Name}")
                .Append($"match{prop.Name}({prop.Name}!")
                .AppendIf(!isOnlyProperty && prop.TypeSymbol.IsValueType, ".Value")
                .AppendLine(");")
                .AppendLine("return;")
                .CloseSwitchCase()
                .NewLine();
        }

        sb.AppendLine("default:");
        sb.AppendLine("throw new XmlException($\"Invalid ChoiceType. '{ChoiceType}'\");");
        sb.CloseBracket();
        sb.CloseBracket();
    }

    private static readonly List<string> s_baseNamespaces =
    [
        "AltaSoft.Choice",
        "System",
        "System.ComponentModel",
        "System.Diagnostics.CodeAnalysis",
        "System.Globalization",
        "System.Text.Json",
        "System.Text.Json.Serialization",
        "System.Xml",
        "System.Xml.Serialization",
        "System.Xml.Schema"
    ];
}

