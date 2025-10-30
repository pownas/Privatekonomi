namespace Privatekonomi.Core.Models;

/// <summary>
/// Represents a like on a group comment
/// </summary>
public class CommentLike
{
    public int CommentLikeId { get; set; }
    
    public int GroupCommentId { get; set; }
    public GroupComment? GroupComment { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
