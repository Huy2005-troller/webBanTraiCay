using System.ComponentModel.DataAnnotations;

namespace Fruitables.Models;

public enum ChatSenderType
{
    Customer,
    Admin
}

public class ChatMessage
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public ChatSenderType SenderType { get; set; }

    public int? SenderUserId { get; set; }

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    public virtual ChatConversation Conversation { get; set; } = null!;

    public virtual User? SenderUser { get; set; }
}
