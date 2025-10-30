# ML Kategorisering - Snabbreferens

> Snabb guide för utvecklare som arbetar med ML-baserad kategorisering

## Snabbstart

### 1. Träna en modell

```csharp
var mlService = serviceProvider.GetRequiredService<ITransactionMLService>();
var metrics = await mlService.TrainModelAsync(userId);

if (metrics != null)
{
    Console.WriteLine($"✓ Accuracy: {metrics.Accuracy:P2}");
}
```

**Krav:** Minst 50 kategoriserade transaktioner, 5+ per kategori

### 2. Förutsäg kategori

```csharp
var prediction = await mlService.PredictCategoryAsync(transaction, userId);

if (prediction != null && !prediction.IsUncertain)
{
    // Använd: prediction.Category
}
```

**Konfidenströskel:** 70% (0.7)

### 3. Spara feedback

```csharp
await mlService.UpdateModelWithFeedbackAsync(
    userId, 
    transaction, 
    actualCategory,
    predictedCategory, 
    confidence
);
```

---

## API-översikt

### ITransactionMLService

| Metod | Beskrivning | Returnerar |
|-------|-------------|------------|
| `TrainModelAsync(userId)` | Träna ny modell | `ModelMetrics?` |
| `PredictCategoryAsync(transaction, userId)` | Förutsäg kategori | `CategoryPrediction?` |
| `PredictBatchAsync(transactions, userId)` | Batch-förutsägelse | `List<CategoryPrediction>` |
| `IsModelTrainedAsync(userId)` | Kolla om modell finns | `bool` |
| `UpdateModelWithFeedbackAsync(...)` | Spara feedback | `Task` |

---

## Viktiga konstanter

```csharp
// I TransactionMLService
private const int MinimumTransactionsRequired = 50;
private const int MinimumExamplesPerCategory = 5;
private const float ConfidenceThreshold = 0.7f; // 70%
```

---

## Arbetsflöde i TransactionService

```csharp
// I CreateTransactionAsync()
if (!transaction.TransactionCategories.Any())
{
    Category? suggestedCategory = null;
    
    // 1. Försök ML först
    if (_mlService != null && !string.IsNullOrEmpty(transaction.UserId))
    {
        var mlPrediction = await _mlService.PredictCategoryAsync(
            transaction, transaction.UserId);
        
        if (mlPrediction != null && !mlPrediction.IsUncertain)
        {
            suggestedCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == mlPrediction.Category);
        }
    }
    
    // 2. Fallback till regelbaserad
    if (suggestedCategory == null)
    {
        var ruleCategoryId = await _categoryRuleService
            .ApplyCategoryRulesAsync(transaction.Description, transaction.Payee);
        
        if (ruleCategoryId.HasValue)
        {
            suggestedCategory = await _context.Categories
                .FindAsync(ruleCategoryId.Value);
        }
    }
}
```

---

## Databasschema

### MLModel

```sql
CREATE TABLE MLModels (
    ModelId INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450) NOT NULL,
    ModelPath NVARCHAR(500) NOT NULL,
    TrainedAt DATETIME2 NOT NULL,
    TrainingRecordsCount INT NOT NULL,
    Accuracy REAL NOT NULL,
    Precision REAL NOT NULL,
    Recall REAL NOT NULL,
    Metrics NVARCHAR(2000),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
CREATE INDEX IX_MLModels_UserId ON MLModels(UserId);
```

### UserFeedback

```sql
CREATE TABLE UserFeedbacks (
    FeedbackId INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450) NOT NULL,
    TransactionId INT NOT NULL,
    PredictedCategory NVARCHAR(100) NOT NULL,
    PredictedConfidence REAL NOT NULL,
    ActualCategory NVARCHAR(100) NOT NULL,
    WasCorrectionNeeded BIT NOT NULL,
    FeedbackDate DATETIME2 NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
);
CREATE INDEX IX_UserFeedbacks_UserId ON UserFeedbacks(UserId);
CREATE INDEX IX_UserFeedbacks_TransactionId ON UserFeedbacks(TransactionId);
CREATE INDEX IX_UserFeedbacks_FeedbackDate ON UserFeedbacks(FeedbackDate);
```

---

## Feature Engineering

### Extraherade features

```csharp
private TransactionFeatures ExtractFeatures(Transaction transaction)
{
    var amount = (float)transaction.Amount;
    
    return new TransactionFeatures
    {
        Description = transaction.Description ?? string.Empty,
        Payee = transaction.Payee ?? string.Empty,
        Amount = amount,
        AmountLog = amount > 0 ? (float)Math.Log10((double)amount) : 0f,
        DayOfWeek = (float)transaction.Date.DayOfWeek,
        DayOfMonth = (float)transaction.Date.Day,
        IsWeekend = transaction.Date.DayOfWeek == DayOfWeek.Saturday || 
                    transaction.Date.DayOfWeek == DayOfWeek.Sunday,
        IsMonthStart = transaction.Date.Day <= 5,
        IsMonthEnd = transaction.Date.Day >= 
            DateTime.DaysInMonth(transaction.Date.Year, transaction.Date.Month) - 5
    };
}
```

---

## ML.NET Pipeline

