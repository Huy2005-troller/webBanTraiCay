using Fruitables.Data;
using Fruitables.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Fruitables.Controllers;

public class ContactController : Controller
{
    private readonly IContactService _contactService;
    private readonly ICartService _cartService;
    private readonly IChatService _chatService;
    private readonly ApplicationDbContext _context;

    public ContactController(
        IContactService contactService,
        ICartService cartService,
        IChatService chatService,
        ApplicationDbContext context)
    {
        _contactService = contactService;
        _cartService = cartService;
        _chatService = chatService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sessionId = GetSessionId();
        ViewBag.CartCount = await _cartService.GetCartCountAsync(sessionId);
        ViewBag.ChatCustomerName = string.Empty;
        ViewBag.ChatCustomerPhone = string.Empty;
        ViewBag.ChatConversationId = 0;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Name, u.Phone })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                var phone = string.IsNullOrWhiteSpace(user.Phone) ? "Chưa cập nhật" : user.Phone;
                var conversation = await _chatService.StartOrGetConversationAsync(userId, null, user.Name, phone);
                ViewBag.ChatCustomerName = user.Name;
                ViewBag.ChatCustomerPhone = phone;
                ViewBag.ChatConversationId = conversation.Id;
            }
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(string name, string email, string message)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
        {
            TempData["Error"] = "Please fill in all fields.";
            return RedirectToAction(nameof(Index));
        }

        await _contactService.SendMessageAsync(name, email, message);
        TempData["Success"] = "Your message has been sent successfully!";
        return RedirectToAction(nameof(Index));
    }

    private string GetSessionId()
    {
        var sessionId = HttpContext.Session.GetString("SessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("SessionId", sessionId);
        }
        return sessionId;
    }
}
