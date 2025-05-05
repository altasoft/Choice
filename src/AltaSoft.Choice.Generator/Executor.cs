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

    private static PropertyDetails ProcessProperty(ITypeSymbol typeSymbol, IPropertySymbol propertySymbol)
    {
        var xmlTagAttribute = propertySymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == Constants.XmlTagAttributeFullName);
        var xmlElementName = (string?)xmlTagAttribute?.ConstructorArguments[0].Value ?? propertySymbol.Name;

        var typeFullName = propertySymbol.Type.GetFriendlyName();
        var propertyName = propertySymbol.Name;

        string? customSerializerMethod = null;
        string? customDeserializerMethod = null;

        var propType = propertySymbol.Type;
        if (propType is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            propType = namedType.TypeArguments[0];
        }

        if (propType.TypeKind is TypeKind.Enum)
        {
            var typeMethods = typeSymbol.GetMembersOfType<IMethodSymbol>().ToList();
            customSerializerMethod = typeMethods.FirstOrDefault(x => x.Name == propertyName + "ToString")?.Name ?? null;
            customDeserializerMethod = typeMethods.FirstOrDefault(x => x.Name == "StringTo" + propertyName)?.Name ?? null;
        }

        return new PropertyDetails(
            name: propertyName,
            typeName: typeFullName.Replace("?", ""),
            @namespace: propertySymbol.ContainingNamespace.ToDisplayString(),
            xmlNameValue: xmlElementName,
            summary: propertySymbol.GetSummaryText(),
            getterAccessibility: propertySymbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
            setterAccessibility: propertySymbol.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
            customSerializerMethod: customSerializerMethod,
            customDeserializerMethod: customDeserializerMethod,
            typeSymbol: propertySymbol.Type);
    }

    private static SourceCodeBuilder Process(INamedTypeSymbol typeSymbol, List<IPropertySymbol> properties)
    {
        var processedProperties = properties.ConvertAll(x => ProcessProperty(typeSymbol, x));
        var usingStatements = processedProperties.Select(x => x.Namespace).Concat(s_baseNamespaces);
        var sb = new SourceCodeBuilder();

        sb.AppendSourceHeader("AltaSoft Choice.Generator");
        sb.AppendUsings(usingStatements);
        sb.AppendNamespace(typeSymbol.ContainingNamespace.ToDisplayString());

        sb.AppendLine("#pragma warning disable CS8774 // Member must have a non-null value when exiting.")
            .AppendLine("#pragma warning disable CS0628 // New protected member declared in sealed type")
            .NewLine();

        var inheritance = typeSymbol.Interfaces.Any(x => x.GetFullName() == Constants.IXmlSerializableFullName) ? null : "IXmlSerializable";
        sb.AppendClass(typeSymbol.IsRecord, typeSymbol.GetModifiers() ?? "public partial", typeSymbol.Name, inheritance);

        var hasDefaultCtor = typeSymbol.Constructors.Any(x => x.Parameters.Length == 0);
        if (!hasDefaultCtor)
        {
            var isSealedType = typeSymbol.IsSealed;
            if (isSealedType)
            {
                sb.AppendLine("[Obsolete(\"Only for deserializer\", true)]");
            }
            sb.Append(isSealedType ? "public " : "protected ").Append(typeSymbol.Name).AppendLine("()")
                .OpenBracket()
                .CloseBracket()
                .NewLine();
        }

        sb.AppendSummary("<para>Choice enum </para>");
        sb.AppendLine("[JsonIgnore]");
        sb.AppendLine("public ChoiceOf ChoiceType { get; private set; }");
        sb.NewLine();

        foreach (var p in processedProperties)
        {

            var fieldName = p.Name.ToFieldName();

            sb.Append("private ").Append(p.TypeName).Append("? ").Append(fieldName).AppendLine(";").NewLine();

            if (p.Summary is not null)
                sb.AppendSummary(p.Summary);

            sb.AppendLine("[DisallowNull]");
            sb.Append("public partial ").Append(p.TypeName).Append("? ").Append(p.Name)
                .OpenBracket();

            sb.AppendIfNotEmpty(p.GetterAccessibility.GetPropertyAccessibilityString()).Append("get => ")
            .Append(fieldName).AppendLine(";");

            sb.AppendIfNotEmpty(p.SetterAccessibility.GetPropertyAccessibilityString()).AppendLine("set")
            .OpenBracket()
            .Append(fieldName).AppendLine(" = value ?? throw new InvalidOperationException(\"Choice value cannot be null\");")
            .AppendLines(processedProperties.Where(x => x.Name != p.Name).Select(v => $"{v.Name.ToFieldName()} = null;"))
            .Append("ChoiceType = ChoiceOf.").Append(p.Name).AppendLine(";")
            .CloseBracket()
            .CloseBracket();

            sb.NewLine();

            if (p is not { CustomDeserializerMethod: not null, CustomSerializerMethod: not null })
            {
                sb.Append("private static readonly XmlSerializer s").Append(fieldName)
                    .Append("Serializer = new (typeof(").Append(p.TypeName).Append("), ").Append($"new XmlRootAttribute(\"{p.XmlNameValue}\")").AppendLine(");");
            }

        }

        sb.NewLine();

        foreach (var prop in processedProperties)
        {

            var typeFullName = typeSymbol.GetFullName();
            sb.AppendSummary($"Creates a new <see cref=\"{typeFullName}\"/> instance " +
                             $"and sets its value using the specified <see cref=\"{prop.TypeName}\"/>.");
            sb.AppendParamDescription("value", "The value to assign to the created choice instance.");

            sb.Append($"public static {typeFullName} CreateAs").Append(prop.Name).Append("(")
                .Append(prop.TypeName).Append(" value) => new () { ").Append(prop.Name).AppendLine(" = value };");

            sb.NewLine();
        }

        ProcessMatch(sb, processedProperties);

        sb.NewLine();

        ProcessSwitch(sb, processedProperties);

        sb.NewLine();
        ProcessXmlSerialization(sb, typeSymbol, processedProperties);

        sb.NewLine();

        if (!ProcessImplicitOperators(sb, typeSymbol.Name, processedProperties))
            sb.NewLine();

        sb.AppendSummary("<para>Choice enumeration</para>");

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

    private static bool ProcessImplicitOperators(SourceCodeBuilder sb, string typeName, List<PropertyDetails> processedProperties)
    {
        if (processedProperties.Count == 0 || processedProperties.Select(x => x.TypeName).Distinct().Count() != processedProperties.Count)
            return false;

        foreach (var property in processedProperties)
        {
            sb.AppendSummary($"Implicitly converts an <see cref=\"{property.TypeName}\"/> to an <see cref=\"{typeName}\"/>.");
            sb.AppendParamDescription("value", $"The <see cref=\"{property.TypeName}\"/> to convert.");
            sb.AppendBlock("returns", $"<see cref=\"{typeName}\"/> instance representing the code.");

            sb.Append("public static implicit operator ").Append(typeName).Append("(")
                .Append(property.TypeName).Append(" value) => CreateAs").Append(property.Name).AppendLine("(value);");
            sb.NewLine();
        }

        return true;
    }

    private static void ProcessXmlSerialization(SourceCodeBuilder sb, ITypeSymbol type, List<PropertyDetails> processedProperties)
    {
        sb.AppendInheritDoc()
            .AppendLine("public XmlSchema? GetSchema() => null;")
            .NewLine();

        //read
        var xmlPropNames = string.Join(" or ", processedProperties.Select(x => $"<{x.XmlNameValue}>"));

        sb.AppendInheritDoc()
            .AppendLine("public void ReadXml(XmlReader reader)")
            .OpenBracket()
            .AppendLine("ArgumentNullException.ThrowIfNull(reader);")
            .NewLine()
            .AppendLine("reader.MoveToContent();")
            .NewLine()
            .AppendLine("if (reader.IsEmptyElement)")
            .AppendLine($"\tthrow new XmlException(\"{type.Name} element must contain exactly one of {xmlPropNames}.\");")
            .NewLine()
            .AppendLine("reader.ReadStartElement();")
            .NewLine()
            .AppendLine("var sawChoice = false;")
            .AppendLine("while (reader.MoveToContent() == XmlNodeType.Element)")
            .OpenBracket()
            .AppendLine("if (sawChoice)")
            .AppendLine($"\tthrow new XmlException(\"{type.Name} must contain at most one of {xmlPropNames}.\");")
            .NewLine()
            .AppendLine("switch (reader.LocalName)")
            .OpenBracket();

        foreach (var prop in processedProperties)
        {
            sb.AppendSwitchCase($"\"{prop.XmlNameValue}\"")
                .Append(prop.Name).Append(" = ");

            if (prop.CustomDeserializerMethod is not null)
            {
                sb.Append(prop.CustomDeserializerMethod).AppendLine("(reader.ReadElementContentAsString());");
            }
            else
            {
                sb.Append("(").Append(prop.TypeName).Append(")(s")
                    .Append(prop.Name.ToFieldName())
                    .Append("Serializer.Deserialize(reader) ?? throw new XmlException(\" The value of ").Append(prop.XmlNameValue).AppendLine(" cannot be null\"));");
            }

            sb.AppendLine("sawChoice = true;")
                .AppendLine("break;")
                .CloseSwitchCase()
                .NewLine();
        }

        sb.AppendDefaultSwitchCase()
            .AppendLine("reader.Skip();")
            .AppendLine("break;")
            .CloseSwitchCase();

        sb.CloseBracket();
        sb.CloseBracket().NewLine();

        sb.AppendLine("reader.ReadEndElement();")
            .NewLine()
            .AppendLine("if (!sawChoice)")
            .AppendLine($"\tthrow new XmlException(\"{type.Name} must contain exactly one of {xmlPropNames}\");");

        sb.CloseBracket();
        sb.NewLine();

        // write
        sb.AppendInheritDoc()
            .AppendLine("public void WriteXml(XmlWriter writer)")
            .OpenBracket()
            .AppendLine("ArgumentNullException.ThrowIfNull(writer);")
            .NewLine()
            .AppendLine("switch (ChoiceType)")
            .OpenBracket();

        foreach (var prop in processedProperties)
        {
            sb.AppendSwitchCase($"ChoiceOf.{prop.Name}");

            if (prop.CustomSerializerMethod is not null)
            {
                sb.Append("writer.WriteStartElement(\"").Append(prop.XmlNameValue).AppendLine("\");");
                sb.Append("writer.WriteString(").Append(prop.CustomSerializerMethod).Append($"({prop.Name}")
                    .AppendIf(prop.TypeSymbol.IsValueType, "!.Value").AppendLine("));");
                sb.AppendLine("writer.WriteEndElement();");
            }
            else
            {
                sb.Append("s").Append(prop.Name.ToFieldName()).Append("Serializer.Serialize(writer, ").Append(prop.Name)
                    .AppendLine("!, XmlNamespaceHelper.EmptyNamespace);");
            }

            sb.AppendLine("break;")
             .CloseSwitchCase()
             .NewLine();

        }

        sb.AppendDefaultSwitchCase()
            .AppendLine("throw new InvalidOperationException($\"Invalid ChoiceType. '{ChoiceType}'\");")
            .CloseSwitchCase();

        sb.CloseBracket();
        sb.CloseBracket();

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
        foreach (var prop in processedProperties)
        {
            sb.Append($"ChoiceOf.{prop.Name} => match").Append($"{prop.Name}({prop.Name}!")
                .AppendIf(prop.TypeSymbol.IsValueType, ".Value")
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
        foreach (var prop in processedProperties)
        {
            sb.AppendSwitchCase($"ChoiceOf.{prop.Name}")
                .Append($"match{prop.Name}({prop.Name}!")
                .AppendIf(prop.TypeSymbol.IsValueType, ".Value")
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
        "System.Text.Json",
        "System.Text.Json.Serialization",
        "System.Xml",
        "System.Xml.Serialization",
        "System.Xml.Schema"
    ];
}
