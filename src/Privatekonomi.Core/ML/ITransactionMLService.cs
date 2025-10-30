using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.ML;

/// <summary>
/// Service interface for ML-based transaction categorization.
/// </summary>
public interface ITransactionMLService
{
    /// <summary>
    /// Train a new ML model for a specific user using their transaction history.
    /// </summary>
    /// <param name="userId">User ID to train the model for.</param>
    /// <returns>Model metrics after training.</returns>
    Task<ModelMetrics?> TrainModelAsync(string userId);
    
    /// <summary>
    /// Evaluate an existing ML model.
    /// </summary>
    /// <param name="userId">User ID whose model to evaluate.</param>
    /// <returns>Model metrics from evaluation.</returns>
    Task<ModelMetrics?> EvaluateModelAsync(string userId);
    
    /// <summary>
    /// Predict category for a single transaction.
    /// </summary>
    /// <param name="transaction">Transaction to categorize.</param>
    /// <param name="userId">User ID for model selection.</param>
    /// <returns>Category prediction with confidence score.</returns>
    Task<CategoryPrediction?> PredictCategoryAsync(Transaction transaction, string userId);
    
    /// <summary>
    /// Predict categories for multiple transactions in batch.
    /// </summary>
    /// <param name="transactions">List of transactions to categorize.</param>
    /// <param name="userId">User ID for model selection.</param>
    /// <returns>List of category predictions.</returns>
    Task<List<CategoryPrediction>> PredictBatchAsync(List<Transaction> transactions, string userId);
    
    /// <summary>
    /// Save a trained model to disk.
    /// </summary>
    /// <param name="userId">User ID whose model to save.</param>
    /// <param name="modelPath">Path where to save the model.</param>
    Task SaveModelAsync(string userId, string modelPath);
    
    /// <summary>
    /// Load a trained model from disk.
    /// </summary>
    /// <param name="userId">User ID whose model to load.</param>
    Task LoadModelAsync(string userId);
    
    /// <summary>
    /// Check if a user has a trained model available.
    /// </summary>
    /// <param name="userId">User ID to check.</param>
    /// <returns>True if the user has a trained model, false otherwise.</returns>
    Task<bool> IsModelTrainedAsync(string userId);
    
    /// <summary>
    /// Update the model with user feedback for continuous improvement.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="transaction">Transaction that was categorized.</param>
    /// <param name="correctCategory">Correct category chosen by the user.</param>
    /// <param name="predictedCategory">Category that was predicted by ML.</param>
    /// <param name="confidence">Confidence of the prediction.</param>
    Task UpdateModelWithFeedbackAsync(string userId, Transaction transaction, string correctCategory, string predictedCategory, float confidence);
}