```csharp
private IEstimator<ITransformer> BuildTrainingPipeline()
{
    return _mlContext.Transforms.Conversion.ConvertType(
            // Boolean -> Float conversion
            new[] {
                new InputOutputColumnPair("IsWeekendFloat", "IsWeekend"),
                new InputOutputColumnPair("IsMonthStartFloat", "IsMonthStart"),
                new InputOutputColumnPair("IsMonthEndFloat", "IsMonthEnd")
            },
            DataKind.Single)
        // Text processing
        .Append(_mlContext.Transforms.Text.NormalizeText(
            "NormalizedDescription", "Description"))
        .Append(_mlContext.Transforms.Text.TokenizeIntoWords(
            "TokenizedDescription", "NormalizedDescription"))
        .Append(_mlContext.Transforms.Conversion.MapValueToKey(
            "TokenizedDescriptionKeys", "TokenizedDescription"))
        .Append(_mlContext.Transforms.Text.ProduceNgrams(
            "DescriptionFeaturized", "TokenizedDescriptionKeys"))
        // Feature concatenation
        .Append(_mlContext.Transforms.Concatenate(
            "Features",
            "DescriptionFeaturized",
            "Amount", "AmountLog", "DayOfWeek", "DayOfMonth",
            "IsWeekendFloat", "IsMonthStartFloat", "IsMonthEndFloat"))
        // Convert category to key
        .Append(_mlContext.Transforms.Conversion.MapValueToKey(
            "Label", "Category"))
        // Train SDCA
        .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
            labelColumnName: "Label",
            featureColumnName: "Features"))
        // Convert back to category name
        .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
            "PredictedCategory", "PredictedLabel"));
}
```

---

## Vanliga fel och lösningar

### Fel 1: Null returneras från TrainModelAsync

**Orsak:** Otillräcklig träningsdata

**Lösning:**
```csharp
var count = await _context.Transactions
    .Where(t => t.UserId == userId && t.TransactionCategories.Any())
    .CountAsync();

if (count < 50)
{
    return "Behöver minst 50 kategoriserade transaktioner";
}
```

### Fel 2: Null returneras från PredictCategoryAsync

**Orsak:** Ingen modell tränad

**Lösning:**
```csharp
if (!await _mlService.IsModelTrainedAsync(userId))
{
    await _mlService.TrainModelAsync(userId);
}
```

### Fel 3: Låg accuracy

**Lösning:**
- Vänta på mer data
- Kontrollera datakvalitet
- Verifiera konsekvent kategorisering

---

## Testning

### Unit test exempel

```csharp
[Fact]
public async Task TrainModelAsync_SufficientData_ReturnsMetrics()
{
    // Arrange
    await SeedTransactionsAsync(userId, 100);

    // Act
    var result = await _mlService.TrainModelAsync(userId);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Accuracy >= 0 && result.Accuracy <= 1);
}
```

### Kör alla ML-tester

```bash
dotnet test --filter "FullyQualifiedName~TransactionMLServiceTests"
```

---

## Performance Tips

1. **Cache modeller** - Modeller cachas automatiskt i minnet
2. **Batch processing** - Använd `PredictBatchAsync` för många transaktioner
3. **Lazy loading** - Modeller laddas endast vid behov
4. **Cleanup** - Rensa gamla modeller regelbundet

---

## GDPR-hantering

### Radera användardata

```csharp
public async Task DeleteUserMLDataAsync(string userId)
{
    // Ta bort modeller
    var models = await _context.MLModels
        .Where(m => m.UserId == userId)
        .ToListAsync();
    
    foreach (var model in models)
    {
        if (File.Exists(model.ModelPath))
            File.Delete(model.ModelPath);
    }
    
    _context.MLModels.RemoveRange(models);
    
    // Ta bort feedback
    var feedback = await _context.UserFeedbacks
        .Where(f => f.UserId == userId)
        .ToListAsync();
    
    _context.UserFeedbacks.RemoveRange(feedback);
    
    await _context.SaveChangesAsync();
}
```

---

## Debugging

### Aktivera debug-loggning

```json
{
  "Logging": {
    "LogLevel": {
      "Privatekonomi.Core.ML": "Debug"
    }
  }
}
```

### Viktiga loggmeddelanden

- `Starting model training for user {UserId}`
- `Insufficient training data for user {UserId}`
- `Model training completed. Accuracy: {Accuracy:P2}`
- `No trained model found for user {UserId}`

---

## Filstruktur

```
src/Privatekonomi.Core/
├── ML/
│   ├── ITransactionMLService.cs          # Interface
│   ├── TransactionMLService.cs           # Implementation
│   └── TransactionMLModels.cs            # Models
├── Models/
│   ├── MLModel.cs                        # DB entity
│   └── UserFeedback.cs                   # DB entity
└── Services/
    └── TransactionService.cs             # Integration

tests/Privatekonomi.Core.Tests/
└── TransactionMLServiceTests.cs          # 9 unit tests

docs/
├── ML_CATEGORIZATION.md                  # Full docs
└── ML_CATEGORIZATION_QUICK_REFERENCE.md  # This file
```

---

## Resurser

- [Full dokumentation](ML_CATEGORIZATION.md)
- [ML.NET Docs](https://docs.microsoft.com/en-us/dotnet/machine-learning/)
- [SDCA Algorithm](https://www.microsoft.com/en-us/research/publication/stochastic-dual-coordinate-ascent-methods-for-regularized-loss-minimization/)

---

**Senast uppdaterad:** 2025-10-30
