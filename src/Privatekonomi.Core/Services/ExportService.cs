using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;

namespace Privatekonomi.Core.Services;

public class ExportService : IExportService
{
    private readonly PrivatekonomyContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public ExportService(PrivatekonomyContext context, ICurrentUserService? currentUserService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<byte[]> ExportTransactionsToCsvAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Date <= toDate.Value);
        }

        var transactions = await query.OrderBy(t => t.Date).ToListAsync();

        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Datum,Beskrivning,Belopp,Typ,Bank,Kategorier,Taggar,Noteringar,Källa,Valuta");

        // Data rows
        foreach (var transaction in transactions)
        {
            var categories = string.Join("; ", transaction.TransactionCategories.Select(tc => tc.Category.Name));
            var bankName = transaction.BankSource?.Name ?? "";
            var type = transaction.IsIncome ? "Inkomst" : "Utgift";
            
            csv.AppendLine($"{transaction.Date:yyyy-MM-dd}," +
                          $"\"{EscapeCsv(transaction.Description)}\"," +
                          $"{transaction.Amount:F2}," +
                          $"{type}," +
                          $"\"{EscapeCsv(bankName)}\"," +
                          $"\"{EscapeCsv(categories)}\"," +
                          $"\"{EscapeCsv(transaction.Tags ?? "")}\"," +
                          $"\"{EscapeCsv(transaction.Notes ?? "")}\"," +
                          $"\"{EscapeCsv(transaction.ImportSource ?? "")}\"," +
                          $"{transaction.Currency}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportTransactionsToJsonAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Transactions
            .Include(t => t.BankSource)
            .Include(t => t.TransactionCategories)
            .ThenInclude(tc => tc.Category)
            .AsQueryable();

        // Filter by current user if authenticated
        if (_currentUserService?.IsAuthenticated == true && _currentUserService.UserId != null)
        {
            query = query.Where(t => t.UserId == _currentUserService.UserId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Date <= toDate.Value);
        }

        var transactions = await query.OrderBy(t => t.Date).ToListAsync();

        var exportData = transactions.Select(t => new
        {
            t.TransactionId,
            t.Date,
            t.Description,
            t.Amount,
            Type = t.IsIncome ? "Inkomst" : "Utgift",
            Bank = t.BankSource?.Name,
            Categories = t.TransactionCategories.Select(tc => new
            {
                tc.Category.Name,
                tc.Category.Color,
                tc.Amount,
                tc.Percentage
            }).ToList(),
            t.Tags,
            t.Notes,
            t.Currency,
            t.Payee,
            t.ImportSource,
            t.Cleared,
            t.CreatedAt,
            t.UpdatedAt
        });

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(exportData, options);
        return Encoding.UTF8.GetBytes(json);
    }

    public async Task<byte[]> ExportBudgetToCsvAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .Include(b => b.BudgetCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId);

        if (budget == null)
        {
            throw new ArgumentException($"Budget with ID {budgetId} not found");
        }

        var csv = new StringBuilder();
        
        // Budget header info
        csv.AppendLine($"Budget: {budget.Name}");
        csv.AppendLine($"Period: {budget.StartDate:yyyy-MM-dd} - {budget.EndDate:yyyy-MM-dd}");
        csv.AppendLine($"Typ: {budget.Period}");
        csv.AppendLine();
        
        // Category data header
        csv.AppendLine("Kategori,Planerat Belopp");

        // Budget categories
        foreach (var bc in budget.BudgetCategories)
        {
            csv.AppendLine($"\"{EscapeCsv(bc.Category.Name)}\",{bc.PlannedAmount:F2}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportFullBackupAsync()
    {
        var backup = new
        {
            ExportDate = DateTime.UtcNow,
            Version = "1.0",
            Data = new
            {
                Transactions = await _context.Transactions
                    .Include(t => t.BankSource)
                    .Include(t => t.TransactionCategories)
                    .ThenInclude(tc => tc.Category)
                    .ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                Budgets = await _context.Budgets
                    .Include(b => b.BudgetCategories)
                    .ThenInclude(bc => bc.Category)
                    .ToListAsync(),
                Goals = await _context.Goals.ToListAsync(),
                Investments = await _context.Investments.ToListAsync(),
                Loans = await _context.Loans.ToListAsync(),
                BankSources = await _context.BankSources.ToListAsync(),
                BankConnections = await _context.BankConnections.ToListAsync(),
                Assets = await _context.Assets.ToListAsync(),
                Households = await _context.Households
                    .Include(h => h.Members)
                    .ToListAsync()
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        var json = JsonSerializer.Serialize(backup, options);
        return Encoding.UTF8.GetBytes(json);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Escape quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}
