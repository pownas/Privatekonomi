using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;
using Privatekonomi.Api.Exceptions;
using Privatekonomi.Api.Models;

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
        [FromQuery] int? household_id,
        [FromQuery] int page = 1,
        [FromQuery] int per_page = 50)
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
        
        if (household_id.HasValue)
        {
            transactions = transactions.Where(t => t.HouseholdId == household_id.Value);
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        if (transaction == null)
        {
            throw new NotFoundException("Transaction", id);
        }
        return Ok(transaction);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(
        [FromQuery] DateTime from, 
        [FromQuery] DateTime to)
    {
        var transactions = await _transactionService.GetTransactionsByDateRangeAsync(from, to);
        return Ok(transactions);
    }

    [HttpGet("unmapped")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetUnmappedTransactions()
    {
        var transactions = await _transactionService.GetUnmappedTransactionsAsync();
        return Ok(transactions);
    }

    [HttpPut("{id}/categories")]
    public async Task<IActionResult> UpdateTransactionCategories(int id, List<TransactionCategory> categories)
    {
        await _transactionService.UpdateTransactionCategoriesAsync(id, categories);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
        return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.TransactionId }, createdTransaction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            // Convert category DTOs to tuples
            var categories = request.Categories?
                .Select(c => (c.CategoryId, c.Amount))
                .ToList();

            // Get user ID and IP address for audit logging
            var userId = User?.Identity?.Name;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var updatedTransaction = await _transactionService.UpdateTransactionWithAuditAsync(
                id,
                request.Amount,
                request.Date,
                request.Description,
                request.Payee,
                request.Notes,
                request.Tags,
                categories,
                request.UpdatedAt,
                userId,
                ipAddress);

            return Ok(updatedTransaction);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            throw new NotFoundException("Transaction", id);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("locked"))
        {
            throw new ForbiddenException(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("modified by another user"))
        {
            throw new ConflictException(ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        await _transactionService.DeleteTransactionAsync(id);
        return NoContent();
    }

    [HttpGet("household/{householdId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByHousehold(int householdId)
    {
        var transactions = await _transactionService.GetTransactionsByHouseholdAsync(householdId);
        return Ok(transactions);
    }

    [HttpGet("household/{householdId}/date-range")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByHouseholdAndDateRange(
        int householdId,
        [FromQuery] DateTime from, 
        [FromQuery] DateTime to)
    {
        var transactions = await _transactionService.GetTransactionsByHouseholdAndDateRangeAsync(householdId, from, to);
        return Ok(transactions);
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
