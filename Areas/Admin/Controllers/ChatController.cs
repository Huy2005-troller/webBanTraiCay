using Fruitables.Services.Interfaces;
using Fruitables.ViewModels;
using Fruitables.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fruitables.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new AdminChatViewModel
        {
            Conversations = await _chatService.GetAdminConversationsAsync()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Messages(int conversationId)
    {
        if (conversationId <= 0)
        {
            return BadRequest(new { error = "ConversationId không hợp lệ." });
        }

        await _chatService.MarkConversationReadAsync(conversationId, readByAdmin: true, userId: null, guestSessionId: null);
        var messages = await _chatService.GetMessagesAsync(conversationId);
        return Json(messages.Select(ChatMessagePayload.FromDto));
    }
}
