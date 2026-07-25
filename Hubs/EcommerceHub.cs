using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fruitables.Services.Interfaces;

namespace Fruitables.Hubs
{
    public class EcommerceHub : Hub
    {
        private readonly IChatService _chatService;

        public EcommerceHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Join user-specific group
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User:{userId}");
                }

                // Join Admins group if user is Admin or SuperAdmin
                if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
            }

            await base.OnConnectedAsync();
        }

        public async Task<ChatConversationPayload> StartContactChat(string name, string phone)
        {
            var userId = GetCurrentUserId();
            var guestSessionId = userId.HasValue ? null : GetOrCreateGuestSessionId();

            var conversation = await _chatService.StartOrGetConversationAsync(userId, guestSessionId, name, phone);
            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversation.Id));

            var payload = ChatConversationPayload.FromDto(conversation);
            await Clients.Group("Admins").SendAsync("ContactChatStarted", payload);
            return payload;
        }

        public async Task<ChatMessagePayload> SendCustomerMessage(int conversationId, string message)
        {
            var userId = GetCurrentUserId();
            var guestSessionId = userId.HasValue ? null : GetOrCreateGuestSessionId();
            var chatMessage = await _chatService.AddCustomerMessageAsync(conversationId, userId, guestSessionId, message);
            var conversation = await _chatService.GetConversationAsync(conversationId);

            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));

            var messagePayload = ChatMessagePayload.FromDto(chatMessage);
            var conversationPayload = conversation == null ? null : ChatConversationPayload.FromDto(conversation);

            await Clients.Group(GetConversationGroup(conversationId)).SendAsync("ContactChatMessageReceived", messagePayload);
            await Clients.Group("Admins").SendAsync("ContactChatConversationUpdated", conversationPayload);
            return messagePayload;
        }

        public async Task<List<ChatMessagePayload>> GetCustomerMessages(int conversationId)
        {
            var userId = GetCurrentUserId();
            var guestSessionId = userId.HasValue ? null : GetOrCreateGuestSessionId();

            if (!await _chatService.CustomerCanAccessConversationAsync(conversationId, userId, guestSessionId))
            {
                throw new HubException("Unauthorized to read this conversation.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));
            await _chatService.MarkConversationReadAsync(conversationId, readByAdmin: false, userId, guestSessionId);

            var messages = await _chatService.GetMessagesAsync(conversationId);
            return messages.Select(ChatMessagePayload.FromDto).ToList();
        }

        public async Task JoinAdminConversation(int conversationId)
        {
            EnsureAdmin();
            if (conversationId <= 0) throw new HubException("Invalid conversationId.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));
            await _chatService.MarkConversationReadAsync(conversationId, readByAdmin: true, userId: null, guestSessionId: null);
        }

        public async Task<ChatMessagePayload> SendAdminMessage(int conversationId, string message)
        {
            EnsureAdmin();
            var adminUserId = GetCurrentUserId() ?? throw new HubException("Admin user id is missing.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));

            var chatMessage = await _chatService.AddAdminMessageAsync(conversationId, adminUserId, message);
            var conversation = await _chatService.GetConversationAsync(conversationId);
            var messagePayload = ChatMessagePayload.FromDto(chatMessage);
            var conversationPayload = conversation == null ? null : ChatConversationPayload.FromDto(conversation);

            await Clients.Group(GetConversationGroup(conversationId)).SendAsync("ContactChatMessageReceived", messagePayload);
            await Clients.Group("Admins").SendAsync("ContactChatConversationUpdated", conversationPayload);
            return messagePayload;
        }

        public async Task MarkConversationRead(int conversationId)
        {
            var isAdmin = IsAdmin();
            var userId = GetCurrentUserId();
            var guestSessionId = isAdmin || userId.HasValue ? null : GetOrCreateGuestSessionId();

            await _chatService.MarkConversationReadAsync(conversationId, isAdmin, userId, guestSessionId);
        }

        public async Task JoinOrderGroup(int orderId, [Microsoft.AspNetCore.Mvc.FromServices] Fruitables.Data.ApplicationDbContext dbContext)
        {
            if (orderId <= 0) throw new HubException("Invalid orderId.");

            // Optional: verify if user owns the order or is admin
            if (Context.User != null && (Context.User.IsInRole("Admin") || Context.User.IsInRole("SuperAdmin")))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Order:{orderId}");
                return;
            }

            // Customer
            var userIdStr = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var orderExists = await dbContext.Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId);
                if (orderExists)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Order:{orderId}");
                    return;
                }
            }
            
            throw new HubException("Unauthorized to join this order group.");
        }

        public async Task LeaveOrderGroup(int orderId)
        {
            if (orderId <= 0) return;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order:{orderId}");
        }

        public async Task JoinProductGroup(int productId)
        {
            if (productId <= 0) throw new HubException("Invalid productId.");
            // Anyone can join product group to see stock updates
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Product:{productId}");
        }

        public async Task LeaveProductGroup(int productId)
        {
            if (productId <= 0) return;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Product:{productId}");
        }

        private int? GetCurrentUserId()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }

        private bool IsAdmin()
        {
            return Context.User?.Identity?.IsAuthenticated == true &&
                   (Context.User.IsInRole("Admin") || Context.User.IsInRole("SuperAdmin"));
        }

        private void EnsureAdmin()
        {
            if (!IsAdmin())
            {
                throw new HubException("Unauthorized admin chat action.");
            }
        }

        private string GetOrCreateGuestSessionId()
        {
            var session = Context.GetHttpContext()?.Session;
            var sessionId = session?.GetString("SessionId");
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = System.Guid.NewGuid().ToString();
                session?.SetString("SessionId", sessionId);
            }

            return sessionId;
        }

        private static string GetConversationGroup(int conversationId) => $"Conversation:{conversationId}";
    }

    public class ChatConversationPayload
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadByAdminCount { get; set; }
        public int UnreadByCustomerCount { get; set; }

        public static ChatConversationPayload FromDto(Fruitables.ViewModels.ChatConversationDto dto)
        {
            return new ChatConversationPayload
            {
                Id = dto.Id,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                LastMessagePreview = dto.LastMessagePreview,
                LastMessageAt = dto.LastMessageAt,
                UnreadByAdminCount = dto.UnreadByAdminCount,
                UnreadByCustomerCount = dto.UnreadByCustomerCount
            };
        }
    }

    public class ChatMessagePayload
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public int? SenderUserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static ChatMessagePayload FromDto(Fruitables.ViewModels.ChatMessageDto dto)
        {
            return new ChatMessagePayload
            {
                Id = dto.Id,
                ConversationId = dto.ConversationId,
                SenderType = dto.SenderType.ToString(),
                SenderUserId = dto.SenderUserId,
                Message = dto.Message,
                CreatedAt = dto.CreatedAt
            };
        }
    }
}
