using Fruitables.ViewModels;

namespace Fruitables.Services.Interfaces;

public interface IChatService
{
    Task<ChatConversationDto> StartOrGetConversationAsync(int? userId, string? guestSessionId, string name, string phone);
    Task<bool> CustomerCanAccessConversationAsync(int conversationId, int? userId, string? guestSessionId);
    Task<ChatMessageDto> AddCustomerMessageAsync(int conversationId, int? userId, string? guestSessionId, string message);
    Task<ChatMessageDto> AddAdminMessageAsync(int conversationId, int adminUserId, string message);
    Task<List<ChatConversationDto>> GetAdminConversationsAsync();
    Task<List<ChatMessageDto>> GetMessagesAsync(int conversationId);
    Task<ChatConversationDto?> GetConversationAsync(int conversationId);
    Task MarkConversationReadAsync(int conversationId, bool readByAdmin, int? userId, string? guestSessionId);
}
