using Privatekonomi.Core.Data;
using Privatekonomi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Privatekonomi.Core.Services;

/// <summary>
/// Service for managing Swedish tax deductions (ROT/RUT)
/// </summary>
public interface ITaxDeductionService
{
    /// <summary>
    /// Add a ROT/RUT deduction
    /// </summary>
    Task<TaxDeduction> AddDeductionAsync(TaxDeduction deduction);
    
    /// <summary>
    /// Get all deductions for a tax year
    /// </summary>
    Task<List<TaxDeduction>> GetDeductionsByYearAsync(int taxYear);
    
    /// <summary>
    /// Calculate total deductible amount for a tax year
    /// </summary>
    Task<decimal> GetTotalDeductibleAmountAsync(int taxYear);
    
    /// <summary>
    /// Calculate ROT deduction (50% of labor cost, max 50,000 SEK per person per year)
    /// </summary>
    decimal CalculateRotDeduction(decimal laborCost);
    
    /// <summary>
    /// Calculate RUT deduction (50% of cost, max 75,000 SEK per person per year)
    /// </summary>
    decimal CalculateRutDeduction(decimal cost);
}

public class TaxDeductionService : ITaxDeductionService
{
    private readonly PrivatekonomyContext _context;
    
    // Swedish tax rules for 2024/2025
    private const decimal ROT_DEDUCTION_RATE = 0.5m; // 50%
    private const decimal RUT_DEDUCTION_RATE = 0.5m; // 50%
    private const decimal ROT_MAX_PER_PERSON = 50000m; // SEK
    private const decimal RUT_MAX_PER_PERSON = 75000m; // SEK
    
    public TaxDeductionService(PrivatekonomyContext context)
    {
        _context = context;
    }
    
    public async Task<TaxDeduction> AddDeductionAsync(TaxDeduction deduction)
    {
        // Calculate deductible amount based on type
        if (string.Equals(deduction.Type, "ROT", StringComparison.OrdinalIgnoreCase))
        {
            deduction.DeductibleAmount = CalculateRotDeduction(deduction.Amount);
        }
        else if (string.Equals(deduction.Type, "RUT", StringComparison.OrdinalIgnoreCase))
        {
            deduction.DeductibleAmount = CalculateRutDeduction(deduction.Amount);
        }
        
        deduction.CreatedAt = DateTime.UtcNow;
        
        _context.Set<TaxDeduction>().Add(deduction);
        await _context.SaveChangesAsync();
        
        return deduction;
    }
    
    public async Task<List<TaxDeduction>> GetDeductionsByYearAsync(int taxYear)
    {
        return await _context.Set<TaxDeduction>()
            .Include(d => d.Transaction)
            .Where(d => d.TaxYear == taxYear)
            .OrderBy(d => d.WorkDate)
            .ToListAsync();
    }
    
    public async Task<decimal> GetTotalDeductibleAmountAsync(int taxYear)
    {
        var deductions = await GetDeductionsByYearAsync(taxYear);
        return deductions.Sum(d => d.DeductibleAmount);
    }
    
    public decimal CalculateRotDeduction(decimal laborCost)
    {
        // ROT: 50% of labor cost, max 50,000 SEK per person per year
        var deduction = laborCost * ROT_DEDUCTION_RATE;
        return Math.Min(deduction, ROT_MAX_PER_PERSON);
    }
    
    public decimal CalculateRutDeduction(decimal cost)
    {
        // RUT: 50% of total cost, max 75,000 SEK per person per year
        var deduction = cost * RUT_DEDUCTION_RATE;
        return Math.Min(deduction, RUT_MAX_PER_PERSON);
    }
}
