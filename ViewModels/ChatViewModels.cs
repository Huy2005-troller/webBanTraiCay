using Fruitables.Models;

namespace Fruitables.ViewModels;

public class ChatConversationDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public ChatConversationStatus Status { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadByAdminCount { get; set; }
    public int UnreadByCustomerCount { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public ChatSenderType SenderType { get; set; }
    public int? SenderUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class AdminChatViewModel
{
    public List<ChatConversationDto> Conversations { get; set; } = new();
}
