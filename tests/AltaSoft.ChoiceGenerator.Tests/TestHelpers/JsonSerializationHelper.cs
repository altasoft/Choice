using System.Text.Json;
using System.Text.Json.Serialization;

namespace AltaSoft.ChoiceGenerator.Tests.TestHelpers;

/// <summary>
/// Helper methods for JSON serialization in tests
/// </summary>
public static class JsonSerializationHelper
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public static string SerializeToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, s_options);
    }

    /// <summary>
    /// Deserializes an object from JSON string
    /// </summary>
    public static T? DeserializeFromJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, s_options);
    }

    /// <summary>
    /// Performs a round-trip serialization test: object -> JSON -> object
    /// </summary>
    public static T? RoundTrip<T>(T original)
    {
        var json = SerializeToJson(original);
        return DeserializeFromJson<T>(json);
    }

    /// <summary>
    /// Normalizes JSON string for comparison (removes formatting differences)
    /// </summary>
    public static string NormalizeJson(string json)
    {
        // Parse and re-serialize to normalize formatting
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, s_options);
    }
}
