# Test Helpers

Reusable test utilities for the AltaSoft Choice Generator test suite.

## XmlSerializationHelper

Provides consistent XML serialization and deserialization utilities for tests.

### Methods

- **`SerializeToXml<T>(T obj)`** - Serializes object to XML string
- **`DeserializeFromXml<T>(string xml)`** - Deserializes XML string to object
- **`RoundTrip<T>(T original)`** - Serializes then deserializes (tests round-trip fidelity)
- **`NormalizeXml(string xml)`** - Normalizes XML for comparison (removes formatting differences)

### Example Usage

```csharp
var payment = PaymentMethod.CreateAsCreditCard(new CreditCardPayment("4111...", "John", "12/25", "123"));

// Serialize
var xml = XmlSerializationHelper.SerializeToXml(payment);
Assert.Contains("<CreditCard>", xml);

// Deserialize
var deserialized = XmlSerializationHelper.DeserializeFromXml<PaymentMethod>(xml);
Assert.Equal(PaymentMethod.ChoiceOf.CreditCard, deserialized.ChoiceType);

// Round-trip
var roundTripped = XmlSerializationHelper.RoundTrip(payment);
Assert.Equal(payment.CreditCard.CardNumber, roundTripped.CreditCard.CardNumber);
```

### Configuration

- Uses `OmitXmlDeclaration = false`
- Indentation enabled for readable output
- UTF-8 encoding

## JsonSerializationHelper

Provides consistent JSON serialization and deserialization utilities for tests.

### Methods

- **`SerializeToJson<T>(T obj)`** - Serializes object to JSON string
- **`DeserializeFromJson<T>(string json)`** - Deserializes JSON string to object
- **`RoundTrip<T>(T original)`** - Serializes then deserializes (tests round-trip fidelity)
- **`NormalizeJson(string json)`** - Normalizes JSON for comparison (removes formatting differences)

### Example Usage

```csharp
var search = SearchCriteria.CreateAsKeyword("laptop");

// Serialize
var json = JsonSerializationHelper.SerializeToJson(search);
Assert.Contains("\"keyword\": \"laptop\"", json);

// Deserialize
var deserialized = JsonSerializationHelper.DeserializeFromJson<SearchCriteria>(json);
Assert.Equal("laptop", deserialized.Keyword);

// Round-trip
var roundTripped = JsonSerializationHelper.RoundTrip(search);
Assert.Equal(search.Keyword, roundTripped.Keyword);
```

### Configuration

- Uses `WriteIndented = true` for readable output
- Includes `JsonStringEnumConverter` for enum handling
- `PropertyNamingPolicy = null` (respects `JsonPropertyName` attributes)

## Benefits

✅ **Consistency** - All tests use the same serialization settings
✅ **Reduced Duplication** - No need to recreate serializer instances in each test
✅ **Maintainability** - Single place to update serialization configuration
✅ **Readability** - Helper methods express intent clearly (e.g., `RoundTrip`)
