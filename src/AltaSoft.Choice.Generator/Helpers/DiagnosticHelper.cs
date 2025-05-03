using System;
using Microsoft.CodeAnalysis;

namespace AltaSoft.Choice.Generator.Helpers;

/// <summary>
/// A utility class for generating diagnostic messages related to domain primitives and code generation.
/// </summary>
internal static class DiagnosticHelper
{
    private const string Category = "AltaSoft.Choice.Generator";

    /// <summary>
    /// Creates a diagnostic for general error
    /// </summary>
    /// <param name="location">The location where the diagnostic occurs.</param>
    /// <param name="ex"></param>
    /// <returns>A diagnostic indicating that the general error happened.</returns>
    internal static Diagnostic GeneralError(Location? location, Exception ex)
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor(
                "AL1000",
                "An exception was thrown by the AltaSoft.Choice generator",
                "An exception was thrown by the AltaSoft.Choice generator: `{0}`{1}{2}",
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true),
            location, ex, SourceCodeBuilder.NewLineString, ex.StackTrace);
    }

    /// <summary>
    /// Creates a diagnostic for a class that must be partial to generate an empty constructor.
    /// </summary>
    /// <param name="location">The location where the diagnostic occurs.</param>
    /// <returns>A diagnostic indicating that a class must be partial to generate an empty constructor.</returns>
    internal static Diagnostic ClassMustBePartial(Location? location)
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor(
                "AL1002",
                "Class must be partial to generate Empty constructor",
                "Class must be partial to generate Empty constructor",
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true), location);
    }
}
