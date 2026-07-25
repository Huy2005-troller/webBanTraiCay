using System.ComponentModel.DataAnnotations;

namespace Fruitables.Models;

public enum ChatConversationStatus
{
    Open,
    Closed
}

public class ChatConversation
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [MaxLength(100)]
    public string? GuestSessionId { get; set; }

    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    public ChatConversationStatus Status { get; set; } = ChatConversationStatus.Open;

    public DateTime? LastMessageAt { get; set; }

    [MaxLength(500)]
    public string? LastMessagePreview { get; set; }

    public int UnreadByAdminCount { get; set; }

    public int UnreadByCustomerCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }

    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
