namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing OAuth state tokens for CSRF protection
/// </summary>
public interface IOAuthStateService
{
    /// <summary>
    /// Generates and stores a new state token
    /// </summary>
    string GenerateState(string bankName);

    /// <summary>
    /// Validates a state token
    /// </summary>
    bool ValidateState(string state, string bankName);

    /// <summary>
    /// Removes a state token after use
    /// </summary>
    void RemoveState(string state);
}
