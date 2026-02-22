using System.ComponentModel.DataAnnotations;

namespace Privatekonomi.Core.Models;

/// <summary>
/// Tracks the recurring schedule for a bill, enabling automatic generation of future bill instances
/// </summary>
public class BillSchedule
{
    public int BillScheduleId { get; set; }

    /// <summary>
    /// The recurring bill this schedule belongs to
    /// </summary>
    [Required]
    public int BillId { get; set; }
    public Bill? Bill { get; set; }

    /// <summary>
    /// Recurrence frequency: Monthly, Quarterly, Yearly, Weekly, BiMonthly
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Frequency { get; set; } = "Monthly";

    /// <summary>
    /// Day of month the bill is typically due (1–31).
    /// Reserved for future use when exact due-day scheduling is supported.
    /// </summary>
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// Date when the schedule starts generating bill occurrences
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Optional date when the schedule ends (null = ongoing)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Next date a new bill occurrence should be created (the future due date)
    /// </summary>
    [Required]
    public DateTime NextOccurrenceDate { get; set; }

    /// <summary>
    /// Whether the schedule is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// How many days before the due date the next bill occurrence should be created.
    /// Allows reminders to be sent before the bill is due.
    /// </summary>
    public int DaysBeforeCreate { get; set; } = 7;

    /// <summary>
    /// How many days before the due date to send a reminder notification
    /// </summary>
    public int ReminderDaysBefore { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
