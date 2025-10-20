using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IBankSourceService _bankSourceService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        ITransactionService transactionService,
        IBankSourceService bankSourceService,
        ILogger<ReportsController> logger)
    {
        _transactionService = transactionService;
        _bankSourceService = bankSourceService;
        _logger = logger;
    }

    /// <summary>
    /// Hämta nettoprövning över tid
    /// </summary>
    [HttpGet("networth")]
    public async Task<ActionResult<NetWorthReport>> GetNetWorth(
        [FromQuery] DateTime? start_date,
        [FromQuery] DateTime? end_date)
    {
        try
        {
            var startDate = start_date ?? DateTime.Now.AddYears(-1);
            var endDate = end_date ?? DateTime.Now;

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            
            // Calculate net worth over time
            var groupedByMonth = transactions
                .OrderBy(t => t.Date)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new NetWorthDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Income = g.Where(t => t.IsIncome).Sum(t => t.Amount),
                    Expense = g.Where(t => !t.IsIncome).Sum(t => t.Amount),
                    NetWorth = g.Sum(t => t.IsIncome ? t.Amount : -t.Amount)
                })
                .ToList();

            // Calculate cumulative net worth
            decimal cumulative = 0;
            foreach (var point in groupedByMonth)
            {
                cumulative += point.NetWorth;
                point.CumulativeNetWorth = cumulative;
            }

            return Ok(new NetWorthReport
            {
                StartDate = startDate,
                EndDate = endDate,
                DataPoints = groupedByMonth,
                CurrentNetWorth = cumulative
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating net worth");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Månatlig sammanfattning (inkomst, utgift, top-kategorier)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<MonthlySummary>> GetSummary(
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        try
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;

            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);

            var income = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            var expense = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

            // Get top categories by expense
            var topCategories = transactions
                .Where(t => !t.IsIncome)
                .SelectMany(t => t.TransactionCategories.Select(tc => new
                {
                    CategoryId = tc.CategoryId,
                    CategoryName = tc.Category?.Name ?? "Unknown",
                    Amount = t.Amount * tc.Percentage / 100
                }))
                .GroupBy(x => new { x.CategoryId, x.CategoryName })
                .Select(g => new CategorySummary
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalAmount = g.Sum(x => x.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(10)
                .ToList();

            return Ok(new MonthlySummary
            {
                Year = targetYear,
                Month = targetMonth,
                Income = income,
                Expense = expense,
                Net = income - expense,
                TransactionCount = transactions.Count(),
                TopCategories = topCategories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly summary");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class NetWorthReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<NetWorthDataPoint> DataPoints { get; set; } = new();
    public decimal CurrentNetWorth { get; set; }
}

public class NetWorthDataPoint
{
    public DateTime Date { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetWorth { get; set; }
    public decimal CumulativeNetWorth { get; set; }
}

public class MonthlySummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
    public int TransactionCount { get; set; }
    public List<CategorySummary> TopCategories { get; set; } = new();
}

public class CategorySummary
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
}
