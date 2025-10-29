namespace Privatekonomi.Core.ML;

/// <summary>
/// Represents the features extracted from a transaction for ML training and prediction.
/// </summary>
public class TransactionFeatures
{
    /// <summary>
    /// Transaction description text.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Payee name (if available).
    /// </summary>
    public string Payee { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction amount (normalized).
    /// </summary>
    public float Amount { get; set; }
    
    /// <summary>
    /// Log-transformed amount for better distribution.
    /// </summary>
    public float AmountLog { get; set; }
    
    /// <summary>
    /// Day of week (1-7).
    /// </summary>
    public float DayOfWeek { get; set; }
    
    /// <summary>
    /// Day of month (1-31).
    /// </summary>
    public float DayOfMonth { get; set; }
    
    /// <summary>
    /// Whether the transaction occurred on a weekend.
    /// </summary>
    public bool IsWeekend { get; set; }
    
    /// <summary>
    /// Whether the transaction occurred at the start of the month (first 5 days).
    /// </summary>
    public bool IsMonthStart { get; set; }
    
    /// <summary>
    /// Whether the transaction occurred at the end of the month (last 5 days).
    /// </summary>
    public bool IsMonthEnd { get; set; }
    
    /// <summary>
    /// The category label (target for training).
    /// </summary>
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Represents the prediction result from the ML model.
/// </summary>
public class CategoryPrediction
{
    /// <summary>
    /// Predicted category name.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence score (0-1).
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Indicates if the prediction is uncertain (confidence < 0.7).
    /// </summary>
    public bool IsUncertain => Confidence < 0.7f;
    
    /// <summary>
    /// Alternative category predictions with their confidence scores.
    /// </summary>
    public Dictionary<string, float> AlternativeCategories { get; set; } = new();
}

/// <summary>
/// Model metrics from evaluation.
/// </summary>
public class ModelMetrics
{
    public double Accuracy { get; set; }
    public double MacroPrecision { get; set; }
    public double MacroRecall { get; set; }
    public double MicroAccuracy { get; set; }
    public double LogLoss { get; set; }
    public double LogLossReduction { get; set; }
}
