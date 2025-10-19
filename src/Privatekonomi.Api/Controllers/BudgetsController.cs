using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ILogger<BudgetsController> _logger;

    public BudgetsController(IBudgetService budgetService, ILogger<BudgetsController> logger)
    {
        _budgetService = budgetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Budget>>> GetBudgets(
        [FromQuery] DateTime? period_start,
        [FromQuery] DateTime? period_end)
    {
        try
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            // Filter by period if provided
            if (period_start.HasValue)
            {
                budgets = budgets.Where(b => b.EndDate >= period_start.Value);
            }
            
            if (period_end.HasValue)
            {
                budgets = budgets.Where(b => b.StartDate <= period_end.Value);
            }
            
            return Ok(budgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budgets");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Budget>> GetBudget(int id)
    {
        try
        {
            var budget = await _budgetService.GetBudgetByIdAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            return Ok(budget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Budget>>> GetActiveBudgets()
    {
        try
        {
            var budgets = await _budgetService.GetActiveBudgetsAsync();
            return Ok(budgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active budgets");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/actual-amounts")]
    public async Task<ActionResult<Dictionary<int, decimal>>> GetActualAmounts(int id)
    {
        try
        {
            var actualAmounts = await _budgetService.GetActualAmountsByCategoryAsync(id);
            return Ok(actualAmounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving actual amounts for budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsForBudget(int id)
    {
        try
        {
            var transactions = await _budgetService.GetTransactionsForBudgetAsync(id);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/transactions-by-category")]
    public async Task<ActionResult<Dictionary<int, List<Transaction>>>> GetTransactionsByCategoryForBudget(int id)
    {
        try
        {
            var transactions = await _budgetService.GetTransactionsByCategoryForBudgetAsync(id);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions by category for budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Budget>> CreateBudget(Budget budget)
    {
        try
        {
            var createdBudget = await _budgetService.CreateBudgetAsync(budget);
            return CreatedAtAction(nameof(GetBudget), new { id = createdBudget.BudgetId }, createdBudget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(int id, Budget budget)
    {
        if (id != budget.BudgetId)
        {
            return BadRequest();
        }

        try
        {
            await _budgetService.UpdateBudgetAsync(budget);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        try
        {
            await _budgetService.DeleteBudgetAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget {BudgetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
