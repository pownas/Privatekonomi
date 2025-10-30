namespace Privatekonomi.Core.Models;

/// <summary>
/// Stores user feedback on ML predictions for continuous model improvement.
/// This feedback is used to retrain and improve the model over time.
/// </summary>
public class UserFeedback
{
    public int FeedbackId { get; set; }
    
    /// <summary>
    /// User ID who provided the feedback.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction ID that was categorized.
    /// </summary>
    public int TransactionId { get; set; }
    
    /// <summary>
    /// Category predicted by the ML model.
    /// </summary>
    public string PredictedCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence score (0-1) of the ML prediction.
    /// </summary>
    public float PredictedConfidence { get; set; }
    
    /// <summary>
    /// Actual category chosen by the user.
    /// </summary>
    public string ActualCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the user had to correct the ML prediction.
    /// </summary>
    public bool WasCorrectionNeeded { get; set; }
    
    /// <summary>
    /// When the feedback was provided.
    /// </summary>
    public DateTime FeedbackDate { get; set; }
    
    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public ApplicationUser? User { get; set; }
    
    /// <summary>
    /// Navigation property to the transaction.
    /// </summary>
    public Transaction? Transaction { get; set; }
}
