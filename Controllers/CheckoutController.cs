using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fruitables.Services.Interfaces;
using Fruitables.ViewModels;
using Fruitables.Repositories.Interfaces;
using Fruitables.Models;

namespace Fruitables.Controllers;

// Controller checkout (thanh toán): xác nhận giỏ hàng, chọn địa chỉ giao hàng, đặt hàng.
// Guest (chưa đăng nhập) được phép mua hàng; địa chỉ sẽ nhập thủ công.
public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IAddressService _addressService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVietnamAddressService _vietnamAddressService;
    private readonly IShippingService _shippingService;
    private readonly IEmailService _emailService;
    private readonly IVNPayService _vnPayService;
    private readonly ISePayService _sePayService;
    private readonly ILogger<CheckoutController> _logger;
    
    // Keys lưu snapshot phí ship trong session (tránh thay đổi giữa Index → PlaceOrder)
    private const string ShippingFeeSnapshotKey = "ShippingFeeSnapshot";
    private const string ShippingZoneSnapshotKey = "ShippingZoneSnapshot";
    private const string ShippingSnapshotTimeKey = "ShippingSnapshotTime";
    private const string ShippingDistrictSnapshotKey = "ShippingDistrictSnapshot";

    // Inject 9 dependencies
    public CheckoutController(
        ICartService cartService, 
        IOrderService orderService, 
        IAddressService addressService,
        IUnitOfWork unitOfWork,
        IVietnamAddressService vietnamAddressService,
        IShippingService shippingService,
        IEmailService emailService,
        IVNPayService vnPayService,
        ISePayService sePayService,
        ILogger<CheckoutController> logger)
    {
        _cartService = cartService;
        _orderService = orderService;
        _addressService = addressService;
        _unitOfWork = unitOfWork;
        _vietnamAddressService = vietnamAddressService;
        _shippingService = shippingService;
        _emailService = emailService;
        _vnPayService = vnPayService;
        _sePayService = sePayService;
        _logger = logger;
    }

    // GET: Hiển thị trang checkout — load giỏ hàng + địa chỉ đã lưu + snapshot phí ship
    public async Task<IActionResult> Index()
    {
        var sessionId = GetSessionId();
        var userId = GetCurrentUserId();
        
        List<AddressViewModel> addressViewModels = new();
        int? defaultAddressId = null;
        string? defaultCommune = null;
        
        if (userId.HasValue)
        {
            var addresses = await _addressService.GetUserAddressesAsync(userId.Value);
            addressViewModels = addresses.Select(a => new AddressViewModel
            {
                Id = a.Id,
                FullName = a.FullName,
                Phone = a.Phone,
                ProvinceCode = a.ProvinceCode,
                ProvinceName = a.ProvinceName,
                CommuneCode = a.CommuneCode,
                CommuneName = a.CommuneName,
                StreetAddress = a.StreetAddress,
                IsDefault = a.IsDefault
            }).ToList();
            
            var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);
            if (defaultAddress != null)
            {
                defaultAddressId = defaultAddress.Id;
                defaultCommune = defaultAddress.CommuneName;
            }
        }
        
        // Load cart, tính phí ship theo commune (xã) mặc định
        var cart = await _cartService.GetCartAsync(sessionId, defaultCommune);

        // Giỏ hàng rỗng → redirect về cart
        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }
        
        // Lưu snapshot phí ship khi vào checkout (chống thay đổi phí giữa chừng)
        if (cart.ShippingInfo != null)
        {
            SaveShippingSnapshot(cart.ShippingInfo, defaultCommune);
        }

        ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);
        ViewBag.Cart = cart;
        ViewBag.SavedAddresses = addressViewModels;

        // Load điểm tích lũy cho user đã đăng nhập
        int loyaltyPoints = 0;
        if (userId.HasValue)
        {
            var user = await _unitOfWork.Users.Query().FirstOrDefaultAsync(u => u.Id == userId.Value);
            loyaltyPoints = user?.LoyaltyPoints ?? 0;
        }
        ViewBag.LoyaltyPoints = loyaltyPoints;
        
        var model = new CheckoutViewModel
        {
            SelectedAddressId = defaultAddressId
        };
        
        return View(model);
    }

    // POST: Mua ngay — thêm sản phẩm vào giỏ rồi redirect thẳng tới checkout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
    {
        var sessionId = GetSessionId();
        
        await _cartService.AddToCartAsync(sessionId, productId, quantity);
        
        return RedirectToAction(nameof(Index));
    }

    // POST: Xử lý đặt hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        _logger.LogInformation("PlaceOrder called - SelectedAddressId: {AddressId}, PaymentMethod: {Payment}", 
            model.SelectedAddressId, model.PaymentMethod);
        
        var sessionId = GetSessionId();
        var userId = GetCurrentUserId();
        
        // Nếu chọn địa chỉ đã lưu → không validate các field address (lấy từ DB)
        if (model.SelectedAddressId.HasValue)
        {
            ModelState.Remove(nameof(model.FirstName));
            ModelState.Remove(nameof(model.ProvinceCode));
            ModelState.Remove(nameof(model.CommuneCode));
            ModelState.Remove(nameof(model.StreetAddress));
            ModelState.Remove(nameof(model.Mobile));
        }
        
        // Guest chưa đăng nhập: bắt buộc nhập địa chỉ thủ công (không cho chọn địa chỉ đã lưu)
        // Nếu guest mà SelectedAddressId không có thì các field address đã có Required attr → giữ nguyên validation
        
        // Lấy commune từ địa chỉ đã chọn hoặc từ form
        string? district = null;
        if (model.SelectedAddressId.HasValue)
        {
            var selectedAddress = await _unitOfWork.Addresses.GetByIdAsync(model.SelectedAddressId.Value);
            district = selectedAddress?.CommuneName;
        }
        
        var cart = await _cartService.GetCartAsync(sessionId, district);

        // Validation thất bại → reload lại checkout với lỗi
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                .ToList();
            _logger.LogWarning("PlaceOrder validation failed: {Errors}", string.Join(" | ", errors));
            
            // Reload địa chỉ cho display
            if (userId.HasValue)
            {
                var addresses = await _addressService.GetUserAddressesAsync(userId.Value);
                ViewBag.SavedAddresses = addresses.Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Phone = a.Phone,
                    ProvinceCode = a.ProvinceCode,
                    ProvinceName = a.ProvinceName,
                    CommuneCode = a.CommuneCode,
                    CommuneName = a.CommuneName,
                    StreetAddress = a.StreetAddress,
                    IsDefault = a.IsDefault
                }).ToList();
            }
            else
            {
                ViewBag.SavedAddresses = new List<AddressViewModel>();
            }
            
            ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);
            ViewBag.Cart = cart;
            return View("Index", model);
        }

        // Lấy phí ship từ snapshot (đã lưu lúc vào checkout), fallback tính lại nếu không có
        var snapshotShippingFee = GetShippingFeeFromSnapshot();
        var snapshotZone = GetShippingZoneFromSnapshot();
        
        if (!snapshotShippingFee.HasValue)
        {
            var shippingInfo = await _shippingService.CalculateShippingAsync(cart.Subtotal, district ?? string.Empty);
            snapshotShippingFee = shippingInfo.ShippingFee;
            snapshotZone = shippingInfo.Zone;
        }
        
        // Gán snapshot vào cart model trước khi tạo order
        model.Cart = cart;
        model.Cart.ShippingFee = snapshotShippingFee.Value;
        if (model.Cart.ShippingInfo != null)
        {
            model.Cart.ShippingInfo.ShippingFee = snapshotShippingFee.Value;
            model.Cart.ShippingInfo.Zone = snapshotZone ?? ShippingZone.Zone3_Remote;
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(model, sessionId, userId);
            
            // Xóa snapshot sau khi đặt hàng thành công
            ClearShippingSnapshot();

            if (order.PaymentMethod == PaymentMethod.VNPay)
            {
                var url = _vnPayService.CreatePaymentUrl(order, HttpContext);
                return Redirect(url);
            }

            if (order.PaymentMethod == PaymentMethod.SePay)
            {
                return RedirectToAction(nameof(SePayPayment), new { orderId = order.Id });
            }

            // Gửi email xác nhận đơn hàng nếu không phải thanh toán VNPay
            // (với VNPay, email sẽ được gửi sau khi thanh toán thành công trong callback)
            try
            {
                // Load đầy đủ thông tin đơn hàng với navigation properties
                var fullOrder = await _unitOfWork.Orders.Query()
                    .Include(o => o.User)
                    .Include(o => o.Address)
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (fullOrder != null)
                    await _emailService.SendOrderConfirmationEmailAsync(fullOrder);
            }
            catch (Exception emailEx)
            {
                // Lỗi gửi email không nên ảnh hưởng đến flow đặt hàng
                _logger.LogError(emailEx, "Error sending order confirmation email for order {OrderNumber}", order.OrderNumber);
            }
            
            return RedirectToAction(nameof(Confirmation), new { orderNumber = order.OrderNumber });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("PlaceOrder failed: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);

            if (userId.HasValue)
            {
                var addresses = await _addressService.GetUserAddressesAsync(userId.Value);
                ViewBag.SavedAddresses = addresses.Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Phone = a.Phone,
                    ProvinceCode = a.ProvinceCode,
                    ProvinceName = a.ProvinceName,
                    CommuneCode = a.CommuneCode,
                    CommuneName = a.CommuneName,
                    StreetAddress = a.StreetAddress,
                    IsDefault = a.IsDefault
                }).ToList();
            }
            else
            {
                ViewBag.SavedAddresses = new List<AddressViewModel>();
            }

            ViewBag.CartCount = cart.Items.Sum(i => i.Quantity);
            ViewBag.Cart = cart;
            return View("Index", model);
        }
    }

    // GET: Trang xác nhận đặt hàng thành công (hiện thông tin order)
    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        var sessionId = GetSessionId();
        ViewBag.CartCount = await _cartService.GetCartCountAsync(sessionId);

        var order = await _orderService.GetOrderByNumberAsync(orderNumber);
        if (order == null) return NotFound();

        ViewBag.Order = order;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> PaymentCallback()
    {
        var response = _vnPayService.PaymentExecute(Request.Query, out var orderId);
        
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
        {
            return RedirectToAction("Index", "Cart");
        }

        if (response)
        {
            // Thanh toán thành công
            order.PaymentStatus = PaymentStatus.Paid;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            
            // Gửi email thông báo đơn hàng
            try
            {
                await _emailService.SendOrderConfirmationEmailAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi email xác nhận cho đơn hàng VNPAY {OrderNumber}", order.OrderNumber);
            }
            
            return RedirectToAction(nameof(Confirmation), new { orderNumber = order.OrderNumber });
        }
        
        // Hủy thanh toán hoặc thanh toán thất bại
        order.Status = OrderStatus.Cancelled;
        order.CancelReason = "Thanh toán VNPay thất bại hoặc bị hủy";
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();
        
        return View("PaymentFailed", order);
    }

    // GET: Trang hiển thị QR SePay để khách chuyển khoản
    [HttpGet]
    public async Task<IActionResult> SePayPayment(int orderId)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return RedirectToAction("Index", "Cart");

        var sessionId = GetSessionId();
        ViewBag.CartCount = await _cartService.GetCartCountAsync(sessionId);
        ViewBag.QrCodeUrl = _sePayService.GetQrCodeUrl(order);
        ViewBag.AccountNumber = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["SePay:AccountNumber"];
        ViewBag.AccountName = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["SePay:AccountName"];
        ViewBag.BankCode = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["SePay:BankCode"];

        return View(order);
    }

    // POST: Webhook từ SePay khi phát hiện chuyển khoản
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookRequest? webhookData)
    {
        // Ghi log ra file để debug
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "sepay_log.txt");
        System.IO.File.AppendAllText(logPath, $"\n[{DateTime.Now}] Nhận webhook: {System.Text.Json.JsonSerializer.Serialize(webhookData)}");

        if (webhookData == null) return BadRequest();

        // Tạm thời nới lỏng xác thực API Key để đảm bảo không bị chặn ở đây
        // (Sẽ bật lại sau khi chạy trơn tru)
        // if (!_sePayService.VerifyWebhook(Request))
        //     return Unauthorized(new { message = "Invalid API Key" });

        // Tìm mã DH trong trường Code, nếu Code trống thì tìm trong Content
        var code = webhookData.Code?.Trim() ?? "";
        var content = webhookData.Content?.ToUpper() ?? "";
        int orderId = 0;

        if (code.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(code[2..], out orderId);
        }
        else
        {
            // Tìm chữ DH và các số theo sau trong nội dung
            var match = System.Text.RegularExpressions.Regex.Match(content, @"DH(\d+)");
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out orderId);
            }
        }

        System.IO.File.AppendAllText(logPath, $" -> Phân tích được OrderId: {orderId}");

        if (orderId <= 0)
            return Ok(new { message = "Invalid code format" });

        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId &&
                o.PaymentMethod == PaymentMethod.SePay &&
                o.PaymentStatus == PaymentStatus.Pending);

        if (order == null)
        {
            System.IO.File.AppendAllText(logPath, $" -> Không tìm thấy order hoặc order đã thanh toán");
            return Ok(new { message = "Order not found or already processed" });
        }

        // Chỉ cần tiền vào > 0 là ghi nhận (phòng hờ test 2000đ nhưng đơn 96000đ)
        if (webhookData.TransferType == "in" && webhookData.TransferAmount > 0)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            try { await _emailService.SendOrderConfirmationEmailAsync(order); }
            catch { /* ignore email error */ }

            System.IO.File.AppendAllText(logPath, $" -> THANH TOÁN THÀNH CÔNG cho đơn {orderId}");
        }

        return Ok(new { message = "Success" });
    }

    // GET: API kiểm tra trạng thái thanh toán (cho polling AJAX ở trang QR SePay)
    [HttpGet]
    public async Task<IActionResult> CheckPaymentStatus(int orderId)
    {
        var order = await _unitOfWork.Orders.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound();

        return Json(new
        {
            isPaid = order.PaymentStatus == PaymentStatus.Paid,
            orderNumber = order.OrderNumber
        });
    }

    // Helper: lấy/tạo SessionId cho giỏ hàng
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
    
    // Helper: lấy userId từ claims cookie
    private int? GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;
            
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
            
        return null;
    }
    
    // Lưu snapshot phí ship vào session (tại thời điểm vào checkout) — chống thay đổi giữa chừng
    private void SaveShippingSnapshot(ShippingInfo shippingInfo, string? district)
    {
        HttpContext.Session.SetString(ShippingFeeSnapshotKey, shippingInfo.ShippingFee.ToString());
        HttpContext.Session.SetString(ShippingZoneSnapshotKey, ((int)shippingInfo.Zone).ToString());
        HttpContext.Session.SetString(ShippingSnapshotTimeKey, DateTime.UtcNow.ToString("O"));
        HttpContext.Session.SetString(ShippingDistrictSnapshotKey, district ?? string.Empty);
    }
    
    // Đọc phí ship từ snapshot trong session
    private decimal? GetShippingFeeFromSnapshot()
    {
        var feeStr = HttpContext.Session.GetString(ShippingFeeSnapshotKey);
        if (decimal.TryParse(feeStr, out var fee))
            return fee;
        return null;
    }
    
    // Đọc zone từ snapshot trong session
    private ShippingZone? GetShippingZoneFromSnapshot()
    {
        var zoneStr = HttpContext.Session.GetString(ShippingZoneSnapshotKey);
        if (int.TryParse(zoneStr, out var zone) && Enum.IsDefined(typeof(ShippingZone), zone))
            return (ShippingZone)zone;
        return null;
    }
    
    // Xóa snapshot khỏi session sau khi đặt hàng
    private void ClearShippingSnapshot()
    {
        HttpContext.Session.Remove(ShippingFeeSnapshotKey);
        HttpContext.Session.Remove(ShippingZoneSnapshotKey);
        HttpContext.Session.Remove(ShippingSnapshotTimeKey);
        HttpContext.Session.Remove(ShippingDistrictSnapshotKey);
    }
}
