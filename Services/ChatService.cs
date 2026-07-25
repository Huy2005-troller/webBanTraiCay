using Fruitables.Data;
using Fruitables.Models;
using Fruitables.Services.Interfaces;
using Fruitables.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Fruitables.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;

    public ChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatConversationDto> StartOrGetConversationAsync(int? userId, string? guestSessionId, string name, string phone)
    {
        name = CleanRequired(name, 200, "Tên khách hàng");
        phone = CleanRequired(phone, 20, "Số điện thoại");

        ChatConversation? conversation = null;

        if (userId.HasValue)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user != null)
            {
                name = string.IsNullOrWhiteSpace(user.Name) ? name : user.Name.Trim();
                phone = string.IsNullOrWhiteSpace(user.Phone) ? phone : user.Phone.Trim();
            }

            conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.Status == ChatConversationStatus.Open);
        }
        else if (!string.IsNullOrWhiteSpace(guestSessionId))
        {
            conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.GuestSessionId == guestSessionId && c.Status == ChatConversationStatus.Open);
        }

        if (conversation == null)
        {
            conversation = new ChatConversation
            {
                UserId = userId,
                GuestSessionId = userId.HasValue ? null : guestSessionId,
                CustomerName = name,
                CustomerPhone = phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ChatConversations.Add(conversation);
        }
        else
        {
            conversation.CustomerName = name;
            conversation.CustomerPhone = phone;
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ToConversationDto(conversation);
    }

    public async Task<bool> CustomerCanAccessConversationAsync(int conversationId, int? userId, string? guestSessionId)
    {
        var conversation = await _context.ChatConversations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return false;
        if (userId.HasValue) return conversation.UserId == userId.Value;
        return !string.IsNullOrWhiteSpace(guestSessionId) && conversation.GuestSessionId == guestSessionId;
    }

    public async Task<ChatMessageDto> AddCustomerMessageAsync(int conversationId, int? userId, string? guestSessionId, string message)
    {
        if (!await CustomerCanAccessConversationAsync(conversationId, userId, guestSessionId))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền gửi tin nhắn trong cuộc chat này.");
        }

        return await AddMessageAsync(conversationId, ChatSenderType.Customer, userId, message);
    }

    public async Task<ChatMessageDto> AddAdminMessageAsync(int conversationId, int adminUserId, string message)
    {
        return await AddMessageAsync(conversationId, ChatSenderType.Admin, adminUserId, message);
    }

    public async Task<List<ChatConversationDto>> GetAdminConversationsAsync()
    {
        return await _context.ChatConversations.AsNoTracking()
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => new ChatConversationDto
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                Status = c.Status,
                LastMessagePreview = c.LastMessagePreview,
                LastMessageAt = c.LastMessageAt,
                UnreadByAdminCount = c.UnreadByAdminCount,
                UnreadByCustomerCount = c.UnreadByCustomerCount
            })
            .ToListAsync();
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(int conversationId)
    {
        return await _context.ChatMessages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderType = m.SenderType,
                SenderUserId = m.SenderUserId,
                Message = m.Message,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead
            })
            .ToListAsync();
    }

    public async Task<ChatConversationDto?> GetConversationAsync(int conversationId)
    {
        var conversation = await _context.ChatConversations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        return conversation == null ? null : ToConversationDto(conversation);
    }

    public async Task MarkConversationReadAsync(int conversationId, bool readByAdmin, int? userId, string? guestSessionId)
    {
        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null) return;

        if (!readByAdmin && !await CustomerCanAccessConversationAsync(conversationId, userId, guestSessionId))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền đọc cuộc chat này.");
        }

        if (readByAdmin)
        {
            conversation.UnreadByAdminCount = 0;
        }
        else
        {
            conversation.UnreadByCustomerCount = 0;
        }

        conversation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<ChatMessageDto> AddMessageAsync(int conversationId, ChatSenderType senderType, int? senderUserId, string message)
    {
        message = CleanRequired(message, 1000, "Tin nhắn");

        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
        {
            throw new InvalidOperationException("Không tìm thấy cuộc chat.");
        }

        var now = DateTime.UtcNow;
        var chatMessage = new ChatMessage
        {
            ConversationId = conversationId,
            SenderType = senderType,
            SenderUserId = senderUserId,
            Message = message,
            CreatedAt = now,
            IsRead = false
        };

        conversation.LastMessageAt = now;
        conversation.LastMessagePreview = message.Length > 500 ? message[..500] : message;
        conversation.UpdatedAt = now;

        if (senderType == ChatSenderType.Customer)
        {
            conversation.UnreadByAdminCount++;
        }
        else
        {
            conversation.UnreadByCustomerCount++;
        }

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return ToMessageDto(chatMessage);
    }

    private static string CleanRequired(string value, int maxLength, string fieldName)
    {
        value = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} không được để trống.");
        }

        return value.Length > maxLength ? value[..maxLength] : value;
    }

    private static ChatConversationDto ToConversationDto(ChatConversation conversation)
    {
        return new ChatConversationDto
        {
            Id = conversation.Id,
            CustomerName = conversation.CustomerName,
            CustomerPhone = conversation.CustomerPhone,
            Status = conversation.Status,
            LastMessagePreview = conversation.LastMessagePreview,
            LastMessageAt = conversation.LastMessageAt,
            UnreadByAdminCount = conversation.UnreadByAdminCount,
            UnreadByCustomerCount = conversation.UnreadByCustomerCount
        };
    }

    private static ChatMessageDto ToMessageDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderType = message.SenderType,
            SenderUserId = message.SenderUserId,
            Message = message.Message,
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead
        };
    }
}
