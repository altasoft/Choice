# Test Models

Realistic domain models for testing the AltaSoft Choice Generator.

## Overview

These models represent real-world scenarios to make tests more professional, maintainable, and understandable. They replace generic test types like `TwoValueTypeChoice` and `SinglePropertyChoice` with domain-specific models.

## Models

### PaymentMethod

E-commerce payment choice supporting multiple payment types.

**Choice Properties:**
- `CreditCard` (CreditCardPayment) - Credit/debit card payment
- `BankTransfer` (BankTransferPayment) - Direct bank transfer
- `PayPal` (PayPalPayment) - PayPal payment

**Helper Classes:**
- `CreditCardPayment` - CardNumber, CardHolderName, ExpiryDate, Cvv
- `BankTransferPayment` - AccountNumber, RoutingNumber, BankName
- `PayPalPayment` - Email, TransactionId

**Usage:**
```csharp
var card = new CreditCardPayment("4111111111111111", "John Doe", "12/25", "123");
var payment = PaymentMethod.CreateAsCreditCard(card);

payment.Match(
	creditCard => ProcessCard(creditCard),
	bankTransfer => ProcessBankTransfer(bankTransfer),
	payPal => ProcessPayPal(payPal)
);
```

**Tests:** Creation, Switch/Match, JSON serialization, XML serialization

---

### ShippingOption

Shipping method choice for e-commerce orders.

**Choice Properties:**
- `Standard` (ShippingDetails) - Standard shipping
- `Express` (ShippingDetails) - Express shipping  
- `Overnight` (ShippingDetails) - Overnight shipping

**Helper Classes:**
- `ShippingDetails` - Cost, EstimatedDays, Carrier

**Usage:**
```csharp
var express = new ShippingDetails(15.99m, 2, "FedEx");
var shipping = ShippingOption.CreateAsExpress(express);

var totalCost = shipping.Match(
	standard => standard.Cost,
	express => express.Cost,
	overnight => overnight.Cost
);
```

**Tests:** Creation, Switch/Match with cost calculation, JSON/XML round-trips

---

### SearchCriteria

Product catalog search choice supporting different search types.

**Choice Properties:**
- `Keyword` (string) - Text search
- `CategoryId` (int) - Category filter
- `DateRange` (DateRange class) - Date range filter
- `PriceRange` (PriceRange struct) - Price range filter

**Helper Classes:**
- `DateRange` - StartDate, EndDate
- `PriceRange` (struct) - MinPrice, MaxPrice

**Usage:**
```csharp
var search = SearchCriteria.CreateAsKeyword("laptop");
// OR
var search = SearchCriteria.CreateAsCategoryId(42);
// OR  
var search = SearchCriteria.CreateAsDateRange(new DateRange(start, end));

var query = search.Match(
	keyword => $"WHERE Name LIKE '%{keyword}%'",
	categoryId => $"WHERE CategoryId = {categoryId}",
	dateRange => $"WHERE Date BETWEEN '{dateRange.StartDate}' AND '{dateRange.EndDate}'",
	priceRange => $"WHERE Price BETWEEN {priceRange.MinPrice} AND {priceRange.MaxPrice}"
);
```

**Tests:** Mixed value types (string, int, class, struct), LINQ integration, JSON/XML serialization

---

### NotificationChannel

Simple single-property choice for notification delivery.

**Choice Properties:**
- `Channel` (NotificationChannelType enum, required) - Email, SMS, Push, InApp

**Usage:**
```csharp
var channel = NotificationChannel.CreateAsChannel(NotificationChannelType.Email);

// Implicit conversion
NotificationChannel smsChannel = NotificationChannelType.SMS;
```

**Tests:** Single-property choice, enum handling, implicit operators, required property

---

## Design Principles

✅ **Realistic** - Models represent actual domain concepts (payments, shipping, search)
✅ **Varied** - Different patterns (multi-choice, single-property, mixed types, structs)
✅ **Documented** - XML doc comments on all types and properties
✅ **Professional** - Follows C# naming conventions and patterns

## JSON/XML Attributes

Models use appropriate attributes for serialization control:
- `[JsonPropertyName("camelCase")]` for JSON
- `[XmlTag("PascalCase")]` for XML
- Both respect Choice Generator conventions

## Test Coverage

These models are used across multiple test classes:
- **ChoiceTypeCreationTests** - Factory methods and implicit operators
- **ChoiceTypeSwitchMatchTests** - Match/Switch behavior
- **ChoiceSerializationJsonTests** - JSON serialization round-trips
- **ChoiceSerializationXmlTests** - XML serialization round-trips
