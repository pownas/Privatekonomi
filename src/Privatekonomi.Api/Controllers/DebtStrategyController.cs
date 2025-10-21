using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Core.Models;
using Privatekonomi.Core.Services;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebtStrategyController : ControllerBase
{
    private readonly IDebtStrategyService _debtStrategyService;
    private readonly ILoanService _loanService;

    public DebtStrategyController(IDebtStrategyService debtStrategyService, ILoanService loanService)
    {
        _debtStrategyService = debtStrategyService;
        _loanService = loanService;
    }

    /// <summary>
    /// Get amortization schedule for a specific loan
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <param name="extraMonthlyPayment">Extra monthly payment amount</param>
    [HttpGet("amortization-schedule/{loanId}")]
    public async Task<ActionResult<List<AmortizationScheduleEntry>>> GetAmortizationSchedule(
        int loanId, 
        [FromQuery] decimal extraMonthlyPayment = 0)
    {
        var loan = await _loanService.GetLoanByIdAsync(loanId);
        if (loan == null)
        {
            return NotFound("Loan not found");
        }

        var schedule = _debtStrategyService.GenerateAmortizationSchedule(loan, extraMonthlyPayment);
        return Ok(schedule);
    }

    /// <summary>
    /// Calculate snowball debt payoff strategy
    /// </summary>
    /// <param name="availableMonthlyPayment">Total available monthly payment amount</param>
    [HttpGet("snowball")]
    public async Task<ActionResult<DebtPayoffStrategy>> GetSnowballStrategy(
        [FromQuery] decimal availableMonthlyPayment)
    {
        if (availableMonthlyPayment <= 0)
        {
            return BadRequest("Available monthly payment must be greater than 0");
        }

        var strategy = await _debtStrategyService.CalculateSnowballStrategy(availableMonthlyPayment);
        return Ok(strategy);
    }

    /// <summary>
    /// Calculate avalanche debt payoff strategy
    /// </summary>
    /// <param name="availableMonthlyPayment">Total available monthly payment amount</param>
    [HttpGet("avalanche")]
    public async Task<ActionResult<DebtPayoffStrategy>> GetAvalancheStrategy(
        [FromQuery] decimal availableMonthlyPayment)
    {
        if (availableMonthlyPayment <= 0)
        {
            return BadRequest("Available monthly payment must be greater than 0");
        }

        var strategy = await _debtStrategyService.CalculateAvalancheStrategy(availableMonthlyPayment);
        return Ok(strategy);
    }

    /// <summary>
    /// Compare snowball and avalanche strategies
    /// </summary>
    /// <param name="availableMonthlyPayment">Total available monthly payment amount</param>
    [HttpGet("compare")]
    public async Task<ActionResult<object>> CompareStrategies(
        [FromQuery] decimal availableMonthlyPayment)
    {
        if (availableMonthlyPayment <= 0)
        {
            return BadRequest("Available monthly payment must be greater than 0");
        }

        var (snowball, avalanche) = await _debtStrategyService.CompareStrategies(availableMonthlyPayment);
        return Ok(new { Snowball = snowball, Avalanche = avalanche });
    }

    /// <summary>
    /// Analyze the impact of extra payments on a loan
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <param name="extraMonthlyPayment">Extra monthly payment amount</param>
    [HttpGet("extra-payment-analysis/{loanId}")]
    public async Task<ActionResult<ExtraPaymentAnalysis>> AnalyzeExtraPayment(
        int loanId,
        [FromQuery] decimal extraMonthlyPayment)
    {
        if (extraMonthlyPayment <= 0)
        {
            return BadRequest("Extra monthly payment must be greater than 0");
        }

        var loan = await _loanService.GetLoanByIdAsync(loanId);
        if (loan == null)
        {
            return NotFound("Loan not found");
        }

        var analysis = _debtStrategyService.AnalyzeExtraPayment(loan, extraMonthlyPayment);
        return Ok(analysis);
    }

    /// <summary>
    /// Calculate debt-free date for all current loans
    /// </summary>
    [HttpGet("debt-free-date")]
    public async Task<ActionResult<DateTime?>> GetDebtFreeDate()
    {
        var debtFreeDate = await _debtStrategyService.CalculateDebtFreeDate();
        return Ok(new { DebtFreeDate = debtFreeDate });
    }
}
