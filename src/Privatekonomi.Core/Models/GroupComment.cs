namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a comment in a savings group
/// </summary>
public class GroupComment
{
    public int GroupCommentId { get; set; }
    
    public int SavingsGroupId { get; set; }
    public SavingsGroup? SavingsGroup { get; set; }
    
    /// <summary>
    /// Optional reference to a specific goal in the group
    /// </summary>
    public int? GroupGoalId { get; set; }
    public GroupGoal? GroupGoal { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}
