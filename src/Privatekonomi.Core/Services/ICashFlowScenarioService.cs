using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for cash flow scenario analysis and compound interest calculations
/// </summary>
public interface ICashFlowScenarioService
{
    /// <summary>
    /// Calculate a scenario projection based on the provided parameters
    /// </summary>
    Task<ScenarioProjection> CalculateScenarioAsync(CashFlowScenario scenario);
    
    /// <summary>
    /// Get suggested default values based on user's actual financial data
    /// </summary>
    Task<CashFlowScenario> GetUserBasedDefaultsAsync();
    
    /// <summary>
    /// Get default values for non-authenticated users
    /// </summary>
    CashFlowScenario GetGuestDefaults();
    
    /// <summary>
    /// Calculate and compare multiple scenarios
    /// </summary>
    Task<List<ScenarioProjection>> CompareMultipleScenariosAsync(List<CashFlowScenario> scenarios);
}
