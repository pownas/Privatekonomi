namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a trained machine learning model for transaction categorization.
/// Each user has their own model trained on their specific transaction patterns.
/// </summary>
public class MLModel
{
    public int ModelId { get; set; }
    
    /// <summary>
    /// User ID who owns this model.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Path to the saved model file (relative or absolute).
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;
    
    /// <summary>
    /// When the model was trained.
    /// </summary>
    public DateTime TrainedAt { get; set; }
    
    /// <summary>
    /// Number of transactions used for training.
    /// </summary>
    public int TrainingRecordsCount { get; set; }
    
    /// <summary>
    /// Model accuracy (0-1) on test set.
    /// </summary>
    public float Accuracy { get; set; }
    
    /// <summary>
    /// Model precision (0-1) on test set.
    /// </summary>
    public float Precision { get; set; }
    
    /// <summary>
    /// Model recall (0-1) on test set.
    /// </summary>
    public float Recall { get; set; }
    
    /// <summary>
    /// JSON string containing detailed metrics from the model evaluation.
    /// </summary>
    public string? Metrics { get; set; }
    
    /// <summary>
    /// Navigation property to the user who owns this model.
    /// </summary>
    public ApplicationUser? User { get; set; }
}
