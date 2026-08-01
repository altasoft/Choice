using System;
using System.Text.Json.Serialization;
using AltaSoft.Choice;

namespace AltaSoft.ChoiceGenerator.Tests.TestModels;

/// <summary>
/// Represents search criteria for a product catalog
/// Tests choices with different value types
/// </summary>
[Choice]
public sealed partial class SearchCriteria
{
    /// <summary>
    /// Search by keyword (string)
    /// </summary>
    [XmlTag("Keyword")]
    [JsonPropertyName("keyword")]
    public partial string? Keyword { get; set; }

    /// <summary>
    /// Search by category ID (int)
    /// </summary>
    [XmlTag("CategoryId")]
    [JsonPropertyName("categoryId")]
    public partial int? CategoryId { get; set; }

    /// <summary>
    /// Search by date range
    /// </summary>
    [XmlTag("DateRange")]
    [JsonPropertyName("dateRange")]
    public partial DateRange? DateRange { get; set; }

    /// <summary>
    /// Search by price range
    /// </summary>
    [XmlTag("PriceRange")]
    [JsonPropertyName("priceRange")]
    public partial PriceRange? PriceRange { get; set; }
}

/// <summary>
/// Date range for filtering
/// </summary>
public sealed class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public DateRange() { }

    public DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Price range for filtering (struct to test value type behavior)
/// </summary>
public struct PriceRange
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }

    public PriceRange(decimal minPrice, decimal maxPrice)
    {
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }
}
