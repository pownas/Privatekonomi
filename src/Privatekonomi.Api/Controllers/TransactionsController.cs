using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<TransactionListResponse>> GetTransactions(
        [FromQuery] int? account_id,
        [FromQuery] DateTime? start_date,
        [FromQuery] DateTime? end_date,
        [FromQuery] int? category_id,
        [FromQuery] int page = 1,
        [FromQuery] int per_page = 50)
    {
        try
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            
            // Apply filters
            if (account_id.HasValue)
            {
                transactions = transactions.Where(t => t.BankSourceId == account_id.Value);
            }
            
            if (start_date.HasValue)
            {
                transactions = transactions.Where(t => t.Date >= start_date.Value);
            }
            
            if (end_date.HasValue)
            {
                transactions = transactions.Where(t => t.Date <= end_date.Value);
            }
            
            if (category_id.HasValue)
            {
                transactions = transactions.Where(t => 
                    t.TransactionCategories.Any(tc => tc.CategoryId == category_id.Value));
            }
            
            // Apply pagination
            var totalCount = transactions.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)per_page);
            
            var paginatedTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * per_page)
                .Take(per_page)
                .ToList();
            
            return Ok(new TransactionListResponse
            {
                Transactions = paginatedTransactions,
                Page = page,
                PerPage = per_page,
                TotalCount = totalCount,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(
        [FromQuery] DateTime from, 
        [FromQuery] DateTime to)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(from, to);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions by date range");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("unmapped")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetUnmappedTransactions()
    {
        try
        {
            var transactions = await _transactionService.GetUnmappedTransactionsAsync();
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unmapped transactions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/categories")]
    public async Task<IActionResult> UpdateTransactionCategories(int id, List<TransactionCategory> categories)
    {
        try
        {
            await _transactionService.UpdateTransactionCategoriesAsync(id, categories);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Transaction not found {TransactionId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction categories for {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        try
        {
            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.TransactionId }, createdTransaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, Transaction transaction)
    {
        if (id != transaction.TransactionId)
        {
            return BadRequest();
        }

        try
        {
            await _transactionService.UpdateTransactionAsync(transaction);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            await _transactionService.DeleteTransactionAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class TransactionListResponse
{
    public IEnumerable<Transaction> Transactions { get; set; } = new List<Transaction>();
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
