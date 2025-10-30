using System.ComponentModel.DataAnnotations;

namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a reminder for a bill
/// </summary>
public class BillReminder
{
    public int BillReminderId { get; set; }
    
    public int BillId { get; set; }
    public Bill? Bill { get; set; }
    
    /// <summary>
    /// Date to send reminder
    /// </summary>
    [Required]
    public DateTime ReminderDate { get; set; }
    
    /// <summary>
    /// Whether reminder was sent
    /// </summary>
    public bool IsSent { get; set; } = false;
    
    /// <summary>
    /// Date reminder was sent
    /// </summary>
    public DateTime? SentDate { get; set; }
    
    /// <summary>
    /// Reminder method: Email, Notification, SMS
    /// </summary>
    [MaxLength(50)]
    public string ReminderMethod { get; set; } = "Notification";
    
    [MaxLength(500)]
    public string? Message { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
